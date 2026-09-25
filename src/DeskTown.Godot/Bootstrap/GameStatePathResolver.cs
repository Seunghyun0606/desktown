using DeskTown.Application.Ports;
using DeskTown.Persistence;
using global::Godot;

namespace DeskTown.Presentation.Bootstrap;

/// <summary>Maps the Godot user data location to the platform-neutral JSON store.</summary>
public static class GameStatePathResolver
{
    public static IGameStateStore CreateDefaultStore()
    {
        var path = ProjectSettings.GlobalizePath("user://desktown/save.json");
        return new JsonGameStateStore(path);
    }
}
