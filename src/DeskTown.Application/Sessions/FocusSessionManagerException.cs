namespace DeskTown.Application.Sessions;

public sealed class FocusSessionManagerException : InvalidOperationException
{
    public FocusSessionManagerException(string message)
        : base(message)
    {
    }
}
