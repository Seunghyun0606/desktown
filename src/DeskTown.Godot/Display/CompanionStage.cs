using DeskTown.Domain.Simulation;
using global::Godot;

namespace DeskTown.Presentation.Display;

public partial class CompanionStage : Control
{
    private MinaPlaceholderView? _mina;
    private MinaSimulationState _latest = new(MinaLogicalActivity.Idle, MinaLocation.Home);

    public override void _Ready()
    {
        CatalogRectArt.Bind(GetNode<ColorRect>("SkyWindow"), "CMP-WIN-001");
        CatalogRectArt.Bind(GetNode<ColorRect>("Floor"), "CMP-FLR-001");
        CatalogRectArt.Bind(GetNode<ColorRect>("Lamp"), "CMP-LMP-001");
        CatalogRectArt.Bind(GetNode<ColorRect>("Workbench"), "CMP-WRK-001");
        CatalogRectArt.Bind(GetNode<ColorRect>("Stool"), "CMP-STL-001");
        CatalogRectArt.Bind(GetNode<ColorRect>("Plant"), "CMP-DEC-001");
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
