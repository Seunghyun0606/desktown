using global::Godot;

namespace DeskTown.Presentation;

public partial class FirstLaunch : Control
{
    public event Action? ContinueRequested;

    public override void _Ready() =>
        GetNode<Button>("Panel/Content/Continue").Pressed += () => ContinueRequested?.Invoke();
}
