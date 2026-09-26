using DeskTown.Application.Sessions;
using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;

namespace DeskTown.Application.Persistence;

/// <summary>One immutable application checkpoint; platform details remain outside this model.</summary>
public sealed record GameStateSnapshot(
    SettingsSnapshot Settings,
    FocusSnapshot Focus,
    TownSnapshot Town,
    MinaSnapshot Mina,
    PlayerSnapshot Player,
    PendingPresentationSnapshot PendingPresentation);

public sealed record SettingsSnapshot(
    string DisplayMode,
    WindowPlacementSnapshot Companion,
    WindowPlacementSnapshot Ghost,
    int CompanionScalePercent,
    int GhostOpacityPercent,
    bool AudioEnabled,
    bool OnboardingCompleted,
    bool ReducedMotion,
    bool CompletionNoticeEnabled = true);

public sealed record WindowPlacementSnapshot(string MonitorId, string Anchor, int OffsetX, int OffsetY);

public sealed record FocusSnapshot(
    SessionLedger Ledger,
    FocusSessionCheckpoint? ActiveCheckpoint,
    IReadOnlyList<DisplayModeChangeSnapshot> ModeChanges);

public sealed record DisplayModeChangeSnapshot(DateTimeOffset AtUtc, string DisplayMode);

public sealed record TownSnapshot(
    ProjectSystem Project,
    IReadOnlyList<string> UnlockedProjectIds,
    IReadOnlyList<string> PendingEventIds,
    IReadOnlyList<string> ConsumedEventIds);

/// <summary>Scalar logical identifiers keep this contract independent of the Mina renderer.</summary>
public sealed record MinaSnapshot(
    string Activity,
    string Location,
    string ProjectId,
    bool ShortIdleStretchPlayed,
    bool WorkshopCelebrated);

public sealed record PlayerSnapshot(long TotalFocusTicks, DateOnly TodayUtcDate, long TodayFocusTicks);

public sealed record PendingPresentationSnapshot(
    IReadOnlyList<string> PendingIds,
    IReadOnlyList<string> ConsumedIds);

public sealed record StoredGameState(long Revision, DateTimeOffset SavedAtUtc, GameStateSnapshot Snapshot);
