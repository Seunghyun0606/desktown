using System.Collections.Frozen;

namespace DeskTown.Domain.Focus;

public sealed class SessionLedgerEntry
{
    internal SessionLedgerEntry(FocusSession session)
    {
        SessionId = session.Id;
        Status = session.Status;
        StartedAtUtc = session.StartedAtUtc!.Value;
        EndedAtUtc = session.EndedAtUtc!.Value;
        TargetDuration = session.TargetDuration;
        CountedDuration = session.CountedDuration;
        IdleDuration = session.IdleDuration;
        IntendedProcessNames = session.IntendedProcessNames.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    public FocusSessionId SessionId { get; }

    public FocusSessionStatus Status { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public DateTimeOffset EndedAtUtc { get; }

    public TimeSpan TargetDuration { get; }

    public TimeSpan CountedDuration { get; }

    public TimeSpan IdleDuration { get; }

    public IReadOnlySet<string> IntendedProcessNames { get; }

    internal bool Matches(SessionLedgerEntry other)
    {
        return SessionId == other.SessionId
            && Status == other.Status
            && StartedAtUtc == other.StartedAtUtc
            && EndedAtUtc == other.EndedAtUtc
            && TargetDuration == other.TargetDuration
            && CountedDuration == other.CountedDuration
            && IdleDuration == other.IdleDuration
            && IntendedProcessNames.SetEquals(other.IntendedProcessNames);
    }
}
