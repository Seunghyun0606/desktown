using global::Godot;

namespace DeskTown.Presentation;

/// <summary>Skippable visual hold. The project state was committed before this runs.</summary>
public partial class RewardReveal : Control
{
    private bool _complete;
    public event Action? WorkshopChanged;
    public event Action? Finished;

    public override void _Ready() =>
        GetNode<Button>("Panel/Content/Skip").Pressed += Finish;

    public async Task PlayAsync(bool reducedMotion)
    {
        _complete = false;
        Visible = true;
        GetNode<Button>("Panel/Content/Skip").Disabled = true;
        GetNode<Label>("Panel/Content/Beat").Text = "Mina finishes the last piece...";
        await ToSignal(GetTree().CreateTimer(reducedMotion ? 0.1 : 2.0), SceneTreeTimer.SignalName.Timeout);
        if (_complete || !IsInsideTree()) return;
        WorkshopChanged?.Invoke();
        GetNode<Label>("Panel/Content/Beat").Text = "✨  The Workshop is complete.";
        GetNode<Button>("Panel/Content/Skip").Disabled = false;
        if (reducedMotion) Finish();
    }

    public void Finish()
    {
        if (_complete) return;
        _complete = true;
        Visible = false;
        Finished?.Invoke();
    }
}
