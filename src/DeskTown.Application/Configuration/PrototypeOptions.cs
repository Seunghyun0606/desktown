namespace DeskTown.Application.Configuration;

public sealed record PrototypeOptions(
    TimeSpan ShortIdleThreshold,
    TimeSpan LongIdleThreshold,
    TimeSpan LogicalTickInterval,
    TimeSpan CheckpointInterval,
    int VisibleMaxFramesPerSecond)
{
    public static PrototypeOptions Default { get; } = new(
        ShortIdleThreshold: TimeSpan.FromSeconds(60),
        LongIdleThreshold: TimeSpan.FromSeconds(180),
        LogicalTickInterval: TimeSpan.FromSeconds(1),
        CheckpointInterval: TimeSpan.FromSeconds(15),
        VisibleMaxFramesPerSecond: 30);

    public OptionsValidationResult Validate()
    {
        var errors = new List<string>();

        if (ShortIdleThreshold <= TimeSpan.Zero)
        {
            errors.Add("Short idle threshold must be positive.");
        }

        if (LongIdleThreshold <= ShortIdleThreshold)
        {
            errors.Add("Long idle threshold must be greater than short idle threshold.");
        }

        if (LogicalTickInterval <= TimeSpan.Zero)
        {
            errors.Add("Logical tick interval must be positive.");
        }

        if (CheckpointInterval < LogicalTickInterval)
        {
            errors.Add("Checkpoint interval cannot be shorter than the logical tick interval.");
        }

        if (VisibleMaxFramesPerSecond is < 1 or > 30)
        {
            errors.Add("Visible FPS must be between 1 and the prototype maximum of 30.");
        }

        return new OptionsValidationResult(errors);
    }
}

public sealed record OptionsValidationResult(IReadOnlyList<string> Errors)
{
    public bool IsValid => Errors.Count == 0;
}
