using DeskTown.Domain.Focus;

namespace DeskTown.Domain.Tests;

public sealed class FocusSessionTests
{
    [Fact]
    public void Restored_session_is_suspended_at_exact_checkpoint_and_rejects_invalid_durations()
    {
        var started = new DateTimeOffset(2026, 9, 25, 8, 0, 0, TimeSpan.Zero);
        var id = FocusSessionId.From(Guid.Parse("239b80a4-b9e5-4e87-8356-188eb2a7e5a0"));
        var session = FocusSession.RestoreSuspended(id, started,
            TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(7), TimeSpan.FromMinutes(2), ["code.exe"]);

        Assert.Equal(FocusSessionStatus.Suspended, session.Status);
        Assert.Equal(TimeSpan.FromMinutes(7), session.CountedDuration);
        Assert.Equal(TimeSpan.FromMinutes(2), session.IdleDuration);
        Assert.Equal(started, session.StartedAtUtc);
        Assert.Throws<ArgumentOutOfRangeException>(() => FocusSession.RestoreSuspended(
            id, started, TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(26), TimeSpan.Zero));
    }
    private static readonly DateTimeOffset StartTime =
        new(2026, 9, 25, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_builds_a_ready_display_independent_session()
    {
        var session = CreateSession([" code.exe ", "CODE.EXE", "notion.exe", " "]);

        Assert.Equal(FocusSessionStatus.Ready, session.Status);
        Assert.Null(session.StartedAtUtc);
        Assert.Null(session.EndedAtUtc);
        Assert.Equal(TimeSpan.Zero, session.CountedDuration);
        Assert.Equal(TimeSpan.Zero, session.IdleDuration);
        Assert.Equal(TimeSpan.FromMinutes(25), session.RemainingDuration);
        Assert.Equal(2, session.IntendedProcessNames.Count);
        Assert.Contains("code.exe", session.IntendedProcessNames);
        Assert.Contains("notion.exe", session.IntendedProcessNames);
        Assert.DoesNotContain(
            typeof(FocusSession).GetProperties(),
            property => property.Name.Contains("DisplayMode", StringComparison.Ordinal));
    }

    [Fact]
    public void Create_rejects_an_empty_id()
    {
        Assert.Throws<ArgumentException>(() =>
            FocusSession.Create(default, TimeSpan.FromMinutes(25)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_a_non_positive_target(int targetMinutes)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FocusSession.Create(SessionId(), TimeSpan.FromMinutes(targetMinutes)));
    }

    [Fact]
    public void Start_moves_ready_session_to_running()
    {
        var session = CreateSession();

        session.Start(StartTime);

        Assert.Equal(FocusSessionStatus.Running, session.Status);
        Assert.Equal(StartTime, session.StartedAtUtc);
    }

    [Fact]
    public void Start_rejects_a_second_start()
    {
        var session = RunningSession();

        var exception = Assert.Throws<FocusSessionTransitionException>(() =>
            session.Start(StartTime.AddMinutes(1)));

        Assert.Equal(FocusSessionStatus.Running, exception.CurrentStatus);
        Assert.Equal("start", exception.Operation);
    }

    [Fact]
    public void Start_requires_a_utc_timestamp()
    {
        var session = CreateSession();
        var localOffset = StartTime.ToOffset(TimeSpan.FromHours(9));

        Assert.Throws<ArgumentException>(() => session.Start(localOffset));
    }

    [Fact]
    public void Accumulate_records_elapsed_and_idle_without_penalty()
    {
        var session = RunningSession();

        session.Accumulate(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(2));

        Assert.Equal(TimeSpan.FromMinutes(5), session.CountedDuration);
        Assert.Equal(TimeSpan.FromMinutes(2), session.IdleDuration);
        Assert.Equal(TimeSpan.FromMinutes(20), session.RemainingDuration);
    }

    [Fact]
    public void Accumulate_clamps_elapsed_and_idle_at_the_target()
    {
        var session = RunningSession();
        session.Accumulate(TimeSpan.FromMinutes(24), TimeSpan.FromMinutes(1));

        session.Accumulate(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(3));

        Assert.Equal(TimeSpan.FromMinutes(25), session.CountedDuration);
        Assert.Equal(TimeSpan.FromMinutes(2), session.IdleDuration);
        Assert.Equal(TimeSpan.Zero, session.RemainingDuration);
        Assert.True(session.IsTargetReached);
    }

    [Fact]
    public void Accumulate_rejects_idle_greater_than_elapsed()
    {
        var session = RunningSession();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            session.Accumulate(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(11)));
    }

    [Fact]
    public void Suspended_session_does_not_accept_elapsed_time()
    {
        var session = RunningSession();
        session.Suspend();

        Assert.Throws<FocusSessionTransitionException>(() =>
            session.Accumulate(TimeSpan.FromMinutes(1), TimeSpan.Zero));
        Assert.Equal(TimeSpan.Zero, session.CountedDuration);
    }

    [Fact]
    public void Suspend_and_resume_preserve_totals()
    {
        var session = RunningSession();
        session.Accumulate(TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(1));

        session.Suspend();
        Assert.Equal(FocusSessionStatus.Suspended, session.Status);

        session.Resume();
        Assert.Equal(FocusSessionStatus.Running, session.Status);
        Assert.Equal(TimeSpan.FromMinutes(4), session.CountedDuration);
        Assert.Equal(TimeSpan.FromMinutes(1), session.IdleDuration);
    }

    [Fact]
    public void Complete_rejects_a_session_below_target()
    {
        var session = RunningSession();
        session.Accumulate(TimeSpan.FromMinutes(24), TimeSpan.Zero);

        Assert.Throws<FocusSessionTransitionException>(() =>
            session.Complete(StartTime.AddMinutes(24)));
    }

    [Fact]
    public void Complete_ends_a_running_session_at_target()
    {
        var session = RunningSession();
        session.Accumulate(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(10));
        var endTime = StartTime.AddMinutes(25);

        session.Complete(endTime);

        Assert.Equal(FocusSessionStatus.Completed, session.Status);
        Assert.Equal(endTime, session.EndedAtUtc);
        Assert.True(session.IsTerminal);
        Assert.Equal(TimeSpan.FromMinutes(10), session.IdleDuration);
    }

    [Fact]
    public void Stop_ends_a_running_session_early()
    {
        var session = RunningSession();
        session.Accumulate(TimeSpan.FromMinutes(7), TimeSpan.FromMinutes(2));
        var endTime = StartTime.AddMinutes(8);

        session.Stop(endTime);

        Assert.Equal(FocusSessionStatus.Stopped, session.Status);
        Assert.Equal(endTime, session.EndedAtUtc);
        Assert.Equal(TimeSpan.FromMinutes(7), session.CountedDuration);
        Assert.True(session.IsTerminal);
    }

    [Fact]
    public void Stop_is_valid_while_suspended()
    {
        var session = RunningSession();
        session.Suspend();

        session.Stop(StartTime.AddMinutes(1));

        Assert.Equal(FocusSessionStatus.Stopped, session.Status);
    }

    [Fact]
    public void End_rejects_a_timestamp_before_start()
    {
        var session = RunningSession();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            session.Stop(StartTime.AddSeconds(-1)));
    }

