using DeskTown.Domain.Projects;
using DeskTown.Domain.Simulation;
using DeskTown.Application.Display;
using DeskTown.Presentation.Display;
using global::Godot;

namespace DeskTown.Presentation;

/// <summary>Small world view. It reads a snapshot and never awards progress.</summary>
public partial class TownScene : Control
{
    private TownProjection? _pending;
    private double _ambientSeconds;
    private double _minaIdleSeconds;
    private double _minaTravelX;
    private int _minaTargetX;
    private bool _minaInitialized;
    private bool _minaTravelling;
    private MinaSimulationState? _presentedMina;
    private Sprite2D? _minaShadow;
    private CatalogEffectView? _smoke;
    private CatalogEffectView? _fire;
    private readonly List<CatalogEffectView> _grass = new();
    private bool _reducedMotion;
    private WorkshopState _visualWorkshop;
    public event Action? FocusRequested;

    public override void _Ready()
    {
        GetNode<Button>("Hud/Focus").Pressed += () => FocusRequested?.Invoke();
        VisibilityChanged += () => SetProcess(IsVisibleInTree());
        SetProcess(IsVisibleInTree());
        GetNode<TownNpcPlaceholderView>("World/NoahView").Configure(false);
        GetNode<TownNpcPlaceholderView>("World/RumiView").Configure(true);
        _minaShadow = TownEnvironment.Bind(this);
        CatalogRectArt.Bind(GetNode<ColorRect>("World/House"), "BLD-HOU-001",
            placeholderLabel: GetNode<Label>("World/House/Name"));
        CatalogRectArt.Bind(GetNode<ColorRect>("World/LockedArea"), "BLD-LCK-001",
            placeholderLabel: GetNode<Label>("World/LockedArea/Name"));
        CatalogRectArt.Bind(GetNode<ColorRect>("World/Tree"), "ENV-TRE-001");
        CatalogRectArt.Bind(GetNode<ColorRect>("World/Campfire"), "FX-FIR-001");
        var world = GetNode<Control>("World");
        _smoke = CatalogEffectView.Create(world, "FX-SMK-001", 4, 4, true, 461, 118);
        if (_smoke is not null) _smoke.FlipH = true;
        _fire = CatalogEffectView.Create(world, "FX-FIR-001", 4, 6, true, 312, 296);
        if (_fire is not null) GetNode<ColorRect>("World/Campfire").Hide();
        foreach (var (x, y) in new[] { (96, 296), (564, 278) })
            if (CatalogEffectView.Create(world, "FX-GRS-001", 2, 2, true, x, y) is { } grass)
                _grass.Add(grass);
        BindEffects();
        BindAmbient(TimeSpan.Zero);
        if (_pending is not null) Bind(_pending);
    }

    public void Bind(TownProjection snapshot, TimeSpan? todayFocus = null,
        WorkshopState? visualWorkshop = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _pending = snapshot;
        if (!IsNodeReady()) return;

        var workshop = GetNode<ColorRect>("World/Workshop");
        var label = GetNode<Label>("World/Workshop/State");
        var state = visualWorkshop ?? snapshot.Workshop;
        _visualWorkshop = state;
        BindEffects();
        (workshop.Color, label.Text) = state switch
        {
            WorkshopState.Broken => (Color.FromHtml("806f66"), "Broken Workshop"),
            WorkshopState.Repairing => (Color.FromHtml("ac8362"), "Repairing Workshop"),
            WorkshopState.Complete => (Color.FromHtml("d2a878"), "Workshop Complete"),
            _ => throw new ArgumentOutOfRangeException(nameof(snapshot))
        };
        CatalogRectArt.Bind(workshop, "BLD-WRK-001", (int)state,
            placeholderLabel: label);
        GetNode<Label>("Hud/Project").Text =
            snapshot.Events.RailwayTeaserUnlocked
                ? "Explore Old Railway  0 / 45 Focus  •  Coming in the next build"
                : $"Restore Workshop  {Math.Floor(snapshot.Progress.CountedDuration.TotalMinutes):0} / 25 Focus";
        GetNode<Button>("Hud/Focus").Disabled = snapshot.Workshop == WorkshopState.Complete;
        if (todayFocus is { } elapsed)
            GetNode<Label>("Hud/Today").Text = $"Completed today  {Math.Floor(elapsed.TotalMinutes):0} min";
        var mina = GetNode<MinaPlaceholderView>("World/MinaView");
        var minaX = snapshot.Mina.Location == MinaLocation.Home ? 168 : 437;
        // Changes that happened while Town was hidden are already true on
        // reopening. Animate travel only if the change was actually visible.
        if (!_minaInitialized || _reducedMotion ||
            (!IsVisibleInTree() && _presentedMina?.Location != snapshot.Mina.Location))
        {
            _minaInitialized = true;
            if (_presentedMina?.Location != snapshot.Mina.Location) _minaIdleSeconds = 0;
            _minaTravelX = minaX;
            _minaTargetX = minaX;
            _minaTravelling = false;
            mina.Position = new Vector2(minaX, 228);
            mina.Bind(snapshot.Mina);
        }
        else if (_presentedMina?.Location != snapshot.Mina.Location)
        {
            _minaTravelX = mina.Position.X;
            _minaTargetX = minaX;
            _minaTravelling = Math.Abs(_minaTravelX - minaX) > 0.01;
            mina.FacingLeft = minaX < _minaTravelX;
            mina.Play(new(MinaAnimationClip.Walk));
            _minaIdleSeconds = 0;
        }
        else if (!_minaTravelling && _presentedMina != snapshot.Mina)
            mina.Bind(snapshot.Mina);
        _presentedMina = snapshot.Mina;
        SyncMinaShadow();
        var minaName = GetNode<Label>("World/Mina");
        minaName.Position = new Vector2(minaX, 271);
        minaName.Text = "Mina";
    }

