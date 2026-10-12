using global::Godot;
using DeskTown.Presentation.Display;

namespace DeskTown.Presentation;

public partial class DiscoveryEvent : Control
{
    public event Action? Acknowledged;

    public override void _Ready()
    {
        CatalogUiArt.Panel(this);
        GetNode<Button>("Panel/Content/Continue").Icon = CatalogUiArt.Texture("UI-ICO-001", 8);
        if (CatalogUiArt.Insert(GetNode<VBoxContainer>("Panel/Content"), "ITM-MAP-001", new Vector2(192, 128), 2) is not null)
        {
            var panel = GetNode<PanelContainer>("Panel");
            panel.OffsetTop = -190;
            panel.OffsetBottom = 190;
        }
        GetNode<Button>("Panel/Content/Continue").Pressed += () => Acknowledged?.Invoke();
    }
}
