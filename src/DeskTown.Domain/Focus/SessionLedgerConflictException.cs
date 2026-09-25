namespace DeskTown.Domain.Focus;

public sealed class SessionLedgerConflictException : InvalidOperationException
{
    public SessionLedgerConflictException(FocusSessionId sessionId)
        : base($"Session '{sessionId}' was already recorded with different data.")
    {
        SessionId = sessionId;
    }

    public FocusSessionId SessionId { get; }
}
