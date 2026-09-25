namespace DeskTown.Domain.Events;

public sealed record EventTransition(EventSystemCheckpoint State, bool Changed);
