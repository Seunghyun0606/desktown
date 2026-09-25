using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;
using DeskTown.Domain.Simulation;

namespace DeskTown.Domain.Tests;

public sealed class MinaStateMachineTests
{
    [Fact]
    public void New_mina_is_idle_at_home_and_starts_work_with_a_walk_intent()
    {
        var mina = new MinaStateMachine();

        Assert.Equal(new MinaSimulationState(MinaLogicalActivity.Idle, MinaLocation.Home), mina.State);
        var started = mina.Observe(FocusSessionStatus.Running, TimeSpan.Zero);

        Assert.Equal(MinaLogicalActivity.Work, started.State.Activity);
        Assert.Equal(MinaLocation.Workshop, started.State.Location);
        Assert.Equal(new MinaPresentationIntent(MinaAnimationClip.Walk, MinaAnimationClip.Work),
            started.Presentation);
        Assert.True(started.Changed);
        Assert.False(mina.Observe(FocusSessionStatus.Running, TimeSpan.Zero).Changed);
    }

    [Theory]
    [InlineData(59, MinaLogicalActivity.Work, MinaAnimationClip.Work, false)]
    [InlineData(60, MinaLogicalActivity.Work, MinaAnimationClip.Stretch, true)]
    [InlineData(179, MinaLogicalActivity.Work, MinaAnimationClip.Stretch, true)]
    [InlineData(180, MinaLogicalActivity.Rest, MinaAnimationClip.Rest, false)]
    public void Idle_boundaries_follow_configured_default_thresholds(
        int idleSeconds,
        MinaLogicalActivity expectedActivity,
        MinaAnimationClip expectedClip,
        bool hasFollowUp)
    {
        var mina = new MinaStateMachine();
        mina.Observe(FocusSessionStatus.Running, TimeSpan.Zero);

        var transition = mina.Observe(FocusSessionStatus.Running, TimeSpan.FromSeconds(idleSeconds));

        Assert.Equal(expectedActivity, transition.State.Activity);
        Assert.Equal(expectedClip, transition.Presentation.PrimaryClip);
        Assert.Equal(hasFollowUp, transition.Presentation.FollowUpClip is not null);
    }

    [Fact]
    public void Stretch_is_one_shot_during_a_short_idle_episode_and_resets_on_activity()
    {
        var mina = new MinaStateMachine();
        mina.Observe(FocusSessionStatus.Running, TimeSpan.Zero);

        var first = mina.Observe(FocusSessionStatus.Running, TimeSpan.FromSeconds(60));
        var repeated = mina.Observe(FocusSessionStatus.Running, TimeSpan.FromSeconds(61));
        var nearLong = mina.Observe(FocusSessionStatus.Running, TimeSpan.FromSeconds(179));

        Assert.Equal(new MinaPresentationIntent(MinaAnimationClip.Stretch, MinaAnimationClip.Work),
            first.Presentation);
        Assert.Equal(new MinaPresentationIntent(MinaAnimationClip.Work), repeated.Presentation);
        Assert.False(repeated.Changed);
        Assert.False(nearLong.Changed);

        mina.Observe(FocusSessionStatus.Running, TimeSpan.Zero);
        Assert.Equal(MinaAnimationClip.Stretch,
            mina.Observe(FocusSessionStatus.Running, TimeSpan.FromSeconds(60))
                .Presentation.PrimaryClip);
    }

