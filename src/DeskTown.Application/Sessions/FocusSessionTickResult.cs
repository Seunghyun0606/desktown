namespace DeskTown.Application.Sessions;

public sealed record FocusSessionTickResult(
    TimeSpan AppliedElapsed,
    TimeSpan AppliedIdle,
    bool Completed)
{
    public static FocusSessionTickResult Suspended { get; } =
        new(TimeSpan.Zero, TimeSpan.Zero, false);
}
