using global::Godot;
using DeskTown.Presentation.Display;

namespace DeskTown.Presentation;

public partial class FirstLaunch : Control
{
    public event Action? ContinueRequested;

    public override void _Ready()
    {
        CatalogUiArt.Panel(this);
        var content = GetNode<VBoxContainer>("Panel/Content");
        if (CatalogUiArt.Insert(content, "UI-LOG-001", new Vector2(256, 96), 0) is not null)
            content.GetNode<Label>("Title").Hide();
        GetNode<Button>("Panel/Content/Continue").Pressed += () => ContinueRequested?.Invoke();
    }
}
