using DeskTown.Domain.Focus;

namespace DeskTown.Domain.Projects;

/// <summary>
/// The deterministic result of observing a cumulative Focus Energy total.
/// ObservedDelta may exceed AppliedToProject when the project reaches its cap.
/// </summary>
public sealed record ProjectEnergyApplication(
    FocusEnergy ObservedDelta,
    FocusEnergy AppliedToProject,
    FocusEnergy Progress,
    WorkshopState WorkshopState,
    ProjectCompleted? Completion)
{
    public bool Changed => AppliedToProject != FocusEnergy.Zero;
}
