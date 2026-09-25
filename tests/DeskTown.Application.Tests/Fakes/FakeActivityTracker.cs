using DeskTown.Application.Activity;
using DeskTown.Application.Ports;

namespace DeskTown.Application.Tests.Fakes;

internal sealed class FakeActivityTracker : IActivityTracker
{
    private readonly Queue<ActivitySample> _samples = new();

    public TimeSpan SampleInterval { get; init; } = TimeSpan.FromSeconds(1);

    public Exception? Exception { get; set; }

    public int SampleCount { get; private set; }

    public void Enqueue(ActivitySample sample)
    {
        _samples.Enqueue(sample);
    }

    public ActivitySample Sample()
    {
        SampleCount++;
        if (Exception is not null)
        {
            throw Exception;
        }

        return _samples.Count > 0 ? _samples.Dequeue() : ActivitySample.Unknown;
    }
}
