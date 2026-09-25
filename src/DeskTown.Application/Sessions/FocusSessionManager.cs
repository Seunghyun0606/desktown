using DeskTown.Application.Ports;
using DeskTown.Domain.Focus;

namespace DeskTown.Application.Sessions;

public sealed class FocusSessionManager
{
    private readonly IWallClock _wallClock;
    private readonly IMonotonicClock _monotonicClock;
    private TimeSpan? _lastMonotonicElapsed;

    public FocusSessionManager(IWallClock wallClock, IMonotonicClock monotonicClock)
    {
        _wallClock = wallClock ?? throw new ArgumentNullException(nameof(wallClock));
        _monotonicClock = monotonicClock ?? throw new ArgumentNullException(nameof(monotonicClock));
    }

    public event Action<FocusSessionLifecycleEvent>? LifecycleChanged;

    public FocusSession? CurrentSession { get; private set; }

    public FocusSession? ActiveSession =>
        CurrentSession?.Status is FocusSessionStatus.Running or FocusSessionStatus.Suspended
            ? CurrentSession
            : null;

    public FocusSession Start(FocusSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (ActiveSession is not null)
        {
            throw new FocusSessionManagerException("A focus session is already active.");
        }

        var nowUtc = ReadUtcNow();
        var monotonicBaseline = _monotonicClock.Elapsed;
        session.Start(nowUtc);

        CurrentSession = session;
        _lastMonotonicElapsed = monotonicBaseline;
        Publish(new FocusSessionStarted(session.Id, nowUtc, session.CountedDuration));
        return session;
    }

    public FocusSessionTickResult Tick(TimeSpan idleSinceLastTick)
    {
        var session = RequireActiveSession();
        if (session.Status == FocusSessionStatus.Suspended)
        {
            return FocusSessionTickResult.Suspended;
        }

        return AdvanceRunningSession(session, idleSinceLastTick);
    }

    /// <summary>
    /// Applies an observed idle state to the actual monotonic interval. This
    /// avoids treating a nominal one-second sample as elapsed time when a tick
    /// arrives early or late.
    /// </summary>
    public FocusSessionTickResult Tick(bool observedIdle)
    {
        var session = RequireActiveSession();
        if (session.Status == FocusSessionStatus.Suspended)
        {
            return FocusSessionTickResult.Suspended;
        }

        return AdvanceRunningSession(session, observedIdle);
    }

    public FocusSessionStatus Suspend(TimeSpan idleSinceLastTick)
    {
        var session = RequireActiveSession();
        if (session.Status != FocusSessionStatus.Running)
        {
            throw new FocusSessionManagerException("Only a running focus session can be suspended.");
        }

        var tick = AdvanceRunningSession(session, idleSinceLastTick);
        if (tick.Completed)
        {
            return FocusSessionStatus.Completed;
        }

        var nowUtc = ReadUtcNow();
        session.Suspend();
        _lastMonotonicElapsed = null;
        Publish(new FocusSessionSuspended(session.Id, nowUtc, session.CountedDuration));
        return session.Status;
    }

    public FocusSessionStatus Suspend(bool observedIdle)
    {
        var session = RequireActiveSession();
        if (session.Status != FocusSessionStatus.Running)
        {
            throw new FocusSessionManagerException("Only a running focus session can be suspended.");
        }

        var tick = AdvanceRunningSession(session, observedIdle);
        if (tick.Completed)
        {
            return FocusSessionStatus.Completed;
        }

        var nowUtc = ReadUtcNow();
        session.Suspend();
        _lastMonotonicElapsed = null;
        Publish(new FocusSessionSuspended(session.Id, nowUtc, session.CountedDuration));
        return session.Status;
    }

