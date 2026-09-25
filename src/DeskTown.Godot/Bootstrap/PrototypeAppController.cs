using DeskTown.Application.Display;
using DeskTown.Application.Lifecycle;
using DeskTown.Application.Persistence;
using DeskTown.Application.Runtime;
using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;
using DeskTown.Platform.Windows.Activity;
using DeskTown.Platform.Windows.Processes;
using DeskTown.Presentation.Display;
using global::Godot;

namespace DeskTown.Presentation.Bootstrap;

/// <summary>Godot command binding. Gameplay and persistence remain in PrototypeRuntime.</summary>
public partial class PrototypeAppController : Node
{
    private readonly WindowsActivityTracker _activity = new();
    private readonly WindowsProcessCatalog _catalog = new();
    private DisplayModeController? _display;
    private PrototypeRuntime? _game;
    private global::Godot.Timer? _timer;
    private TimeSpan _tickInterval;
    private TimeSpan _checkpointInterval;
    private bool _busy;
    private bool _revealing;

    public bool IsFocusActive => _game?.ActiveSession is not null;

    public void Configure(DisplayModeController display, TimeSpan tick, TimeSpan checkpoint)
    {
        _display = display;
        _tickInterval = tick;
        _checkpointInterval = checkpoint;
    }

    public void Start()
    {
        var tray = Root<TrayHost>("TrayHost");
        tray.OpenRequested += OpenTown;
        tray.ModeRequested += mode => _ = RunAsync(() => ChangeModeAsync(mode));
        tray.CompanionScaleRequested += scale => _ = RunAsync(() => ChangeScaleAsync(scale));
        tray.GhostPositionRequested += anchor => _ = RunAsync(() =>
            _game!.ChangeSettingsAsync(_game.Settings with
            { Ghost = _game.Settings.Ghost with { Anchor = anchor } }));
        tray.GhostOpacityRequested += opacity => _ = RunAsync(() =>
            _game!.ChangeSettingsAsync(_game.Settings with { GhostOpacityPercent = opacity }));
        tray.AudioToggleRequested += () => _ = RunAsync(() =>
            _game!.ChangeSettingsAsync(_game.Settings with
            { AudioEnabled = !_game.Settings.AudioEnabled }));
        tray.EndFocusRequested += () => _ = RunAsync(EndFocusAsync);
        tray.QuitRequested += RequestQuit;

        Root<FirstLaunch>("MainShell/FirstLaunch").ContinueRequested += () =>
            _ = RunAsync(CompleteOnboardingAsync);
        Root<TownScene>("MainShell/Town").FocusRequested += OpenSetup;
        Root<FocusSetup>("MainShell/FocusSetup").StartRequested +=
            (duration, apps, mode) => _ = RunAsync(() => StartFocusAsync(duration, apps, mode));
        Root<FocusSetup>("MainShell/FocusSetup").BackRequested += OpenTown;
        Root<RecoveryPrompt>("MainShell/RecoveryPrompt").ChoiceSelected += choice =>
            _ = RunAsync(() => ResolveRecoveryAsync(choice));
        Root<RewardReveal>("MainShell/RewardReveal").Finished += () =>
            _ = RunAsync(FinishRevealAsync);
        Root<RewardReveal>("MainShell/RewardReveal").WorkshopChanged += () =>
        {
            if (_game is not null)
                Root<TownScene>("MainShell/Town").Bind(_game.World.Town,
                    _game.TodayFocus);
        };
        Root<DiscoveryEvent>("MainShell/DiscoveryEvent").Acknowledged += () =>
            _ = RunAsync(AcknowledgeDiscoveryAsync);
        GetWindow().CloseRequested += () =>
        {
            if (IsFocusActive) ReturnToFocus();
            else RequestQuit();
        };

        _timer = new global::Godot.Timer { WaitTime = _tickInterval.TotalSeconds };
        AddChild(_timer);
        _timer.Timeout += () => _ = TickAsync();
        _ = InitializeAsync();
    }

    public void CompanionClosed()
    {
        _display?.CompanionCloseRequested();
        if (IsFocusActive) _ = RunAsync(() => _game!.ChangeModeAsync(DisplayMode.Hidden));
    }

    public void CompanionMoved(WindowPlacementSnapshot placement)
    {
        if (_game is not null)
            _ = RunAsync(() => _game.ChangeSettingsAsync(
                _game.Settings with { Companion = placement }));
    }

    private T Root<T>(string path) where T : Node => GetParent().GetNode<T>(path);

