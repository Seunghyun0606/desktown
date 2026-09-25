using DeskTown.Domain.Simulation;

namespace DeskTown.Application.Display;

public readonly record struct MinaClipSpec(int Frames, int Fps, bool Loop);

/// <summary>Presentation clock only. Logical Mina and Focus never depend on frames.</summary>
public sealed class MinaClipPlayback
{
    private MinaAnimationClip? _followUp;
    private TimeSpan _elapsed;

    public MinaAnimationClip Clip { get; private set; } = MinaAnimationClip.Idle;
    public int Frame { get; private set; }

    public static MinaClipSpec Spec(MinaAnimationClip clip) => clip switch
    {
        MinaAnimationClip.Idle => new(4, 2, true),
        MinaAnimationClip.Walk => new(6, 8, true),
        MinaAnimationClip.Work => new(6, 6, true),
        MinaAnimationClip.Rest => new(4, 2, true),
        MinaAnimationClip.Stretch => new(4, 4, false),
        MinaAnimationClip.Celebrate => new(6, 8, false),
        _ => throw new ArgumentOutOfRangeException(nameof(clip))
    };

    public static MinaPresentationIntent ForState(MinaSimulationState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return new(state.Activity switch
        {
            MinaLogicalActivity.Idle => MinaAnimationClip.Idle,
            MinaLogicalActivity.Work => MinaAnimationClip.Work,
            MinaLogicalActivity.Rest => MinaAnimationClip.Rest,
            MinaLogicalActivity.Celebrate => MinaAnimationClip.Celebrate,
            _ => throw new ArgumentOutOfRangeException(nameof(state))
        });
    }

    public void Apply(MinaPresentationIntent intent)
    {
        ArgumentNullException.ThrowIfNull(intent);
        _ = Spec(intent.PrimaryClip);
        if (intent.FollowUpClip is { } next) _ = Spec(next);
        if (Clip == intent.PrimaryClip && _followUp == intent.FollowUpClip)
            return;
        Clip = intent.PrimaryClip;
        _followUp = intent.FollowUpClip;
        _elapsed = TimeSpan.Zero;
        Frame = 0;
    }

    public void Advance(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delta));
        _elapsed += delta;
        var spec = Spec(Clip);
        var rawFrame = (long)Math.Floor(_elapsed.TotalSeconds * spec.Fps);
        if (!spec.Loop && rawFrame >= spec.Frames && _followUp is { } next)
        {
            _elapsed -= TimeSpan.FromSeconds((double)spec.Frames / spec.Fps);
            Clip = next;
            _followUp = null;
            spec = Spec(Clip);
            rawFrame = (long)Math.Floor(_elapsed.TotalSeconds * spec.Fps);
        }
        Frame = spec.Loop ? (int)(rawFrame % spec.Frames)
            : (int)Math.Min(rawFrame, spec.Frames - 1);
    }
}
