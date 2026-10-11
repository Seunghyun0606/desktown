using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>Approved static Town tiles. Missing catalog art retains geometric ground.</summary>
internal static class TownEnvironment
{
    public static Sprite2D? Bind(Control town)
    {
        var world = town.GetNode<Control>("World");
        var terrain = new Node2D { Name = "Terrain" };
        world.AddChild(terrain);
        world.MoveChild(terrain, 0);
        Sprite2D? Place(string id, int frame, int x, int y)
        {
            if (VisualAssetCatalog.Frame(id, frame) is not { } image) return null;
            var sprite = new Sprite2D
            {
                Texture = image.Sheet, RegionEnabled = true, RegionRect = image.Source,
                Centered = false, Position = new Vector2(x, y),
                TextureFilter = CanvasItem.TextureFilterEnum.Nearest
            };
            terrain.AddChild(sprite);
            return sprite;
        }
        if (VisualAssetCatalog.Frame("ENV-GRA-001", 0) is not null)
        {
            town.GetNode<ColorRect>("Ground").Hide();
            world.GetNode<ColorRect>("Path").Hide();
            for (var y = 0; y < 360; y += 32)
                for (var x = 0; x < 640; x += 32)
                    Place("ENV-GRA-001", (x / 32 + y / 32) % 2, x, y);
            for (var y = 128; y < 192; y += 32)
                for (var x = 512; x < 608; x += 32) Place("ENV-DIR-001", 0, x, y);
            for (var x = 16; x < 640; x += 32) Place("ENV-PTH-001", 0, x, 264);
            foreach (var x in new[] { 144, 432 })
            {
                Place("ENV-PTH-001", 1, x, 232);
                Place("ENV-PTH-001", 2, x, 264);
            }
            for (var y = 136; y < 264; y += 32) Place("ENV-PTH-001", 1, 528, y);
            Place("ENV-PTH-001", 2, 528, 264);
        }
        Place("ENV-TRE-001", 1, 292, 74);
        foreach (var (id, frame, x, y) in new[] {
            ("ENV-BUS-001",0,32,248), ("ENV-BUS-001",1,242,216), ("ENV-BUS-001",0,572,163), ("ENV-BUS-001",1,575,286),
            ("ENV-FLW-001",0,116,300), ("ENV-FLW-001",1,336,292), ("ENV-FLW-001",0,476,310),
            ("ENV-FEN-001",0,248,215), ("ENV-FEN-001",1,280,215), ("ENV-FEN-001",0,312,215),
            ("ENV-ROC-001",0,82,315), ("ENV-ROC-001",1,576,200) }) Place(id, frame, x, y);
        return Place("CMP-SHD-001", 0, 176, 266);
    }
}
