using DeskTown.Application.Display;
using DeskTown.Application.Persistence;
using DeskTown.Domain.Simulation;
using DeskTown.Platform.Windows.Windows;
using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>Native Companion surface. The window never owns Focus or Town state.</summary>
public partial class CompanionWindowHost : Window, IFocusDisplaySurface
{
    public event Action? HideRequested;
    public event Action<WindowPlacementSnapshot>? PlacementChanged;
    public event Action<int>? ScaleRequested;
    private CompanionStage? _stage;
    private global::Godot.Timer? _placementSaveTimer;
    private WindowPlacementSnapshot _placement = new("screen:0", "BottomRight", 24, 24);
    private int _scalePercent = 100;

    public override void _Ready()
    {
        CloseRequested += RequestHide;
        GetNode<Button>("Stage/Close").Pressed += RequestHide;
        GetNode<Button>("Stage/ScaleDown").Pressed += () => RequestScale(-1);
        GetNode<Button>("Stage/ScaleUp").Pressed += () => RequestScale(1);
        _stage = GetNode<CompanionStage>("Stage");
        _stage.ProcessMode = ProcessModeEnum.Disabled;
        _placementSaveTimer = new global::Godot.Timer { OneShot = true, WaitTime = 0.35 };
        AddChild(_placementSaveTimer);
        _placementSaveTimer.Timeout += SaveCurrentPlacement;
        // This window starts hidden; the application chooses when it is shown.
        Visible = false;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMPositionChanged && Visible)
            _placementSaveTimer?.Start();
    }

    public override void _Input(InputEvent input)
    {
        if (!Visible || input is not InputEventMouseButton click
            || click.ButtonIndex != MouseButton.Left || !click.Pressed)
            return;

        // The labeled header is the drag handle; size and close controls remain clickable.
        if (click.Position.X >= 0 && click.Position.X < 258
            && click.Position.Y >= 0 && click.Position.Y < 28)
        {
            StartDrag();
            GetViewport().SetInputAsHandled();
        }
    }

    public new void Show()
    {
        if (!Visible)
        {
            var monitors = WorkAreas();
            var bounds = CompanionPlacement.Restore(_placement, _scalePercent, monitors);
            Size = new Vector2I(bounds.Width, bounds.Height);
            Position = new Vector2I(bounds.X, bounds.Y);
            _stage?.Refresh();
            if (_stage is not null) _stage.ProcessMode = ProcessModeEnum.Inherit;
            Visible = true;
        }
    }

    public new void Hide()
    {
        if (Visible)
        {
            _placementSaveTimer?.Stop();
            SaveCurrentPlacement();
        }
        Visible = false;
        if (_stage is not null) _stage.ProcessMode = ProcessModeEnum.Disabled;
    }

    public void Bind(CompanionProjection snapshot) => _stage?.Bind(snapshot);

    public void Play(MinaPresentationIntent presentation) => _stage?.Play(presentation);

    public WindowPlacementSnapshot CurrentPlacement()
    {
        if (!Visible) return _placement;
        var monitors = WorkAreas();
        var monitor = CurrentScreen >= 0 && CurrentScreen < monitors.Count
            ? monitors[CurrentScreen] : monitors[0];
        return CompanionPlacement.Capture(Position.X, Position.Y, monitor);
    }

    public void Configure(WindowPlacementSnapshot placement, int scalePercent)
    {
        _ = CompanionPlacement.SizeForScale(scalePercent);
        _placement = placement ?? throw new ArgumentNullException(nameof(placement));
        _scalePercent = scalePercent;
        GetNode<Label>("Stage/DragLabel").Text = $"DeskTown  •  drag here  •  {scalePercent}%";
        GetNode<Button>("Stage/ScaleDown").Disabled = scalePercent == 75;
        GetNode<Button>("Stage/ScaleUp").Disabled = scalePercent == 150;
        if (Visible)
        {
            Visible = false;
            Show();
        }
    }

    internal static IReadOnlyList<MonitorWorkArea> WorkAreas()
    {
        var result = new List<MonitorWorkArea>();
        for (var i = 0; i < DisplayServer.GetScreenCount(); i++)
        {
            var rect = DisplayServer.ScreenGetUsableRect(i);
            var monitorId = WindowsMonitorIdentity.AtOrFallback(
                rect.Position.X + rect.Size.X / 2,
                rect.Position.Y + rect.Size.Y / 2, $"screen:{i}");
            result.Add(new MonitorWorkArea(monitorId, rect.Position.X,
                rect.Position.Y, rect.Size.X, rect.Size.Y));
        }
        if (result.Count == 0)
        {
            var rect = DisplayServer.ScreenGetUsableRect();
            result.Add(new MonitorWorkArea("screen:0", rect.Position.X,
                rect.Position.Y, Math.Max(360, rect.Size.X), Math.Max(200, rect.Size.Y)));
        }
        return result;
    }

    private void RequestHide() => HideRequested?.Invoke();

    private void SaveCurrentPlacement()
    {
        if (!Visible) return;
        var placement = CurrentPlacement();
        if (placement == _placement) return;
        _placement = placement;
        PlacementChanged?.Invoke(placement);
    }

    private void RequestScale(int direction)
    {
        var scales = new[] { 75, 100, 125, 150 };
        var index = Array.IndexOf(scales, _scalePercent);
        var next = Math.Clamp(index + direction, 0, scales.Length - 1);
        if (next != index) ScaleRequested?.Invoke(scales[next]);
    }
}
