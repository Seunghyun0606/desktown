using DeskTown.Domain.Focus;

namespace DeskTown.Application.Sessions;

/// <summary>
/// Terminal orchestration result. NewlyRecorded is false only for an idempotent
/// replay of the exact same terminal session snapshot.
/// </summary>
public sealed record FocusSessionFinalized(
    FocusSessionCheckpoint Checkpoint,
    bool NewlyRecorded,
    FocusEnergy TotalEnergy);
