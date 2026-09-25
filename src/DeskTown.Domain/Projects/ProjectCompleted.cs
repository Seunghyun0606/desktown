namespace DeskTown.Domain.Projects;

/// <summary>
/// A one-shot logical completion emitted by the project progression boundary.
/// Presentation and discovery systems consume this later; they do not own the
/// completed project state.
/// </summary>
public sealed record ProjectCompleted(ProjectId ProjectId);
