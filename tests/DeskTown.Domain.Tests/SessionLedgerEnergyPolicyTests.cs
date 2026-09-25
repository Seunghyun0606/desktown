using DeskTown.Domain.Focus;

namespace DeskTown.Domain.Tests;

public sealed class SessionLedgerEnergyPolicyTests
{
    private static readonly DateTimeOffset StartTime =
        new(2026, 9, 25, 9, 0, 0, TimeSpan.Zero);

    private readonly ElapsedTimeEnergyPolicy _policy = new();

    [Fact]
    public void Empty_ledger_has_zero_energy()
    {
        var ledger = new SessionLedger();

        Assert.Equal(FocusEnergy.Zero, _policy.CalculateTotal(ledger));
    }

    [Fact]
    public void Policy_rejects_a_null_ledger()
    {
        Assert.Throws<ArgumentNullException>(() => _policy.CalculateTotal(null!));
    }

    [Fact]
    public void Ledger_rejects_a_non_terminal_session()
    {
        var ledger = new SessionLedger();
        var session = CreateSession(Guid.NewGuid());
        session.Start(StartTime);

        Assert.Throws<ArgumentException>(() => ledger.TryRecord(session));
        Assert.Empty(ledger.Entries);
    }

    [Fact]
    public void Ledger_rejects_a_null_session()
    {
        var ledger = new SessionLedger();

        Assert.Throws<ArgumentNullException>(() => ledger.TryRecord(null!));
    }

    [Fact]
    public void Stopped_session_retains_raw_counted_and_idle_time()
    {
        var ledger = new SessionLedger();
        var session = FinalizedSession(
            Guid.NewGuid(),
            TimeSpan.FromSeconds(90),
            TimeSpan.FromSeconds(45));

        var added = ledger.TryRecord(session);
        var entry = Assert.Single(ledger.Entries);

        Assert.True(added);
        Assert.Equal(TimeSpan.FromSeconds(90), ledger.TotalCountedDuration);
        Assert.Equal(TimeSpan.FromSeconds(45), ledger.TotalIdleDuration);
        Assert.Equal(90, ledger.TotalCountedSeconds);
        Assert.Equal(45, ledger.TotalIdleSeconds);
        Assert.Equal(TimeSpan.FromSeconds(90), entry.CountedDuration);
        Assert.Equal(FocusSessionStatus.Stopped, entry.Status);
    }

    [Fact]
    public void Idle_time_does_not_reduce_energy()
    {
        var activeLedger = LedgerWith(
            FinalizedSession(Guid.NewGuid(), TimeSpan.FromMinutes(5), TimeSpan.Zero));
        var idleLedger = LedgerWith(
            FinalizedSession(Guid.NewGuid(), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5)));