    private async Task InitializeAsync()
    {
        try
        {
            _game = await PrototypeRuntime.OpenAsync(GameStatePathResolver.CreateDefaultStore(),
                _activity, new SystemWallClock(), new StopwatchMonotonicClock(), _checkpointInterval);
            Root<CompanionWindowHost>("CompanionWindow").Configure(
                _game.Settings.Companion, _game.Settings.CompanionScalePercent);
            Root<Label>("MainShell/Center/Message").Visible = false;
            Root<TownScene>("MainShell/Town").Bind(_game.World.Town, _game.TodayFocus);
            Root<FirstLaunch>("MainShell/FirstLaunch").Visible = !_game.OnboardingCompleted;
            Root<TownScene>("MainShell/Town").Visible = _game.OnboardingCompleted;
            if (_game.PendingRecovery is { } checkpoint)
                Root<RecoveryPrompt>("MainShell/RecoveryPrompt").ShowCheckpoint(checkpoint);
            else if (_game.OnboardingCompleted &&
                (_game.World.Town.Events.WorkshopRevealPending ||
                 _game.World.Town.Events.RailwayDiscoveryPending))
                OpenTown();
        }
        catch (Exception error) { ShowError(error); }
    }

    private async Task CompleteOnboardingAsync()
    {
        await _game!.CompleteOnboardingAsync();
        Root<FirstLaunch>("MainShell/FirstLaunch").Visible = false;
        Root<TownScene>("MainShell/Town").Visible = true;
    }

    private void OpenSetup()
    {
        if (_game is null || IsFocusActive) return;
        var setup = Root<FocusSetup>("MainShell/FocusSetup");
        setup.Bind(_game.World.Town, _catalog.GetRunningProcessNames());
        setup.Visible = true;
    }

    private async Task StartFocusAsync(TimeSpan duration,
        IReadOnlyCollection<string> apps, DisplayMode mode)
    {
        if (_game is null || IsFocusActive || mode == DisplayMode.Ghost) return;
        await _game.StartAsync(duration, apps, mode);
        Root<FocusSetup>("MainShell/FocusSetup").Visible = false;
        Root<CompanionWindowHost>("CompanionWindow").Bind(_game.World.Companion);
        _display!.SetDisplayMode(mode);
        GetWindow().Visible = false;
        Root<TrayHost>("TrayHost").SetStatus(true, false);
        _timer!.Start();
    }

    private async Task TickAsync()
    {
        if (_busy || _game?.ActiveSession is null) return;
        _busy = true;
        try
        {
            var tick = await _game.TickAsync(_activity.LastIdleAge);
            if (_game.LastMinaTransition is { Changed: true } transition)
                Root<CompanionWindowHost>("CompanionWindow").Play(transition.Presentation);
            if (tick.Completed) FinishFocus();
        }
        catch (Exception error) { ShowError(error); }
        finally { _busy = false; }
    }

    private async Task EndFocusAsync()
    {
        if (_game?.ActiveSession is null) return;
        await _game.StopAsync();
        FinishFocus();
    }

    private void FinishFocus()
    {
        _timer?.Stop();
        _display?.SetDisplayMode(DisplayMode.Hidden);
        Root<TrayHost>("TrayHost").SetStatus(false, true);
        // The main window stays hidden until the user opens it.
        if (OperatingSystem.IsWindows())
        {
            try
            {
                var message = _game?.World.Town.Workshop == WorkshopState.Complete
                    ? "Mina finished her work. The Workshop has changed."
                    : "Mina finished her work.";
                DisplayServer.SendToastNotification("DeskTown", message, null!, default);
            }
            catch (Exception error)
            {
                GD.PrintErr($"DeskTown notification unavailable: {error.GetType().Name}");
                // Tray tooltip already holds the passive fallback.
            }
        }
    }

    private async Task ChangeModeAsync(DisplayMode mode)
    {
        if (_game is null || mode == DisplayMode.Ghost) return;
        _display!.SetDisplayMode(mode);
        if (IsFocusActive) GetWindow().Visible = false;
        await _game.ChangeModeAsync(mode);
    }

    private async Task ChangeScaleAsync(int scale)
    {
        if (_game is null) return;
        Root<CompanionWindowHost>("CompanionWindow")
            .Configure(_game.Settings.Companion, scale);
        await _game.ChangeSettingsAsync(_game.Settings with { CompanionScalePercent = scale });
    }

