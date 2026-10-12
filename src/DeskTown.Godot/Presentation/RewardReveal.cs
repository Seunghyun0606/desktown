using global::Godot;
using DeskTown.Presentation.Display;

namespace DeskTown.Presentation;

/// <summary>Skippable visual hold. The project state was committed before this runs.</summary>
public partial class RewardReveal : Control
{
    private bool _complete;
    private TextureRect? _workshop;
    private CatalogEffectView? _spark;
    public event Action? WorkshopChanged;
    public event Action? Finished;

    public override void _Ready()
    {
        CatalogUiArt.Panel(this);
        _workshop = CatalogUiArt.Insert(GetNode<VBoxContainer>("Panel/Content"), "BLD-WRK-001", new Vector2(320, 256), 1);
        if (_workshop is not null)
        {
            _spark = CatalogEffectView.Create(_workshop, "FX-SPK-001", 4, 8, false, 0, 138);
            if (_spark is not null) _spark.Scale = new Vector2(2, 2);
            _workshop.Resized += () =>
            {
                if (_spark is not null) _spark.Position = new Vector2((_workshop.Size.X - 320) / 2 + 144, 138);
            };
        }
        var panel = GetNode<PanelContainer>("Panel");
        panel.OffsetTop = -200;
        panel.OffsetBottom = 200;
        GetNode<Button>("Panel/Content/Skip").Pressed += Finish;
    }

    public async Task PlayAsync(bool reducedMotion)
    {
        _complete = false;
        _spark?.SetEnabled(false);
        if (_workshop is not null) _workshop.Texture = CatalogUiArt.Texture("BLD-WRK-001", 1);
        Visible = true;
        GetNode<Button>("Panel/Content/Skip").Disabled = true;
        GetNode<Label>("Panel/Content/Beat").Text = "Mina finishes the last piece...";
        await ToSignal(GetTree().CreateTimer(reducedMotion ? 0.1 : 2.0), SceneTreeTimer.SignalName.Timeout);
        if (_complete || !IsInsideTree()) return;
        WorkshopChanged?.Invoke();
        if (_workshop is not null) _workshop.Texture = CatalogUiArt.Texture("BLD-WRK-001", 2);
        if (!reducedMotion) _spark?.Trigger();
        GetNode<Label>("Panel/Content/Beat").Text = "The Workshop is complete.";
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
