using DeskTown.Domain.Simulation;
using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>Opt-in export spike only; normal mode remains unavailable pending native QA.</summary>
public partial class GhostWindowHost : Window
{
    private Node2D? _stage;

    public override void _Ready()
    {
        _stage = GetNode<Node2D>("GhostStage");
        _stage.ProcessMode = ProcessModeEnum.Disabled;
        Visible = false;
    }

    public void Bind(GhostProjection snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        GetNode<MinaPlaceholderView>("GhostStage/MinaView").Bind(snapshot.Mina);
    }

    public void Preview(bool enabled)
    {
        if (enabled)
        {
            var usable = DisplayServer.ScreenGetUsableRect();
            Position = usable.Position + usable.Size - Size - new Vector2I(48, 48);
            Bind(new GhostProjection(new MinaSimulationState(
                MinaLogicalActivity.Work, MinaLocation.Workshop)));
        }
        Visible = enabled;
        if (_stage is not null)
            _stage.ProcessMode = enabled ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
    }
}
