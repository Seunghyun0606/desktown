using DeskTown.Application.Display;
using DeskTown.Application.Persistence;

namespace DeskTown.Application.Tests;

public sealed class GhostPlacementTests
{
    [Theory]
    [InlineData("TopLeft", -1576, 24)]
    [InlineData("TopRight", -264, 24)]
    [InlineData("BottomLeft", -1576, 656)]
    [InlineData("BottomRight", -264, 656)]
    public void Restores_all_corners_on_negative_coordinate_monitor(string anchor, int x, int y)
    {
        var monitors = new[] { new MonitorWorkArea("main", 0, 0, 1920, 1040),
            new MonitorWorkArea("left", -1600, 0, 1600, 860) };
        var bounds = GhostPlacement.Restore(
            new WindowPlacementSnapshot("left", anchor, 24, 24), 240, 180, monitors);
        Assert.Equal(new WindowBounds(x, y, 240, 180, "left"), bounds);
    }

    [Fact]
    public void Missing_monitor_falls_back_and_clamps_entire_overlay()
    {
        var bounds = GhostPlacement.Restore(
            new WindowPlacementSnapshot("removed", "BottomRight", -900, -900),
            240, 180, [new MonitorWorkArea("main", 30, 20, 400, 300)]);
        Assert.Equal(new WindowBounds(190, 140, 240, 180, "main"), bounds);
    }
}
