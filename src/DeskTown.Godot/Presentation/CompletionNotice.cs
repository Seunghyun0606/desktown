using global::Godot;

namespace DeskTown.Presentation;

/// <summary>A short unfocusable notice; Town opens only after an explicit click.</summary>
public partial class CompletionNotice : Window
{
    private int _generation;
    public event Action? OpenRequested;

    public override void _Ready()
    {
        Visible = false;
        GetNode<Button>("Panel/Content/Open").Pressed += () =>
        {
            Dismiss();
            OpenRequested?.Invoke();
        };
    }

    public void Dismiss()
    {
        _generation++;
        Visible = false;
    }

    public async void ShowMessage(string message)
    {
        var current = ++_generation;
        GetNode<Label>("Panel/Content/Message").Text = message;
        var usable = DisplayServer.ScreenGetUsableRect();
        Position = usable.Position + usable.Size - Size - new Vector2I(24, 24);
        Visible = true;
        await ToSignal(GetTree().CreateTimer(6.0), SceneTreeTimer.SignalName.Timeout);
        if (IsInsideTree() && current == _generation) Visible = false;
    }
}
