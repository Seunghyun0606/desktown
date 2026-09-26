namespace DeskTown.Application.Persistence;

/// <summary>Wire DTOs. Durations and Energy are always TimeSpan ticks (100 ns), never rounded minutes.</summary>
public sealed class SaveEnvelopeV1
{
    public int SchemaVersion { get; set; } = 1;
    public long SaveRevision { get; set; }
    public DateTimeOffset SavedAtUtc { get; set; }
    public SettingsDto? Settings { get; set; }
    public FocusDto? Focus { get; set; }
    public TownDto? Town { get; set; }
    public MinaDto? Mina { get; set; }
    public PlayerDto? Player { get; set; }
    public PendingPresentationDto? PendingPresentation { get; set; }
    public IntegrityDto? Integrity { get; set; }
}

public sealed class IntegrityDto
{
    public string PayloadSha256 { get; set; } = string.Empty;
}

public sealed class SettingsDto
{
    public string DisplayMode { get; set; } = "Companion";
    public WindowPlacementDto Companion { get; set; } = new();
    public WindowPlacementDto Ghost { get; set; } = new();
    public int CompanionScalePercent { get; set; } = 100;
    public int GhostOpacityPercent { get; set; } = 65;
    public bool AudioEnabled { get; set; } = true;
    public bool OnboardingCompleted { get; set; }
    public bool ReducedMotion { get; set; }
    // Older V1 saves omit this field and retain the original notice behavior.
    public bool CompletionNoticeEnabled { get; set; } = true;
}

public sealed class WindowPlacementDto
{
    public string MonitorId { get; set; } = string.Empty;
    public string Anchor { get; set; } = "BottomRight";
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
}

public sealed class FocusDto
{
    public List<FinalizedSessionDto> Ledger { get; set; } = [];
    public ActiveSessionDto? ActiveCheckpoint { get; set; }
    public List<DisplayModeChangeDto> ModeChanges { get; set; } = [];
}

public sealed class FinalizedSessionDto
{
    public Guid SessionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset EndedAtUtc { get; set; }
    public long TargetTicks { get; set; }
    public long CountedTicks { get; set; }
    public long IdleTicks { get; set; }
    public List<string> IntendedProcessNames { get; set; } = [];
}

public sealed class ActiveSessionDto
{
    public Guid SessionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? StartedAtUtc { get; set; }
    public DateTimeOffset? EndedAtUtc { get; set; }
    public long TargetTicks { get; set; }
    public long CountedTicks { get; set; }
    public long IdleTicks { get; set; }
    public List<string> IntendedProcessNames { get; set; } = [];
    public long ActiveTicks { get; set; }
    public long ObservedIdleTicks { get; set; }
    public long UnknownTicks { get; set; }
    public List<ProcessDurationDto> Processes { get; set; } = [];
    public long TotalRecordedEnergyTicks { get; set; }
}

public sealed class ProcessDurationDto
{
    public string ProcessName { get; set; } = string.Empty;
    public long DurationTicks { get; set; }
}

public sealed class DisplayModeChangeDto
{
    public DateTimeOffset AtUtc { get; set; }
    public string DisplayMode { get; set; } = string.Empty;
}

public sealed class TownDto
{
    public string CurrentProjectId { get; set; } = "RestoreWorkshop";
    public long ProjectProgressTicks { get; set; }
    public long LastObservedCumulativeEnergyTicks { get; set; }
    public string WorkshopState { get; set; } = "Broken";
    public List<string> UnlockedProjectIds { get; set; } = [];
    public List<string> PendingEventIds { get; set; } = [];
    public List<string> ConsumedEventIds { get; set; } = [];
}

public sealed class MinaDto
{
    public string Activity { get; set; } = "Idle";
    public string Location { get; set; } = "Home";
    public string ProjectId { get; set; } = "RestoreWorkshop";
    public bool ShortIdleStretchPlayed { get; set; }
    public bool WorkshopCelebrated { get; set; }
}

public sealed class PlayerDto
{
    public long TotalFocusTicks { get; set; }
    public DateOnly TodayUtcDate { get; set; }
    public long TodayFocusTicks { get; set; }
}

public sealed class PendingPresentationDto
{
    public List<string> PendingIds { get; set; } = [];
    public List<string> ConsumedIds { get; set; } = [];
}
