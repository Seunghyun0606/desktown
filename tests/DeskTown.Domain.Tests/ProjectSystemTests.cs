using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;

namespace DeskTown.Domain.Tests;

public sealed class ProjectSystemTests
{
    [Fact]
    public void New_project_is_broken_and_has_an_exact_25_focus_target()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();

        Assert.Equal(ProjectId.RestoreWorkshop, project.CurrentProjectId);
        Assert.Equal(FocusEnergy.Zero, project.Progress);
        Assert.Equal(TimeSpan.FromMinutes(25), project.Target.CountedDuration);
        Assert.Equal(25, project.Target.DisplayedWholeMinutes);
        Assert.Equal(WorkshopState.Broken, project.WorkshopState);
        Assert.False(project.IsComplete);
    }

    [Fact]
    public void First_positive_delta_moves_workshop_to_repairing()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();

        var result = project.ApplyCumulativeEnergy(Energy(seconds: 1));

        Assert.Equal(Energy(seconds: 1), project.Progress);
        Assert.Equal(WorkshopState.Repairing, project.WorkshopState);
        Assert.True(result.Changed);
        Assert.Null(result.Completion);
    }

    [Fact]
    public void Zero_total_keeps_workshop_broken()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();

        var result = project.ApplyCumulativeEnergy(FocusEnergy.Zero);

        Assert.False(result.Changed);
        Assert.Equal(FocusEnergy.Zero, result.ObservedDelta);
        Assert.Equal(WorkshopState.Broken, result.WorkshopState);
        Assert.Null(result.Completion);
    }

    [Fact]
    public void Progress_below_target_does_not_complete()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();

        var result = project.ApplyCumulativeEnergy(
            FocusEnergy.FromCountedDuration(TimeSpan.FromMinutes(25) - TimeSpan.FromTicks(1)));

        Assert.Equal(WorkshopState.Repairing, result.WorkshopState);
        Assert.False(project.IsComplete);
        Assert.Null(result.Completion);
    }

    [Fact]
    public void Exact_target_completes_and_emits_the_domain_event()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();

        var result = project.ApplyCumulativeEnergy(Energy(minutes: 25));

        Assert.True(project.IsComplete);
        Assert.Equal(project.Target, project.Progress);
        Assert.Equal(WorkshopState.Complete, result.WorkshopState);
        var completion = Assert.IsType<ProjectCompleted>(result.Completion);
        Assert.Equal(ProjectId.RestoreWorkshop, completion.ProjectId);
    }

    [Fact]
    public void Progress_is_clamped_when_observed_delta_exceeds_target()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();

        var result = project.ApplyCumulativeEnergy(Energy(minutes: 40));

        Assert.Equal(Energy(minutes: 40), result.ObservedDelta);
        Assert.Equal(Energy(minutes: 25), result.AppliedToProject);
        Assert.Equal(Energy(minutes: 25), project.Progress);
        Assert.Equal(Energy(minutes: 40), project.LastObservedCumulativeEnergy);
        Assert.NotNull(result.Completion);
    }

    [Fact]
    public void Replaying_same_cumulative_total_is_an_idempotent_no_op()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();
        project.ApplyCumulativeEnergy(Energy(minutes: 10));

        var replay = project.ApplyCumulativeEnergy(Energy(minutes: 10));

        Assert.False(replay.Changed);
        Assert.Equal(FocusEnergy.Zero, replay.ObservedDelta);
        Assert.Equal(FocusEnergy.Zero, replay.AppliedToProject);
        Assert.Equal(Energy(minutes: 10), replay.Progress);
        Assert.Null(replay.Completion);
    }

    [Fact]
    public void Completion_is_emitted_exactly_once_across_replay_and_later_energy()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();

        var first = project.ApplyCumulativeEnergy(Energy(minutes: 25));
        var replay = project.ApplyCumulativeEnergy(Energy(minutes: 25));
        var later = project.ApplyCumulativeEnergy(Energy(minutes: 30));

        Assert.NotNull(first.Completion);
        Assert.Null(replay.Completion);
        Assert.Null(later.Completion);
        Assert.Equal(project.Target, project.Progress);
        Assert.Equal(Energy(minutes: 30), project.LastObservedCumulativeEnergy);
    }

    [Fact]
    public void Split_cumulative_updates_equal_one_shot_progress()
    {
        var split = ProjectSystem.CreateRestoreWorkshop();
        split.ApplyCumulativeEnergy(Energy(minutes: 7));
        split.ApplyCumulativeEnergy(Energy(minutes: 18));
        var splitCompletion = split.ApplyCumulativeEnergy(Energy(minutes: 25));

        var oneShot = ProjectSystem.CreateRestoreWorkshop();
        var oneShotCompletion = oneShot.ApplyCumulativeEnergy(Energy(minutes: 25));

        Assert.Equal(oneShot.Progress, split.Progress);
        Assert.Equal(oneShot.WorkshopState, split.WorkshopState);
        Assert.NotNull(splitCompletion.Completion);
        Assert.NotNull(oneShotCompletion.Completion);
    }

    [Fact]
    public void Baseline_marks_historical_energy_as_already_observed()
    {
        var project = ProjectSystem.CreateRestoreWorkshop(Energy(minutes: 100));

        var replayedHistory = project.ApplyCumulativeEnergy(Energy(minutes: 100));
        var newEnergy = project.ApplyCumulativeEnergy(Energy(minutes: 105));

        Assert.False(replayedHistory.Changed);
        Assert.Equal(Energy(minutes: 5), newEnergy.AppliedToProject);
        Assert.Equal(Energy(minutes: 5), project.Progress);
        Assert.Equal(WorkshopState.Repairing, project.WorkshopState);
    }

    [Fact]
    public void Restored_progress_and_high_watermark_continue_from_only_new_energy()
    {
        var project = ProjectSystem.RestoreWorkshop(
            progress: Energy(minutes: 12),
            lastObservedCumulativeEnergy: Energy(minutes: 100));

        var result = project.ApplyCumulativeEnergy(Energy(minutes: 103));

        Assert.Equal(Energy(minutes: 3), result.ObservedDelta);
        Assert.Equal(Energy(minutes: 15), project.Progress);
        Assert.Equal(Energy(minutes: 103), project.LastObservedCumulativeEnergy);
        Assert.Null(result.Completion);
    }

    [Fact]
    public void Restored_complete_project_never_reemits_completion()
    {
        var project = ProjectSystem.RestoreWorkshop(
            progress: Energy(minutes: 25),
            lastObservedCumulativeEnergy: Energy(minutes: 25));

        var result = project.ApplyCumulativeEnergy(Energy(minutes: 30));

        Assert.True(project.IsComplete);
        Assert.Null(result.Completion);
    }

    [Fact]
    public void Restore_rejects_progress_above_target()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProjectSystem.RestoreWorkshop(
                progress: Energy(minutes: 26),
                lastObservedCumulativeEnergy: Energy(minutes: 26)));
    }

    [Fact]
    public void Restore_rejects_high_watermark_below_progress()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProjectSystem.RestoreWorkshop(
                progress: Energy(minutes: 10),
                lastObservedCumulativeEnergy: Energy(minutes: 9)));
    }

    [Fact]
    public void Decreasing_cumulative_total_is_rejected_without_mutation()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();
        project.ApplyCumulativeEnergy(Energy(minutes: 10));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            project.ApplyCumulativeEnergy(Energy(minutes: 9)));

        Assert.Equal(Energy(minutes: 10), project.Progress);
        Assert.Equal(Energy(minutes: 10), project.LastObservedCumulativeEnergy);
        Assert.Equal(WorkshopState.Repairing, project.WorkshopState);
    }

    [Fact]
    public void Delta_after_partial_progress_applies_only_new_energy()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();
        project.ApplyCumulativeEnergy(Energy(minutes: 5));

        var result = project.ApplyCumulativeEnergy(Energy(minutes: 8));

        Assert.Equal(Energy(minutes: 3), result.ObservedDelta);
        Assert.Equal(Energy(minutes: 3), result.AppliedToProject);
        Assert.Equal(Energy(minutes: 8), project.Progress);
    }

    [Fact]
    public void Subsecond_energy_is_preserved_without_rounding_drift()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();

        project.ApplyCumulativeEnergy(
            FocusEnergy.FromCountedDuration(TimeSpan.FromMilliseconds(30_500)));
        project.ApplyCumulativeEnergy(
            FocusEnergy.FromCountedDuration(TimeSpan.FromSeconds(61)));

        Assert.Equal(TimeSpan.FromSeconds(61), project.Progress.CountedDuration);
        Assert.Equal(1, project.Progress.DisplayedWholeMinutes);
    }

    [Fact]
    public void Completion_result_contains_clamped_final_snapshot()
    {
        var project = ProjectSystem.CreateRestoreWorkshop();

        var result = project.ApplyCumulativeEnergy(Energy(minutes: 26));

        Assert.Equal(project.Target, result.Progress);
        Assert.Equal(WorkshopState.Complete, result.WorkshopState);
        var completion = Assert.IsType<ProjectCompleted>(result.Completion);
        Assert.Equal(ProjectId.RestoreWorkshop, completion.ProjectId);
    }

    [Fact]
    public void Project_domain_has_no_display_godot_or_platform_dependency()
    {
        var projectTypes = new[]
        {
            typeof(ProjectId),
            typeof(WorkshopState),
            typeof(ProjectCompleted),
            typeof(ProjectEnergyApplication),
            typeof(ProjectSystem)
        };

        Assert.All(
            projectTypes,
            type =>
            {
                Assert.DoesNotContain("DisplayMode", type.ToString(), StringComparison.Ordinal);
                Assert.DoesNotContain("Godot", type.ToString(), StringComparison.Ordinal);
                Assert.DoesNotContain("Windows", type.ToString(), StringComparison.Ordinal);
            });

        Assert.All(
            projectTypes.SelectMany(type => type.GetMembers()),
            member => Assert.DoesNotContain("DisplayMode", member.Name, StringComparison.Ordinal));
    }

    private static FocusEnergy Energy(long minutes = 0, long seconds = 0)
    {
        return FocusEnergy.FromCountedDuration(
            TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds));
    }
}
