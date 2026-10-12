namespace DeskTown.Application.Display;

public enum NpcAmbientAction { Idle, Walk, Read }
public sealed record NpcAmbientPose(double X, double Y, NpcAmbientAction Action,
    bool FacingLeft, TimeSpan ClipElapsed);
public sealed record NpcAmbientFrame(NpcAmbientPose Noah, NpcAmbientPose Rumi)
{
    // Offsets are in the original 1x art coordinates; Town renders at 2x.
    public double NoahOffsetX => Noah.X;
    public double RumiOffsetX => Rumi.X;
    public bool NoahReading => Noah.Action == NpcAmbientAction.Read;
}

/// <summary>Visible-time presentation routes. No randomness, persistence or gameplay state.</summary>
public static class NpcAmbientSchedule
{
    private readonly record struct Visit(double X, double Y, double Hold, NpcAmbientAction Action, bool Left = false);
    private const double Speed = 18;
    // Read by the fence, stroll toward the Workshop, visit the fire and House.
    // Idle beats let Noah put the book away before walking.
    private static readonly Visit[] NoahRoute =
    [
        new(0, 0, 8, NpcAmbientAction.Idle),
        new(0, 0, 19, NpcAmbientAction.Read),
        new(0, 0, 3, NpcAmbientAction.Idle),
        new(92, 0, 11, NpcAmbientAction.Idle),
        new(-22, 0, 3, NpcAmbientAction.Idle),
        new(8, 54, 23, NpcAmbientAction.Read, true),
        new(8, 54, 4, NpcAmbientAction.Idle, true),
        new(-156, 20, 13, NpcAmbientAction.Idle)
    ];
    // Separate street lane and rhythm; observe the Library without entering.
    private static readonly Visit[] RumiRoute =
    [
        new(0, 0, 9, NpcAmbientAction.Idle),
        new(42, -40, 4, NpcAmbientAction.Idle),
        new(262, -40, 12, NpcAmbientAction.Idle),
        new(144, -40, 7, NpcAmbientAction.Idle, true),
        new(42, -40, 5, NpcAmbientAction.Idle, true),
        new(0, 0, 17, NpcAmbientAction.Idle)
    ];
    private static readonly Visit[] MinaRoute =
    [
        new(0, 0, 12, NpcAmbientAction.Idle),
        new(-28, 0, 7, NpcAmbientAction.Idle, true),
        new(18, 0, 10, NpcAmbientAction.Idle)
    ];

    public static TimeSpan NoahCycle => TimeSpan.FromSeconds(Duration(NoahRoute));
    public static TimeSpan RumiCycle => TimeSpan.FromSeconds(Duration(RumiRoute));
    public static NpcAmbientFrame At(TimeSpan elapsed) => new(Pose(NoahRoute, elapsed), Pose(RumiRoute, elapsed));
    public static NpcAmbientPose MinaAt(TimeSpan elapsed) => Pose(MinaRoute, elapsed);

    private static double Travel(Visit from, Visit to) =>
        Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2)) / Speed;
    private static double Duration(Visit[] route) => route.Select((visit, index) =>
        visit.Hold + Travel(visit, route[(index + 1) % route.Length])).Sum();

    private static NpcAmbientPose Pose(Visit[] route, TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(elapsed));
        var time = elapsed.TotalSeconds % Duration(route);
        for (var i = 0; i < route.Length; i++)
        {
            var from = route[i];
            if (time < from.Hold)
                return new(from.X, from.Y, from.Action, from.Left, TimeSpan.FromSeconds(time));
            time -= from.Hold;
            var to = route[(i + 1) % route.Length];
            var travel = Travel(from, to);
            if (time < travel)
            {
                // Short speed ramps with a constant-speed middle. Integral = 1.
                var t = time / travel;
                var ramp = Math.Min(0.15, 0.3 / travel);
                var distance = t < ramp ? t * t / (2 * ramp)
                    : t > 1 - ramp ? 1 - ramp - (1 - t) * (1 - t) / (2 * ramp)
                    : t - ramp / 2;
                var fraction = distance / (1 - ramp);
                return new(from.X + (to.X - from.X) * fraction,
                    from.Y + (to.Y - from.Y) * fraction, NpcAmbientAction.Walk,
                    to.X == from.X ? from.Left : to.X < from.X, TimeSpan.FromSeconds(time));
            }
            time -= travel;
        }
        return new(route[0].X, route[0].Y, route[0].Action, route[0].Left, TimeSpan.Zero);
    }
}
