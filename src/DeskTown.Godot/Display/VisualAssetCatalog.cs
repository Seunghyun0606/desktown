using System.Text.Json;
using global::Godot;

namespace DeskTown.Presentation.Display;

internal readonly record struct VisualAssetFrame(Texture2D Sheet, Rect2 Source);

/// <summary>Presentation-only lookup; absent or invalid art keeps geometric fallback.</summary>
internal static class VisualAssetCatalog
{
    private const string CatalogPath = "res://assets/catalogs/asset_catalog.json";
    private static readonly Dictionary<string, Texture2D?> Textures = new();
    private static Dictionary<string, Entry>? _entries;
    private sealed record Entry(string Resource, int Width, int Height, int Frames);

    public static VisualAssetFrame? Frame(string assetId, int frame = 0)
    {
        _entries ??= ReadEntries();
        if (!_entries.TryGetValue(assetId, out var entry) || frame < 0 || frame >= entry.Frames)
            return null;
        if (!Textures.TryGetValue(assetId, out var texture))
        {
            texture = ResourceLoader.Load<Texture2D>(entry.Resource);
            Textures[assetId] = texture;
        }
        if (texture is null || texture.GetWidth() != entry.Width * entry.Frames ||
            texture.GetHeight() != entry.Height) return null;
        return new VisualAssetFrame(texture,
            new Rect2(frame * entry.Width, 0, entry.Width, entry.Height));
    }

    private static Dictionary<string, Entry> ReadEntries()
    {
        var entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
        try
        {
            using var file = global::Godot.FileAccess.Open(CatalogPath,
                global::Godot.FileAccess.ModeFlags.Read);
            if (file is null) return entries;
            using var json = JsonDocument.Parse(file.GetAsText());
            foreach (var row in json.RootElement.GetProperty("assets").EnumerateArray())
            {
                if (row.GetProperty("status").GetString() is not ("placeholder" or "production") ||
                    row.GetProperty("kind").GetString() != "visual") continue;
                var resource = row.GetProperty("resource").GetString();
                if (resource is null || !resource.StartsWith("res://assets/art/", StringComparison.Ordinal))
                    continue;
                var size = row.GetProperty("frameSize");
                var width = size[0].GetInt32();
                var height = size[1].GetInt32();
                var frames = row.GetProperty("frames").GetInt32();
                if (width <= 0 || height <= 0 || frames <= 0) continue;
                entries.Add(row.GetProperty("id").GetString()!, new Entry(resource, width, height, frames));
            }
        }
        catch (Exception)
        {
            // Catalog errors are caught by CI; released builds keep drawing placeholders.
            entries.Clear();
        }
        return entries;
    }
}
