namespace DeskTown.Application.Lifecycle;

public enum SystemPauseReason { SessionLocked, SessionDisconnected, PowerSuspended }

/// <summary>Independent Windows pause signals must all clear before Focus resumes.</summary>
public sealed class SystemPauseState
{
    private readonly HashSet<SystemPauseReason> _reasons = [];

    public bool IsPaused => _reasons.Count > 0;

    public void Set(SystemPauseReason reason, bool paused)
    {
        if (!Enum.IsDefined(reason)) throw new ArgumentOutOfRangeException(nameof(reason));
        if (paused) _reasons.Add(reason);
        else _reasons.Remove(reason);
    }
}
