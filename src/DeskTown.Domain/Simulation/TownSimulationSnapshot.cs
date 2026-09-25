using DeskTown.Domain.Focus;
using DeskTown.Domain.Events;
using DeskTown.Domain.Projects;

namespace DeskTown.Domain.Simulation;

/// <summary>Logical, immutable projections; a renderer may choose any of them.</summary>
public sealed record TownSimulationSnapshot(
    TimeSpan LogicalTime,
    TownProjection Town,
    CompanionProjection Companion,
    GhostProjection Ghost);

public sealed record TownProjection(
    ProjectId CurrentProject,
    WorkshopState Workshop,
    FocusEnergy Progress,
    FocusEnergy Target,
    MinaSimulationState Mina,
    EventSystemCheckpoint Events);

public sealed record CompanionProjection(MinaSimulationState Mina);

public sealed record GhostProjection(MinaSimulationState Mina);
