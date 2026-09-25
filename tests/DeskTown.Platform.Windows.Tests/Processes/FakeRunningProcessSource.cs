using DeskTown.Platform.Windows.Processes;

namespace DeskTown.Platform.Windows.Tests.Processes;

internal sealed class FakeRunningProcessSource : IRunningProcessSource
{
    public IReadOnlyCollection<string> ProcessNames { get; init; } = Array.Empty<string>();

    public Exception? Exception { get; init; }

    public IReadOnlyCollection<string> ReadProcessNames()
    {
        if (Exception is not null)
        {
            throw Exception;
        }

        return ProcessNames;
    }
}
