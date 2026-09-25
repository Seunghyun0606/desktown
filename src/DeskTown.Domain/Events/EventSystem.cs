using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;

namespace DeskTown.Domain.Events;

/// <summary>
/// Tracks the single Workshop reveal and Railway discovery. A completion is
/// committed to ProjectSystem first; presentation acknowledgements are separate.
/// </summary>
public sealed class EventSystem
{
    public const string WorkshopRevealId = "workshop_complete";
    public const string RailwayDiscoveryId = "old_railway_map";
    public static readonly FocusEnergy RailwayTeaserTarget =
        FocusEnergy.FromCountedDuration(TimeSpan.FromMinutes(45));

    private readonly ProjectSystem _project;
    private EventSystemCheckpoint _state;

    private EventSystem(ProjectSystem project, EventSystemCheckpoint state)
    {
        _project = project;
        _state = state;
    }

    public EventSystemCheckpoint State => _state;

    public bool IsRailwayPlayable => false;

    public static EventSystem Create(ProjectSystem project) =>
        new(project ?? throw new ArgumentNullException(nameof(project)), EventSystemCheckpoint.Empty);

    public static EventSystem Restore(ProjectSystem project, EventSystemCheckpoint state)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(state);
        Validate(project, state);
        return new EventSystem(project, state);
    }

    /// <summary>
    /// Reconciles a completed Project even when a crash occurred after the
    /// project commit but before the pending presentation was saved.
    /// </summary>
    public EventTransition ReconcileCompletion()
    {
        if (!_project.IsComplete || _state.WorkshopRevealPending || _state.WorkshopRevealConsumed)
            return new EventTransition(_state, false);

        _state = _state with { WorkshopRevealPending = true };
        return new EventTransition(_state, true);
    }

    public EventTransition ObserveCompletion(ProjectCompleted completion)
    {
        ArgumentNullException.ThrowIfNull(completion);
        if (completion.ProjectId != ProjectId.RestoreWorkshop || !_project.IsComplete)
            throw new InvalidOperationException("A completed Workshop is required.");
        return ReconcileCompletion();
    }

    public EventTransition AcknowledgeWorkshopReveal()
    {
        if (_state.WorkshopRevealConsumed) return new EventTransition(_state, false);
        if (!_state.WorkshopRevealPending || !_project.IsComplete)
            throw new InvalidOperationException("There is no pending Workshop reveal.");

        _state = _state with
        {
            WorkshopRevealPending = false,
            WorkshopRevealConsumed = true,
            RailwayDiscoveryPending = true,
            RailwayTeaserUnlocked = true
        };
        return new EventTransition(_state, true);
    }

    public EventTransition AcknowledgeRailwayDiscovery()
    {
        if (_state.RailwayDiscoveryConsumed) return new EventTransition(_state, false);
        if (!_state.RailwayDiscoveryPending)
            throw new InvalidOperationException("There is no pending Railway discovery.");

        _state = _state with { RailwayDiscoveryPending = false, RailwayDiscoveryConsumed = true };
        return new EventTransition(_state, true);
    }

    private static void Validate(ProjectSystem project, EventSystemCheckpoint state)
    {
        if ((state.WorkshopRevealPending && state.WorkshopRevealConsumed)
            || (state.RailwayDiscoveryPending && state.RailwayDiscoveryConsumed)
            || (!project.IsComplete && (state.WorkshopRevealPending || state.WorkshopRevealConsumed
                || state.RailwayDiscoveryPending || state.RailwayDiscoveryConsumed
                || state.RailwayTeaserUnlocked))
            || (state.WorkshopRevealPending && (state.RailwayDiscoveryPending
                || state.RailwayDiscoveryConsumed || state.RailwayTeaserUnlocked))
            || (state.WorkshopRevealConsumed != state.RailwayTeaserUnlocked)
            || (state.WorkshopRevealConsumed !=
                (state.RailwayDiscoveryPending || state.RailwayDiscoveryConsumed)))
            throw new ArgumentException("Invalid Workshop/Railway event checkpoint.", nameof(state));
    }
}
