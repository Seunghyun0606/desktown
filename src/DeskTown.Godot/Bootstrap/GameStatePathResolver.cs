using DeskTown.Application.Ports;
using DeskTown.Persistence;
using global::Godot;

namespace DeskTown.Presentation.Bootstrap;

/// <summary>Maps the Godot user data location to the platform-neutral JSON store.</summary>
public static class GameStatePathResolver
{
    public static bool IsDemoRequested => OS.GetCmdlineUserArgs().Contains("--desktown-demo");

    public static IGameStateStore CreateDefaultStore()
    {
        var path = IsDemoRequested
            ? System.Environment.GetEnvironmentVariable("DESKTOWN_DEMO_SAVE")
            : ProjectSettings.GlobalizePath("user://desktown/save.json");
        if (IsDemoRequested && (string.IsNullOrWhiteSpace(path) ||
            !Path.IsPathFullyQualified(path) || !File.Exists(path)))
            throw new InvalidOperationException("Demo requires an existing isolated absolute save path.");
        return new JsonGameStateStore(path);
    }
}
