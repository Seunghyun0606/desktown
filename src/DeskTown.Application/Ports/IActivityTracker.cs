using DeskTown.Application.Activity;

namespace DeskTown.Application.Ports;

/// <summary>
/// Produces minimal, aggregated activity observations. The application
/// coordinator owns scheduling and invokes this only while a FocusSession is
/// active.
/// </summary>
public interface IActivityTracker
{
    TimeSpan SampleInterval { get; }

    ActivitySample Sample();
}
