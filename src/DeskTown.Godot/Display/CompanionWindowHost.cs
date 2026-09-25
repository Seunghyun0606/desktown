using DeskTown.Application.Display;
using DeskTown.Application.Persistence;
using DeskTown.Domain.Simulation;
using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>Native Companion surface. The window never owns Focus or Town state.</summary>
public partial class CompanionWindowHost : Window, IFocusDisplaySurface
{
    public event Action? HideRequested;
    public event Action<WindowPlacementSnapshot>? PlacementChanged;
    private CompanionStage? _stage;
    private WindowPlacementSnapshot _placement = new("screen:0", "BottomRight", 24, 24);
    private int _scalePercent = 100;

    public override void _Ready()
    {
        CloseRequested += RequestHide;
        GetNode<Button>("Stage/Close").Pressed += RequestHide;
        _stage = GetNode<CompanionStage>("Stage");
        _stage.ProcessMode = ProcessModeEnum.Disabled;
        // This window starts hidden; the application chooses when it is shown.
        Visible = false;
    }

    public override void _Input(InputEvent input)
    {
        if (!Visible || input is not InputEventMouseButton click
            || click.ButtonIndex != MouseButton.Left || !click.Pressed)
            return;

        // Only the empty upper strip is a drag handle. The close control remains clickable.
        if (click.Position.X >= 0 && click.Position.X < 324
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
            var monitor = WorkAreas().FirstOrDefault(m => m.Id == $"screen:{CurrentScreen}")
                ?? WorkAreas()[0];
            _placement = CompanionPlacement.Capture(Position.X, Position.Y, monitor);
            PlacementChanged?.Invoke(_placement);
        }
        Visible = false;
        if (_stage is not null) _stage.ProcessMode = ProcessModeEnum.Disabled;
    }

    public void Bind(CompanionProjection snapshot) => _stage?.Bind(snapshot);

    public void Play(MinaPresentationIntent presentation) => _stage?.Play(presentation);

    public void Configure(WindowPlacementSnapshot placement, int scalePercent)
    {
        _ = CompanionPlacement.SizeForScale(scalePercent);
        _placement = placement ?? throw new ArgumentNullException(nameof(placement));
        _scalePercent = scalePercent;
        if (Visible)
        {
            Visible = false;
            Show();
        }
    }

    private static IReadOnlyList<MonitorWorkArea> WorkAreas()
    {
        var result = new List<MonitorWorkArea>();
        for (var i = 0; i < DisplayServer.GetScreenCount(); i++)
        {
            var rect = DisplayServer.ScreenGetUsableRect(i);
            result.Add(new MonitorWorkArea($"screen:{i}", rect.Position.X,
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
}
