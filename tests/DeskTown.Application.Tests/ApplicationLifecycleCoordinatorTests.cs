using DeskTown.Application.Lifecycle;
using DeskTown.Application.Persistence;
using DeskTown.Application.Ports;
using DeskTown.Application.Sessions;
using DeskTown.Application.Tests.Fakes;
using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;

namespace DeskTown.Application.Tests;

public sealed class ApplicationLifecycleCoordinatorTests
{
    private static readonly DateTimeOffset Started = new(2026, 9, 25, 8, 0, 0, TimeSpan.Zero);
    private static readonly FocusSessionId Id = FocusSessionId.From(
        Guid.Parse("41af0d0f-5d68-43b4-aa1e-5c87342f95c1"));

    [Fact]
    public async Task Periodic_checkpoint_uses_monotonic_15_second_cadence_and_event_saves_immediately()
    {
        var store = new FakeStore();
        var wall = new FakeWallClock(Started);
        var mono = new FakeMonotonicClock();
        var lifecycle = new ApplicationLifecycleCoordinator(store, wall, mono,
            TimeSpan.FromSeconds(15));
        await lifecycle.InitializeAsync();
        var snapshot = Snapshot(new SessionLedger(), RunningCheckpoint());

        Assert.True(await lifecycle.SaveAsync(snapshot, CheckpointReason.FocusStarted));
        mono.Advance(TimeSpan.FromSeconds(14));
        Assert.False(await lifecycle.SaveAsync(snapshot, CheckpointReason.Periodic));
        mono.Advance(TimeSpan.FromSeconds(1));
        Assert.True(await lifecycle.SaveAsync(snapshot, CheckpointReason.Periodic));
        Assert.True(await lifecycle.SaveAsync(snapshot, CheckpointReason.DisplayChanged));
        Assert.True(await lifecycle.SaveAsync(snapshot, CheckpointReason.Suspended));
        Assert.Equal(new long[] { 1, 2, 3, 4 }, store.Revisions);
    }

    [Fact]
    public async Task Failed_checkpoint_does_not_advance_revision_or_interval()
    {
        var store = new FakeStore { FailNextSave = true };
        var mono = new FakeMonotonicClock();
        var lifecycle = new ApplicationLifecycleCoordinator(store, new FakeWallClock(Started),
            mono, TimeSpan.FromSeconds(15));
        await lifecycle.InitializeAsync();
        var snapshot = Snapshot(new SessionLedger(), RunningCheckpoint());

        await Assert.ThrowsAsync<IOException>(() =>
            lifecycle.SaveAsync(snapshot, CheckpointReason.FocusStarted));
        Assert.Equal(0, lifecycle.Revision);
        Assert.True(await lifecycle.SaveAsync(snapshot, CheckpointReason.Periodic));
        Assert.Equal(1, lifecycle.Revision);
        Assert.Equal(new long[] { 1 }, store.Revisions);
    }

    [Fact]
    public async Task Resume_after_two_hours_counts_only_new_monotonic_seconds()
    {
        var ledger = new SessionLedger();
        var checkpoint = RunningCheckpoint();
        var store = new FakeStore
        {
            Loaded = new StoredGameState(7, Started.AddMinutes(5), Snapshot(ledger, checkpoint))
        };
        var wall = new FakeWallClock(Started.AddHours(2));
        var mono = new FakeMonotonicClock();
        var lifecycle = new ApplicationLifecycleCoordinator(store, wall, mono,
            TimeSpan.FromSeconds(15));
        await lifecycle.InitializeAsync();
        var coordinator = Coordinator(wall, mono, ledger);

        var resolution = lifecycle.ResolveRecovery(RecoveryChoice.Resume, coordinator);
        Assert.Equal(FocusSessionStatus.Running, resolution.Status);
        Assert.Null(lifecycle.PendingRecovery);
        Assert.Equal(TimeSpan.FromMinutes(5), coordinator.ActiveSession!.CountedDuration);
        mono.Advance(TimeSpan.FromSeconds(5));
        wall.Advance(TimeSpan.FromSeconds(5));
        coordinator.Tick();
        Assert.Equal(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(5),
            coordinator.ActiveSession!.CountedDuration);
        Assert.Equal(TimeSpan.FromMinutes(1), coordinator.ActiveSession.IdleDuration);
    }

