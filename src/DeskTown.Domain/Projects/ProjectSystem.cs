using DeskTown.Domain.Focus;

namespace DeskTown.Domain.Projects;

/// <summary>
/// Applies cumulative Focus Energy to the single playable v0.1 project without
/// depending on display, rendering, platform APIs, or presentation events.
/// </summary>
public sealed class ProjectSystem
{
    public static readonly FocusEnergy RestoreWorkshopTarget =
        FocusEnergy.FromCountedDuration(TimeSpan.FromMinutes(25));

    private FocusEnergy _lastObservedCumulativeEnergy;

    private ProjectSystem(
        FocusEnergy progress,
        FocusEnergy cumulativeEnergyBaseline)
    {
        Progress = progress;
        _lastObservedCumulativeEnergy = cumulativeEnergyBaseline;
    }

    public ProjectId CurrentProjectId => ProjectId.RestoreWorkshop;

    public FocusEnergy Progress { get; private set; }

    public FocusEnergy Target => RestoreWorkshopTarget;

    public FocusEnergy LastObservedCumulativeEnergy => _lastObservedCumulativeEnergy;

    public WorkshopState WorkshopState => Progress == FocusEnergy.Zero
        ? global::DeskTown.Domain.Projects.WorkshopState.Broken
        : Progress.CompareTo(Target) < 0
            ? global::DeskTown.Domain.Projects.WorkshopState.Repairing
            : global::DeskTown.Domain.Projects.WorkshopState.Complete;

    public bool IsComplete =>
        WorkshopState == global::DeskTown.Domain.Projects.WorkshopState.Complete;

    /// <summary>
    /// Creates Restore Workshop. Energy at or before the baseline is historical
    /// and will never be applied to this project.
    /// </summary>
    public static ProjectSystem CreateRestoreWorkshop(
        FocusEnergy cumulativeEnergyBaseline = default)
    {
        return new ProjectSystem(FocusEnergy.Zero, cumulativeEnergyBaseline);
    }

    /// <summary>
    /// Rehydrates the aggregate from a future persistence adapter. This method
    /// validates domain state but performs no serialization or file access.
    /// </summary>
    public static ProjectSystem RestoreWorkshop(
        FocusEnergy progress,
        FocusEnergy lastObservedCumulativeEnergy)
    {
        if (progress.CompareTo(RestoreWorkshopTarget) > 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(progress),
                "Restore Workshop progress cannot exceed its target.");
        }

        if (lastObservedCumulativeEnergy.CompareTo(progress) < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lastObservedCumulativeEnergy),
                "Observed cumulative Energy cannot be below project progress.");
        }

        return new ProjectSystem(progress, lastObservedCumulativeEnergy);
    }

    /// <summary>
    /// Observes the latest total from IEnergyPolicy.CalculateTotal and applies
    /// only total.DeltaSince(the previous observation). Replaying a total is a
    /// no-op; a decreasing total is rejected before state mutation.
    /// </summary>
    public ProjectEnergyApplication ApplyCumulativeEnergy(FocusEnergy cumulativeEnergy)
    {
        var observedDelta = cumulativeEnergy.DeltaSince(_lastObservedCumulativeEnergy);
        var wasComplete = IsComplete;
        var remaining = Target.DeltaSince(Progress);
        var applied = Minimum(observedDelta, remaining);
        var nextProgress = Progress + applied;

        _lastObservedCumulativeEnergy = cumulativeEnergy;
        Progress = nextProgress;

        var completion = !wasComplete && IsComplete
            ? new ProjectCompleted(CurrentProjectId)
            : null;

        return new ProjectEnergyApplication(
            observedDelta,
            applied,
            Progress,
            WorkshopState,
            completion);
    }

    private static FocusEnergy Minimum(FocusEnergy left, FocusEnergy right)
    {
        return left.CompareTo(right) <= 0 ? left : right;
    }
}
