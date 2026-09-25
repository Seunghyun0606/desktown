using DeskTown.Application.Persistence;
using DeskTown.Application.Ports;

namespace DeskTown.Persistence;

/// <summary>
/// Simple configurable-path V1 repository. E8-002 will add serialized atomic
/// replace, backup, recovery, and migrations; callers must not treat this as
/// crash-safe until that task is completed.
/// </summary>
public sealed class JsonGameStateStore : IGameStateStore
{
    private readonly string _saveFilePath;

    public JsonGameStateStore(string saveFilePath)
    {
        if (string.IsNullOrWhiteSpace(saveFilePath))
            throw new ArgumentException("A save file path is required.", nameof(saveFilePath));
        _saveFilePath = Path.GetFullPath(saveFilePath);
    }

    public async Task<StoredGameState?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_saveFilePath)) return null;
        var bytes = await File.ReadAllBytesAsync(_saveFilePath, cancellationToken);
        return JsonSaveCodec.Decode(bytes);
    }

    public async Task SaveAsync(GameStateSnapshot snapshot, long revision,
        DateTimeOffset savedAtUtc, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSaveCodec.Encode(snapshot, revision, savedAtUtc);
        if (File.Exists(_saveFilePath))
        {
            // Preserve an unsupported or damaged original for E8-002 recovery.
            // This is a sequential guard, not the atomic concurrency protocol.
            var existing = JsonSaveCodec.Decode(
                await File.ReadAllBytesAsync(_saveFilePath, cancellationToken));
            if (revision <= existing.Revision)
                throw new InvalidDataException("A save revision must increase.");
        }

        var directory = Path.GetDirectoryName(_saveFilePath)!;
        Directory.CreateDirectory(directory);
        await File.WriteAllBytesAsync(_saveFilePath, bytes, cancellationToken);
    }
}
