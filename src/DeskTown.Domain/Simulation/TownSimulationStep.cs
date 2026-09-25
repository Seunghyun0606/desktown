using DeskTown.Domain.Projects;
using DeskTown.Domain.Events;

namespace DeskTown.Domain.Simulation;

public sealed record TownSimulationStep(
    TownSimulationSnapshot Snapshot,
    ProjectEnergyApplication Project,
    MinaTransition Mina,
    EventTransition Events);
