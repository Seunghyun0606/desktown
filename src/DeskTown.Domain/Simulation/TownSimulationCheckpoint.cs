using DeskTown.Domain.Focus;

namespace DeskTown.Domain.Simulation;

/// <summary>Exact cumulative baseline and logical time for restart without replaying frames.</summary>
public sealed record TownSimulationCheckpoint(
    TimeSpan LogicalTime,
    FocusEnergy ProjectProgress,
    FocusEnergy LastObservedEnergy,
    MinaStateCheckpoint Mina);
