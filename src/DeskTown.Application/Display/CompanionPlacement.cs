using DeskTown.Application.Persistence;

namespace DeskTown.Application.Display;

public sealed record MonitorWorkArea(string Id, int X, int Y, int Width, int Height);
public sealed record WindowBounds(int X, int Y, int Width, int Height, string MonitorId);

/// <summary>Screen-coordinate math independent of windowing APIs.</summary>
public static class CompanionPlacement
{
    public static (int Width, int Height) SizeForScale(int percent) => percent switch
    {
        75 => (270, 150),
        100 => (360, 200),
        125 => (450, 250),
        150 => (540, 300),
        _ => throw new ArgumentOutOfRangeException(nameof(percent))
    };

    public static WindowBounds Restore(WindowPlacementSnapshot placement,
        int percent, IReadOnlyList<MonitorWorkArea> monitors)
    {
        ArgumentNullException.ThrowIfNull(placement);
        if (monitors.Count == 0) throw new ArgumentException("At least one monitor required.", nameof(monitors));
        var monitor = monitors.FirstOrDefault(m => m.Id == placement.MonitorId) ?? monitors[0];
        if (monitor.Width <= 0 || monitor.Height <= 0)
            throw new ArgumentException("Invalid monitor work area.", nameof(monitors));
        var (width, height) = SizeForScale(percent);
        var (x, y) = placement.Anchor switch
        {
            "TopLeft" or "Free" => (monitor.X + placement.OffsetX, monitor.Y + placement.OffsetY),
            "TopRight" => (monitor.X + monitor.Width - width - placement.OffsetX,
                monitor.Y + placement.OffsetY),
            "BottomLeft" => (monitor.X + placement.OffsetX,
                monitor.Y + monitor.Height - height - placement.OffsetY),
            "BottomRight" => (monitor.X + monitor.Width - width - placement.OffsetX,
                monitor.Y + monitor.Height - height - placement.OffsetY),
            _ => throw new ArgumentException("Unknown placement anchor.", nameof(placement))
        };
        return new WindowBounds(
            Math.Clamp(x, monitor.X, monitor.X + Math.Max(0, monitor.Width - width)),
            Math.Clamp(y, monitor.Y, monitor.Y + Math.Max(0, monitor.Height - height)),
            width, height, monitor.Id);
    }

    public static WindowPlacementSnapshot Capture(int x, int y, MonitorWorkArea monitor) =>
        new(monitor.Id, "Free", x - monitor.X, y - monitor.Y);
}
