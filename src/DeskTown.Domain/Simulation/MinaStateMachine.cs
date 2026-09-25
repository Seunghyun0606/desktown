using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;

namespace DeskTown.Domain.Simulation;

/// <summary>
/// One logical Mina, independent of rendering, display choice and rewards.
/// The caller supplies consecutive user inactivity, not a productivity score.
/// </summary>
public sealed class MinaStateMachine
{
    private readonly MinaThresholds _thresholds;
    private bool _shortIdleStretchPlayed;
    private bool _workshopCelebrated;

    public MinaStateMachine(MinaThresholds? thresholds = null)
    {
        _thresholds = thresholds ?? MinaThresholds.Default;
        _thresholds.Validate();
        State = new MinaSimulationState(MinaLogicalActivity.Idle, MinaLocation.Home);
    }

    public MinaSimulationState State { get; private set; }

    public MinaStateCheckpoint CaptureCheckpoint() =>
        new(State, _shortIdleStretchPlayed, _workshopCelebrated);

    public static MinaStateMachine Restore(
        MinaStateCheckpoint checkpoint,
        MinaThresholds? thresholds = null)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        ArgumentNullException.ThrowIfNull(checkpoint.State);

        if (!Enum.IsDefined(checkpoint.State.Activity)
            || !Enum.IsDefined(checkpoint.State.Location)
            || (checkpoint.State.Activity == MinaLogicalActivity.Idle
                ? checkpoint.State.Location != MinaLocation.Home
                : checkpoint.State.Location != MinaLocation.Workshop)
            || (checkpoint.ShortIdleStretchPlayed
                && checkpoint.State.Activity != MinaLogicalActivity.Work)
            || (checkpoint.State.Activity == MinaLogicalActivity.Celebrate
                && !checkpoint.WorkshopCelebrated))
        {
            throw new ArgumentException("Invalid Mina logical checkpoint.", nameof(checkpoint));
        }

        var machine = new MinaStateMachine(thresholds)
        {
            State = checkpoint.State,
            _shortIdleStretchPlayed = checkpoint.ShortIdleStretchPlayed,
            _workshopCelebrated = checkpoint.WorkshopCelebrated
        };

        return machine;
    }

    /// <param name="sessionStatus">Null means no current focus session.</param>
    /// <param name="consecutiveIdle">
    /// Time since the most recent user input within a running session. The outer
    /// application owns this counter; it never changes Focus Energy.
    /// </param>
    public MinaTransition Observe(
        FocusSessionStatus? sessionStatus,
        TimeSpan consecutiveIdle)
    {
        if (consecutiveIdle < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(consecutiveIdle));
        }

        if (sessionStatus is not null && !Enum.IsDefined(sessionStatus.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(sessionStatus));
        }

        // The reveal presentation remains visible until explicitly acknowledged.
        if (State.Activity == MinaLogicalActivity.Celebrate)
        {
            return Stable(MinaAnimationClip.Celebrate);
        }

        if (sessionStatus == FocusSessionStatus.Running)
        {
            if (consecutiveIdle >= _thresholds.LongIdle)
            {
                _shortIdleStretchPlayed = false;
                return SetState(MinaLogicalActivity.Rest, MinaLocation.Workshop,
                    MinaAnimationClip.Rest);
            }

            if (consecutiveIdle >= _thresholds.ShortIdle)
            {
                if (!_shortIdleStretchPlayed)
                {
                    _shortIdleStretchPlayed = true;
                    State = new MinaSimulationState(MinaLogicalActivity.Work, MinaLocation.Workshop);
                    return new MinaTransition(State,
                        new MinaPresentationIntent(MinaAnimationClip.Stretch, MinaAnimationClip.Work), true);
                }

                return SetState(MinaLogicalActivity.Work, MinaLocation.Workshop,
                    MinaAnimationClip.Work);
            }

            _shortIdleStretchPlayed = false;
            var returning = State.Activity is MinaLogicalActivity.Idle or MinaLogicalActivity.Rest;
            return returning
                ? SetState(MinaLogicalActivity.Work, MinaLocation.Workshop,
                    MinaAnimationClip.Walk, MinaAnimationClip.Work)
                : SetState(MinaLogicalActivity.Work, MinaLocation.Workshop, MinaAnimationClip.Work);
        }

        _shortIdleStretchPlayed = false;
        return sessionStatus == FocusSessionStatus.Suspended
            ? SetState(MinaLogicalActivity.Rest, MinaLocation.Workshop, MinaAnimationClip.Rest)
            : SetState(MinaLogicalActivity.Idle, MinaLocation.Home, MinaAnimationClip.Idle);
    }

    /// <summary>
    /// Called when the pending Workshop reward is revealed. The project owns
    /// completion; Mina only displays it and ignores repeated delivery.
    /// </summary>
    public MinaTransition CelebrateProject(ProjectCompleted completion)
    {
        ArgumentNullException.ThrowIfNull(completion);
        if (completion.ProjectId != ProjectId.RestoreWorkshop)
        {
            throw new ArgumentOutOfRangeException(nameof(completion));
        }

        if (_workshopCelebrated)
        {
            return Stable(State.Activity == MinaLogicalActivity.Celebrate
                ? MinaAnimationClip.Celebrate
                : ClipFor(State.Activity));
        }

        _shortIdleStretchPlayed = false;
        _workshopCelebrated = true;
        return SetState(MinaLogicalActivity.Celebrate, MinaLocation.Workshop,
            MinaAnimationClip.Work, MinaAnimationClip.Celebrate);
    }

    /// <summary>Presentation acknowledgement ends the celebration, once.</summary>
    public MinaTransition AcknowledgeCelebration()
    {
        return State.Activity == MinaLogicalActivity.Celebrate
            ? SetState(MinaLogicalActivity.Idle, MinaLocation.Home, MinaAnimationClip.Idle)
            : Stable(ClipFor(State.Activity));
    }

    private MinaTransition SetState(
        MinaLogicalActivity activity,
        MinaLocation location,
        MinaAnimationClip clip,
        MinaAnimationClip? followUp = null)
    {
        var next = new MinaSimulationState(activity, location);
        var changed = next != State || followUp is not null;
        State = next;
        return new MinaTransition(State, new MinaPresentationIntent(clip, followUp), changed);
    }

    private MinaTransition Stable(MinaAnimationClip clip) =>
        new(State, new MinaPresentationIntent(clip), false);

    private static MinaAnimationClip ClipFor(MinaLogicalActivity activity) => activity switch
    {
        MinaLogicalActivity.Idle => MinaAnimationClip.Idle,
        MinaLogicalActivity.Work => MinaAnimationClip.Work,
        MinaLogicalActivity.Rest => MinaAnimationClip.Rest,
        MinaLogicalActivity.Celebrate => MinaAnimationClip.Celebrate,
        _ => throw new ArgumentOutOfRangeException(nameof(activity))
    };
}
