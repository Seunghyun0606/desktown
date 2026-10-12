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
    private global::Godot.Timer? _ambientTimer;
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
        _ambientTimer = new global::Godot.Timer { WaitTime = 5.0 };
        AddChild(_ambientTimer);
        _ambientTimer.Timeout += UpdateAmbient;
        VisibilityChanged += () =>
        {
            if (Visible)
            {
                _ambientTimer.Start();
                UpdateAmbient();
            }
            else _ambientTimer.Stop();
        };
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
        if (Visible)
        {
            _ambientTimer.Start();
            UpdateAmbient();
        }
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
        mina.Bind(snapshot.Mina);
        var minaX = snapshot.Mina.Location == MinaLocation.Home ? 168 : 437;
        mina.Position = new Vector2(minaX, 228);
        if (_minaShadow is not null) _minaShadow.Position = mina.Position + new Vector2(8, 38);
        var minaName = GetNode<Label>("World/Mina");
        minaName.Position = new Vector2(minaX, 271);
        minaName.Text = "Mina";
    }

    private void UpdateAmbient()
    {
        if (!Visible) return;
        BindAmbient(TimeSpan.FromMilliseconds(Time.GetTicksMsec()));
    }

    public void SetReducedMotion(bool reduced)
    {
        _reducedMotion = reduced;
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
        var noahX = 268 + MathF.Round(frame.NoahOffsetX / 2f);
        var rumiX = 226 + MathF.Round(frame.RumiOffsetX / 2f);
        noah.Position = new Vector2(noahX, 271);
        noah.Text = frame.NoahReading ? "Noah  •  Read" : "Noah";
        GetNode<TownNpcPlaceholderView>("World/NoahView").Position =
            new Vector2(noahX, 228);
        GetNode<TownNpcPlaceholderView>("World/NoahView").SetReading(frame.NoahReading);
        GetNode<Label>("World/Rumi").Position = new Vector2(rumiX, 325);
        GetNode<TownNpcPlaceholderView>("World/RumiView").Position =
            new Vector2(rumiX, 282);
    }
}
