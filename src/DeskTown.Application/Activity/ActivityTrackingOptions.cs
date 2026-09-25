using DeskTown.Application.Configuration;

namespace DeskTown.Application.Activity;

public sealed record ActivityTrackingOptions(
    TimeSpan SampleInterval,
    TimeSpan IdleThreshold)
{
    public static ActivityTrackingOptions Default { get; } =
        FromPrototypeOptions(PrototypeOptions.Default);

    public static ActivityTrackingOptions FromPrototypeOptions(PrototypeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new ActivityTrackingOptions(
            options.LogicalTickInterval,
            options.ShortIdleThreshold);
    }

    public void EnsureValid()
    {
        if (SampleInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(SampleInterval),
                "The activity sample interval must be positive.");
        }

        if (IdleThreshold <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(IdleThreshold),
                "The idle threshold must be positive.");
        }
    }
}
