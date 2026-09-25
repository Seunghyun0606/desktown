using DeskTown.Domain.Events;
using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;

namespace DeskTown.Domain.Tests;

public sealed class EventSystemTests
{
    [Fact]
    public void Completion_queues_only_workshop_reveal_until_it_is_acknowledged()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();
        var events = EventSystem.Create(project);
        Assert.Throws<InvalidOperationException>(() => events.AcknowledgeWorkshopReveal());

        var completion = project.ApplyCumulativeEnergy(ProjectSystem.RestoreWorkshopTarget).Completion!;
        Assert.True(events.ObserveCompletion(completion).Changed);
        Assert.False(events.ObserveCompletion(completion).Changed);
        Assert.True(events.State.WorkshopRevealPending);
        Assert.False(events.State.RailwayTeaserUnlocked);
        Assert.False(events.State.RailwayDiscoveryPending);

        Assert.True(events.AcknowledgeWorkshopReveal().Changed);
        Assert.False(events.AcknowledgeWorkshopReveal().Changed);
        Assert.True(events.State.WorkshopRevealConsumed);
        Assert.True(events.State.RailwayDiscoveryPending);
        Assert.True(events.State.RailwayTeaserUnlocked);
        Assert.False(events.IsRailwayPlayable);
        Assert.Equal(TimeSpan.FromMinutes(45), EventSystem.RailwayTeaserTarget.CountedDuration);

        Assert.True(events.AcknowledgeRailwayDiscovery().Changed);
        Assert.False(events.AcknowledgeRailwayDiscovery().Changed);
        Assert.True(events.State.RailwayDiscoveryConsumed);
        Assert.False(events.State.RailwayDiscoveryPending);
        Assert.Equal(ProjectId.RestoreWorkshop, project.CurrentProjectId);
    }

    [Fact]
    public void Restart_before_reveal_replays_pending_and_never_duplicates_progress()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();
        project.ApplyCumulativeEnergy(ProjectSystem.RestoreWorkshopTarget);
        var restored = ProjectSystem.RestoreWorkshop(project.Progress,
            project.LastObservedCumulativeEnergy);
        var events = EventSystem.Restore(restored, EventSystemCheckpoint.Empty);

        Assert.True(events.ReconcileCompletion().Changed);
        var pending = EventSystem.Restore(restored, events.State);
        Assert.False(pending.ReconcileCompletion().Changed);
        Assert.True(pending.State.WorkshopRevealPending);
        Assert.Equal(FocusEnergy.Zero,
            restored.ApplyCumulativeEnergy(project.LastObservedCumulativeEnergy).ObservedDelta);
        pending.AcknowledgeWorkshopReveal();
        var afterReveal = EventSystem.Restore(restored, pending.State);
        Assert.False(afterReveal.ObserveCompletion(new ProjectCompleted(ProjectId.RestoreWorkshop)).Changed);
        Assert.True(afterReveal.State.RailwayDiscoveryPending);
    }

    [Theory]
    [InlineData(true, true, false, false, false)]
    [InlineData(false, true, false, false, false)]
    [InlineData(false, true, true, false, false)]
    [InlineData(false, true, false, false, true)]
    public void Impossible_event_checkpoints_are_rejected(
        bool revealPending, bool revealConsumed, bool discoveryPending,
        bool discoveryConsumed, bool unlocked)
    {
        var project = ProjectSystem.CreateRestoreWorkshop();
        project.ApplyCumulativeEnergy(ProjectSystem.RestoreWorkshopTarget);
        Assert.Throws<ArgumentException>(() => EventSystem.Restore(project,
            new EventSystemCheckpoint(revealPending, revealConsumed,
                discoveryPending, discoveryConsumed, unlocked)));
    }

    [Fact]
    public void Incomplete_project_cannot_claim_reward()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();
        Assert.Throws<ArgumentException>(() => EventSystem.Restore(project,
            new EventSystemCheckpoint(true, false, false, false, false)));
        Assert.Throws<InvalidOperationException>(() => EventSystem.Create(project)
            .ObserveCompletion(new ProjectCompleted(ProjectId.RestoreWorkshop)));
    }
}
