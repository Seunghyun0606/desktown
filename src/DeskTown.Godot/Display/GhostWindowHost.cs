using DeskTown.Domain.Simulation;
using DeskTown.Application.Ports;
using DeskTown.Platform.Windows.Ghost;
using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>Opt-in export spike only; normal mode remains unavailable pending native QA.</summary>
public partial class GhostWindowHost : Window
{
    private Node2D? _stage;
    private readonly IGhostWindowPlatform _platform = new WindowsGhostWindowPlatform();
    private nint _nativeWindow;

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

    public void Preview(bool enabled)
    {
        if (enabled)
        {
            var usable = DisplayServer.ScreenGetUsableRect();
            Position = usable.Position + usable.Size - Size - new Vector2I(48, 48);
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
