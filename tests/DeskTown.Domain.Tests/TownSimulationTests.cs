using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;
using DeskTown.Domain.Simulation;

namespace DeskTown.Domain.Tests;

public sealed class TownSimulationTests
{
    [Fact]
    public void Sixty_minute_headless_run_equals_sparse_observations_and_restored_jump()
    {
        var eachSecond = TownSimulation.Create();
        for (var second = 1; second <= 3600; second++)
            eachSecond.AdvanceTo(TimeSpan.FromSeconds(second), Energy(second),
                FocusSessionStatus.Running, TimeSpan.Zero);

        var sparse = TownSimulation.Create();
        sparse.AdvanceTo(TimeSpan.FromMinutes(10), Energy(600),
            FocusSessionStatus.Running, TimeSpan.Zero);
        var restored = TownSimulation.Restore(sparse.CaptureCheckpoint());
        var jumped = restored.AdvanceTo(TimeSpan.FromMinutes(60), Energy(3600),
            FocusSessionStatus.Running, TimeSpan.Zero);

        Assert.Equal(eachSecond.Snapshot, jumped.Snapshot);
        Assert.Equal(WorkshopState.Complete, jumped.Snapshot.Town.Workshop);
        Assert.Equal(TimeSpan.FromMinutes(25), jumped.Snapshot.Town.Progress.CountedDuration);
        Assert.NotNull(jumped.Project.Completion);
        Assert.Null(restored.AdvanceTo(TimeSpan.FromMinutes(60), Energy(3600),
            FocusSessionStatus.Running, TimeSpan.Zero).Project.Completion);
    }

    [Fact]
    public void Companion_ghost_and_hidden_consume_same_logical_state_without_affecting_reward()
    {
        var simulation = TownSimulation.Create();
        var snapshot = simulation.AdvanceTo(TimeSpan.FromSeconds(180), Energy(180),
            FocusSessionStatus.Running, TimeSpan.FromSeconds(180)).Snapshot;

        Assert.Equal(snapshot.Town.Mina, snapshot.Companion.Mina);
        Assert.Equal(snapshot.Companion.Mina, snapshot.Ghost.Mina);
        Assert.Equal(MinaLogicalActivity.Rest, snapshot.Town.Mina.Activity);
        Assert.Equal(TimeSpan.FromMinutes(3), snapshot.Town.Progress.CountedDuration);
        // There is no Hidden renderer: reading or skipping a projection cannot change state.
        Assert.Equal(snapshot, simulation.Snapshot);
    }

    [Fact]
    public void Resume_delta_does_not_replay_completion_or_celebration()
    {
        var simulation = TownSimulation.Create();
        var first = simulation.AdvanceTo(TimeSpan.FromMinutes(25), Energy(1500),
            FocusSessionStatus.Completed, TimeSpan.Zero);
        Assert.NotNull(first.Project.Completion);
        Assert.Equal(MinaLogicalActivity.Idle, first.Snapshot.Town.Mina.Activity);

        var restored = TownSimulation.Restore(simulation.CaptureCheckpoint());
        Assert.Null(restored.AdvanceTo(TimeSpan.FromMinutes(30), Energy(1800),
            FocusSessionStatus.Running, TimeSpan.Zero).Project.Completion);
        var reveal = restored.RevealWorkshopCompletion();
        Assert.Equal(MinaLogicalActivity.Celebrate, reveal.State.Activity);
        var duringReveal = TownSimulation.Restore(restored.CaptureCheckpoint());
        Assert.False(duringReveal.RevealWorkshopCompletion().Changed);
        duringReveal.AcknowledgeCelebration();
        Assert.Equal(MinaLogicalActivity.Idle, duringReveal.Snapshot.Town.Mina.Activity);
    }

    [Fact]
    public void Invalid_time_and_idle_do_not_change_state()
    {
        var simulation = TownSimulation.Create();
        simulation.AdvanceTo(TimeSpan.FromSeconds(2), Energy(2),
            FocusSessionStatus.Running, TimeSpan.Zero);
        var before = simulation.CaptureCheckpoint();
        Assert.Throws<ArgumentOutOfRangeException>(() => simulation.AdvanceTo(
            TimeSpan.FromSeconds(1), Energy(3), FocusSessionStatus.Running, TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => simulation.AdvanceTo(
            TimeSpan.FromSeconds(3), Energy(3), FocusSessionStatus.Running,
            TimeSpan.FromTicks(-1)));
        Assert.Equal(before, simulation.CaptureCheckpoint());
    }

    private static FocusEnergy Energy(int seconds) =>
        FocusEnergy.FromCountedDuration(TimeSpan.FromSeconds(seconds));
}
