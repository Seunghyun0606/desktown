using DeskTown.Domain.Projects;

namespace DeskTown.Domain.Simulation;

public sealed record TownSimulationStep(
    TownSimulationSnapshot Snapshot,
    ProjectEnergyApplication Project,
    MinaTransition Mina);
