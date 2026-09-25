namespace DeskTown.Domain.Simulation;

/// <summary>Configuration supplied by the outer application layer.</summary>
public sealed record MinaThresholds(TimeSpan ShortIdle, TimeSpan LongIdle)
{
    public static MinaThresholds Default { get; } = new(
        TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(180));

    public void Validate()
    {
        if (ShortIdle <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ShortIdle), "Short idle must be positive.");
        }

        if (LongIdle <= ShortIdle)
        {
            throw new ArgumentOutOfRangeException(
                nameof(LongIdle), "Long idle must exceed short idle.");
        }
    }
}
