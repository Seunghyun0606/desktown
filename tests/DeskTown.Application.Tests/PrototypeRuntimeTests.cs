using DeskTown.Application.Display;
using DeskTown.Application.Lifecycle;
using DeskTown.Application.Runtime;
using DeskTown.Application.Tests.Fakes;
using DeskTown.Domain.Projects;
using DeskTown.Persistence;

namespace DeskTown.Application.Tests;

public sealed class PrototypeRuntimeTests
{
    [Fact]
    public async Task Mode_changes_do_not_alter_completion_and_reveal_survives_restart()
    {
        var directory = NewDirectory();
        try
        {
            var store = new JsonGameStateStore(Path.Combine(directory, "save.json"));
            var wall = new FakeWallClock(new DateTimeOffset(2026, 9, 25, 8, 0, 0, TimeSpan.Zero));
            var clock = new FakeMonotonicClock();
            var game = await PrototypeRuntime.OpenAsync(store, new FakeActivityTracker(),
                wall, clock, TimeSpan.FromSeconds(15));
            await game.CompleteOnboardingAsync();
            await game.StartAsync(TimeSpan.FromMinutes(25), [], DisplayMode.Companion);

            for (var minute = 0; minute < 25; minute++)
            {
                if (minute == 8) await game.ChangeModeAsync(DisplayMode.Hidden);
                if (minute == 16) await game.ChangeModeAsync(DisplayMode.Companion);
                clock.Advance(TimeSpan.FromMinutes(1));
                wall.Advance(TimeSpan.FromMinutes(1));
                await game.TickAsync(TimeSpan.Zero);
            }

            Assert.Null(game.ActiveSession);
            Assert.Equal(WorkshopState.Complete, game.World.Town.Workshop);
            Assert.True(game.World.Town.Events.WorkshopRevealPending);
            Assert.Equal(TimeSpan.FromMinutes(25), game.TotalFocus);

            var restarted = await PrototypeRuntime.OpenAsync(store, new FakeActivityTracker(),
                wall, new FakeMonotonicClock(), TimeSpan.FromSeconds(15));
            Assert.Equal(WorkshopState.Complete, restarted.World.Town.Workshop);
            Assert.True(restarted.World.Town.Events.WorkshopRevealPending);
            await restarted.RevealWorkshopAsync();
            await restarted.AcknowledgeWorkshopAsync();
            Assert.True(restarted.World.Town.Events.RailwayDiscoveryPending);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public async Task Restart_recovery_ends_at_checkpoint_without_wall_time()
    {
        var directory = NewDirectory();
        try
        {
            var store = new JsonGameStateStore(Path.Combine(directory, "save.json"));
            var wall = new FakeWallClock(new DateTimeOffset(2026, 9, 25, 8, 0, 0, TimeSpan.Zero));
            var clock = new FakeMonotonicClock();
            var game = await PrototypeRuntime.OpenAsync(store, new FakeActivityTracker(),
                wall, clock, TimeSpan.FromSeconds(15));
            await game.StartAsync(TimeSpan.FromMinutes(25), [], DisplayMode.Hidden);
            clock.Advance(TimeSpan.FromMinutes(1));
            wall.Advance(TimeSpan.FromMinutes(1));
            await game.TickAsync(TimeSpan.Zero);

            wall.Advance(TimeSpan.FromHours(3));
            var restarted = await PrototypeRuntime.OpenAsync(store, new FakeActivityTracker(),
                wall, new FakeMonotonicClock(), TimeSpan.FromSeconds(15));
            Assert.NotNull(restarted.PendingRecovery);
            await restarted.ResolveRecoveryAsync(RecoveryChoice.EndAtCheckpoint);
            Assert.Equal(TimeSpan.FromMinutes(1), restarted.TotalFocus);
            Assert.Null(restarted.PendingRecovery);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    private static string NewDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "desktown-runtime-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
