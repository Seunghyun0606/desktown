using DeskTown.Application.Display;
using DeskTown.Application.Lifecycle;
using DeskTown.Application.Persistence;
using DeskTown.Application.Ports;
using DeskTown.Application.Sessions;
using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;
using DeskTown.Domain.Simulation;

namespace DeskTown.Application.Runtime;

/// <summary>One application composition seam. Screens read snapshots and send commands.</summary>
public sealed class PrototypeRuntime
{
    private readonly IWallClock _wallClock;
    private readonly ApplicationLifecycleCoordinator _lifecycle;
    private readonly FocusSessionCoordinator _focus;
    private readonly SessionLedger _ledger;
    private readonly TownSimulation _town;
    private readonly List<DisplayModeChangeSnapshot> _modeChanges;
    private FocusSessionCheckpoint? _lastFocusCheckpoint;
    private TimeSpan _observedSessionDuration;
    private SettingsSnapshot _settings;

    private PrototypeRuntime(IWallClock wallClock, ApplicationLifecycleCoordinator lifecycle,
        FocusSessionCoordinator focus, SessionLedger ledger, TownSimulation town,
        SettingsSnapshot settings, List<DisplayModeChangeSnapshot> modeChanges)
    {
        _wallClock = wallClock;
        _lifecycle = lifecycle;
        _focus = focus;
        _ledger = ledger;
        _town = town;
        _settings = settings;
        _modeChanges = modeChanges;
        _focus.CheckpointAvailable += checkpoint => _lastFocusCheckpoint = checkpoint;
    }

    public static async Task<PrototypeRuntime> OpenAsync(IGameStateStore store,
        IActivityTracker activity, IWallClock wallClock, IMonotonicClock monotonicClock,
        TimeSpan checkpointInterval, CancellationToken cancellationToken = default)
    {
        var lifecycle = new ApplicationLifecycleCoordinator(store, wallClock,
            monotonicClock, checkpointInterval);
        var loaded = await lifecycle.InitializeAsync(cancellationToken);
        var ledger = loaded?.Snapshot.Focus.Ledger ?? new SessionLedger();
        var focus = new FocusSessionCoordinator(
            new FocusSessionManager(wallClock, monotonicClock), activity,
            ledger, new ElapsedTimeEnergyPolicy());
        var town = loaded is null ? TownSimulation.Create() : RestoreTown(loaded.Snapshot);
        var runtime = new PrototypeRuntime(wallClock, lifecycle, focus, ledger, town,
            loaded?.Snapshot.Settings ?? DefaultSettings(),
            loaded?.Snapshot.Focus.ModeChanges.ToList() ?? []);
        runtime._lastFocusCheckpoint = loaded?.Snapshot.Focus.ActiveCheckpoint;
        runtime._observedSessionDuration = runtime._lastFocusCheckpoint?.CountedDuration ?? TimeSpan.Zero;
        return runtime;
    }

    public SettingsSnapshot Settings => _settings;
    public TownSimulationSnapshot World => _town.Snapshot;
    public FocusSession? ActiveSession => _focus.ActiveSession;
    public FocusSessionCheckpoint? PendingRecovery => _lifecycle.PendingRecovery;
    public FocusActivitySummary Activity => _focus.CurrentActivity;
    public MinaTransition? LastMinaTransition { get; private set; }
    public TimeSpan TotalFocus => _ledger.TotalCountedDuration;
    public TimeSpan TodayFocus => TimeSpan.FromTicks(_ledger.Entries
        .Where(entry => DateOnly.FromDateTime(entry.EndedAtUtc.UtcDateTime) ==
            DateOnly.FromDateTime(_wallClock.UtcNow.UtcDateTime))
        .Sum(entry => entry.CountedDuration.Ticks));
    public bool OnboardingCompleted => _settings.OnboardingCompleted;

    public async Task CompleteOnboardingAsync()
    {
        _settings = _settings with { OnboardingCompleted = true };
        await SaveAsync(CheckpointReason.WorldChanged);
    }

    public async Task StartAsync(TimeSpan duration, IReadOnlyCollection<string> intendedApps,
        DisplayMode mode)
    {
        if (PendingRecovery is not null)
            throw new InvalidOperationException("Resolve the saved session first.");
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        _settings = _settings with { DisplayMode = mode.ToString() };
        _observedSessionDuration = TimeSpan.Zero;
        _focus.StartFocus(FocusSessionId.Create(), duration, intendedApps);
        Advance(TimeSpan.Zero, TimeSpan.Zero);
        await SaveAsync(CheckpointReason.FocusStarted);
    }

    public async Task<FocusSessionTickResult> TickAsync(TimeSpan consecutiveIdle)
    {
        var result = _focus.Tick();
        Advance(result.AppliedElapsed, consecutiveIdle);
        await SaveAsync(result.Completed ? CheckpointReason.FocusEnded : CheckpointReason.Periodic);
        return result;
    }

