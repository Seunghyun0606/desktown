using DeskTown.Application.Display;
using DeskTown.Domain.Simulation;

namespace DeskTown.Application.Tests;

public sealed class MinaClipPlaybackTests
{
    [Theory]
    [InlineData(MinaAnimationClip.Idle, 4, 2, true)]
    [InlineData(MinaAnimationClip.Walk, 6, 8, true)]
    [InlineData(MinaAnimationClip.Work, 6, 6, true)]
    [InlineData(MinaAnimationClip.Rest, 4, 2, true)]
    [InlineData(MinaAnimationClip.Stretch, 4, 4, false)]
    [InlineData(MinaAnimationClip.Celebrate, 6, 8, false)]
    public void Matches_sprite_contract(MinaAnimationClip clip, int frames, int fps, bool loop) =>
        Assert.Equal(new MinaClipSpec(frames, fps, loop), MinaClipPlayback.Spec(clip));

    [Fact]
    public void Steady_clip_does_not_restart_and_one_shot_returns_to_work()
    {
        var player = new MinaClipPlayback();
        player.Apply(new MinaPresentationIntent(MinaAnimationClip.Work));
        player.Advance(TimeSpan.FromMilliseconds(500));
        Assert.Equal(3, player.Frame);
        player.Apply(new MinaPresentationIntent(MinaAnimationClip.Work));
        Assert.Equal(3, player.Frame);

        player.Apply(new MinaPresentationIntent(MinaAnimationClip.Stretch, MinaAnimationClip.Work));
        player.Advance(TimeSpan.FromSeconds(1.25));
        Assert.Equal(MinaAnimationClip.Work, player.Clip);
        Assert.Equal(1, player.Frame);
    }

    [Fact]
    public void Current_logical_snapshot_restores_correct_steady_clip()
    {
        var player = new MinaClipPlayback();
        player.Apply(MinaClipPlayback.ForState(new MinaSimulationState(
            MinaLogicalActivity.Rest, MinaLocation.Workshop)));
        Assert.Equal(MinaAnimationClip.Rest, player.Clip);
    }
}
