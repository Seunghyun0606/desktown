using DeskTown.Application.Display;
using DeskTown.Application.Persistence;

namespace DeskTown.Application.Tests;

public sealed class CompanionPlacementTests
{
    [Theory]
    [InlineData(75, 270, 150)]
    [InlineData(100, 360, 200)]
    [InlineData(125, 450, 250)]
    [InlineData(150, 540, 300)]
    public void Supports_only_four_window_sizes(int scale, int width, int height) =>
        Assert.Equal((width, height), CompanionPlacement.SizeForScale(scale));

    [Fact]
    public void Negative_desktop_coordinates_round_trip_after_drag()
    {
        var monitors = new[] { new MonitorWorkArea("main", 0, 0, 1920, 1040),
            new MonitorWorkArea("left", -1600, 0, 1600, 860) };
        var captured = CompanionPlacement.Capture(-1250, 430, monitors[1]);
        var restored = CompanionPlacement.Restore(captured, 100, monitors);
        Assert.Equal(new WindowBounds(-1250, 430, 360, 200, "left"), restored);
    }

    [Fact]
    public void Missing_monitor_falls_back_and_clamps_all_edges()
    {
        var monitors = new[] { new MonitorWorkArea("main", 30, 20, 640, 400) };
        var restored = CompanionPlacement.Restore(
            new WindowPlacementSnapshot("removed", "Free", -900, 999), 150, monitors);
        Assert.Equal(new WindowBounds(30, 120, 540, 300, "main"), restored);
    }
}
