using System.Diagnostics;
using DeskTown.Application.Ports;

namespace DeskTown.Presentation.Bootstrap;

public sealed class SystemWallClock : IWallClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class StopwatchMonotonicClock : IMonotonicClock
{
    private readonly Stopwatch _watch = Stopwatch.StartNew();
    public TimeSpan Elapsed => _watch.Elapsed;
}
