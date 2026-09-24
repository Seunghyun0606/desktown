using System.Text.Json;
using DeskTown.Application.Configuration;
using DeskTown.Application.Ports;
using global::Godot;

namespace DeskTown.Presentation.Bootstrap;

public sealed class GodotStructuredLogger : IStructuredLogger
{
    public void Information(string eventName, IReadOnlyDictionary<string, object?> fields)
    {
        Write("information", eventName, fields, exception: null);
    }

    public void Error(
        string eventName,
        Exception exception,
        IReadOnlyDictionary<string, object?> fields)
    {
        ArgumentNullException.ThrowIfNull(exception);
        Write("error", eventName, fields, exception.GetType().Name);
    }

    private static void Write(
        string level,
        string eventName,
        IReadOnlyDictionary<string, object?> fields,
        string? exception)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        PrivacyFieldPolicy.EnsureAllowed(fields.Keys);

        var payload = fields.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);

        payload["level"] = level;
        payload["event_name"] = eventName;
        payload["timestamp_utc"] = DateTimeOffset.UtcNow;

        if (exception is not null)
        {
            payload["exception_type"] = exception;
        }

        GD.Print(JsonSerializer.Serialize(payload));
    }
}
