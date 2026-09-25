using DeskTown.Platform.Windows.Activity;

namespace DeskTown.Platform.Windows.Tests.Activity;

internal sealed class FakeWindowsActivityNativeApi : IWindowsActivityNativeApi
{
    public bool ForegroundAvailable { get; set; } = true;
    public uint ForegroundProcessId { get; set; } = 42;
    public bool IdleAvailable { get; set; } = true;
    public TimeSpan IdleDuration { get; set; }
    public Exception? ForegroundException { get; set; }
    public Exception? IdleException { get; set; }
    public int ForegroundReadCount { get; private set; }
    public int IdleReadCount { get; private set; }

    public bool TryGetForegroundProcessId(out uint processId)
    {
        ForegroundReadCount++;
        if (ForegroundException is not null)
        {
            throw ForegroundException;
        }

        processId = ForegroundProcessId;
        return ForegroundAvailable;
    }

    public bool TryGetIdleDuration(out TimeSpan idleDuration)
    {
        IdleReadCount++;
        if (IdleException is not null)
        {
            throw IdleException;
        }

        idleDuration = IdleDuration;
        return IdleAvailable;
    }
}
