using System.Text.Json.Nodes;

namespace DeskTown.Persistence;

/// <summary>A pure, one-version transformation. The store owns revision and integrity.</summary>
public interface ISaveMigration
{
    int FromVersion { get; }
    int ToVersion { get; }
    JsonNode Apply(JsonNode source);
}
