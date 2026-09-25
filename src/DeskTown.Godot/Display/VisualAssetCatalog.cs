using System.Text.Json;
using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>Presentation-only asset lookup. Missing art keeps geometric fallback.</summary>
internal static class VisualAssetCatalog
{
    private const string CatalogPath = "res://assets/catalogs/asset_catalog.json";
    private static readonly Dictionary<string, Texture2D?> Textures = new();
    private static Dictionary<string, string>? _paths;

    public static Texture2D? Texture(string assetId)
    {
        if (Textures.TryGetValue(assetId, out var cached)) return cached;
        _paths ??= ReadPaths();
        Texture2D? texture = null;
        if (_paths.TryGetValue(assetId, out var path))
            texture = ResourceLoader.Load<Texture2D>(path);
        Textures[assetId] = texture;
        return texture;
    }

    private static Dictionary<string, string> ReadPaths()
    {
        var paths = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            using var file = global::Godot.FileAccess.Open(CatalogPath,
                global::Godot.FileAccess.ModeFlags.Read);
            if (file is null) return paths;
            using var json = JsonDocument.Parse(file.GetAsText());
            foreach (var row in json.RootElement.GetProperty("assets").EnumerateArray())
            {
                if (row.GetProperty("status").GetString() == "unassigned" ||
                    row.GetProperty("kind").GetString() != "visual") continue;
                var resource = row.GetProperty("resource").GetString();
                if (resource is not null && resource.StartsWith("res://assets/art/", StringComparison.Ordinal))
                    paths.Add(row.GetProperty("id").GetString()!, resource);
            }
        }
        catch (Exception)
        {
            // Catalog errors are caught by CI; released builds keep drawing placeholders.
            paths.Clear();
        }
        return paths;
    }
}
