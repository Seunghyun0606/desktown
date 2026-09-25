namespace DeskTown.Application.Ports;

public interface IMonotonicClock
{
    TimeSpan Elapsed { get; }
}
