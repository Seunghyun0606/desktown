using DeskTown.Application.Display;
using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>Native Companion surface. The window never owns Focus or Town state.</summary>
public partial class CompanionWindowHost : Window, IFocusDisplaySurface
{
    public event Action? HideRequested;

    public override void _Ready()
    {
        CloseRequested += RequestHide;
        GetNode<Button>("Stage/Close").Pressed += RequestHide;
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
            var usable = DisplayServer.ScreenGetUsableRect();
            Position = usable.Position + usable.Size - Size - new Vector2I(24, 24);
            Visible = true;
        }
    }

    public new void Hide() => Visible = false;

    private void RequestHide() => HideRequested?.Invoke();
}
