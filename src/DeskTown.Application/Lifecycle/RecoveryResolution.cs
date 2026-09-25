using DeskTown.Application.Sessions;
using DeskTown.Domain.Focus;

namespace DeskTown.Application.Lifecycle;

public sealed record RecoveryResolution(
    FocusSessionStatus Status,
    FocusSessionFinalized? Finalized);
