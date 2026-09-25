namespace DeskTown.Application.Display;

/// <summary>Small deterministic visual offsets. It cannot mutate Town state.</summary>
public static class NpcAmbientSchedule
{
    public static NpcAmbientFrame At(TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(elapsed));
        var phase = ((long)(elapsed.TotalSeconds / 5)) % 8;
        return new NpcAmbientFrame(
            NoahOffsetX: phase is 1 or 2 ? 12 : phase is 3 or 4 ? 24 : 0,
            NoahReading: phase is >= 4 and <= 6,
            RumiOffsetX: phase is 2 or 3 ? -10 : phase is 4 or 5 ? 10 : 0);
    }
}

public sealed record NpcAmbientFrame(int NoahOffsetX, bool NoahReading, int RumiOffsetX);
