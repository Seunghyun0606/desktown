namespace DeskTown.Application.Ports;

public interface IWallClock
{
    DateTimeOffset UtcNow { get; }
}
