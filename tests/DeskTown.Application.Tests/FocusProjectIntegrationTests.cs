using DeskTown.Application.Activity;
using DeskTown.Application.Sessions;
using DeskTown.Application.Tests.Fakes;
using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;

namespace DeskTown.Application.Tests;

public sealed class FocusProjectIntegrationTests
{
    [Fact]
    public void Two_finalized_sessions_complete_workshop_once_without_reapplying_history()
    {
        var wallClock = new FakeWallClock(
            new DateTimeOffset(2026, 9, 25, 9, 0, 0, TimeSpan.Zero));
        var monotonicClock = new FakeMonotonicClock();
        var tracker = new FakeActivityTracker();
        var ledger = new SessionLedger();
        var coordinator = new FocusSessionCoordinator(
            new FocusSessionManager(wallClock, monotonicClock),
            tracker,
            ledger,
            new ElapsedTimeEnergyPolicy());
        var project = ProjectSystem.CreateRestoreWorkshop();
        var applications = new List<ProjectEnergyApplication>();
        coordinator.SessionFinalized += finalized =>
            applications.Add(project.ApplyCumulativeEnergy(finalized.TotalEnergy));

        tracker.Enqueue(ActivitySample.Active("code", TimeSpan.FromSeconds(1)));
        coordinator.StartFocus(FocusSessionId.Create(), TimeSpan.FromMinutes(10), ["code"]);
        wallClock.Advance(TimeSpan.FromMinutes(10));
        monotonicClock.Advance(TimeSpan.FromMinutes(10));
        coordinator.Tick();

        Assert.Equal(WorkshopState.Repairing, project.WorkshopState);
        Assert.Equal(TimeSpan.FromMinutes(10), project.Progress.CountedDuration);
        Assert.Null(Assert.Single(applications).Completion);

        tracker.Enqueue(ActivitySample.Idle("notepad", TimeSpan.FromSeconds(1)));
        coordinator.StartFocus(FocusSessionId.Create(), TimeSpan.FromMinutes(15), ["code"]);
        wallClock.Advance(TimeSpan.FromMinutes(15));
        monotonicClock.Advance(TimeSpan.FromMinutes(15));
        coordinator.Tick();

        Assert.Equal(WorkshopState.Complete, project.WorkshopState);
        Assert.Equal(TimeSpan.FromMinutes(25), project.Progress.CountedDuration);
        Assert.Equal(ProjectId.RestoreWorkshop, applications[1].Completion?.ProjectId);
        Assert.Equal(TimeSpan.FromMinutes(15), applications[1].ObservedDelta.CountedDuration);
        Assert.Equal(TimeSpan.FromMinutes(25), ledger.TotalCountedDuration);

        var replay = project.ApplyCumulativeEnergy(coordinator.TotalEnergy);
        Assert.False(replay.Changed);
        Assert.Null(replay.Completion);
    }
}
