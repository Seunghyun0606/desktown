namespace DeskTown.Application.Ports;

public interface IStructuredLogger
{
    void Information(string eventName, IReadOnlyDictionary<string, object?> fields);

    void Error(string eventName, Exception exception, IReadOnlyDictionary<string, object?> fields);
}
