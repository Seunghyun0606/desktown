namespace DeskTown.Application.Lifecycle;

/// <summary>Monotonic 15-second cadence; meaningful transitions always save.</summary>
public sealed class CheckpointScheduler
{
    private readonly TimeSpan _interval;
    private TimeSpan? _lastSavedAt;

    public CheckpointScheduler(TimeSpan interval)
    {
        if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval));
        _interval = interval;
    }

    public bool IsDue(CheckpointReason reason, bool running, TimeSpan monotonicElapsed)
    {
        if (monotonicElapsed < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(monotonicElapsed));
        if (reason != CheckpointReason.Periodic) return true;
        return running && (_lastSavedAt is null
            || monotonicElapsed < _lastSavedAt.Value
            || monotonicElapsed - _lastSavedAt.Value >= _interval);
    }

    public void MarkSaved(TimeSpan monotonicElapsed) => _lastSavedAt = monotonicElapsed;
}