    [Fact]
    public void Terminal_session_rejects_further_transitions()
    {
        var session = RunningSession();
        session.Stop(StartTime.AddMinutes(1));

        Assert.Throws<FocusSessionTransitionException>(session.Suspend);
        Assert.Throws<FocusSessionTransitionException>(session.Resume);
        Assert.Throws<FocusSessionTransitionException>(() =>
            session.Stop(StartTime.AddMinutes(2)));
    }

    [Fact]
    public void Invalid_status_transition_matrix_is_rejected()
    {
        var allowed = new HashSet<(FocusSessionStatus Status, TestOperation Operation)>
        {
            (FocusSessionStatus.Ready, TestOperation.Start),
            (FocusSessionStatus.Running, TestOperation.Accumulate),
            (FocusSessionStatus.Running, TestOperation.Suspend),
            (FocusSessionStatus.Running, TestOperation.Complete),
            (FocusSessionStatus.Running, TestOperation.Stop),
            (FocusSessionStatus.Suspended, TestOperation.Resume),
            (FocusSessionStatus.Suspended, TestOperation.Stop)
        };

        foreach (var status in Enum.GetValues<FocusSessionStatus>())
        {
            foreach (var operation in Enum.GetValues<TestOperation>())
            {
                if (allowed.Contains((status, operation)))
                {
                    continue;
                }

                var session = SessionIn(status);
                var exception = Assert.Throws<FocusSessionTransitionException>(() =>
                    Apply(operation, session));

                Assert.Equal(status, exception.CurrentStatus);
            }
        }
    }

    [Fact]
    public void Wall_clock_elapsed_uses_as_of_time_until_session_ends()
    {
        var session = RunningSession();

        Assert.Equal(
            TimeSpan.FromMinutes(12),
            session.CalculateWallClockElapsed(StartTime.AddMinutes(12)));

        session.Stop(StartTime.AddMinutes(15));

        Assert.Equal(
            TimeSpan.FromMinutes(15),
            session.CalculateWallClockElapsed(StartTime.AddHours(1)));
    }

    [Fact]
    public void Wall_clock_elapsed_is_zero_before_start()
    {
        var session = CreateSession();

        Assert.Equal(TimeSpan.Zero, session.CalculateWallClockElapsed(StartTime));
    }

    private static FocusSession RunningSession()
    {
        var session = CreateSession();
        session.Start(StartTime);
        return session;
    }

    private static FocusSession SessionIn(FocusSessionStatus status)
    {
        var session = CreateSession();
        if (status == FocusSessionStatus.Ready)
        {
            return session;
        }

        session.Start(StartTime);
        if (status == FocusSessionStatus.Running)
        {
            return session;
        }

        if (status == FocusSessionStatus.Suspended)
        {
            session.Suspend();
            return session;
        }

        if (status == FocusSessionStatus.Completed)
        {
            session.Accumulate(TimeSpan.FromMinutes(25), TimeSpan.Zero);
            session.Complete(StartTime.AddMinutes(25));
            return session;
        }

        session.Stop(StartTime.AddMinutes(1));
        return session;
    }

    private static void Apply(TestOperation operation, FocusSession session)
    {
        switch (operation)
        {
            case TestOperation.Start:
                session.Start(StartTime.AddMinutes(30));
                break;
            case TestOperation.Accumulate:
                session.Accumulate(TimeSpan.FromSeconds(1), TimeSpan.Zero);
                break;
            case TestOperation.Suspend:
                session.Suspend();
                break;
            case TestOperation.Resume:
                session.Resume();
                break;
            case TestOperation.Complete:
                session.Complete(StartTime.AddMinutes(30));
                break;
            case TestOperation.Stop:
                session.Stop(StartTime.AddMinutes(30));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
        }
    }

    private static FocusSession CreateSession(IEnumerable<string>? intendedProcesses = null)
    {
        return FocusSession.Create(
            SessionId(),
            TimeSpan.FromMinutes(25),
            intendedProcesses);
    }

    private static FocusSessionId SessionId()
    {
        return FocusSessionId.From(Guid.Parse("6d73db89-805f-4ddc-a895-da225c1068d2"));
    }

    private enum TestOperation
    {
        Start,
        Accumulate,
        Suspend,
        Resume,
        Complete,
        Stop
    }
}
