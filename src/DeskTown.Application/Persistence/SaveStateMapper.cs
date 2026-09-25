using DeskTown.Application.Sessions;
using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;

namespace DeskTown.Application.Persistence;

/// <summary>Explicit allowlist mapping. No OS handles, titles, URLs, or visual clips enter the save.</summary>
public static class SaveStateMapper
{
    public static SaveEnvelopeV1 ToEnvelope(GameStateSnapshot snapshot, long revision, DateTimeOffset savedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (revision < 0 || savedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Revision must be nonnegative and save time must be UTC.");
        }

        var ledger = snapshot.Focus.Ledger;
        var checkpoint = snapshot.Focus.ActiveCheckpoint;
        var eventState = EventSaveMapper.Restore(snapshot.Town.Project,
            snapshot.Town.UnlockedProjectIds, snapshot.Town.PendingEventIds,
            snapshot.Town.ConsumedEventIds, snapshot.PendingPresentation.PendingIds,
            snapshot.PendingPresentation.ConsumedIds).State;
        var savedTown = EventSaveMapper.Town(snapshot.Town.Project, eventState);
        var savedPresentation = EventSaveMapper.Presentation(eventState);
        if (snapshot.Town.Project.LastObservedCumulativeEnergy.CountedDuration > ledger.TotalCountedDuration
            || snapshot.Player.TotalFocusTicks != ledger.TotalCountedDuration.Ticks
            || (checkpoint is not null
                && checkpoint.TotalRecordedEnergy.CountedDuration != ledger.TotalCountedDuration))
        {
            throw new InvalidDataException("Focus totals, project high-watermark, and player total disagree.");
        }

        var envelope = new SaveEnvelopeV1
        {
            SaveRevision = revision,
            SavedAtUtc = savedAtUtc,
            Settings = new SettingsDto
            {
                DisplayMode = snapshot.Settings.DisplayMode,
                Companion = ToDto(snapshot.Settings.Companion),
                Ghost = ToDto(snapshot.Settings.Ghost),
                CompanionScalePercent = snapshot.Settings.CompanionScalePercent,
                GhostOpacityPercent = snapshot.Settings.GhostOpacityPercent,
                AudioEnabled = snapshot.Settings.AudioEnabled,
                OnboardingCompleted = snapshot.Settings.OnboardingCompleted,
                ReducedMotion = snapshot.Settings.ReducedMotion
            },
            Focus = new FocusDto
            {
                Ledger = ledger.Entries.OrderBy(e => e.StartedAtUtc).ThenBy(e => e.SessionId.Value)
                    .Select(e => new FinalizedSessionDto
                    {
                        SessionId = e.SessionId.Value,
                        Status = e.Status.ToString(),
                        StartedAtUtc = e.StartedAtUtc,
                        EndedAtUtc = e.EndedAtUtc,
                        TargetTicks = e.TargetDuration.Ticks,
                        CountedTicks = e.CountedDuration.Ticks,
                        IdleTicks = e.IdleDuration.Ticks,
                        IntendedProcessNames = e.IntendedProcessNames.OrderBy(n => n, StringComparer.Ordinal).ToList()
                    }).ToList(),
                ActiveCheckpoint = checkpoint is null ? null : ToDto(checkpoint),
                ModeChanges = snapshot.Focus.ModeChanges.Select(change => new DisplayModeChangeDto
                {
                    AtUtc = change.AtUtc,
                    DisplayMode = change.DisplayMode
                }).ToList()
            },
            Town = new TownDto
            {
                CurrentProjectId = snapshot.Town.Project.CurrentProjectId.ToString(),
                ProjectProgressTicks = snapshot.Town.Project.Progress.CountedDuration.Ticks,
                LastObservedCumulativeEnergyTicks = snapshot.Town.Project.LastObservedCumulativeEnergy.CountedDuration.Ticks,
                WorkshopState = snapshot.Town.Project.WorkshopState.ToString(),
                UnlockedProjectIds = savedTown.UnlockedProjectIds.ToList(),
                PendingEventIds = savedTown.PendingEventIds.ToList(),
                ConsumedEventIds = savedTown.ConsumedEventIds.ToList()
            },
            Mina = new MinaDto
            {
                Activity = snapshot.Mina.Activity,
                Location = snapshot.Mina.Location,
                ProjectId = snapshot.Mina.ProjectId,
                ShortIdleStretchPlayed = snapshot.Mina.ShortIdleStretchPlayed,
                WorkshopCelebrated = snapshot.Mina.WorkshopCelebrated
            },
            Player = new PlayerDto
            {
                TotalFocusTicks = snapshot.Player.TotalFocusTicks,
                TodayUtcDate = snapshot.Player.TodayUtcDate,
                TodayFocusTicks = snapshot.Player.TodayFocusTicks
            },
            PendingPresentation = new PendingPresentationDto
            {
                PendingIds = savedPresentation.PendingIds.ToList(),
                ConsumedIds = savedPresentation.ConsumedIds.ToList()
            }
        };

        // Use the same validations on save and load, before any bytes are written.
        _ = FromEnvelope(envelope);
        return envelope;
    }

