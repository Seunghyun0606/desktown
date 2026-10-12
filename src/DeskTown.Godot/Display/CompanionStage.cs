using DeskTown.Domain.Simulation;
using global::Godot;

namespace DeskTown.Presentation.Display;

public partial class CompanionStage : Control
{
    private MinaPlaceholderView? _mina;
    private MinaSimulationState _latest = new(MinaLogicalActivity.Idle, MinaLocation.Home);
    private CatalogEffectView? _spark;
    private CatalogEffectView? _steam;
    private bool _reducedMotion;
    private MinaAnimationClip _clip = MinaAnimationClip.Idle;
    private bool _returningToBench;
    private double _returnElapsed;

    public override void _Ready()
    {
        CatalogRectArt.Bind(GetNode<ColorRect>("SkyWindow"), "CMP-WIN-001");
        CatalogRectArt.Bind(GetNode<ColorRect>("Floor"), "CMP-FLR-001");
        CatalogRectArt.Bind(GetNode<ColorRect>("Lamp"), "CMP-LMP-001");
        CatalogRectArt.Bind(GetNode<ColorRect>("Workbench"), "CMP-WRK-001");
        CatalogRectArt.Bind(GetNode<ColorRect>("Stool"), "CMP-STL-001");
        CatalogRectArt.Bind(GetNode<ColorRect>("Plant"), "CMP-DEC-001");
        BindShadow();
        _mina = GetNode<MinaPlaceholderView>("MinaView");
        _spark = CatalogEffectView.Create(this, "FX-SPK-001", 4, 8, false, 137, 119);
        _steam = CatalogEffectView.Create(this, "FX-TEA-001", 4, 4, true, 129, 116);
        CatalogEffectView.Create(this, "CMP-TOL-001", 2, 1, true, 198, 103)?.SetEnabled(true, false);
        _mina.FrameChanged += OnMinaFrame;
        _mina.Bind(_latest);
    }

    public void SetReducedMotion(bool reduced)
    {
        _reducedMotion = reduced;
        if (reduced && _returningToBench) FinishReturn();
        _mina?.SetReducedMotion(reduced);
        if (reduced) _spark?.SetEnabled(false);
        _steam?.SetEnabled(!reduced && _clip == MinaAnimationClip.Rest);
    }

    private void OnMinaFrame(MinaAnimationClip clip, int frame)
    {
        _clip = clip;
        if (_returningToBench && clip != MinaAnimationClip.Walk) FinishReturn();
        _steam?.SetEnabled(!_reducedMotion && clip == MinaAnimationClip.Rest);
        if (_reducedMotion || clip != MinaAnimationClip.Work) _spark?.SetEnabled(false);
        else if (frame == 2) _spark?.Trigger();
    }

    private void BindShadow()
    {
        var shadow = GetNode<Sprite2D>("Shadow");
        if (VisualAssetCatalog.Frame("CMP-SHD-001") is not { } image)
        {
            // An unassigned or invalid shadow must not leave a solid geometric box.
            shadow.Texture = null;
            shadow.Visible = false;
            return;
        }
        shadow.Texture = image.Sheet;
        shadow.RegionRect = image.Source;
        shadow.Visible = true;
    }

    public void Bind(CompanionProjection snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _latest = snapshot.Mina;
        _mina?.Bind(_latest);
    }

    public void Play(MinaPresentationIntent presentation)
    {
        if (_returningToBench) FinishReturn();
        if (_reducedMotion && presentation.PrimaryClip == MinaAnimationClip.Walk
            && presentation.FollowUpClip is { } next) presentation = new(next);
        if (!_reducedMotion && presentation.PrimaryClip == MinaAnimationClip.Walk
            && presentation.FollowUpClip == MinaAnimationClip.Work)
        {
            _returningToBench = true;
            _returnElapsed = 0;
            if (_mina is not null) _mina.Position = new Vector2(88, 103);
            GetNode<Sprite2D>("Shadow").Position = new Vector2(96, 141);
        }
        _mina?.Play(presentation);
    }

    public override void _Process(double delta)
    {
        if (!_returningToBench || _mina is null) return;
        _returnElapsed += delta;
        _mina.Position = new Vector2(88 + (float)Math.Round(12 * Math.Min(1, _returnElapsed / 0.75)), 103);
        GetNode<Sprite2D>("Shadow").Position = _mina.Position + new Vector2(8, 38);
    }

    private void FinishReturn()
    {
        _returningToBench = false;
        if (_mina is not null) _mina.Position = new Vector2(100, 103);
        GetNode<Sprite2D>("Shadow").Position = new Vector2(108, 141);
    }

    public void Refresh()
    {
        if (_returningToBench) FinishReturn();
        _mina?.Bind(_latest);
    }
}
