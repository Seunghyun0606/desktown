namespace DeskTown.Domain.Simulation;

/// <summary>Domain values required to restore one-shot presentation boundaries.</summary>
public sealed record MinaStateCheckpoint(
    MinaSimulationState State,
    bool ShortIdleStretchPlayed,
    bool WorkshopCelebrated);
