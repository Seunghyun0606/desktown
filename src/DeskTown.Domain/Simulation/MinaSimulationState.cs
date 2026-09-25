namespace DeskTown.Domain.Simulation;

/// <summary>Immutable logical state, shared by every display mode.</summary>
public sealed record MinaSimulationState(MinaLogicalActivity Activity, MinaLocation Location);
