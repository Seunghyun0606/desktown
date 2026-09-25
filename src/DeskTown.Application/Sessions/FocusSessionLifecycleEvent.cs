using DeskTown.Domain.Focus;

namespace DeskTown.Application.Sessions;

public abstract record FocusSessionLifecycleEvent(
    FocusSessionId SessionId,
    DateTimeOffset OccurredAtUtc,
    TimeSpan CountedDuration);

public sealed record FocusSessionStarted(
    FocusSessionId SessionId,
    DateTimeOffset OccurredAtUtc,
    TimeSpan CountedDuration)
    : FocusSessionLifecycleEvent(SessionId, OccurredAtUtc, CountedDuration);

public sealed record FocusSessionSuspended(
    FocusSessionId SessionId,
    DateTimeOffset OccurredAtUtc,
    TimeSpan CountedDuration)
    : FocusSessionLifecycleEvent(SessionId, OccurredAtUtc, CountedDuration);

public sealed record FocusSessionResumed(
    FocusSessionId SessionId,
    DateTimeOffset OccurredAtUtc,
    TimeSpan CountedDuration)
    : FocusSessionLifecycleEvent(SessionId, OccurredAtUtc, CountedDuration);

public sealed record FocusSessionCompleted(
    FocusSessionId SessionId,
    DateTimeOffset OccurredAtUtc,
    TimeSpan CountedDuration)
    : FocusSessionLifecycleEvent(SessionId, OccurredAtUtc, CountedDuration);

public sealed record FocusSessionStopped(
    FocusSessionId SessionId,
    DateTimeOffset OccurredAtUtc,
    TimeSpan CountedDuration)
    : FocusSessionLifecycleEvent(SessionId, OccurredAtUtc, CountedDuration);
