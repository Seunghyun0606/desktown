namespace DeskTown.Domain.Simulation;

public sealed record MinaTransition(
    MinaSimulationState State,
    MinaPresentationIntent Presentation,
    bool Changed);