    public static StoredGameState FromEnvelope(SaveEnvelopeV1 envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        if (envelope.SchemaVersion != 1 || envelope.SaveRevision < 0
            || envelope.SavedAtUtc.Offset != TimeSpan.Zero || envelope.SavedAtUtc == default)
        {
            throw new InvalidDataException("Unsupported schema, revision, or timestamp.");
        }

        var settings = Required(envelope.Settings);
        var focus = Required(envelope.Focus);
        var town = Required(envelope.Town);
        var mina = Required(envelope.Mina);
        var player = Required(envelope.Player);
        var presentation = Required(envelope.PendingPresentation);
        var companion = Required(settings.Companion);
        var ghost = Required(settings.Ghost);
        ValidateMode(settings.DisplayMode);
        if (settings.CompanionScalePercent is not (75 or 100 or 125 or 150)
            || settings.GhostOpacityPercent is not (40 or 55 or 65 or 70 or 85))
        {
            throw new InvalidDataException("Unsupported display preset.");
        }

        ValidatePlacement(companion);
        ValidatePlacement(ghost);
        var ledger = new SessionLedger();
        foreach (var entry in Required(focus.Ledger))
        {
            if (entry is null || entry.CountedTicks > entry.TargetTicks
                || entry.IdleTicks > entry.CountedTicks)
                throw new InvalidDataException("Invalid finalized session duration.");

            var session = FocusSession.Create(FocusSessionId.From(entry.SessionId),
                PositiveDuration(entry.TargetTicks), ProcessNames(entry.IntendedProcessNames));
            session.Start(Utc(entry.StartedAtUtc));
            session.Accumulate(Duration(entry.CountedTicks), Duration(entry.IdleTicks));
            if (entry.Status == nameof(FocusSessionStatus.Completed))
            {
                session.Complete(Utc(entry.EndedAtUtc));
            }
            else if (entry.Status == nameof(FocusSessionStatus.Stopped))
            {
                session.Stop(Utc(entry.EndedAtUtc));
            }
            else
            {
                throw new InvalidDataException("Ledger entry must be terminal.");
            }

            if (!ledger.TryRecord(session))
            {
                throw new InvalidDataException("Duplicate finalized session ID in save.");
            }
        }

        if (town.CurrentProjectId != nameof(ProjectId.RestoreWorkshop))
        {
            throw new InvalidDataException("Unsupported project ID.");
        }

        var project = ProjectSystem.RestoreWorkshop(
            FocusEnergy.FromCountedDuration(Duration(town.ProjectProgressTicks)),
            FocusEnergy.FromCountedDuration(Duration(town.LastObservedCumulativeEnergyTicks)));
        if (project.WorkshopState.ToString() != town.WorkshopState
            || project.LastObservedCumulativeEnergy.CountedDuration > ledger.TotalCountedDuration
            || player.TotalFocusTicks != ledger.TotalCountedDuration.Ticks
            || player.TodayFocusTicks < 0 || player.TodayFocusTicks > player.TotalFocusTicks
            || player.TodayUtcDate == default)
        {
            throw new InvalidDataException("Inconsistent project, ledger, or player totals.");
        }

        var eventState = EventSaveMapper.Restore(project,
            Identifiers(town.UnlockedProjectIds), Identifiers(town.PendingEventIds),
            Identifiers(town.ConsumedEventIds), Identifiers(presentation.PendingIds),
            Identifiers(presentation.ConsumedIds)).State;

        // Match MinaStateMachine.Restore's V1 logical invariants. Animation clips
        // (Walk/Stretch) are transient presentation state and are not serialized.
        if (mina.Activity is not ("Idle" or "Work" or "Rest" or "Celebrate")
            || mina.Location is not ("Home" or "Workshop")
            || mina.ProjectId != nameof(ProjectId.RestoreWorkshop)
            || (mina.Activity == "Idle" ? mina.Location != "Home" : mina.Location != "Workshop")
            || (mina.ShortIdleStretchPlayed && mina.Activity != "Work")
            || (mina.Activity == "Celebrate" && !mina.WorkshopCelebrated))
        {
            throw new InvalidDataException("Invalid Mina logical checkpoint.");
        }

        var activeCheckpoint = focus.ActiveCheckpoint is null ? null : FromDto(focus.ActiveCheckpoint, ledger);
        var modeChanges = Required(focus.ModeChanges).Select(change =>
        {
            ValidateMode(change.DisplayMode);
            return new DisplayModeChangeSnapshot(Utc(change.AtUtc), change.DisplayMode);
        }).ToArray();
        var snapshot = new GameStateSnapshot(
            new SettingsSnapshot(settings.DisplayMode,
                new WindowPlacementSnapshot(companion.MonitorId, companion.Anchor, companion.OffsetX, companion.OffsetY),
                new WindowPlacementSnapshot(ghost.MonitorId, ghost.Anchor, ghost.OffsetX, ghost.OffsetY),
                settings.CompanionScalePercent, settings.GhostOpacityPercent, settings.AudioEnabled,
                settings.OnboardingCompleted, settings.ReducedMotion),
            new FocusSnapshot(ledger, activeCheckpoint, modeChanges),
            EventSaveMapper.Town(project, eventState),
            new MinaSnapshot(mina.Activity, mina.Location, mina.ProjectId,
                mina.ShortIdleStretchPlayed, mina.WorkshopCelebrated),
            new PlayerSnapshot(player.TotalFocusTicks, player.TodayUtcDate, player.TodayFocusTicks),
            EventSaveMapper.Presentation(eventState));
        return new StoredGameState(envelope.SaveRevision, envelope.SavedAtUtc, snapshot);
    }

