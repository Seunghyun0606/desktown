using global::Godot;

namespace DeskTown.Presentation;

public partial class DiscoveryEvent : Control
{
    public event Action? Acknowledged;

    public override void _Ready() =>
        GetNode<Button>("Panel/Content/Continue").Pressed += () => Acknowledged?.Invoke();
}