    [Fact]
    public void Long_idle_and_suspend_rest_and_activity_return_walks_to_work()
    {
        var mina = new MinaStateMachine();
        mina.Observe(FocusSessionStatus.Running, TimeSpan.Zero);

        var longIdle = mina.Observe(FocusSessionStatus.Running, TimeSpan.FromSeconds(180));
        Assert.Equal(MinaLogicalActivity.Rest, longIdle.State.Activity);
        Assert.Equal(MinaAnimationClip.Rest, longIdle.Presentation.PrimaryClip);

        var back = mina.Observe(FocusSessionStatus.Running, TimeSpan.Zero);
        Assert.Equal(MinaLogicalActivity.Work, back.State.Activity);
        Assert.Equal(new MinaPresentationIntent(MinaAnimationClip.Walk, MinaAnimationClip.Work),
            back.Presentation);

        var suspended = mina.Observe(FocusSessionStatus.Suspended, TimeSpan.Zero);
        Assert.Equal(MinaLogicalActivity.Rest, suspended.State.Activity);
        Assert.Equal(MinaAnimationClip.Rest, suspended.Presentation.PrimaryClip);
        Assert.False(mina.Observe(FocusSessionStatus.Suspended, TimeSpan.Zero).Changed);

        var resumed = mina.Observe(FocusSessionStatus.Running, TimeSpan.Zero);
        Assert.Equal(new MinaPresentationIntent(MinaAnimationClip.Walk, MinaAnimationClip.Work),
            resumed.Presentation);
    }

    [Theory]
    [InlineData(FocusSessionStatus.Ready)]
    [InlineData(FocusSessionStatus.Stopped)]
    [InlineData(FocusSessionStatus.Completed)]
    public void Inactive_session_returns_mina_home(FocusSessionStatus status)
    {
        var mina = new MinaStateMachine();
        mina.Observe(FocusSessionStatus.Running, TimeSpan.Zero);

        var result = mina.Observe(status, TimeSpan.Zero);

        Assert.Equal(new MinaSimulationState(MinaLogicalActivity.Idle, MinaLocation.Home), result.State);
        Assert.Equal(MinaAnimationClip.Idle, result.Presentation.PrimaryClip);
    }

    [Fact]
    public void No_session_and_tracking_unavailable_can_keep_mina_idle_or_work_without_invented_activity()
    {
        var mina = new MinaStateMachine();
        Assert.False(mina.Observe(null, TimeSpan.Zero).Changed);

        // An unavailable tracker is handled by the caller; do not synthesize idle.
        mina.Observe(FocusSessionStatus.Running, TimeSpan.Zero);
        Assert.Equal(MinaLogicalActivity.Work,
            mina.Observe(FocusSessionStatus.Running, TimeSpan.Zero).State.Activity);
    }

