using DeskTown.Domain.Events;
using DeskTown.Domain.Projects;

namespace DeskTown.Application.Persistence;

/// <summary>Maps the small V1 ID sets to one validated, deterministic event state.</summary>
public static class EventSaveMapper
{
    public static EventSystem Restore(ProjectSystem project,
        IReadOnlyList<string> unlockedProjects,
        IReadOnlyList<string> pendingEvents,
        IReadOnlyList<string> consumedEvents,
        IReadOnlyList<string> pendingPresentations,
        IReadOnlyList<string> consumedPresentations)
    {
        CheckIds(unlockedProjects, nameof(unlockedProjects),
            nameof(ProjectId.RestoreWorkshop), nameof(ProjectId.ExploreOldRailway));
        CheckIds(pendingEvents, nameof(pendingEvents), EventSystem.RailwayDiscoveryId);
        CheckIds(consumedEvents, nameof(consumedEvents), EventSystem.RailwayDiscoveryId);
        CheckIds(pendingPresentations, nameof(pendingPresentations), EventSystem.WorkshopRevealId);
        CheckIds(consumedPresentations, nameof(consumedPresentations), EventSystem.WorkshopRevealId);

        var checkpoint = new EventSystemCheckpoint(
            pendingPresentations.Contains(EventSystem.WorkshopRevealId),
            consumedPresentations.Contains(EventSystem.WorkshopRevealId),
            pendingEvents.Contains(EventSystem.RailwayDiscoveryId),
            consumedEvents.Contains(EventSystem.RailwayDiscoveryId),
            unlockedProjects.Contains(nameof(ProjectId.ExploreOldRailway)));
        var events = EventSystem.Restore(project, checkpoint);
        events.ReconcileCompletion();
        return events;
    }

    public static TownSnapshot Town(ProjectSystem project, EventSystemCheckpoint state) =>
        new(project,
            state.RailwayTeaserUnlocked
                ? [nameof(ProjectId.RestoreWorkshop), nameof(ProjectId.ExploreOldRailway)]
                : [nameof(ProjectId.RestoreWorkshop)],
            state.RailwayDiscoveryPending ? [EventSystem.RailwayDiscoveryId] : [],
            state.RailwayDiscoveryConsumed ? [EventSystem.RailwayDiscoveryId] : []);

    public static PendingPresentationSnapshot Presentation(EventSystemCheckpoint state) =>
        new(state.WorkshopRevealPending ? [EventSystem.WorkshopRevealId] : [],
            state.WorkshopRevealConsumed ? [EventSystem.WorkshopRevealId] : []);

    private static void CheckIds(IReadOnlyList<string> values, string field, params string[] allowed)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count != values.Distinct(StringComparer.Ordinal).Count()
            || values.Any(value => !allowed.Contains(value, StringComparer.Ordinal)))
            throw new InvalidDataException($"Unknown or duplicate ID in {field}.");
    }
}
