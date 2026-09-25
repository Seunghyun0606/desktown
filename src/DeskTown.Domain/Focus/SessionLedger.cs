namespace DeskTown.Domain.Focus;

/// <summary>
/// An append-only, idempotent record of finalized focus sessions.
/// </summary>
public sealed class SessionLedger
{
    private readonly Dictionary<FocusSessionId, SessionLedgerEntry> _entries = [];
    private long _totalCountedTicks;
    private long _totalIdleTicks;

    public IReadOnlyCollection<SessionLedgerEntry> Entries => _entries.Values;

    public TimeSpan TotalCountedDuration => TimeSpan.FromTicks(_totalCountedTicks);

    public TimeSpan TotalIdleDuration => TimeSpan.FromTicks(_totalIdleTicks);

    public long TotalCountedSeconds => _totalCountedTicks / TimeSpan.TicksPerSecond;

    public long TotalIdleSeconds => _totalIdleTicks / TimeSpan.TicksPerSecond;

    /// <summary>
    /// Records a terminal session. Returns false when the exact same session
    /// snapshot was already applied. Conflicting reuse of an ID is rejected.
    /// </summary>
    public bool TryRecord(FocusSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!session.IsTerminal)
        {
            throw new ArgumentException(
                "Only completed or stopped sessions can be recorded.",
                nameof(session));
        }

        var candidate = new SessionLedgerEntry(session);

        if (_entries.TryGetValue(candidate.SessionId, out var existing))
        {
            if (!existing.Matches(candidate))
            {
                throw new SessionLedgerConflictException(candidate.SessionId);
            }

            return false;
        }

        var countedTicks = checked(_totalCountedTicks + candidate.CountedDuration.Ticks);
        var idleTicks = checked(_totalIdleTicks + candidate.IdleDuration.Ticks);

        _entries.Add(candidate.SessionId, candidate);
        _totalCountedTicks = countedTicks;
        _totalIdleTicks = idleTicks;

        return true;
    }
}
