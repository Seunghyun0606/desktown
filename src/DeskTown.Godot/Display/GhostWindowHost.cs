using DeskTown.Domain.Simulation;
using DeskTown.Application.Ports;
using DeskTown.Application.Display;
using DeskTown.Application.Persistence;
using DeskTown.Platform.Windows.Ghost;
using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>Opt-in export spike only; normal mode remains unavailable pending native QA.</summary>
public partial class GhostWindowHost : Window
{
    private Node2D? _stage;
    private readonly IGhostWindowPlatform _platform = new WindowsGhostWindowPlatform();
    private nint _nativeWindow;
    private WindowPlacementSnapshot _placement = new("screen:0", "BottomRight", 48, 48);
    private int _opacityPercent = 65;

    public override void _Ready()
    {
        _stage = GetNode<Node2D>("GhostStage");
        _stage.ProcessMode = ProcessModeEnum.Disabled;
        Visible = false;
    }

    public void Bind(GhostProjection snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        GetNode<MinaPlaceholderView>("GhostStage/MinaView").Bind(snapshot.Mina);
    }

    public void Configure(WindowPlacementSnapshot placement, int opacityPercent)
    {
        if (opacityPercent is not (40 or 55 or 65 or 70 or 85))
            throw new ArgumentOutOfRangeException(nameof(opacityPercent));
        _placement = placement ?? throw new ArgumentNullException(nameof(placement));
        _opacityPercent = opacityPercent;
        if (_stage is not null)
            _stage.Modulate = new Color(1, 1, 1, _opacityPercent / 100f);
        if (Visible) RestorePosition();
    }

    private void RestorePosition()
    {
        var bounds = GhostPlacement.Restore(_placement, Size.X, Size.Y,
            CompanionWindowHost.WorkAreas());
        Position = new Vector2I(bounds.X, bounds.Y);
    }

    public void Preview(bool enabled)
    {
        if (enabled)
        {
            RestorePosition();
            Bind(new GhostProjection(new MinaSimulationState(
                MinaLogicalActivity.Work, MinaLocation.Workshop)));
            Visible = true;
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    var window = (nint)DisplayServer.WindowGetNativeHandle(
                        DisplayServer.HandleType.WindowHandle, GetWindowId());
                    _nativeWindow = window;
                    var result = _platform.Apply(window);
                    if (!result.Applied || !_platform.Verify(window).RequiredStylesPresent)
                    {
                        _platform.Remove(window);
                        _nativeWindow = 0;
                        Visible = false;
                        GD.PrintErr($"Ghost QA preview unavailable: {result.Reason}");
                        return;
                    }
                }
                catch (Exception error)
                {
                    if (_nativeWindow != 0) _platform.Remove(_nativeWindow);
                    _nativeWindow = 0;
                    Visible = false;
                    GD.PrintErr($"Ghost QA preview unavailable: {error.GetType().Name}");
                    return;
                }
            }
        }
        else
        {
            Visible = false;
            if (_nativeWindow != 0)
                _platform.Remove(_nativeWindow);
            _nativeWindow = 0;
        }
        if (_stage is not null)
            _stage.ProcessMode = enabled ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
    }

    public override void _ExitTree()
    {
        if (_nativeWindow != 0) _platform.Remove(_nativeWindow);
    }
}
