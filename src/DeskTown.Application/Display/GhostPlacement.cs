using DeskTown.Application.Persistence;

namespace DeskTown.Application.Display;

/// <summary>Screen-coordinate placement for the noninteractive Ghost surface.</summary>
public static class GhostPlacement
{
    public static WindowBounds Restore(WindowPlacementSnapshot placement,
        int width, int height, IReadOnlyList<MonitorWorkArea> monitors)
    {
        ArgumentNullException.ThrowIfNull(placement);
        ArgumentNullException.ThrowIfNull(monitors);
        if (width <= 0 || height <= 0 || monitors.Count == 0)
            throw new ArgumentException("Ghost size and monitor list must be nonempty.");

        var monitor = monitors.FirstOrDefault(m => m.Id == placement.MonitorId) ?? monitors[0];
        if (monitor.Width <= 0 || monitor.Height <= 0)
            throw new ArgumentException("Invalid monitor work area.", nameof(monitors));

        var x = placement.Anchor switch
        {
            "TopLeft" or "BottomLeft" => monitor.X + placement.OffsetX,
            "TopRight" or "BottomRight" => monitor.X + monitor.Width - width - placement.OffsetX,
            _ => throw new ArgumentException("Ghost requires a corner anchor.", nameof(placement))
        };
        var y = placement.Anchor switch
        {
            "TopLeft" or "TopRight" => monitor.Y + placement.OffsetY,
            _ => monitor.Y + monitor.Height - height - placement.OffsetY
        };
        return new WindowBounds(
            Math.Clamp(x, monitor.X, monitor.X + Math.Max(0, monitor.Width - width)),
            Math.Clamp(y, monitor.Y, monitor.Y + Math.Max(0, monitor.Height - height)),
            width, height, monitor.Id);
    }
}
