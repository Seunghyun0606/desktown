using DeskTown.Domain.Projects;
using DeskTown.Domain.Simulation;
using DeskTown.Application.Display;
using global::Godot;

namespace DeskTown.Presentation;

/// <summary>Small world view. It reads a snapshot and never awards progress.</summary>
public partial class TownScene : Control
{
    private TownProjection? _pending;
    private global::Godot.Timer? _ambientTimer;
    public event Action? FocusRequested;

    public override void _Ready()
    {
        GetNode<Button>("Hud/Focus").Pressed += () => FocusRequested?.Invoke();
        _ambientTimer = new global::Godot.Timer { WaitTime = 5.0, Autostart = true };
        AddChild(_ambientTimer);
        _ambientTimer.Timeout += UpdateAmbient;
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
        (workshop.Color, label.Text) = (visualWorkshop ?? snapshot.Workshop) switch
        {
            WorkshopState.Broken => (Color.FromHtml("806f66"), "Broken Workshop"),
            WorkshopState.Repairing => (Color.FromHtml("ac8362"), "Repairing Workshop"),
            WorkshopState.Complete => (Color.FromHtml("d2a878"), "Workshop Complete"),
            _ => throw new ArgumentOutOfRangeException(nameof(snapshot))
        };
        GetNode<Label>("Hud/Project").Text =
            snapshot.Events.RailwayTeaserUnlocked
                ? "Explore Old Railway  0 / 45 Focus  •  Coming in the next build"
                : $"Restore Workshop  {Math.Floor(snapshot.Progress.CountedDuration.TotalMinutes):0} / 25 Focus";
        GetNode<Button>("Hud/Focus").Disabled = snapshot.Workshop == WorkshopState.Complete;
        if (todayFocus is { } elapsed)
            GetNode<Label>("Hud/Today").Text = $"Today  {Math.Floor(elapsed.TotalMinutes):0} min";
        GetNode<Label>("World/Mina").Text = $"Mina  •  {snapshot.Mina.Activity}";
    }

    private void UpdateAmbient()
    {
        if (!Visible) return;
        var frame = NpcAmbientSchedule.At(TimeSpan.FromMilliseconds(Time.GetTicksMsec()));
        var noah = GetNode<Label>("World/Noah");
        noah.Position = new Vector2(510 + frame.NoahOffsetX, 461);
        noah.Text = frame.NoahReading ? "Noah  •  Read" : "Noah";
        GetNode<Label>("World/Rumi").Position = new Vector2(370 + frame.RumiOffsetX, 567);
    }
}
