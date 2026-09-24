using DeskTown.Application.Configuration;

namespace DeskTown.Foundation.Tests;

public sealed class PrototypeOptionsTests
{
    [Fact]
    public void Default_options_match_the_v01_contract()
    {
        var options = PrototypeOptions.Default;

        Assert.True(options.Validate().IsValid);
        Assert.Equal(TimeSpan.FromSeconds(60), options.ShortIdleThreshold);
        Assert.Equal(TimeSpan.FromSeconds(180), options.LongIdleThreshold);
        Assert.Equal(TimeSpan.FromSeconds(1), options.LogicalTickInterval);
        Assert.Equal(TimeSpan.FromSeconds(15), options.CheckpointInterval);
        Assert.Equal(30, options.VisibleMaxFramesPerSecond);
    }

    [Fact]
    public void Long_idle_must_be_greater_than_short_idle()
    {
        var options = PrototypeOptions.Default with
        {
            ShortIdleThreshold = TimeSpan.FromSeconds(180),
            LongIdleThreshold = TimeSpan.FromSeconds(60)
        };

        var result = options.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("Long idle", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(31)]
    public void Visible_fps_must_stay_inside_prototype_budget(int framesPerSecond)
    {
        var options = PrototypeOptions.Default with
        {
            VisibleMaxFramesPerSecond = framesPerSecond
        };

        Assert.False(options.Validate().IsValid);
    }
}
