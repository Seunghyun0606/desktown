using DeskTown.Application.Persistence;
using DeskTown.Application.Ports;
using DeskTown.Application.Sessions;
using DeskTown.Domain.Focus;

namespace DeskTown.Application.Lifecycle;

/// <summary>
/// Loads a durable snapshot, exposes an explicit startup decision, and schedules
/// full-aggregate saves. The host captures a consistent GameStateSnapshot at
/// every call; no UI, renderer or Windows API enters this layer.
/// </summary>
public sealed class ApplicationLifecycleCoordinator
{
    private readonly IGameStateStore _store;
    private readonly IWallClock _wallClock;
    private readonly IMonotonicClock _monotonicClock;
    private readonly CheckpointScheduler _scheduler;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _initialized;
    private long _revision;

    public ApplicationLifecycleCoordinator(IGameStateStore store, IWallClock wallClock,
        IMonotonicClock monotonicClock, TimeSpan checkpointInterval)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _wallClock = wallClock ?? throw new ArgumentNullException(nameof(wallClock));
        _monotonicClock = monotonicClock ?? throw new ArgumentNullException(nameof(monotonicClock));
        _scheduler = new CheckpointScheduler(checkpointInterval);
    }

    public StoredGameState? LoadedState { get; private set; }
    public FocusSessionCheckpoint? PendingRecovery { get; private set; }
    public long Revision => _revision;

    public async Task<StoredGameState?> InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized) throw new InvalidOperationException("Lifecycle already initialized.");
        LoadedState = await _store.LoadAsync(cancellationToken);
        _revision = LoadedState?.Revision ?? 0;
        PendingRecovery = LoadedState?.Snapshot.Focus.ActiveCheckpoint;
        _initialized = true;
        return LoadedState;
    }

    /// <summary>
    /// Called only after a coordinator is constructed with the restored ledger.
    /// Both paths restore Suspended first and exclude all time since the checkpoint.
    /// </summary>
    public RecoveryResolution ResolveRecovery(
        RecoveryChoice choice, FocusSessionCoordinator coordinator)
    {
        EnsureInitialized();
        ArgumentNullException.ThrowIfNull(coordinator);
        var checkpoint = PendingRecovery
            ?? throw new InvalidOperationException("There is no Focus session to recover.");
        if (!Enum.IsDefined(choice)) throw new ArgumentOutOfRangeException(nameof(choice));

        coordinator.RestoreSuspended(checkpoint);
        RecoveryResolution result;
        if (choice == RecoveryChoice.Resume)
        {
            coordinator.Resume();
            result = new RecoveryResolution(FocusSessionStatus.Running, null);
        }
        else
        {
            result = new RecoveryResolution(FocusSessionStatus.Stopped, coordinator.Stop());
        }
        PendingRecovery = null;
        return result;
    }

    public async Task<bool> SaveAsync(GameStateSnapshot snapshot, CheckpointReason reason,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!Enum.IsDefined(reason)) throw new ArgumentOutOfRangeException(nameof(reason));
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var elapsed = _monotonicClock.Elapsed;
            var running = snapshot.Focus.ActiveCheckpoint?.Status == FocusSessionStatus.Running;
            if (!_scheduler.IsDue(reason, running, elapsed)) return false;

            var now = _wallClock.UtcNow;
            if (now.Offset != TimeSpan.Zero) throw new InvalidOperationException("UTC clock required.");
            var nextRevision = checked(_revision + 1);
            await _store.SaveAsync(snapshot, nextRevision, now, cancellationToken);
            _revision = nextRevision;
            _scheduler.MarkSaved(elapsed);
            return true;
        }
        finally { _gate.Release(); }
    }

    private void EnsureInitialized()
    {
        if (!_initialized) throw new InvalidOperationException("Load the game state first.");
    }
}
