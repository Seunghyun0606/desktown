using DeskTown.Application.Lifecycle;

namespace DeskTown.Application.Tests;

public sealed class SystemPauseStateTests
{
    [Fact]
    public void LockAndPowerMustBothClearBeforeFocusCanResume()
    {
        var state = new SystemPauseState();
        state.Set(SystemPauseReason.SessionLocked, true);
        state.Set(SystemPauseReason.PowerSuspended, true);
        state.Set(SystemPauseReason.PowerSuspended, true);
        state.Set(SystemPauseReason.PowerSuspended, false);
        Assert.True(state.IsPaused);

        state.Set(SystemPauseReason.SessionLocked, false);
        Assert.False(state.IsPaused);
    }
}
