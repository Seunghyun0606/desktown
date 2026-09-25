using DeskTown.Application.Display;

namespace DeskTown.Application.Tests;

public sealed class NpcAmbientScheduleTests
{
    [Fact]
    public void Schedule_is_replayable_and_has_no_random_or_gameplay_state()
    {
        var frame = NpcAmbientSchedule.At(TimeSpan.FromSeconds(25));
        Assert.Equal(frame, NpcAmbientSchedule.At(TimeSpan.FromSeconds(65)));
        Assert.True(frame.NoahReading);
        Assert.Equal(10, frame.RumiOffsetX);
    }
}
