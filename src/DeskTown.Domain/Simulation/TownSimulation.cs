using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;

namespace DeskTown.Domain.Simulation;

/// <summary>
/// Advances the small world from absolute logical time and cumulative Energy.
/// No frame, window, display mode, or platform clock participates in progression.
/// </summary>
public sealed class TownSimulation
{
    private readonly ProjectSystem _project;
    private readonly MinaStateMachine _mina;
    private TimeSpan _logicalTime;

    private TownSimulation(ProjectSystem project, MinaStateMachine mina, TimeSpan logicalTime)
    {
        _project = project;
        _mina = mina;
        _logicalTime = logicalTime;
    }

    public static TownSimulation Create(MinaThresholds? thresholds = null) =>
        new(ProjectSystem.CreateRestoreWorkshop(), new MinaStateMachine(thresholds), TimeSpan.Zero);

    public static TownSimulation Restore(
        TownSimulationCheckpoint checkpoint, MinaThresholds? thresholds = null)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        if (checkpoint.LogicalTime < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(checkpoint));

        return new TownSimulation(
            ProjectSystem.RestoreWorkshop(checkpoint.ProjectProgress, checkpoint.LastObservedEnergy),
            MinaStateMachine.Restore(checkpoint.Mina, thresholds), checkpoint.LogicalTime);
    }

    public TownSimulationCheckpoint CaptureCheckpoint() =>
        new(_logicalTime, _project.Progress, _project.LastObservedCumulativeEnergy,
            _mina.CaptureCheckpoint());

    public TownSimulationSnapshot Snapshot => new(
        _logicalTime,
        new TownProjection(_project.CurrentProjectId, _project.WorkshopState,
            _project.Progress, _project.Target, _mina.State),
        new CompanionProjection(_mina.State),
        new GhostProjection(_mina.State));

    /// <summary>
    /// One observation may be a 1-second tick or a long jump after resume.
    /// The caller supplies continuous idle age, never cumulative idle totals.
    /// </summary>
    public TownSimulationStep AdvanceTo(
        TimeSpan logicalTime,
        FocusEnergy cumulativeEnergy,
        FocusSessionStatus? sessionStatus,
        TimeSpan consecutiveIdle)
    {
        if (logicalTime < _logicalTime)
            throw new ArgumentOutOfRangeException(nameof(logicalTime), "Logical time cannot go backwards.");
        if (consecutiveIdle < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(consecutiveIdle));
        if (sessionStatus is not null && !Enum.IsDefined(sessionStatus.Value))
            throw new ArgumentOutOfRangeException(nameof(sessionStatus));

        var project = _project.ApplyCumulativeEnergy(cumulativeEnergy);
        var mina = _mina.Observe(sessionStatus, consecutiveIdle);
        _logicalTime = logicalTime;
        return new TownSimulationStep(Snapshot, project, mina);
    }

    /// <summary>Only the later pending-reveal flow calls this when Town opens.</summary>
    public MinaTransition RevealWorkshopCompletion()
    {
        if (!_project.IsComplete)
            throw new InvalidOperationException("The Workshop has not been completed.");
        return _mina.CelebrateProject(new ProjectCompleted(_project.CurrentProjectId));
    }

    public MinaTransition AcknowledgeCelebration() => _mina.AcknowledgeCelebration();
}
