using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>Approved draw-only artwork inside existing controls.</summary>
internal static class CatalogUiArt
{
    public static AtlasTexture? Texture(string id, int frame = 0) =>
        VisualAssetCatalog.Frame(id, frame) is { } art
            ? new AtlasTexture { Atlas = art.Sheet, Region = art.Source } : null;

    public static void Panel(Control screen)
    {
        var paper = new StyleBoxFlat
        {
            BgColor = Color.FromHtml("29241f"), BorderColor = Color.FromHtml("79664f"),
            BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1,
            ContentMarginLeft = 12, ContentMarginRight = 12, ContentMarginTop = 12, ContentMarginBottom = 12
        };
        screen.GetNode<PanelContainer>("Panel").AddThemeStyleboxOverride("panel", paper);
    }

    public static TextureRect? Insert(VBoxContainer content, string id, Vector2 size, int index)
    {
        if (Texture(id) is not { } art) return null;
        var image = new TextureRect
        {
            Texture = art, CustomMinimumSize = size,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = Control.MouseFilterEnum.Ignore, TextureFilter = CanvasItem.TextureFilterEnum.Nearest
        };
        content.AddChild(image);
        content.MoveChild(image, index);
        return image;
    }
}
