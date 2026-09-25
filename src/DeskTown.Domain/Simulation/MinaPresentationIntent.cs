namespace DeskTown.Domain.Simulation;

/// <summary>
/// A presentation hint for a consuming animator. A non-null FollowUpClip plays
/// after the primary clip once; repeated steady clips should not restart playback.
/// </summary>
public sealed record MinaPresentationIntent(
    MinaAnimationClip PrimaryClip,
    MinaAnimationClip? FollowUpClip = null);
