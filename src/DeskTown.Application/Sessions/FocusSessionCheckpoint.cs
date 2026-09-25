using DeskTown.Domain.Focus;

namespace DeskTown.Application.Sessions;

/// <summary>
/// Immutable persistence seam emitted after meaningful session mutations.
/// A later persistence adapter decides when and how to write it.
/// </summary>
public sealed record FocusSessionCheckpoint(
    FocusSessionId SessionId,
    FocusSessionStatus Status,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? EndedAtUtc,
    TimeSpan TargetDuration,
    TimeSpan CountedDuration,
    TimeSpan IdleDuration,
    IReadOnlySet<string> IntendedProcessNames,
    FocusActivitySummary Activity,
    FocusEnergy TotalRecordedEnergy);
