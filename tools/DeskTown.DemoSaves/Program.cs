using System.Globalization;
using DeskTown.Application.Persistence;
using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;
using DeskTown.Domain.Simulation;
using DeskTown.Persistence;

if (args.Length is < 1 or > 2)
    throw new ArgumentException("Usage: DeskTown.DemoSaves <output-dir> [yyyy-MM-dd]");

var day = args.Length == 2
    ? DateOnly.ParseExact(args[1], "yyyy-MM-dd", CultureInfo.InvariantCulture)
    : new DateOnly(2026, 9, 25);
var start = new DateTimeOffset(day.ToDateTime(new TimeOnly(9, 0)), TimeSpan.Zero);
var output = Path.GetFullPath(args[0]);

foreach (var (name, duration, completed, railway) in new[]
{
    ("workshop-repairing", TimeSpan.FromMinutes(10), false, false),
    ("workshop-reveal-pending", TimeSpan.FromMinutes(25), true, false),
    ("railway-teaser", TimeSpan.FromMinutes(25), true, true)
})
{
    var session = FocusSession.Create(
        FocusSessionId.From(Guid.Parse("fe000000-0000-4000-8000-000000000001")),
        TimeSpan.FromMinutes(25));
    session.Start(start);
    session.Accumulate(duration, TimeSpan.Zero);
    if (completed) session.Complete(start + duration);
    else session.Stop(start + duration);
    var ledger = new SessionLedger();
    if (!ledger.TryRecord(session)) throw new InvalidDataException("Demo ledger rejected a session.");

    var town = TownSimulation.Create();
    town.AdvanceTo(duration, FocusEnergy.FromCountedDuration(duration), null, TimeSpan.Zero);
    if (railway)
    {
        town.RevealWorkshopCompletion();
        town.AcknowledgeCelebration();
        town.AcknowledgeRailwayDiscovery();
    }
    var state = town.CaptureCheckpoint();
    var project = ProjectSystem.RestoreWorkshop(state.ProjectProgress, state.LastObservedEnergy);
    var snapshot = new GameStateSnapshot(
        new SettingsSnapshot("Hidden",
            new WindowPlacementSnapshot("screen:0", "BottomRight", 24, 24),
            new WindowPlacementSnapshot("screen:0", "BottomRight", 48, 48),
            100, 65, false, true, false),
        new FocusSnapshot(ledger, null, []),
        EventSaveMapper.Town(project, state.Events),
        new MinaSnapshot(state.Mina.State.Activity.ToString(),
            state.Mina.State.Location.ToString(), nameof(ProjectId.RestoreWorkshop),
            state.Mina.ShortIdleStretchPlayed, state.Mina.WorkshopCelebrated),
        new PlayerSnapshot(ledger.TotalCountedDuration.Ticks, day, duration.Ticks),
        EventSaveMapper.Presentation(state.Events));

    var bytes = JsonSaveCodec.Encode(snapshot, 1, start + duration);
    var restored = JsonSaveCodec.Decode(bytes).Snapshot;
    if (restored.Focus.ActiveCheckpoint is not null ||
        restored.Town.Project.WorkshopState != project.WorkshopState ||
        restored.Town.Project.Progress.CountedDuration != duration ||
        restored.Town.PendingEventIds.Contains("old_railway_map") ||
        restored.PendingPresentation.PendingIds.Contains("workshop_complete") != (completed && !railway) ||
        restored.Town.UnlockedProjectIds.Contains(nameof(ProjectId.ExploreOldRailway)) != railway)
        throw new InvalidDataException($"Demo state validation failed: {name}");

    var folder = Path.Combine(output, name);
    Directory.CreateDirectory(folder);
    await File.WriteAllBytesAsync(Path.Combine(folder, "save.json"), bytes);
    Console.WriteLine($"{name}: {project.WorkshopState}, {duration.TotalMinutes:0} minutes");
}