    private static ActiveSessionDto ToDto(FocusSessionCheckpoint checkpoint) => new()
    {
        SessionId = checkpoint.SessionId.Value,
        Status = checkpoint.Status.ToString(),
        StartedAtUtc = checkpoint.StartedAtUtc,
        EndedAtUtc = checkpoint.EndedAtUtc,
        TargetTicks = checkpoint.TargetDuration.Ticks,
        CountedTicks = checkpoint.CountedDuration.Ticks,
        IdleTicks = checkpoint.IdleDuration.Ticks,
        IntendedProcessNames = checkpoint.IntendedProcessNames.OrderBy(n => n, StringComparer.Ordinal).ToList(),
        ActiveTicks = checkpoint.Activity.ActiveDuration.Ticks,
        ObservedIdleTicks = checkpoint.Activity.IdleDuration.Ticks,
        UnknownTicks = checkpoint.Activity.UnknownDuration.Ticks,
        Processes = checkpoint.Activity.Processes.Select(p => new ProcessDurationDto
        {
            ProcessName = p.ProcessName,
            DurationTicks = p.Duration.Ticks
        }).ToList(),
        TotalRecordedEnergyTicks = checkpoint.TotalRecordedEnergy.CountedDuration.Ticks
    };

    private static FocusSessionCheckpoint FromDto(ActiveSessionDto dto, SessionLedger ledger)
    {
        if (dto.Status is not (nameof(FocusSessionStatus.Running) or nameof(FocusSessionStatus.Suspended))
            || dto.StartedAtUtc is null || dto.EndedAtUtc is not null
            || dto.CountedTicks > dto.TargetTicks || dto.IdleTicks > dto.CountedTicks
            || dto.TotalRecordedEnergyTicks != ledger.TotalCountedDuration.Ticks
            || ledger.Entries.Any(e => e.SessionId.Value == dto.SessionId))
        {
            throw new InvalidDataException("Invalid active session checkpoint.");
        }

        var processes = Required(dto.Processes).Select(p =>
            new ProcessActivityDuration(ProcessName(p.ProcessName), Duration(p.DurationTicks))).ToArray();
        return new FocusSessionCheckpoint(FocusSessionId.From(dto.SessionId),
            Enum.Parse<FocusSessionStatus>(dto.Status), Utc(dto.StartedAtUtc.Value), null,
            PositiveDuration(dto.TargetTicks), Duration(dto.CountedTicks), Duration(dto.IdleTicks),
            ProcessNames(dto.IntendedProcessNames).ToHashSet(StringComparer.OrdinalIgnoreCase),
            new FocusActivitySummary(Duration(dto.ActiveTicks), Duration(dto.ObservedIdleTicks),
                Duration(dto.UnknownTicks), processes),
            FocusEnergy.FromCountedDuration(Duration(dto.TotalRecordedEnergyTicks)));
    }

