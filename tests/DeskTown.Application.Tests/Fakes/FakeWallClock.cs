using DeskTown.Application.Ports;

namespace DeskTown.Application.Tests.Fakes;

internal sealed class FakeWallClock : IWallClock
{
    public FakeWallClock(DateTimeOffset utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTimeOffset UtcNow { get; private set; }

    public void Advance(TimeSpan duration)
    {
        UtcNow += duration;
    }

    public void Set(DateTimeOffset value)
    {
        UtcNow = value;
    }
}
