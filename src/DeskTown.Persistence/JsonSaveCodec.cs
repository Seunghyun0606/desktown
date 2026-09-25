using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using DeskTown.Application.Persistence;

namespace DeskTown.Persistence;

/// <summary>V1 JSON envelope with SHA-256 over the whole JSON object excluding integrity.</summary>
public static class JsonSaveCodec
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions CanonicalOptions = new(JsonSerializerDefaults.Web);

    public static byte[] Encode(GameStateSnapshot snapshot, long revision, DateTimeOffset savedAtUtc)
    {
        var envelope = SaveStateMapper.ToEnvelope(snapshot, revision, savedAtUtc);
        var payload = JsonSerializer.SerializeToNode(envelope, Options) as JsonObject
            ?? throw new InvalidDataException("Save payload is not an object.");
        payload.Remove("integrity");
        var digest = ComputeHash(payload);
        payload["integrity"] = new JsonObject { ["payloadSha256"] = digest };
        return JsonSerializer.SerializeToUtf8Bytes(payload, Options);
    }

    public static StoredGameState Decode(ReadOnlySpan<byte> bytes)
    {
        JsonObject payload;
        try
        {
            payload = JsonNode.Parse(bytes) as JsonObject
                ?? throw new InvalidDataException("Save payload is not a JSON object.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Save JSON is malformed.", error);
        }

        // Check version before parsing DTOs so future versions cannot be mistaken for V1.
        int? version;
        try
        {
            version = payload["schemaVersion"]?.GetValue<int>();
        }
        catch (Exception error) when (error is InvalidOperationException or FormatException)
        {
            throw new InvalidDataException("Invalid save schema version.", error);
        }
        if (version != 1) throw new InvalidDataException("Unsupported save schema version.");

        var integrity = payload["integrity"] as JsonObject;
        string? savedHash;
        try
        {
            savedHash = integrity?["payloadSha256"]?.GetValue<string>();
        }
        catch (Exception error) when (error is InvalidOperationException or FormatException)
        {
            throw new InvalidDataException("Invalid payload integrity hash.", error);
        }
        if (savedHash is null || savedHash.Length != 64
            || !savedHash.All(Uri.IsHexDigit))
        {
            throw new InvalidDataException("Missing or malformed payload integrity hash.");
        }

        // Cloning preserves unknown fields: V1 readers ignore them while the full
        // unknown-inclusive payload remains protected by integrity verification.
        var withoutIntegrity = (JsonObject)payload.DeepClone();
        withoutIntegrity.Remove("integrity");
        var computed = Convert.FromHexString(ComputeHash(withoutIntegrity));
        if (!CryptographicOperations.FixedTimeEquals(computed, Convert.FromHexString(savedHash)))
            throw new InvalidDataException("Payload integrity hash mismatch.");

        try
        {
            var envelope = payload.Deserialize<SaveEnvelopeV1>(Options)
                ?? throw new InvalidDataException("Save envelope is missing.");
            return SaveStateMapper.FromEnvelope(envelope);
        }
        catch (Exception error) when (error is JsonException or ArgumentException or OverflowException)
        {
            throw new InvalidDataException("Save data is invalid.", error);
        }
    }

    internal static int ReadSchemaVersion(ReadOnlySpan<byte> bytes)
    {
        try
        {
            var root = JsonNode.Parse(bytes) as JsonObject
                ?? throw new InvalidDataException("Save payload is not an object.");
            return root["schemaVersion"]?.GetValue<int>()
                ?? throw new InvalidDataException("Save schema version is missing.");
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException or FormatException)
        {
            throw new InvalidDataException("Save schema version is invalid.", error);
        }
    }

    internal static byte[] SealMigratedPayload(JsonObject payload)
    {
        payload.Remove("integrity");
        payload["integrity"] = new JsonObject { ["payloadSha256"] = ComputeHash(payload) };
        return JsonSerializer.SerializeToUtf8Bytes(payload, Options);
    }

    private static string ComputeHash(JsonObject payload)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, CanonicalOptions);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}
