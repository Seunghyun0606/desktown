using DeskTown.Application.Persistence;

namespace DeskTown.Application.Ports;

/// <summary>A complete checkpoint in; a validated checkpoint out. Path choice is an outer-layer concern.</summary>
public interface IGameStateStore
{
    Task<StoredGameState?> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(
        GameStateSnapshot snapshot,
        long revision,
        DateTimeOffset savedAtUtc,
        CancellationToken cancellationToken = default);
}
