using DeskTown.Domain.Projects;
using DeskTown.Domain.Simulation;
using global::Godot;

namespace DeskTown.Presentation;

/// <summary>Small world view. It reads a snapshot and never awards progress.</summary>
public partial class TownScene : Control
{
    private TownProjection? _pending;
    public event Action? FocusRequested;

    public override void _Ready()
    {
        GetNode<Button>("Hud/Focus").Pressed += () => FocusRequested?.Invoke();
        if (_pending is not null) Bind(_pending);
    }

    public void Bind(TownProjection snapshot, TimeSpan? todayFocus = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _pending = snapshot;
        if (!IsNodeReady()) return;

        var workshop = GetNode<ColorRect>("World/Workshop");
        var label = GetNode<Label>("World/Workshop/State");
        (workshop.Color, label.Text) = snapshot.Workshop switch
        {
            WorkshopState.Broken => (Color.FromHtml("806f66"), "Broken Workshop"),
            WorkshopState.Repairing => (Color.FromHtml("ac8362"), "Repairing Workshop"),
            WorkshopState.Complete => (Color.FromHtml("d2a878"), "Workshop Complete"),
            _ => throw new ArgumentOutOfRangeException(nameof(snapshot))
        };
        GetNode<Label>("Hud/Project").Text =
            $"Restore Workshop  {Math.Floor(snapshot.Progress.CountedDuration.TotalMinutes):0} / 25 Focus";
        if (todayFocus is { } elapsed)
            GetNode<Label>("Hud/Today").Text = $"Today  {Math.Floor(elapsed.TotalMinutes):0} min";
    }
}