    [Fact]
    public void Custom_thresholds_are_used_and_invalid_values_are_rejected()
    {
        var mina = new MinaStateMachine(new MinaThresholds(
            TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(8)));
        mina.Observe(FocusSessionStatus.Running, TimeSpan.Zero);

        Assert.Equal(MinaAnimationClip.Work,
            mina.Observe(FocusSessionStatus.Running, TimeSpan.FromSeconds(2)).Presentation.PrimaryClip);
        Assert.Equal(MinaAnimationClip.Stretch,
            mina.Observe(FocusSessionStatus.Running, TimeSpan.FromSeconds(3)).Presentation.PrimaryClip);
        Assert.Equal(MinaLogicalActivity.Rest,
            mina.Observe(FocusSessionStatus.Running, TimeSpan.FromSeconds(8)).State.Activity);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MinaStateMachine(new MinaThresholds(TimeSpan.Zero, TimeSpan.FromSeconds(8))));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MinaStateMachine(new MinaThresholds(TimeSpan.FromSeconds(8), TimeSpan.FromSeconds(8))));
    }

    [Fact]
    public void Completion_is_presentation_only_and_repeated_delivery_is_a_no_op()
    {
        var mina = new MinaStateMachine();
        var project = ProjectSystem.CreateRestoreWorkshop();
        var application = project.ApplyCumulativeEnergy(
            FocusEnergy.FromCountedDuration(TimeSpan.FromMinutes(25)));
        var completed = Assert.IsType<ProjectCompleted>(application.Completion);
        var energyBefore = project.Progress;

        var reveal = mina.CelebrateProject(completed);
        Assert.Equal(new MinaSimulationState(MinaLogicalActivity.Celebrate, MinaLocation.Workshop),
            reveal.State);
        Assert.Equal(new MinaPresentationIntent(MinaAnimationClip.Work, MinaAnimationClip.Celebrate),
            reveal.Presentation);
        Assert.False(mina.CelebrateProject(completed).Changed);
        Assert.Equal(MinaLogicalActivity.Celebrate,
            mina.Observe(FocusSessionStatus.Completed, TimeSpan.Zero).State.Activity);
        var restoredDuringReveal = MinaStateMachine.Restore(mina.CaptureCheckpoint());
        Assert.Equal(MinaLogicalActivity.Celebrate, restoredDuringReveal.State.Activity);

        mina.AcknowledgeCelebration();
        Assert.Equal(MinaLogicalActivity.Idle, mina.State.Activity);
        Assert.False(mina.CelebrateProject(completed).Changed);
        Assert.Equal(energyBefore, project.Progress);
    }

    [Fact]
    public void Checkpoint_restores_short_idle_and_completion_one_shots()
    {
        var mina = new MinaStateMachine();
        mina.Observe(FocusSessionStatus.Running, TimeSpan.Zero);
        mina.Observe(FocusSessionStatus.Running, TimeSpan.FromSeconds(60));

        var restored = MinaStateMachine.Restore(mina.CaptureCheckpoint());
        Assert.False(restored.Observe(FocusSessionStatus.Running, TimeSpan.FromSeconds(61)).Changed);
        Assert.Equal(MinaAnimationClip.Work,
            restored.Observe(FocusSessionStatus.Running, TimeSpan.FromSeconds(62))
                .Presentation.PrimaryClip);

        var completion = new ProjectCompleted(ProjectId.RestoreWorkshop);
        // A reveal may arrive while a short-idle cue is still latched.
        restored.Observe(FocusSessionStatus.Running, TimeSpan.Zero);
        Assert.False(restored.CaptureCheckpoint().ShortIdleStretchPlayed);
        restored.Observe(FocusSessionStatus.Running, TimeSpan.FromSeconds(60));
        Assert.True(restored.CaptureCheckpoint().ShortIdleStretchPlayed);
        restored.CelebrateProject(completion);
        Assert.False(restored.CaptureCheckpoint().ShortIdleStretchPlayed);
        var restoredRevealing = MinaStateMachine.Restore(restored.CaptureCheckpoint());
        Assert.False(restoredRevealing.CelebrateProject(completion).Changed);
        restoredRevealing.AcknowledgeCelebration();
        var restoredAfterReveal = MinaStateMachine.Restore(restoredRevealing.CaptureCheckpoint());
        Assert.False(restoredAfterReveal.CelebrateProject(completion).Changed);
    }

    [Fact]
    public void Same_observations_produce_identical_logical_state_for_any_display_preference()
    {
        var companion = new MinaStateMachine();
        var hidden = new MinaStateMachine();
        var ghost = new MinaStateMachine();

        foreach (var idle in new[] { 0, 59, 60, 120, 180, 0 })
        {
            var observation = TimeSpan.FromSeconds(idle);
            var a = companion.Observe(FocusSessionStatus.Running, observation);
            var b = hidden.Observe(FocusSessionStatus.Running, observation);
            var c = ghost.Observe(FocusSessionStatus.Running, observation);
            Assert.Equal(a, b);
            Assert.Equal(a, c);
        }
    }

    [Fact]
    public void Invalid_observations_and_checkpoints_do_not_mutate_machine()
    {
        var mina = new MinaStateMachine();
        var before = mina.CaptureCheckpoint();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            mina.Observe(FocusSessionStatus.Running, TimeSpan.FromTicks(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            mina.Observe((FocusSessionStatus)999, TimeSpan.Zero));
        Assert.Equal(before, mina.CaptureCheckpoint());

        Assert.Throws<ArgumentException>(() => MinaStateMachine.Restore(new MinaStateCheckpoint(
            new MinaSimulationState(MinaLogicalActivity.Idle, MinaLocation.Workshop), false, false)));
        Assert.Throws<ArgumentException>(() => MinaStateMachine.Restore(new MinaStateCheckpoint(
            new MinaSimulationState(MinaLogicalActivity.Rest, MinaLocation.Workshop), true, false)));
        Assert.Throws<ArgumentException>(() => MinaStateMachine.Restore(new MinaStateCheckpoint(
            new MinaSimulationState(MinaLogicalActivity.Celebrate, MinaLocation.Workshop), false, false)));
    }
}