    private static WindowPlacementDto ToDto(WindowPlacementSnapshot placement) => new()
    {
        MonitorId = placement.MonitorId,
        Anchor = placement.Anchor,
        OffsetX = placement.OffsetX,
        OffsetY = placement.OffsetY
    };

    private static void ValidatePlacement(WindowPlacementDto placement)
    {
        if (placement.MonitorId is null || placement.Anchor is not
            ("BottomLeft" or "BottomRight" or "TopLeft" or "TopRight"))
        {
            throw new InvalidDataException("Invalid window placement.");
        }
    }

    private static void ValidateMode(string? mode)
    {
        if (mode is not ("Companion" or "Hidden" or "Ghost"))
        {
            throw new InvalidDataException("Invalid display mode.");
        }
    }

    private static TimeSpan Duration(long ticks)
    {
        if (ticks < 0) throw new InvalidDataException("Negative duration.");
        return TimeSpan.FromTicks(ticks);
    }

    private static TimeSpan PositiveDuration(long ticks)
    {
        if (ticks <= 0) throw new InvalidDataException("Target duration must be positive.");
        return Duration(ticks);
    }

    private static DateTimeOffset Utc(DateTimeOffset time)
    {
        if (time.Offset != TimeSpan.Zero || time == default)
            throw new InvalidDataException("Save timestamps must be UTC.");
        return time;
    }

    private static string ProcessName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 128
            || name.Any(c => char.IsControl(c) || c is '/' or '\\' or ':'))
        {
            throw new InvalidDataException("Process name must be a bare, non-sensitive name.");
        }

        return name;
    }

    private static IReadOnlyList<string> ProcessNames(List<string>? names) =>
        Required(names).Select(ProcessName).ToArray();

    private static string Identifier(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128
            || value.Any(c => char.IsControl(c) || c is '/' or '\\'))
            throw new InvalidDataException("Invalid logical identifier.");
        return value;
    }

    private static IReadOnlyList<string> Identifiers(List<string>? values) =>
        Required(values).Select(Identifier).ToArray();

    private static T Required<T>(T? value) where T : class =>
        value ?? throw new InvalidDataException("Missing required save section or field.");
}
