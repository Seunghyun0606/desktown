using DeskTown.Domain.Simulation;

namespace DeskTown.Application.Display;

/// <summary>Small Work beats layered over authoritative logical animation commands.</summary>
public sealed class MinaPresentationPlayback
{
    private readonly MinaClipPlayback _base = new();
    private int _stretchFrame;
    private MinaAnimationClip _target = MinaAnimationClip.Idle;
    private double _visibleWorkSeconds;
    private int _beat;
    private bool _reducedMotion;
    public bool ReducedMotion
    {
        get => _reducedMotion;
        set { _reducedMotion = value; if (value) _beat = 0; }
    }
    public MinaAnimationClip Clip => _beat == 2 ? MinaAnimationClip.Stretch : _base.Clip;
    public int Frame => _beat == 1 ? 0 : _beat == 2 ? _stretchFrame : _base.Frame;

    public void Apply(MinaPresentationIntent intent)
    {
        var before = (_base.Clip, _base.HasFollowUp);
        _base.Apply(intent);
        _target = intent.FollowUpClip ?? intent.PrimaryClip;
        if (before != (_base.Clip, _base.HasFollowUp))
        {
            _visibleWorkSeconds = 0;
            _beat = 0;
        }
    }

    public void Advance(TimeSpan delta)
    {
        var transitionTime = _base.TimeUntilFollowUp ?? TimeSpan.Zero;
        _base.Advance(delta);
        if (ReducedMotion || _base.Clip != MinaAnimationClip.Work || _base.HasFollowUp
            || _target != MinaAnimationClip.Work)
        {
            _beat = 0;
            return;
        }
        _visibleWorkSeconds += Math.Max(0, (delta - transitionTime).TotalSeconds);
        var phase = _visibleWorkSeconds % 73.5;
        var next = phase is >= 14 and < 16.5 or >= 47.5 and < 50.5 ? 1
            : phase is >= 35.5 and < 36.5 ? 2 : 0;
        if (next == 2)
        {
            // Sample the beat offset independently of the render delta partition.
            var spec = MinaClipPlayback.Spec(MinaAnimationClip.Stretch);
            _stretchFrame = Math.Min(spec.Frames - 1, (int)((phase - 35.5) * spec.Fps));
        }
        _beat = next;
    }
}
