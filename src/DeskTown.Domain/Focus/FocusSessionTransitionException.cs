namespace DeskTown.Domain.Focus;

public sealed class FocusSessionTransitionException : InvalidOperationException
{
    public FocusSessionTransitionException(
        FocusSessionStatus currentStatus,
        string operation,
        string? reason = null)
        : base(BuildMessage(currentStatus, operation, reason))
    {
        CurrentStatus = currentStatus;
        Operation = operation;
    }

    public FocusSessionStatus CurrentStatus { get; }

    public string Operation { get; }

    private static string BuildMessage(
        FocusSessionStatus currentStatus,
        string operation,
        string? reason)
    {
        var message = $"Cannot {operation} a focus session while it is {currentStatus}.";
        return string.IsNullOrWhiteSpace(reason) ? message : $"{message} {reason}";
    }
}