        Assert.Equal(_policy.CalculateTotal(activeLedger), _policy.CalculateTotal(idleLedger));
        Assert.Equal(5, _policy.CalculateTotal(idleLedger).DisplayedWholeMinutes);
    }

    [Fact]
    public void Intended_process_names_do_not_change_energy()
    {
        var anyApp = LedgerWith(
            FinalizedSession(Guid.NewGuid(), TimeSpan.FromMinutes(5), TimeSpan.Zero));
        var selectedApps = LedgerWith(
            FinalizedSession(
                Guid.NewGuid(),
                TimeSpan.FromMinutes(5),
                TimeSpan.Zero,
                ["code.exe", "notion.exe"]));

        Assert.Equal(_policy.CalculateTotal(anyApp), _policy.CalculateTotal(selectedApps));
    }

    [Fact]
    public void Display_mode_is_absent_from_energy_inputs()
    {
        var domainTypes = new[]
        {
            typeof(FocusEnergy),
            typeof(SessionLedger),
            typeof(SessionLedgerEntry),
            typeof(IEnergyPolicy),
            typeof(ElapsedTimeEnergyPolicy)
        };

        Assert.All(
            domainTypes.SelectMany(type => type.GetMembers()),
            member => Assert.DoesNotContain("DisplayMode", member.Name, StringComparison.Ordinal));
    }

    [Fact]
    public void Multiple_short_sessions_do_not_accumulate_rounding_drift()
    {
        var ledger = new SessionLedger();
        ledger.TryRecord(FinalizedSession(
            Guid.NewGuid(),
            TimeSpan.FromMilliseconds(30_500),
            TimeSpan.Zero));
        ledger.TryRecord(FinalizedSession(
            Guid.NewGuid(),
            TimeSpan.FromMilliseconds(30_500),
            TimeSpan.Zero));

        var energy = _policy.CalculateTotal(ledger);

        Assert.Equal(TimeSpan.FromSeconds(61), ledger.TotalCountedDuration);
        Assert.Equal(61, energy.CountedSeconds);
        Assert.Equal(1, energy.DisplayedWholeMinutes);
    }

    [Fact]
    public void Same_session_snapshot_is_applied_exactly_once()
    {
        var ledger = new SessionLedger();
        var session = FinalizedSession(
            Guid.NewGuid(),
            TimeSpan.FromMinutes(2),
            TimeSpan.FromSeconds(30));

        Assert.True(ledger.TryRecord(session));
        Assert.False(ledger.TryRecord(session));

        Assert.Single(ledger.Entries);
        Assert.Equal(120, _policy.CalculateTotal(ledger).CountedSeconds);
    }

    [Fact]
    public void Reconstructed_matching_session_snapshot_is_applied_exactly_once()
    {
        var ledger = new SessionLedger();
        var id = Guid.NewGuid();
        var first = FinalizedSession(
            id,
            TimeSpan.FromMinutes(2),
            TimeSpan.FromSeconds(30),
            ["code.exe"]);
        var replay = FinalizedSession(
            id,
            TimeSpan.FromMinutes(2),
            TimeSpan.FromSeconds(30),
            ["CODE.EXE"]);

        Assert.True(ledger.TryRecord(first));
        Assert.False(ledger.TryRecord(replay));
        Assert.Single(ledger.Entries);
    }

    [Fact]
    public void Reused_session_id_with_different_data_is_rejected()
    {
        var ledger = new SessionLedger();
        var id = Guid.NewGuid();
        ledger.TryRecord(FinalizedSession(
            id,
            TimeSpan.FromMinutes(1),
            TimeSpan.Zero));

        var exception = Assert.Throws<SessionLedgerConflictException>(() =>
            ledger.TryRecord(FinalizedSession(
                id,
                TimeSpan.FromMinutes(2),
                TimeSpan.Zero)));

        Assert.Equal(FocusSessionId.From(id), exception.SessionId);
        Assert.Single(ledger.Entries);
        Assert.Equal(60, ledger.TotalCountedSeconds);
    }

    [Fact]
    public void Completed_and_early_stopped_time_follow_the_same_policy()
    {
        var stoppedLedger = LedgerWith(
            FinalizedSession(Guid.NewGuid(), TimeSpan.FromMinutes(25), TimeSpan.Zero));
        var completedLedger = LedgerWith(
            FinalizedSession(
                Guid.NewGuid(),
                TimeSpan.FromMinutes(25),
                TimeSpan.Zero,
                complete: true));

        Assert.Equal(_policy.CalculateTotal(stoppedLedger), _policy.CalculateTotal(completedLedger));
    }

    [Fact]
    public void Overflow_does_not_partially_mutate_the_ledger()
    {
        var ledger = new SessionLedger();
        ledger.TryRecord(MaxDurationSession(Guid.NewGuid()));

        Assert.Throws<OverflowException>(() =>
            ledger.TryRecord(MaxDurationSession(Guid.NewGuid())));

        Assert.Single(ledger.Entries);
        Assert.Equal(TimeSpan.MaxValue, ledger.TotalCountedDuration);
        Assert.Equal(TimeSpan.Zero, ledger.TotalIdleDuration);
    }

    [Fact]
    public void Entry_is_an_immutable_snapshot_of_session_metadata()
    {
        var names = new[] { " code.exe ", "CODE.EXE", "notion.exe" };
        var ledger = LedgerWith(
            FinalizedSession(
                Guid.NewGuid(),
                TimeSpan.FromMinutes(1),
                TimeSpan.Zero,
                names));

        var entry = Assert.Single(ledger.Entries);

        Assert.Equal(2, entry.IntendedProcessNames.Count);
        Assert.Contains("code.exe", entry.IntendedProcessNames);
        Assert.Contains("notion.exe", entry.IntendedProcessNames);
    }

    private static SessionLedger LedgerWith(FocusSession session)
    {
        var ledger = new SessionLedger();
        ledger.TryRecord(session);
        return ledger;
    }

    private static FocusSession FinalizedSession(
        Guid id,
        TimeSpan counted,
        TimeSpan idle,
        IEnumerable<string>? intendedProcesses = null,
        bool complete = false)
    {
        var target = complete ? counted : counted + TimeSpan.FromMinutes(1);
        var session = FocusSession.Create(
            FocusSessionId.From(id),
            target,
            intendedProcesses);
        session.Start(StartTime);
        session.Accumulate(counted, idle);

        if (complete)
        {
            session.Complete(StartTime.Add(counted));
        }
        else
        {
            session.Stop(StartTime.Add(counted));
        }

        return session;
    }

    private static FocusSession CreateSession(Guid id)
    {
        return FocusSession.Create(
            FocusSessionId.From(id),
            TimeSpan.FromMinutes(25));
    }

    private static FocusSession MaxDurationSession(Guid id)
    {
        var session = FocusSession.Create(FocusSessionId.From(id), TimeSpan.MaxValue);
        session.Start(StartTime);
        session.Accumulate(TimeSpan.MaxValue, TimeSpan.Zero);
        session.Stop(StartTime);
        return session;
    }
}
