using DeskTown.Application.Display;
using DeskTown.Application.Lifecycle;
using DeskTown.Application.Runtime;
using DeskTown.Persistence;
using DeskTown.Platform.Windows.Activity;
using global::Godot;

namespace DeskTown.Presentation.Bootstrap;

/// <summary>CI-only process boundary check using the exported app's real JSON store.</summary>
internal static class ExportedSaveSmoke
{
    public static async Task RunAsync(SceneTree tree)
    {
        try
        {
            var arguments = OS.GetCmdlineUserArgs().ToArray();
            if (arguments.Length != 1 ||
                arguments[0] is not ("--desktown-ci-save-smoke=write" or
                    "--desktown-ci-save-smoke=recover" or
                    "--desktown-ci-save-smoke=check"))
                throw new ArgumentException("An explicit CI save smoke phase is required.");

            var path = System.Environment.GetEnvironmentVariable("DESKTOWN_CI_SMOKE_SAVE");
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
                throw new ArgumentException("An absolute isolated CI save path is required.");

            var store = new JsonGameStateStore(path);
            var game = await PrototypeRuntime.OpenAsync(store, new WindowsActivityTracker(),
                new SystemWallClock(), new StopwatchMonotonicClock(), TimeSpan.FromSeconds(15));
            var phase = arguments[0]["--desktown-ci-save-smoke=".Length..];
            switch (phase)
            {
                case "write":
                    Require(!File.Exists(path) && game.PendingRecovery is null,
                        "The write phase requires a fresh isolated save.");
                    await game.CompleteOnboardingAsync();
                    await game.ChangeSettingsAsync(game.Settings with
                    {
                        CompanionScalePercent = 125,
                        AudioEnabled = false,
                        ReducedMotion = true
                    });
                    await game.StartAsync(TimeSpan.FromMinutes(25), [], DisplayMode.Hidden);
                    await Task.Delay(1100);
                    await game.TickAsync(TimeSpan.Zero);
                    await game.SaveAsync(CheckpointReason.Quit);
                    Require(File.Exists(path) && game.ActiveSession?.CountedDuration > TimeSpan.Zero,
                        "The active checkpoint was not saved.");
                    break;
                case "recover":
                    Require(game.OnboardingCompleted &&
                        game.PendingRecovery?.CountedDuration > TimeSpan.Zero &&
                        game.Settings.CompanionScalePercent == 125 &&
                        !game.Settings.AudioEnabled && game.Settings.ReducedMotion &&
                        game.Settings.DisplayMode == nameof(DisplayMode.Hidden),
                        "The exported app did not restore its saved settings and Focus checkpoint.");
                    await game.ResolveRecoveryAsync(RecoveryChoice.EndAtCheckpoint);
                    Require(game.PendingRecovery is null && game.ActiveSession is null &&
                        game.TotalFocus > TimeSpan.Zero,
                        "Recovery did not end at the saved checkpoint.");
                    break;
                case "check":
                    Require(game.OnboardingCompleted && game.PendingRecovery is null &&
                        game.ActiveSession is null && game.Settings.CompanionScalePercent == 125 &&
                        !game.Settings.AudioEnabled && game.Settings.ReducedMotion &&
                        game.Settings.DisplayMode == nameof(DisplayMode.Hidden) &&
                        game.TotalFocus > TimeSpan.Zero,
                        "The recovered save was not durable across the next launch.");
                    break;
            }

            GD.Print($"DESKTOWN_CI_SAVE_SMOKE_OK:{phase}");
            tree.Quit();
        }
        catch (Exception error)
        {
            GD.PrintErr($"DESKTOWN_CI_SAVE_SMOKE_FAILED:{error.GetType().Name}: {error.Message}");
            tree.Quit(1);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }
}