    public FocusSession Resume()
    {
        var session = RequireActiveSession();
        if (session.Status != FocusSessionStatus.Suspended)
        {
            throw new FocusSessionManagerException("Only a suspended focus session can be resumed.");
        }

        var nowUtc = ReadUtcNow();
        var monotonicBaseline = _monotonicClock.Elapsed;
        session.Resume();
        _lastMonotonicElapsed = monotonicBaseline;
        Publish(new FocusSessionResumed(session.Id, nowUtc, session.CountedDuration));
        return session;
    }

    public FocusSession Stop(TimeSpan idleSinceLastTick)
    {
        var session = RequireActiveSession();

        if (session.Status == FocusSessionStatus.Running)
        {
            var tick = AdvanceRunningSession(session, idleSinceLastTick);
            if (tick.Completed)
            {
                return session;
            }
        }

        var nowUtc = ReadUtcNow();
        session.Stop(nowUtc);
        _lastMonotonicElapsed = null;
        Publish(new FocusSessionStopped(session.Id, nowUtc, session.CountedDuration));
        return session;
    }

    public FocusSession Stop(bool observedIdle)
    {
        var session = RequireActiveSession();
        if (session.Status == FocusSessionStatus.Running)
        {
            var tick = AdvanceRunningSession(session, observedIdle);
            if (tick.Completed)
            {
                return session;
            }
        }

        var nowUtc = ReadUtcNow();
        session.Stop(nowUtc);
        _lastMonotonicElapsed = null;
        Publish(new FocusSessionStopped(session.Id, nowUtc, session.CountedDuration));
        return session;
    }

    private FocusSessionTickResult AdvanceRunningSession(FocusSession session, bool observedIdle)
    {
        if (_lastMonotonicElapsed is null)
        {
            throw new FocusSessionManagerException("The running session has no monotonic baseline.");
        }

        var elapsed = _monotonicClock.Elapsed - _lastMonotonicElapsed.Value;
        if (elapsed < TimeSpan.Zero)
        {
            throw new FocusSessionManagerException("The monotonic clock moved backwards.");
        }

        return AdvanceRunningSession(session, observedIdle ? elapsed : TimeSpan.Zero);
    }

    private FocusSessionTickResult AdvanceRunningSession(
        FocusSession session,
        TimeSpan idleSinceLastTick)
    {
        if (_lastMonotonicElapsed is null)
        {
            throw new FocusSessionManagerException("The running session has no monotonic baseline.");
        }

        var currentMonotonicElapsed = _monotonicClock.Elapsed;
        var elapsed = currentMonotonicElapsed - _lastMonotonicElapsed.Value;
        if (elapsed < TimeSpan.Zero)
        {
            throw new FocusSessionManagerException("The monotonic clock moved backwards.");
        }

        var beforeCounted = session.CountedDuration;
        var beforeIdle = session.IdleDuration;
        session.Accumulate(elapsed, idleSinceLastTick);
        _lastMonotonicElapsed = currentMonotonicElapsed;

        var appliedElapsed = session.CountedDuration - beforeCounted;
        var appliedIdle = session.IdleDuration - beforeIdle;
        if (!session.IsTargetReached)
        {
            return new FocusSessionTickResult(appliedElapsed, appliedIdle, false);
        }

        var nowUtc = ReadUtcNow();
        session.Complete(nowUtc);
        _lastMonotonicElapsed = null;
        Publish(new FocusSessionCompleted(session.Id, nowUtc, session.CountedDuration));
        return new FocusSessionTickResult(appliedElapsed, appliedIdle, true);
    }

    private FocusSession RequireActiveSession()
    {
        return ActiveSession
            ?? throw new FocusSessionManagerException("There is no active focus session.");
    }

    private DateTimeOffset ReadUtcNow()
    {
        var nowUtc = _wallClock.UtcNow;
        if (nowUtc.Offset != TimeSpan.Zero)
        {
            throw new FocusSessionManagerException("The wall clock must return a UTC timestamp.");
        }

        return nowUtc;
    }

    private void Publish(FocusSessionLifecycleEvent lifecycleEvent)
    {
        LifecycleChanged?.Invoke(lifecycleEvent);
    }
}
