namespace DeskTown.Domain.Events;

/// <summary>One-shot logical markers; visual playback itself is never a source of truth.</summary>
public sealed record EventSystemCheckpoint(
    bool WorkshopRevealPending,
    bool WorkshopRevealConsumed,
    bool RailwayDiscoveryPending,
    bool RailwayDiscoveryConsumed,
    bool RailwayTeaserUnlocked)
{
    public static EventSystemCheckpoint Empty { get; } = new(false, false, false, false, false);
}
