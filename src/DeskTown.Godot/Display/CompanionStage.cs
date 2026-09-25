using DeskTown.Domain.Simulation;
using global::Godot;

namespace DeskTown.Presentation.Display;

public partial class CompanionStage : Control
{
    private MinaPlaceholderView? _mina;
    private MinaSimulationState _latest = new(MinaLogicalActivity.Idle, MinaLocation.Home);

    public override void _Ready()
    {
        _mina = GetNode<MinaPlaceholderView>("MinaView");
        _mina.Bind(_latest);
    }

    public void Bind(CompanionProjection snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _latest = snapshot.Mina;
        _mina?.Bind(_latest);
    }

    public void Play(MinaPresentationIntent presentation) => _mina?.Play(presentation);

    public void Refresh() => _mina?.Bind(_latest);
}
