namespace DeskTown.Application.Activity;

/// <summary>
/// One already-aggregated activity interval. Raw native identifiers and input
/// details are deliberately absent from this boundary type.
/// </summary>
public sealed record ActivitySample
{
    public const string UnknownProcessName = "Unknown";

    public ActivitySample(
        string processName,
        TimeSpan activeDuration,
        TimeSpan idleDuration)
    {
        if (activeDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(activeDuration));
        }

        if (idleDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(idleDuration));
        }

        if (activeDuration > TimeSpan.Zero && idleDuration > TimeSpan.Zero)
        {
            throw new ArgumentException(
                "An activity sample cannot be active and idle at the same time.");
        }

        ProcessName = ProcessNamePolicy.Normalize(processName);
        ActiveDuration = activeDuration;
        IdleDuration = idleDuration;
    }

    public string ProcessName { get; }

    public TimeSpan ActiveDuration { get; }

    public TimeSpan IdleDuration { get; }

    public static ActivitySample Active(string processName, TimeSpan duration) =>
        new(processName, duration, TimeSpan.Zero);

    public static ActivitySample Idle(string processName, TimeSpan duration) =>
        new(processName, TimeSpan.Zero, duration);

    public static ActivitySample Unknown { get; } =
        new(UnknownProcessName, TimeSpan.Zero, TimeSpan.Zero);

}