    public override void _Process(double delta)
    {
        if (!IsVisibleInTree()) return;
        var mina = GetNode<MinaPlaceholderView>("World/MinaView");
        if (!_reducedMotion)
        {
            _ambientSeconds += delta;
            BindAmbient(TimeSpan.FromSeconds(_ambientSeconds));
        }
        if (_minaTravelling && _presentedMina is not null)
        {
            var distance = _minaTargetX - _minaTravelX;
            _minaTravelX += Math.Sign(distance) * Math.Min(Math.Abs(distance), delta * 18);
            mina.Position = new Vector2((float)Math.Round(_minaTravelX), 228);
            if (Math.Abs(_minaTargetX - _minaTravelX) < 0.01)
            {
                _minaTravelling = false;
                _minaIdleSeconds = 0;
                mina.FacingLeft = false;
                mina.Bind(_presentedMina);
            }
        }
        else if (!_reducedMotion && _presentedMina?.Activity == MinaLogicalActivity.Idle)
        {
            _minaIdleSeconds += delta;
            var pose = NpcAmbientSchedule.MinaAt(TimeSpan.FromSeconds(_minaIdleSeconds));
            mina.Position = new Vector2(168 + (float)Math.Round(pose.X), 228);
            mina.FacingLeft = pose.FacingLeft;
            mina.Play(new(pose.Action == NpcAmbientAction.Walk ? MinaAnimationClip.Walk : MinaAnimationClip.Idle));
        }
        SyncMinaShadow();
    }

    private void SyncMinaShadow()
    {
        var mina = GetNode<MinaPlaceholderView>("World/MinaView");
        if (_minaShadow is not null) _minaShadow.Position = mina.Position + new Vector2(8, 38);
        mina.ZIndex = (int)mina.Position.Y + 44;
    }

    public void SetReducedMotion(bool reduced)
    {
        _reducedMotion = reduced;
        GetNode<MinaPlaceholderView>("World/MinaView").SetReducedMotion(reduced);
        if (reduced && _presentedMina is not null)
        {
            _minaTravelling = false;
            var mina = GetNode<MinaPlaceholderView>("World/MinaView");
            mina.Position = new Vector2(_presentedMina.Location == MinaLocation.Home ? 168 : 437, 228);
            mina.FacingLeft = false;
            mina.Bind(_presentedMina);
            BindAmbient(TimeSpan.FromSeconds(_ambientSeconds));
            SyncMinaShadow();
        }
        BindEffects();
    }

    private void BindEffects()
    {
        _smoke?.SetEnabled(!_reducedMotion && _visualWorkshop == WorkshopState.Complete);
        _fire?.SetEnabled(true, !_reducedMotion);
        foreach (var grass in _grass) grass.SetEnabled(!_reducedMotion);
    }

    internal void BindAmbient(TimeSpan elapsed)
    {
        var frame = NpcAmbientSchedule.At(elapsed);
        var noah = GetNode<Label>("World/Noah");
        var noahX = 268 + (float)Math.Round(frame.Noah.X);
        var noahY = 228 + (float)Math.Round(frame.Noah.Y);
        var rumiX = 226 + (float)Math.Round(frame.Rumi.X);
        var rumiY = 282 + (float)Math.Round(frame.Rumi.Y);
        noah.Position = new Vector2(noahX, noahY + 43);
        noah.Text = frame.NoahReading ? "Noah  •  Read" : "Noah";
        var noahView = GetNode<TownNpcPlaceholderView>("World/NoahView");
        noahView.Position = new Vector2(noahX, noahY);
        noahView.ZIndex = (int)noahY + 44;
        noahView.BindPose(QuietPose(frame.Noah));
        GetNode<Label>("World/Rumi").Position = new Vector2(rumiX, rumiY + 43);
        var rumiView = GetNode<TownNpcPlaceholderView>("World/RumiView");
        rumiView.Position = new Vector2(rumiX, rumiY);
        rumiView.ZIndex = (int)rumiY + 44;
        rumiView.BindPose(QuietPose(frame.Rumi));
    }

    private NpcAmbientPose QuietPose(NpcAmbientPose pose) => _reducedMotion
        ? pose with { Action = pose.Action == NpcAmbientAction.Walk ? NpcAmbientAction.Idle : pose.Action,
            ClipElapsed = TimeSpan.Zero } : pose;
}
