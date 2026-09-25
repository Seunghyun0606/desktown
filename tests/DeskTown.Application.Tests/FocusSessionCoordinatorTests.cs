using DeskTown.Application.Activity;
using DeskTown.Application.Sessions;
using DeskTown.Application.Tests.Fakes;
using DeskTown.Domain.Focus;

namespace DeskTown.Application.Tests;

public sealed class FocusSessionCoordinatorTests
{
    private static readonly DateTimeOffset StartTime =
        new(2026, 9, 25, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void EmptyIntendedAppsStartsAsAnyApp()
    {
        var context = CreateContext();

        var session = context.Coordinator.StartFocus(
            SessionId(),
            TimeSpan.FromMinutes(25));

        Assert.Empty(session.IntendedProcessNames);
        Assert.Equal(FocusSessionStatus.Running, session.Status);
        Assert.Single(context.Checkpoints);
    }

    [Fact]
    public void IntendedAppsRemainMetadataAndDoNotGateEnergy()
    {
        var context = CreateContext();
        context.Tracker.Enqueue(ActivitySample.Active("notepad", TimeSpan.FromSeconds(1)));
        var session = context.Coordinator.StartFocus(
            SessionId(),
            TimeSpan.FromSeconds(1),
            new[] { " code ", "CODE" });

        context.Advance(TimeSpan.FromSeconds(1));
        context.Coordinator.Tick();

        Assert.Equal(new[] { "code" }, session.IntendedProcessNames);
        Assert.Equal(TimeSpan.FromSeconds(1), context.Coordinator.TotalEnergy.CountedDuration);
        Assert.Equal(FocusSessionStatus.Completed, session.Status);
    }

    [Fact]
    public void UnknownActivityDoesNotLoseMonotonicCountedTime()
    {
        var context = CreateContext();
        var session = context.Coordinator.StartFocus(SessionId(), TimeSpan.FromSeconds(1));

        context.Advance(TimeSpan.FromSeconds(1));
        var result = context.Coordinator.Tick();

        Assert.True(result.Completed);
        Assert.Equal(TimeSpan.FromSeconds(1), session.CountedDuration);
        Assert.Equal(TimeSpan.Zero, session.IdleDuration);
        Assert.Equal(TimeSpan.FromSeconds(1), context.Coordinator.CurrentActivity.UnknownDuration);
        Assert.Equal(TimeSpan.FromSeconds(1), context.Coordinator.TotalEnergy.CountedDuration);
    }

    [Theory]
    [MemberData(nameof(RecoverableTrackerFailures))]
    public void TrackerFailureDegradesToTimerOnlyFocus(Exception failure)
    {
        var context = CreateContext();
        context.Tracker.Exception = failure;
        var session = context.Coordinator.StartFocus(SessionId(), TimeSpan.FromSeconds(1));

        context.Advance(TimeSpan.FromSeconds(1));
        context.Coordinator.Tick();

        Assert.Equal(FocusSessionStatus.Completed, session.Status);
        Assert.Equal(TimeSpan.FromSeconds(1), context.Coordinator.TotalEnergy.CountedDuration);
        Assert.Equal(TimeSpan.FromSeconds(1), context.Coordinator.CurrentActivity.UnknownDuration);
    }

    [Fact]
    public void ActiveAndIdleSamplesAreDescriptiveAndEnergyRemainsElapsedTime()
    {
        var context = CreateContext();
        context.Tracker.Enqueue(ActivitySample.Active("code", TimeSpan.FromSeconds(1)));
        context.Tracker.Enqueue(ActivitySample.Idle("code", TimeSpan.FromSeconds(1)));
        var session = context.Coordinator.StartFocus(SessionId(), TimeSpan.FromSeconds(2));

        context.Advance(TimeSpan.FromSeconds(1));
        context.Coordinator.Tick();
        context.Advance(TimeSpan.FromSeconds(1));
        context.Coordinator.Tick();

        var activity = context.Coordinator.CurrentActivity;
        Assert.Equal(TimeSpan.FromSeconds(1), activity.ActiveDuration);
        Assert.Equal(TimeSpan.FromSeconds(1), activity.IdleDuration);
        Assert.Equal(TimeSpan.FromSeconds(1), session.IdleDuration);
        Assert.Equal(TimeSpan.FromSeconds(2), context.Coordinator.TotalEnergy.CountedDuration);
        var process = Assert.Single(activity.Processes);
        Assert.Equal("code", process.ProcessName);
        Assert.Equal(TimeSpan.FromSeconds(2), process.Duration);
    }

    [Fact]
    public void EarlyIdleSampleUsesActualMonotonicElapsedWithoutThrowing()
    {
        var context = CreateContext();
        context.Tracker.Enqueue(ActivitySample.Idle("code", TimeSpan.FromSeconds(1)));
        var session = context.Coordinator.StartFocus(SessionId(), TimeSpan.FromSeconds(2));

        context.Advance(TimeSpan.FromMilliseconds(250));
        var tick = context.Coordinator.Tick();

        Assert.Equal(TimeSpan.FromMilliseconds(250), tick.AppliedElapsed);
        Assert.Equal(TimeSpan.FromMilliseconds(250), tick.AppliedIdle);
        Assert.Equal(session.CountedDuration, session.IdleDuration);
    }

    [Fact]
    public void DelayedIdleTickPreservesElapsedAndDoesNotInventProcessObservation()
    {
        var context = CreateContext();
        context.Tracker.Enqueue(ActivitySample.Idle("code", TimeSpan.FromSeconds(1)));
        var session = context.Coordinator.StartFocus(SessionId(), TimeSpan.FromSeconds(10));

        context.Advance(TimeSpan.FromSeconds(5));
        context.Coordinator.Tick();

        Assert.Equal(TimeSpan.FromSeconds(5), session.CountedDuration);
        Assert.Equal(TimeSpan.FromSeconds(5), session.IdleDuration);
        Assert.Equal(TimeSpan.FromSeconds(5), context.Coordinator.CurrentActivity.IdleDuration);
        Assert.Equal(TimeSpan.FromSeconds(1), context.Coordinator.CurrentActivity.Processes.Single(
            process => process.ProcessName == "code").Duration);
        Assert.Equal(TimeSpan.FromSeconds(4), context.Coordinator.CurrentActivity.Processes.Single(
            process => process.ProcessName == ActivitySample.UnknownProcessName).Duration);
    }

    [Fact]
    public void SuspendedTicksDoNotSampleOrAdvance()
    {
        var context = CreateContext();
        context.Tracker.Enqueue(ActivitySample.Active("code", TimeSpan.FromSeconds(1)));
        var session = context.Coordinator.StartFocus(SessionId(), TimeSpan.FromMinutes(5));

        context.Advance(TimeSpan.FromSeconds(1));
        context.Coordinator.Suspend();
        var samplesAtSuspend = context.Tracker.SampleCount;
        context.Advance(TimeSpan.FromMinutes(10));
        var result = context.Coordinator.Tick();

        Assert.Equal(FocusSessionTickResult.Suspended, result);
        Assert.Equal(samplesAtSuspend, context.Tracker.SampleCount);
        Assert.Equal(TimeSpan.FromSeconds(1), session.CountedDuration);
    }

    [Fact]
    public void SuspendRecordsOnlyTheAppliedIdleInterval()
    {
        var context = CreateContext();
        context.Tracker.Enqueue(ActivitySample.Idle("code", TimeSpan.FromSeconds(1)));
        var session = context.Coordinator.StartFocus(SessionId(), TimeSpan.FromMinutes(5));

        context.Advance(TimeSpan.FromMilliseconds(500));
        var status = context.Coordinator.Suspend();

        Assert.Equal(FocusSessionStatus.Suspended, status);
        Assert.Equal(TimeSpan.FromMilliseconds(500), session.IdleDuration);
        Assert.Equal(TimeSpan.FromMilliseconds(500), context.Coordinator.CurrentActivity.IdleDuration);
    }

    [Fact]
    public void ResumeStartsAnewRunningBoundaryWithoutSampling()
    {
        var context = CreateContext();
        context.Tracker.Enqueue(ActivitySample.Active("code", TimeSpan.FromSeconds(1)));
        var session = context.Coordinator.StartFocus(SessionId(), TimeSpan.FromMinutes(5));
        context.Advance(TimeSpan.FromSeconds(1));
        context.Coordinator.Suspend();
        var samplesAtSuspend = context.Tracker.SampleCount;

        context.Advance(TimeSpan.FromMinutes(10));
        context.Coordinator.Resume();
        context.Advance(TimeSpan.FromSeconds(1));
        context.Coordinator.Tick();

        Assert.Equal(samplesAtSuspend + 1, context.Tracker.SampleCount);
        Assert.Equal(TimeSpan.FromSeconds(2), session.CountedDuration);
    }

    [Fact]
    public void EarlyStopRecordsOnceAndCarriesPartialEnergy()
    {
        var context = CreateContext();
        context.Tracker.Enqueue(ActivitySample.Active("code", TimeSpan.FromSeconds(1)));
        var session = context.Coordinator.StartFocus(SessionId(), TimeSpan.FromMinutes(25));
        context.Advance(TimeSpan.FromSeconds(1));

        var finalized = context.Coordinator.Stop();

        Assert.True(finalized.NewlyRecorded);
        Assert.Equal(FocusSessionStatus.Stopped, session.Status);
        Assert.Equal(TimeSpan.FromSeconds(1), finalized.TotalEnergy.CountedDuration);
        Assert.Single(context.Finalized);
        Assert.Equal(finalized, context.Finalized[0]);
    }

    [Fact]
    public void AutomaticCompletionPublishesTerminalCheckpointAfterLedgerUpdate()
    {
        var context = CreateContext();
        context.Tracker.Enqueue(ActivitySample.Active("code", TimeSpan.FromSeconds(1)));
        context.Coordinator.StartFocus(SessionId(), TimeSpan.FromSeconds(1));
        context.Advance(TimeSpan.FromSeconds(1));

        context.Coordinator.Tick();

        var terminal = Assert.Single(context.Finalized);
        Assert.Equal(FocusSessionStatus.Completed, terminal.Checkpoint.Status);
        Assert.Equal(TimeSpan.FromSeconds(1), terminal.Checkpoint.TotalRecordedEnergy.CountedDuration);
        Assert.Equal(terminal.Checkpoint, context.Checkpoints[^1]);
    }

    [Fact]
    public void StartingANewSessionResetsOnlyPerSessionActivity()
    {
        var context = CreateContext();
        context.Tracker.Enqueue(ActivitySample.Active("code", TimeSpan.FromSeconds(1)));
        context.Coordinator.StartFocus(SessionId(), TimeSpan.FromSeconds(1));
        context.Advance(TimeSpan.FromSeconds(1));
        context.Coordinator.Tick();

        context.Coordinator.StartFocus(
            FocusSessionId.From(Guid.Parse("69e4181d-574d-4d17-b46a-1daf2777debb")),
            TimeSpan.FromMinutes(1));

        var activity = context.Coordinator.CurrentActivity;
        Assert.Equal(TimeSpan.Zero, activity.ActiveDuration);
        Assert.Equal(TimeSpan.Zero, activity.IdleDuration);
        Assert.Equal(TimeSpan.Zero, activity.UnknownDuration);
        Assert.Empty(activity.Processes);
        Assert.Equal(TimeSpan.FromSeconds(1), context.Coordinator.TotalEnergy.CountedDuration);
    }

    [Fact]
    public void CoordinatorPublicSurfaceHasNoDisplayModeDependency()
    {
        var publicSurface = typeof(FocusSessionCoordinator)
            .GetMembers()
            .Select(member => member.ToString() ?? string.Empty);

        Assert.DoesNotContain(
            publicSurface,
            signature => signature.Contains("DisplayMode", StringComparison.Ordinal));
    }

    public static TheoryData<Exception> RecoverableTrackerFailures => new()
    {
        new InvalidOperationException(),
        new UnauthorizedAccessException(),
        new PlatformNotSupportedException()
    };

    private static TestContext CreateContext()
    {
        var wallClock = new FakeWallClock(StartTime);
        var monotonicClock = new FakeMonotonicClock();
        var manager = new FocusSessionManager(wallClock, monotonicClock);
        var tracker = new FakeActivityTracker();
        var ledger = new SessionLedger();
        var coordinator = new FocusSessionCoordinator(
            manager,
            tracker,
            ledger,
            new ElapsedTimeEnergyPolicy());
        var checkpoints = new List<FocusSessionCheckpoint>();
        var finalized = new List<FocusSessionFinalized>();
        coordinator.CheckpointAvailable += checkpoints.Add;
        coordinator.SessionFinalized += finalized.Add;

        return new TestContext(
            coordinator,
            tracker,
            wallClock,
            monotonicClock,
            checkpoints,
            finalized);
    }

    private static FocusSessionId SessionId() =>
        FocusSessionId.From(Guid.Parse("f1bed2ee-9a47-4b3a-9f68-7e58e7a4dbe1"));

    private sealed record TestContext(
        FocusSessionCoordinator Coordinator,
        FakeActivityTracker Tracker,
        FakeWallClock WallClock,
        FakeMonotonicClock MonotonicClock,
        List<FocusSessionCheckpoint> Checkpoints,
        List<FocusSessionFinalized> Finalized)
    {
        public void Advance(TimeSpan duration)
        {
            WallClock.Advance(duration);
            MonotonicClock.Advance(duration);
        }
    }
}
