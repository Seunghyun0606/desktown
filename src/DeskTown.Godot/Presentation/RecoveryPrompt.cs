using DeskTown.Application.Lifecycle;
using DeskTown.Application.Sessions;
using global::Godot;

namespace DeskTown.Presentation;

/// <summary>Passive startup decision. The host owns session restoration and saving.</summary>
public partial class RecoveryPrompt : Control
{
    public event Action<RecoveryChoice>? ChoiceSelected;

    public override void _Ready()
    {
        GetNode<Button>("Panel/Content/Resume").Pressed += () =>
            ChoiceSelected?.Invoke(RecoveryChoice.Resume);
        GetNode<Button>("Panel/Content/End").Pressed += () =>
            ChoiceSelected?.Invoke(RecoveryChoice.EndAtCheckpoint);
    }

    public void ShowCheckpoint(FocusSessionCheckpoint checkpoint)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        var minutes = (int)checkpoint.CountedDuration.TotalMinutes;
        var seconds = checkpoint.CountedDuration.Seconds;
        GetNode<Label>("Panel/Content/Progress").Text =
            $"Saved at {minutes:00}:{seconds:00} of your Focus session.";
        Visible = true;
    }

    public void SetBusy(bool busy)
    {
        GetNode<Button>("Panel/Content/Resume").Disabled = busy;
        GetNode<Button>("Panel/Content/End").Disabled = busy;
    }
}
