using DeskTown.Application.Display;
using DeskTown.Domain.Simulation;

namespace DeskTown.Application.Tests;

public sealed class MinaPresentationPlaybackTests
{
    [Fact]
    public void Work_beats_include_still_inspection_and_one_stretch_then_resume_work()
    {
        var player = new MinaPresentationPlayback();
        player.Apply(new(MinaAnimationClip.Work));
        player.Advance(TimeSpan.FromSeconds(14.4));
        Assert.Equal(MinaAnimationClip.Work, player.Clip);
        Assert.Equal(0, player.Frame);
        player.Advance(TimeSpan.FromSeconds(21.35));
        Assert.Equal(MinaAnimationClip.Stretch, player.Clip);
        Assert.Equal(1, player.Frame);
        player.Advance(TimeSpan.FromSeconds(0.8));
        Assert.Equal(MinaAnimationClip.Work, player.Clip);
    }

    [Fact]
    public void Logical_rest_and_reward_interrupt_ambient_work_immediately()
    {
        var player = new MinaPresentationPlayback();
        player.Apply(new(MinaAnimationClip.Work));
        player.Advance(TimeSpan.FromSeconds(35.75));
        player.Apply(new(MinaAnimationClip.Rest));
        Assert.Equal(MinaAnimationClip.Rest, player.Clip);
        player.Advance(TimeSpan.FromSeconds(90));
        Assert.Equal(MinaAnimationClip.Rest, player.Clip);
        player.Apply(new(MinaAnimationClip.Work, MinaAnimationClip.Celebrate));
        player.Advance(TimeSpan.FromSeconds(1.5));
        Assert.Equal(MinaAnimationClip.Celebrate, player.Clip);
    }

    [Fact]
    public void Rebinding_work_does_not_reset_cadence_and_reduced_motion_removes_ambient_stretch()
    {
        var player = new MinaPresentationPlayback();
        player.Apply(new(MinaAnimationClip.Work));
        player.Advance(TimeSpan.FromSeconds(35.75));
        player.Apply(new(MinaAnimationClip.Work));
        Assert.Equal(MinaAnimationClip.Stretch, player.Clip);
        player.ReducedMotion = true;
        Assert.Equal(MinaAnimationClip.Work, player.Clip);
        player.Advance(TimeSpan.FromSeconds(20));
        Assert.Equal(MinaAnimationClip.Work, player.Clip);
    }

    [Fact]
    public void Same_elapsed_time_at_different_render_rates_produces_same_pose()
    {
        var coarse = new MinaPresentationPlayback();
        var fine = new MinaPresentationPlayback();
        coarse.Apply(new(MinaAnimationClip.Work)); fine.Apply(new(MinaAnimationClip.Work));
        coarse.Advance(TimeSpan.FromSeconds(35.75));
        for (var i = 0; i < 143; i++) fine.Advance(TimeSpan.FromSeconds(0.25));
        Assert.Equal((coarse.Clip, coarse.Frame), (fine.Clip, fine.Frame));
    }

    [Fact]
    public void Walk_transition_time_is_excluded_from_work_cadence_at_any_render_rate()
    {
        var coarse = new MinaPresentationPlayback(); var fine = new MinaPresentationPlayback();
        coarse.Apply(new(MinaAnimationClip.Walk, MinaAnimationClip.Work));
        fine.Apply(new(MinaAnimationClip.Walk, MinaAnimationClip.Work));
        coarse.Advance(TimeSpan.FromSeconds(36.5));
        for (var i = 0; i < 146; i++) fine.Advance(TimeSpan.FromSeconds(0.25));
        Assert.Equal((MinaAnimationClip.Stretch, 1), (coarse.Clip, coarse.Frame));
        Assert.Equal((coarse.Clip, coarse.Frame), (fine.Clip, fine.Frame));
    }
}
