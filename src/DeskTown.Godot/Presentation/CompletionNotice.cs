using global::Godot;

namespace DeskTown.Presentation;

/// <summary>A short passive window; it never opens the Town or owns Focus.</summary>
public partial class CompletionNotice : Window
{
    private int _generation;

    public override void _Ready() => Visible = false;

    public async void ShowMessage(string message)
    {
        var current = ++_generation;
        GetNode<Label>("Panel/Message").Text = message;
        var usable = DisplayServer.ScreenGetUsableRect();
        Position = usable.Position + usable.Size - Size - new Vector2I(24, 24);
        Visible = true;
        await ToSignal(GetTree().CreateTimer(6.0), SceneTreeTimer.SignalName.Timeout);
        if (IsInsideTree() && current == _generation) Visible = false;
    }
}
