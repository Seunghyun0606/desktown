using System.Collections.Frozen;

namespace DeskTown.Domain.Focus;

public sealed class FocusSession
{
    private FocusSession(
        FocusSessionId id,
        TimeSpan targetDuration,
        FrozenSet<string> intendedProcessNames)
    {
        Id = id;
        TargetDuration = targetDuration;
        IntendedProcessNames = intendedProcessNames;
        Status = FocusSessionStatus.Ready;
    }

    public FocusSessionId Id { get; }

    public DateTimeOffset? StartedAtUtc { get; private set; }

    public DateTimeOffset? EndedAtUtc { get; private set; }

    public TimeSpan TargetDuration { get; }

    public FocusSessionStatus Status { get; private set; }

    public TimeSpan CountedDuration { get; private set; }

    public TimeSpan IdleDuration { get; private set; }

    public IReadOnlySet<string> IntendedProcessNames { get; }

    public TimeSpan RemainingDuration => TargetDuration - CountedDuration;

    public bool IsTargetReached => CountedDuration >= TargetDuration;

    public bool IsTerminal => Status is FocusSessionStatus.Completed or FocusSessionStatus.Stopped;

    public static FocusSession Create(
        FocusSessionId id,
        TimeSpan targetDuration,
        IEnumerable<string>? intendedProcessNames = null)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("A focus session ID cannot be empty.", nameof(id));
        }

        if (targetDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetDuration),
                targetDuration,
                "Target duration must be positive.");
        }

        return new FocusSession(id, targetDuration, NormalizeProcessNames(intendedProcessNames));
    }

    public void Start(DateTimeOffset startedAtUtc)
    {
        EnsureStatus(FocusSessionStatus.Ready, "start");
        EnsureUtc(startedAtUtc, nameof(startedAtUtc));

        StartedAtUtc = startedAtUtc;
        Status = FocusSessionStatus.Running;
    }

    public void Accumulate(TimeSpan elapsed, TimeSpan idle)
    {
        EnsureStatus(FocusSessionStatus.Running, "accumulate time for");

        if (elapsed < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsed), elapsed, "Elapsed time cannot be negative.");
        }

        if (idle < TimeSpan.Zero || idle > elapsed)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idle),
                idle,
                "Idle time must be between zero and the elapsed time.");
        }

        var appliedElapsed = elapsed <= RemainingDuration ? elapsed : RemainingDuration;
        var appliedIdle = idle <= appliedElapsed ? idle : appliedElapsed;

        CountedDuration += appliedElapsed;
        IdleDuration += appliedIdle;
    }

    public void Suspend()
    {
        EnsureStatus(FocusSessionStatus.Running, "suspend");
        Status = FocusSessionStatus.Suspended;
    }

    public void Resume()
    {
        EnsureStatus(FocusSessionStatus.Suspended, "resume");
        Status = FocusSessionStatus.Running;
    }

    public void Complete(DateTimeOffset endedAtUtc)
    {
        EnsureStatus(FocusSessionStatus.Running, "complete");

        if (!IsTargetReached)
        {
            throw new FocusSessionTransitionException(
                Status,
                "complete",
                "The target duration has not been reached.");
        }

        End(endedAtUtc, FocusSessionStatus.Completed);
    }

    public void Stop(DateTimeOffset endedAtUtc)
    {
        if (Status is not (FocusSessionStatus.Running or FocusSessionStatus.Suspended))
        {
            throw new FocusSessionTransitionException(Status, "stop");
        }

        End(endedAtUtc, FocusSessionStatus.Stopped);
    }

    public TimeSpan CalculateWallClockElapsed(DateTimeOffset asOfUtc)
    {
        EnsureUtc(asOfUtc, nameof(asOfUtc));

        if (StartedAtUtc is null)
        {
            return TimeSpan.Zero;
        }

        var end = EndedAtUtc ?? asOfUtc;
        if (end < StartedAtUtc.Value)
        {
            throw new ArgumentOutOfRangeException(
                nameof(asOfUtc),
                asOfUtc,
                "The elapsed-time boundary cannot precede the session start.");
        }

        return end - StartedAtUtc.Value;
    }

    private void End(DateTimeOffset endedAtUtc, FocusSessionStatus finalStatus)
    {
        EnsureUtc(endedAtUtc, nameof(endedAtUtc));

        if (StartedAtUtc is null || endedAtUtc < StartedAtUtc.Value)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endedAtUtc),
                endedAtUtc,
                "Session end cannot precede session start.");
        }

        EndedAtUtc = endedAtUtc;
        Status = finalStatus;
    }

    private void EnsureStatus(FocusSessionStatus expected, string operation)
    {
        if (Status != expected)
        {
            throw new FocusSessionTransitionException(Status, operation);
        }
    }

    private static void EnsureUtc(DateTimeOffset value, string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Timestamp must use the UTC offset.", parameterName);
        }
    }

    private static FrozenSet<string> NormalizeProcessNames(IEnumerable<string>? processNames)
    {
        return (processNames ?? Array.Empty<string>())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }
}
