namespace DeskTown.Application.Lifecycle;

public enum CheckpointReason
{
    FocusStarted,
    Periodic,
    Suspended,
    DisplayChanged,
    FocusEnded,
    WorldChanged,
    RecoveryDecision,
    Quit
}
