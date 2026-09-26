using DeskTown.Application.Display;
using DeskTown.Application.Lifecycle;
using DeskTown.Application.Runtime;
using DeskTown.Application.Tests.Fakes;
using DeskTown.Domain.Focus;
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
            var interruptedReveal = await PrototypeRuntime.OpenAsync(store,
                new FakeActivityTracker(), wall, new FakeMonotonicClock(),
                TimeSpan.FromSeconds(15));
            Assert.True(interruptedReveal.World.Town.Events.WorkshopRevealPending);
            await interruptedReveal.RevealWorkshopAsync();
            await interruptedReveal.AcknowledgeWorkshopAsync();
            Assert.True(interruptedReveal.World.Town.Events.RailwayDiscoveryPending);
            await interruptedReveal.AcknowledgeRailwayAsync();
            var finalRestart = await PrototypeRuntime.OpenAsync(store, new FakeActivityTracker(),
                wall, new FakeMonotonicClock(), TimeSpan.FromSeconds(15));
            Assert.True(finalRestart.World.Town.Events.RailwayTeaserUnlocked);
            Assert.False(finalRestart.World.Town.Events.RailwayDiscoveryPending);
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

    [Fact]
    public async Task SystemPauseRemainsDurableAndDoesNotAwardLockedTime()
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
            clock.Advance(TimeSpan.FromSeconds(1));
            wall.Advance(TimeSpan.FromSeconds(1));
            await game.TickAsync(TimeSpan.Zero);

            clock.Advance(TimeSpan.FromHours(2));
            wall.Advance(TimeSpan.FromHours(2));
            await game.SuspendForSystemAsync();
            Assert.Equal(TimeSpan.FromSeconds(1), game.ActiveSession!.CountedDuration);
            var restarted = await PrototypeRuntime.OpenAsync(store, new FakeActivityTracker(),
                wall, new FakeMonotonicClock(), TimeSpan.FromSeconds(15));
            Assert.Equal(FocusSessionStatus.Suspended, restarted.PendingRecovery!.Status);
            Assert.Equal(TimeSpan.FromSeconds(1), restarted.PendingRecovery.CountedDuration);

            clock.Advance(TimeSpan.FromHours(1));
            wall.Advance(TimeSpan.FromHours(1));
            await game.ResumeForSystemAsync();
            clock.Advance(TimeSpan.FromSeconds(1));
            wall.Advance(TimeSpan.FromSeconds(1));
            await game.TickAsync(TimeSpan.Zero);
            Assert.Equal(TimeSpan.FromSeconds(2), game.ActiveSession!.CountedDuration);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public async Task CompletedTodayUsesLocalEndDateWhileV1SummaryRemainsUtc()
    {
        var directory = NewDirectory();
        try
        {
            var store = new JsonGameStateStore(Path.Combine(directory, "save.json"));
            var wall = new FakeWallClock(new DateTimeOffset(2026, 9, 25, 14, 59, 30, TimeSpan.Zero));
            var clock = new FakeMonotonicClock();
            var korea = TimeZoneInfo.CreateCustomTimeZone("test-korea", TimeSpan.FromHours(9),
                "test-korea", "test-korea");
            var game = await PrototypeRuntime.OpenAsync(store, new FakeActivityTracker(),
                wall, clock, TimeSpan.FromSeconds(15), displayTimeZone: korea);
            await game.StartAsync(TimeSpan.FromMinutes(1), [], DisplayMode.Hidden);
            clock.Advance(TimeSpan.FromMinutes(1));
            wall.Advance(TimeSpan.FromMinutes(1));
            await game.TickAsync(TimeSpan.Zero);

            Assert.Equal(new DateOnly(2026, 9, 26), game.TodayDisplayDate);
            Assert.Equal(TimeSpan.FromMinutes(1), game.TodayFocus);
            var saved = await store.LoadAsync();
            Assert.Equal(new DateOnly(2026, 9, 25), saved!.Snapshot.Player.TodayUtcDate);
            Assert.Equal(TimeSpan.FromMinutes(1).Ticks, saved.Snapshot.Player.TodayFocusTicks);

            wall.Advance(TimeSpan.FromDays(1));
            Assert.Equal(TimeSpan.Zero, game.TodayFocus);
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