    private async Task ResolveRecoveryAsync(RecoveryChoice choice)
    {
        if (_game is null) return;
        var prompt = Root<RecoveryPrompt>("MainShell/RecoveryPrompt");
        prompt.SetBusy(true);
        var result = await _game.ResolveRecoveryAsync(choice);
        prompt.Visible = false;
        if (result.Status == FocusSessionStatus.Running)
        {
            ReturnToFocus();
            _timer!.Start();
            Root<TrayHost>("TrayHost").SetStatus(true, false);
        }
        else
        {
            Root<TownScene>("MainShell/Town").Bind(_game.World.Town, _game.TodayFocus);
            Root<TrayHost>("TrayHost").SetStatus(false, true);
        }
    }

    public void OpenTown()
    {
        if (_game is null) return;
        _display?.SetDisplayMode(DisplayMode.Hidden);
        var pendingReveal = _game.World.Town.Events.WorkshopRevealPending;
        Root<TownScene>("MainShell/Town").Bind(_game.World.Town,
            _game.TodayFocus, visualWorkshop: pendingReveal ? WorkshopState.Repairing : null);
        Root<TownScene>("MainShell/Town").Visible = true;
        Root<FocusSetup>("MainShell/FocusSetup").Visible = false;
        GetWindow().Visible = true;
        GetWindow().GrabFocus();
        if (pendingReveal && !_revealing)
            _ = RunAsync(StartRevealAsync);
        else if (_game.World.Town.Events.RailwayDiscoveryPending)
            Root<DiscoveryEvent>("MainShell/DiscoveryEvent").Visible = true;
    }

    private async Task StartRevealAsync()
    {
        if (_game is null || _revealing) return;
        _revealing = true;
        await _game.RevealWorkshopAsync();
        Root<CompanionWindowHost>("CompanionWindow").Play(
            new DeskTown.Domain.Simulation.MinaPresentationIntent(
                DeskTown.Domain.Simulation.MinaAnimationClip.Work,
                DeskTown.Domain.Simulation.MinaAnimationClip.Celebrate));
        await Root<RewardReveal>("MainShell/RewardReveal")
            .PlayAsync(_game.Settings.ReducedMotion);
    }

    private async Task FinishRevealAsync()
    {
        if (_game is null || !_revealing) return;
        Root<TownScene>("MainShell/Town").Bind(_game.World.Town, _game.TodayFocus);
        await _game.AcknowledgeWorkshopAsync();
        _revealing = false;
        if (_game.World.Town.Events.RailwayDiscoveryPending)
            Root<DiscoveryEvent>("MainShell/DiscoveryEvent").Visible = true;
    }

    private async Task AcknowledgeDiscoveryAsync()
    {
        if (_game is null || !_game.World.Town.Events.RailwayDiscoveryPending) return;
        await _game.AcknowledgeRailwayAsync();
        Root<DiscoveryEvent>("MainShell/DiscoveryEvent").Visible = false;
        Root<TownScene>("MainShell/Town").Bind(_game.World.Town, _game.TodayFocus);
    }

    private void ReturnToFocus()
    {
        if (_game?.ActiveSession is null) return;
        var mode = Enum.TryParse<DisplayMode>(_game.Settings.DisplayMode, out var selected)
            && selected != DisplayMode.Ghost ? selected : DisplayMode.Hidden;
        Root<CompanionWindowHost>("CompanionWindow").Bind(_game.World.Companion);
        _display!.SetDisplayMode(mode);
        GetWindow().Visible = false;
    }

    private void RequestQuit()
    {
        if (!IsFocusActive) { GetTree().Quit(); return; }
        OpenTown();
        var dialog = new ConfirmationDialog
        {
            Title = "End Focus and quit?",
            DialogText = "Your counted Focus time and Workshop progress will be saved."
        };
        GetWindow().AddChild(dialog);
        dialog.Confirmed += () => _ = RunAsync(async () =>
        {
            await EndFocusAsync();
            await _game!.SaveAsync(CheckpointReason.Quit);
            GetTree().Quit();
        });
        dialog.Canceled += dialog.QueueFree;
        dialog.PopupCentered();
    }

    private async Task RunAsync(Func<Task> action)
    {
        try { await action(); }
        catch (Exception error) { ShowError(error); }
    }

    private void ShowError(Exception error)
    {
        GD.PrintErr($"DeskTown command failed: {error.GetType().Name}");
        var message = Root<Label>("MainShell/Center/Message");
        message.Text = "DeskTown needs attention. Saved progress was not discarded.";
        message.Visible = true;
        GetWindow().Visible = true;
    }
}