    public async Task StopAsync()
    {
        _focus.Stop();
        AdvanceFinalized();
        await SaveAsync(CheckpointReason.FocusEnded);
    }

    public async Task ChangeModeAsync(DisplayMode mode)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        _settings = _settings with { DisplayMode = mode.ToString() };
        if (_focus.ActiveSession is not null)
            _modeChanges.Add(new DisplayModeChangeSnapshot(_wallClock.UtcNow, mode.ToString()));
        await SaveAsync(CheckpointReason.DisplayChanged);
    }

    public async Task ChangeSettingsAsync(SettingsSnapshot settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        await SaveAsync(CheckpointReason.DisplayChanged);
    }

    public async Task<RecoveryResolution> ResolveRecoveryAsync(RecoveryChoice choice)
    {
        var result = _lifecycle.ResolveRecovery(choice, _focus);
        _observedSessionDuration = _focus.CurrentSession?.CountedDuration ?? TimeSpan.Zero;
        Advance(TimeSpan.Zero, TimeSpan.Zero);
        await SaveAsync(CheckpointReason.RecoveryDecision);
        return result;
    }

    public async Task RevealWorkshopAsync()
    {
        _town.RevealWorkshopCompletion();
        await SaveAsync(CheckpointReason.WorldChanged);
    }

    public async Task AcknowledgeWorkshopAsync()
    {
        _town.AcknowledgeCelebration();
        await SaveAsync(CheckpointReason.WorldChanged);
    }

    public async Task AcknowledgeRailwayAsync()
    {
        _town.AcknowledgeRailwayDiscovery();
        await SaveAsync(CheckpointReason.WorldChanged);
    }

    public Task SaveAsync(CheckpointReason reason) =>
        _lifecycle.SaveAsync(Capture(), reason);

    private void AdvanceFinalized()
    {
        var latest = _focus.CurrentSession?.CountedDuration ?? _observedSessionDuration;
        var delta = latest - _observedSessionDuration;
        Advance(delta, TimeSpan.Zero);
    }

    private void Advance(TimeSpan elapsed, TimeSpan consecutiveIdle)
    {
        _observedSessionDuration += elapsed;
        LastMinaTransition = _town.AdvanceTo(_town.Snapshot.LogicalTime + elapsed,
            _focus.TotalEnergy, _focus.ActiveSession?.Status, consecutiveIdle).Mina;
    }

    private GameStateSnapshot Capture()
    {
        var checkpoint = _town.CaptureCheckpoint();
        var project = ProjectSystem.RestoreWorkshop(checkpoint.ProjectProgress,
            checkpoint.LastObservedEnergy);
        var today = DateOnly.FromDateTime(_wallClock.UtcNow.UtcDateTime);
        var todayTicks = TodayFocus.Ticks;
        return new GameStateSnapshot(_settings,
            new FocusSnapshot(_ledger, _focus.ActiveSession is null ? null : _lastFocusCheckpoint,
                _modeChanges.ToArray()),
            EventSaveMapper.Town(project, checkpoint.Events),
            new MinaSnapshot(checkpoint.Mina.State.Activity.ToString(),
                checkpoint.Mina.State.Location.ToString(), nameof(ProjectId.RestoreWorkshop),
                checkpoint.Mina.ShortIdleStretchPlayed, checkpoint.Mina.WorkshopCelebrated),
            new PlayerSnapshot(_ledger.TotalCountedDuration.Ticks, today, todayTicks),
            EventSaveMapper.Presentation(checkpoint.Events));
    }

    private static TownSimulation RestoreTown(GameStateSnapshot snapshot)
    {
        var events = EventSaveMapper.Restore(snapshot.Town.Project,
            snapshot.Town.UnlockedProjectIds, snapshot.Town.PendingEventIds,
            snapshot.Town.ConsumedEventIds, snapshot.PendingPresentation.PendingIds,
            snapshot.PendingPresentation.ConsumedIds);
        var mina = snapshot.Mina;
        return TownSimulation.Restore(new TownSimulationCheckpoint(TimeSpan.Zero,
            snapshot.Town.Project.Progress,
            snapshot.Town.Project.LastObservedCumulativeEnergy,
            new MinaStateCheckpoint(new MinaSimulationState(
                Enum.Parse<MinaLogicalActivity>(mina.Activity),
                Enum.Parse<MinaLocation>(mina.Location)),
                mina.ShortIdleStretchPlayed, mina.WorkshopCelebrated), events.State));
    }

    private static SettingsSnapshot DefaultSettings() => new(
        DisplayMode.Companion.ToString(),
        new WindowPlacementSnapshot("screen:0", "BottomRight", 24, 24),
        new WindowPlacementSnapshot("screen:0", "BottomRight", 48, 48),
        100, 65, true, false, false);
}
