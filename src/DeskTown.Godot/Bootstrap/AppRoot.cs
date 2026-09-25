using DeskTown.Application.Configuration;
using DeskTown.Application.Display;
using DeskTown.Application.Ports;
using DeskTown.Platform.Windows.Lifecycle;
using DeskTown.Presentation.Display;
using DeskTown.Domain.Simulation;
using DeskTown.Presentation;
using global::Godot;

namespace DeskTown.Presentation.Bootstrap;

public partial class AppRoot : Node
{
    private readonly PrototypeOptions _configuredOptions = PrototypeOptions.Default;
    private readonly IStructuredLogger _logger = new GodotStructuredLogger();
    private WindowsSingleInstanceGate? _singleInstance;
    private CancellationTokenSource? _openRequestCancellation;
    private DisplayModeController? _display;
    private int _previewClipIndex;

    public override void _Ready()
    {
        if (OperatingSystem.IsWindows())
        {
            _singleInstance = WindowsSingleInstanceGate.Acquire();
            if (!_singleInstance.IsPrimary)
            {
                _ = RequestOpenAndQuitAsync();
                return;
            }
            _openRequestCancellation = new CancellationTokenSource();
            _ = ListenForOpenRequestsAsync(_openRequestCancellation.Token);
        }

        var resolution = PrototypeOptionsResolver.Resolve(_configuredOptions);
        var options = resolution.Options;

        if (resolution.UsedFallback)
        {
            GetNode<Label>("MainShell/Center/Message").Text =
                "DeskTown\nFoundation ready\nDefault configuration restored";

            _logger.Information(
                "prototype_options_fallback",
                new Dictionary<string, object?>
                {
                    ["validation_error_count"] = resolution.ValidationErrors.Count
                });
        }

        Engine.MaxFps = options.VisibleMaxFramesPerSecond;

        var companion = GetNode<CompanionWindowHost>("CompanionWindow");
        _display = new DisplayModeController(companion);
        companion.HideRequested += _display.CompanionCloseRequested;
        GetNode<TownScene>("MainShell/TownPreview").Bind(TownSimulation.Create().Snapshot.Town);

        _logger.Information(
            "application_started",
            new Dictionary<string, object?>
            {
                ["engine_version"] = Engine.GetVersionInfo()["string"].AsString(),
                ["visible_fps_cap"] = options.VisibleMaxFramesPerSecond,
                ["logical_tick_seconds"] = options.LogicalTickInterval.TotalSeconds,
                ["checkpoint_seconds"] = options.CheckpointInterval.TotalSeconds
            });
    }

    public override void _ExitTree()
    {
        _openRequestCancellation?.Cancel();
        _singleInstance?.Dispose();
        _openRequestCancellation?.Dispose();
    }

    public override void _Input(InputEvent input)
    {
        // Temporary exported-build QA entry point while Focus Setup is not wired.
        if (_display is null || input is not InputEventKey key
            || !key.Pressed || key.Echo)
            return;

        if (key.Keycode == Key.F7)
        {
            var town = GetNode<TownScene>("MainShell/TownPreview");
            town.Visible = !town.Visible;
        }
        else if (key.Keycode == Key.F9)
            _display.SetDisplayMode(_display.Mode == DisplayMode.Companion
                ? DisplayMode.Hidden : DisplayMode.Companion);
        else if (key.Keycode == Key.F8)
        {
            var clips = new[] { MinaAnimationClip.Idle, MinaAnimationClip.Walk,
                MinaAnimationClip.Work, MinaAnimationClip.Rest,
                MinaAnimationClip.Stretch, MinaAnimationClip.Celebrate };
            GetNode<CompanionWindowHost>("CompanionWindow")
                .Play(new MinaPresentationIntent(clips[_previewClipIndex++ % clips.Length]));
        }
        else if (key.Keycode == Key.F10)
        {
            var ghost = GetNode<GhostWindowHost>("GhostWindow");
            ghost.Preview(!ghost.Visible);
        }
        else return;
        GetViewport().SetInputAsHandled();
    }

    private async Task RequestOpenAndQuitAsync()
    {
        try { await _singleInstance!.RequestOpenAsync(); }
        finally { Callable.From(QuitSecondary).CallDeferred(); }
    }

    private async Task ListenForOpenRequestsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _singleInstance!.ListenAsync(() =>
            {
                Callable.From(OpenExistingInstance).CallDeferred();
                return Task.CompletedTask;
            }, cancellationToken);
        }
        catch (IOException error)
        {
            _logger.Information("single_instance_listener_failed",
                new Dictionary<string, object?> { ["reason"] = error.GetType().Name });
        }
    }

    private void OpenExistingInstance() => GetWindow().GrabFocus();

    private void QuitSecondary() => GetTree().Quit();
}
