namespace DeskTown.Application.Sessions;

/// <summary>
/// Privacy-minimal activity totals observed during one Focus session.
/// The monotonic session clock, not these descriptive totals, owns progression.
/// </summary>
public sealed record FocusActivitySummary(
    TimeSpan ActiveDuration,
    TimeSpan IdleDuration,
    TimeSpan UnknownDuration,
    IReadOnlyList<ProcessActivityDuration> Processes)
{
    public static FocusActivitySummary Empty { get; } =
        new(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, Array.Empty<ProcessActivityDuration>());
}
