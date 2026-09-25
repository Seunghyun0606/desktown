using DeskTown.Application.Sessions;
using DeskTown.Application.Tests.Fakes;
using DeskTown.Domain.Focus;

namespace DeskTown.Application.Tests;

public sealed class FocusSessionManagerTests
{
    private static readonly DateTimeOffset StartTime =
        new(2026, 9, 25, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Start_uses_wall_time_and_publishes_a_typed_event()
    {
        var context = CreateContext();
        var session = CreateSession();

        context.Manager.Start(session);

        Assert.Same(session, context.Manager.ActiveSession);
        Assert.Equal(StartTime, session.StartedAtUtc);
        var started = Assert.IsType<FocusSessionStarted>(Assert.Single(context.Events));
        Assert.Equal(session.Id, started.SessionId);
        Assert.Equal(StartTime, started.OccurredAtUtc);
    }

    [Fact]
    public void Start_rejects_a_second_active_session()
    {
        var context = CreateContext();
        context.Manager.Start(CreateSession());

        Assert.Throws<FocusSessionManagerException>(() =>
            context.Manager.Start(CreateSession("69e4181d-574d-4d17-b46a-1daf2777debb")));
    }

    [Fact]
    public void Twenty_five_fake_minutes_complete_exactly()
    {
        var context = CreateContext();
        var session = CreateSession();
        context.Manager.Start(session);

        context.Advance(TimeSpan.FromMinutes(25));
        var result = context.Manager.Tick(TimeSpan.FromMinutes(8));

        Assert.True(result.Completed);
        Assert.Equal(TimeSpan.FromMinutes(25), result.AppliedElapsed);
        Assert.Equal(TimeSpan.FromMinutes(8), result.AppliedIdle);
        Assert.Equal(TimeSpan.FromMinutes(25), session.CountedDuration);
        Assert.Equal(TimeSpan.FromMinutes(8), session.IdleDuration);
        Assert.Equal(FocusSessionStatus.Completed, session.Status);
        Assert.Null(context.Manager.ActiveSession);
        Assert.IsType<FocusSessionCompleted>(context.Events[^1]);
    }

    [Fact]
    public void Elapsed_time_is_clamped_when_a_tick_crosses_the_target()
    {
        var context = CreateContext();
        var session = CreateSession();
        context.Manager.Start(session);

        context.Advance(TimeSpan.FromMinutes(30));
        var result = context.Manager.Tick(TimeSpan.FromMinutes(10));

        Assert.Equal(TimeSpan.FromMinutes(25), result.AppliedElapsed);
        Assert.Equal(TimeSpan.FromMinutes(10), result.AppliedIdle);
        Assert.Equal(TimeSpan.FromMinutes(25), session.CountedDuration);
    }

    [Fact]
    public void Suspended_wall_and_monotonic_time_are_excluded()
    {
        var context = CreateContext();
        var session = CreateSession();
        context.Manager.Start(session);

        context.Advance(TimeSpan.FromMinutes(5));
        context.Manager.Suspend(TimeSpan.FromMinutes(1));

        context.Advance(TimeSpan.FromHours(1));
        var suspendedTick = context.Manager.Tick(TimeSpan.Zero);
        context.Manager.Resume();

        context.Advance(TimeSpan.FromMinutes(5));
        context.Manager.Tick(TimeSpan.FromMinutes(2));

        Assert.Equal(FocusSessionTickResult.Suspended, suspendedTick);
        Assert.Equal(TimeSpan.FromMinutes(10), session.CountedDuration);
        Assert.Equal(TimeSpan.FromMinutes(3), session.IdleDuration);
        Assert.Equal(FocusSessionStatus.Running, session.Status);
    }

    [Fact]
    public void Suspend_flushes_only_time_before_the_suspend_boundary()
    {
        var context = CreateContext();
        var session = CreateSession();
        context.Manager.Start(session);

        context.Advance(TimeSpan.FromSeconds(45));
        var status = context.Manager.Suspend(TimeSpan.FromSeconds(10));

        Assert.Equal(FocusSessionStatus.Suspended, status);
        Assert.Equal(TimeSpan.FromSeconds(45), session.CountedDuration);
        Assert.Equal(TimeSpan.FromSeconds(10), session.IdleDuration);
        Assert.IsType<FocusSessionSuspended>(context.Events[^1]);
    }

    [Fact]
    public void Suspend_completes_instead_when_boundary_reaches_target()
    {
        var context = CreateContext();
        var session = CreateSession();
        context.Manager.Start(session);

        context.Advance(TimeSpan.FromMinutes(25));
        var status = context.Manager.Suspend(TimeSpan.Zero);

        Assert.Equal(FocusSessionStatus.Completed, status);
        Assert.Equal(FocusSessionStatus.Completed, session.Status);
        Assert.IsType<FocusSessionCompleted>(context.Events[^1]);
        Assert.DoesNotContain(context.Events, lifecycleEvent =>
            lifecycleEvent is FocusSessionSuspended);
    }

    [Fact]
    public void Stop_flushes_running_time_and_ends_early()
    {
        var context = CreateContext();
        var session = CreateSession();
        context.Manager.Start(session);

        context.Advance(TimeSpan.FromMinutes(7));
        context.Manager.Stop(TimeSpan.FromMinutes(2));

        Assert.Equal(FocusSessionStatus.Stopped, session.Status);
        Assert.Equal(TimeSpan.FromMinutes(7), session.CountedDuration);
        Assert.Equal(TimeSpan.FromMinutes(2), session.IdleDuration);
        Assert.Null(context.Manager.ActiveSession);
        Assert.IsType<FocusSessionStopped>(context.Events[^1]);
    }

    [Fact]
    public void Stop_while_suspended_adds_no_suspended_time()
    {
        var context = CreateContext();
        var session = CreateSession();
        context.Manager.Start(session);
        context.Advance(TimeSpan.FromMinutes(3));
        context.Manager.Suspend(TimeSpan.Zero);

        context.Advance(TimeSpan.FromHours(2));
        context.Manager.Stop(TimeSpan.Zero);

        Assert.Equal(TimeSpan.FromMinutes(3), session.CountedDuration);
        Assert.Equal(FocusSessionStatus.Stopped, session.Status);
    }

    [Fact]
    public void A_new_session_can_start_after_a_terminal_session()
    {
        var context = CreateContext();
        var first = CreateSession();
        context.Manager.Start(first);
        context.Advance(TimeSpan.FromMinutes(1));
        context.Manager.Stop(TimeSpan.Zero);

        var second = CreateSession("69e4181d-574d-4d17-b46a-1daf2777debb");
        context.Manager.Start(second);

        Assert.Same(second, context.Manager.ActiveSession);
        Assert.Same(second, context.Manager.CurrentSession);
    }

    [Fact]
    public void Tick_rejects_a_backwards_monotonic_clock()
    {
        var context = CreateContext();
        context.MonotonicClock.Set(TimeSpan.FromMinutes(5));
        context.Manager.Start(CreateSession());
        context.MonotonicClock.Set(TimeSpan.FromMinutes(4));

        Assert.Throws<FocusSessionManagerException>(() =>
            context.Manager.Tick(TimeSpan.Zero));
    }

    [Fact]
    public void Lifecycle_events_preserve_order_and_counted_totals()
    {
        var context = CreateContext();
        var session = CreateSession();
        context.Manager.Start(session);
        context.Advance(TimeSpan.FromMinutes(4));
        context.Manager.Suspend(TimeSpan.FromMinutes(1));
        context.Advance(TimeSpan.FromMinutes(10));
        context.Manager.Resume();
        context.Advance(TimeSpan.FromMinutes(2));
        context.Manager.Stop(TimeSpan.Zero);

        Assert.Collection(
            context.Events,
            started => Assert.IsType<FocusSessionStarted>(started),
            suspended =>
            {
                var typed = Assert.IsType<FocusSessionSuspended>(suspended);
                Assert.Equal(TimeSpan.FromMinutes(4), typed.CountedDuration);
            },
            resumed =>
            {
                var typed = Assert.IsType<FocusSessionResumed>(resumed);
                Assert.Equal(TimeSpan.FromMinutes(4), typed.CountedDuration);
            },
            stopped =>
            {
                var typed = Assert.IsType<FocusSessionStopped>(stopped);
                Assert.Equal(TimeSpan.FromMinutes(6), typed.CountedDuration);
            });
    }

    [Fact]
    public void Manager_has_no_display_mode_dependency()
    {
        var publicSurface = typeof(FocusSessionManager)
            .GetMembers()
            .Select(member => member.ToString() ?? string.Empty);

        Assert.DoesNotContain(
            publicSurface,
            signature => signature.Contains("DisplayMode", StringComparison.Ordinal));
    }

    [Fact]
    public void Commands_require_an_active_session()
    {
        var context = CreateContext();

        Assert.Throws<FocusSessionManagerException>(() =>
            context.Manager.Tick(TimeSpan.Zero));
        Assert.Throws<FocusSessionManagerException>(() =>
            context.Manager.Suspend(TimeSpan.Zero));
        Assert.Throws<FocusSessionManagerException>(() => context.Manager.Resume());
        Assert.Throws<FocusSessionManagerException>(() =>
            context.Manager.Stop(TimeSpan.Zero));
    }

    [Fact]
    public void Wall_clock_must_report_utc()
    {
        var context = CreateContext();
        context.WallClock.Set(StartTime.ToOffset(TimeSpan.FromHours(9)));

        Assert.Throws<FocusSessionManagerException>(() =>
            context.Manager.Start(CreateSession()));
    }

    private static TestContext CreateContext()
    {
        var wallClock = new FakeWallClock(StartTime);
        var monotonicClock = new FakeMonotonicClock();
        var manager = new FocusSessionManager(wallClock, monotonicClock);
        var events = new List<FocusSessionLifecycleEvent>();
        manager.LifecycleChanged += events.Add;
        return new TestContext(manager, wallClock, monotonicClock, events);
    }

    private static FocusSession CreateSession(
        string id = "f1bed2ee-9a47-4b3a-9f68-7e58e7a4dbe1")
    {
        return FocusSession.Create(
            FocusSessionId.From(Guid.Parse(id)),
            TimeSpan.FromMinutes(25));
    }

    private sealed record TestContext(
        FocusSessionManager Manager,
        FakeWallClock WallClock,
        FakeMonotonicClock MonotonicClock,
        List<FocusSessionLifecycleEvent> Events)
    {
        public void Advance(TimeSpan duration)
        {
            WallClock.Advance(duration);
            MonotonicClock.Advance(duration);
        }
    }
}
