using DeskTown.Domain.Simulation;
using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>Shared art only; the native Ghost gate remains in GhostWindowHost.</summary>
public partial class GhostStage : Node2D
{
    private CatalogEffectView? _spark;
    private bool _reducedMotion;

    public override void _Ready()
    {
        if (CatalogEffectView.Create(this, "CMP-SHD-001", 1, 1, true, 83, 126) is { } shadow)
        {
            shadow.SetEnabled(true, false);
            MoveChild(shadow, 0);
            GetNode<Polygon2D>("Shadow").Hide();
        }
        if (CatalogEffectView.Create(this, "CMP-WRK-001", 1, 1, true, 110, 76) is { } workbench)
        {
            workbench.SetEnabled(true, false);
            MoveChild(workbench, GetNode<MinaPlaceholderView>("MinaView").GetIndex());
            GetNode<Polygon2D>("WorkProp").Hide();
        }
        _spark = CatalogEffectView.Create(this, "FX-SPK-001", 4, 8, false, 112, 104);
        GetNode<MinaPlaceholderView>("MinaView").FrameChanged += (clip, frame) =>
        {
            if (_reducedMotion || clip != MinaAnimationClip.Work) _spark?.SetEnabled(false);
            else if (frame == 2) _spark?.Trigger();
        };
    }

    public void SetReducedMotion(bool reduced)
    {
        _reducedMotion = reduced;
        GetNode<MinaPlaceholderView>("MinaView").SetReducedMotion(reduced);
        if (reduced) _spark?.SetEnabled(false);
    }
}
