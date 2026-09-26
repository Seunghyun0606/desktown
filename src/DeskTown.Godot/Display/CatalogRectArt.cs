using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>Replaces a geometric prop with a native-size, bottom-centered catalog frame.</summary>
internal static class CatalogRectArt
{
    public static void Bind(ColorRect host, string assetId, int frame = 0, Label? placeholderLabel = null)
    {
        var art = host.GetNodeOrNull<Sprite2D>("CatalogArt");
        var image = VisualAssetCatalog.Frame(assetId, frame);
        if (image is null)
        {
            if (art is not null) art.Visible = false;
            placeholderLabel?.Show();
            return;
        }

        if (art is null)
        {
            art = new Sprite2D { Name = "CatalogArt", TextureFilter = CanvasItem.TextureFilterEnum.Nearest };
            host.AddChild(art);
        }
        art.Texture = image.Value.Sheet;
        art.RegionEnabled = true;
        art.RegionRect = image.Value.Source;
        art.Position = new Vector2(host.Size.X / 2, host.Size.Y - image.Value.Source.Size.Y / 2);
        art.Visible = true;
        host.Color = new Color(host.Color.R, host.Color.G, host.Color.B, 0);
        placeholderLabel?.Hide();
    }
}