    [Fact]
    public async Task End_at_checkpoint_records_exactly_once_without_offline_wall_time()
    {
        var ledger = new SessionLedger();
        var store = new FakeStore
        {
            Loaded = new StoredGameState(4, Started.AddMinutes(5),
                Snapshot(ledger, RunningCheckpoint()))
        };
        var wall = new FakeWallClock(Started.AddHours(3));
        var lifecycle = new ApplicationLifecycleCoordinator(store, wall,
            new FakeMonotonicClock(), TimeSpan.FromSeconds(15));
        await lifecycle.InitializeAsync();
        var coordinator = Coordinator(wall, new FakeMonotonicClock(), ledger);

        var result = lifecycle.ResolveRecovery(RecoveryChoice.EndAtCheckpoint, coordinator);
        Assert.Equal(FocusSessionStatus.Stopped, result.Status);
        Assert.True(result.Finalized!.NewlyRecorded);
        Assert.Equal(TimeSpan.FromMinutes(5), result.Finalized.TotalEnergy.CountedDuration);
        Assert.Equal(TimeSpan.FromMinutes(5), ledger.TotalCountedDuration);
        Assert.Throws<InvalidOperationException>(() =>
            lifecycle.ResolveRecovery(RecoveryChoice.EndAtCheckpoint, coordinator));
    }

    private static FocusSessionCoordinator Coordinator(
        FakeWallClock wall, FakeMonotonicClock mono, SessionLedger ledger) =>
        new(new FocusSessionManager(wall, mono), new FakeActivityTracker(), ledger,
            new ElapsedTimeEnergyPolicy());

    private static FocusSessionCheckpoint RunningCheckpoint() => new(
        Id, FocusSessionStatus.Running, Started, null,
        TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1),
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "code.exe" },
        new FocusActivitySummary(TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(1),
            TimeSpan.Zero, [new ProcessActivityDuration("code.exe", TimeSpan.FromMinutes(5))]),
        FocusEnergy.Zero);

    private static GameStateSnapshot Snapshot(SessionLedger ledger,
        FocusSessionCheckpoint? checkpoint) => new(
        new SettingsSnapshot("Hidden",
            new WindowPlacementSnapshot("", "BottomRight", 0, 0),
            new WindowPlacementSnapshot("", "BottomRight", 0, 0),
            100, 65, true, true, false),
        new FocusSnapshot(ledger, checkpoint, []),
        new TownSnapshot(ProjectSystem.CreateRestoreWorkshop(), ["RestoreWorkshop"], [], []),
        new MinaSnapshot("Idle", "Home", "RestoreWorkshop", false, false),
        new PlayerSnapshot(0, DateOnly.FromDateTime(Started.UtcDateTime), 0),
        new PendingPresentationSnapshot([], []));

    private sealed class FakeStore : IGameStateStore
    {
        public StoredGameState? Loaded { get; set; }
        public bool FailNextSave { get; set; }
        public List<long> Revisions { get; } = [];

        public Task<StoredGameState?> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Loaded);

        public Task SaveAsync(GameStateSnapshot snapshot, long revision,
            DateTimeOffset savedAtUtc, CancellationToken cancellationToken = default)
        {
            if (FailNextSave)
            {
                FailNextSave = false;
                throw new IOException("Injected write failure.");
            }
            Revisions.Add(revision);
            Loaded = new StoredGameState(revision, savedAtUtc, snapshot);
            return Task.CompletedTask;
        }
    }
}
