using DeskTown.Platform.Windows.Activity;

namespace DeskTown.Platform.Windows.Tests.Activity;

internal sealed class FakeProcessNameResolver : IProcessNameResolver
{
    public bool Available { get; set; } = true;
    public string ProcessName { get; set; } = "code";
    public Exception? Exception { get; set; }
    public int ResolveCount { get; private set; }
    public uint LastProcessId { get; private set; }

    public bool TryResolve(uint processId, out string processName)
    {
        ResolveCount++;
        LastProcessId = processId;
        if (Exception is not null)
        {
            throw Exception;
        }

        processName = ProcessName;
        return Available;
    }
}
