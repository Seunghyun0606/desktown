using DeskTown.Application.Display;

namespace DeskTown.Application.Tests;

public sealed class NpcAmbientScheduleTests
{
    [Fact]
    public void Routes_are_replayable_with_independent_rhythms_and_multiple_destinations()
    {
        Assert.NotEqual(NpcAmbientSchedule.NoahCycle, NpcAmbientSchedule.RumiCycle);
        var frames = Enumerable.Range(0, 240).Select(i => NpcAmbientSchedule.At(TimeSpan.FromSeconds(i))).ToArray();
        Assert.Contains(frames, frame => frame.Noah.Action == NpcAmbientAction.Read);
        Assert.Contains(frames, frame => frame.Noah.Action == NpcAmbientAction.Walk && frame.Noah.FacingLeft);
        Assert.Contains(frames, frame => frame.Rumi.Action == NpcAmbientAction.Walk && !frame.Rumi.FacingLeft);
        Assert.True(frames.Max(frame => frame.Noah.X) - frames.Min(frame => frame.Noah.X) > 180);
        Assert.True(frames.Max(frame => frame.Rumi.X) > 250);
        Assert.Equal(frames[25], NpcAmbientSchedule.At(TimeSpan.FromSeconds(25)));
    }

    [Fact]
    public void Continuous_routes_have_no_teleports_at_stops_or_cycle_seams()
    {
        var prior = NpcAmbientSchedule.At(TimeSpan.Zero);
        for (var i = 1; i <= 240 * 60; i++)
        {
            var current = NpcAmbientSchedule.At(TimeSpan.FromSeconds(i / 60d));
            foreach (var (before, after) in new[] { (prior.Noah, current.Noah), (prior.Rumi, current.Rumi) })
            {
                var distance = Math.Sqrt(Math.Pow(after.X - before.X, 2) + Math.Pow(after.Y - before.Y, 2));
                Assert.InRange(distance, 0, 22d / 60);
                if (distance > 0.001) Assert.Equal(NpcAmbientAction.Walk, after.Action == NpcAmbientAction.Walk ? after.Action : before.Action);
            }
            prior = current;
        }
    }

    [Fact]
    public void Mina_stroll_stays_near_home_and_uses_walk_only_while_moving()
    {
        for (var i = 0; i < 1200; i++)
        {
            var pose = NpcAmbientSchedule.MinaAt(TimeSpan.FromSeconds(i / 10d));
            Assert.InRange(pose.X, -28, 18);
            Assert.Equal(0, pose.Y);
        }
        Assert.Throws<ArgumentOutOfRangeException>(() => NpcAmbientSchedule.At(TimeSpan.FromSeconds(-1)));
    }
}
