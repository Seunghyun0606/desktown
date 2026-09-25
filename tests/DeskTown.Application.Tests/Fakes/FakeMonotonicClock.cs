using DeskTown.Application.Ports;

namespace DeskTown.Application.Tests.Fakes;

internal sealed class FakeMonotonicClock : IMonotonicClock
{
    public TimeSpan Elapsed { get; private set; }

    public void Advance(TimeSpan duration)
    {
        Elapsed += duration;
    }

    public void Set(TimeSpan value)
    {
        Elapsed = value;
    }
}
