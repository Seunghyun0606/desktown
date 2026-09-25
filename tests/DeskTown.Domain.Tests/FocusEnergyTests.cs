using DeskTown.Domain.Focus;

namespace DeskTown.Domain.Tests;

public sealed class FocusEnergyTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(59, 0)]
    [InlineData(60, 1)]
    [InlineData(61, 1)]
    [InlineData(119, 1)]
    [InlineData(120, 2)]
    public void Displayed_focus_is_the_whole_completed_minute_count(
        long seconds,
        long expectedMinutes)
    {
        var energy = FocusEnergy.FromCountedSeconds(seconds);

        Assert.Equal(seconds, energy.CountedSeconds);
        Assert.Equal(expectedMinutes, energy.DisplayedWholeMinutes);
    }

    [Fact]
    public void Negative_seconds_are_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FocusEnergy.FromCountedSeconds(-1));
    }

    [Fact]
    public void Addition_preserves_raw_seconds()
    {
        var energy = FocusEnergy.FromCountedSeconds(30)
            + FocusEnergy.FromCountedSeconds(30);

        Assert.Equal(60, energy.CountedSeconds);
        Assert.Equal(1, energy.DisplayedWholeMinutes);
    }

    [Fact]
    public void Addition_preserves_subsecond_precision_before_presentation_rounding()
    {
        var energy = FocusEnergy.FromCountedDuration(TimeSpan.FromMilliseconds(30_500))
            + FocusEnergy.FromCountedDuration(TimeSpan.FromMilliseconds(30_500));

        Assert.Equal(TimeSpan.FromSeconds(61), energy.CountedDuration);
        Assert.Equal(61, energy.CountedSeconds);
        Assert.Equal(1, energy.DisplayedWholeMinutes);
    }

    [Fact]
    public void Delta_since_returns_only_unapplied_energy()
    {
        var total = FocusEnergy.FromCountedDuration(TimeSpan.FromSeconds(90));
        var applied = FocusEnergy.FromCountedDuration(TimeSpan.FromSeconds(30));

        Assert.Equal(
            TimeSpan.FromSeconds(60),
            total.DeltaSince(applied).CountedDuration);
    }

    [Fact]
    public void Delta_since_rejects_an_applied_total_above_current_total()
    {
        var total = FocusEnergy.FromCountedSeconds(30);
        var applied = FocusEnergy.FromCountedSeconds(31);

        Assert.Throws<ArgumentOutOfRangeException>(() => total.DeltaSince(applied));
    }

    [Fact]
    public void Addition_rejects_overflow()
    {
        Assert.Throws<OverflowException>(() =>
            FocusEnergy.FromCountedDuration(TimeSpan.MaxValue)
            + FocusEnergy.FromCountedDuration(TimeSpan.FromTicks(1)));
    }
}
