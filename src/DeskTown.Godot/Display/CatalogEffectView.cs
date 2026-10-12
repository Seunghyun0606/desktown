using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>A visible-only presentation clock. Never owns gameplay or save state.</summary>
internal partial class CatalogEffectView : Sprite2D
{
    private string _assetId = "";
    private int _frames;
    private int _fps;
    private bool _loop;
    private bool _enabled;
    private bool _animated = true;
    private double _elapsed;
    public int CurrentFrame { get; private set; }

    public static CatalogEffectView? Create(Node parent, string id, int frames, int fps,
        bool loop, int x, int y)
    {
        if (VisualAssetCatalog.Frame(id) is null) return null;
        var name = id.Replace('-', '_');
        if (parent.HasNode(name)) name += "_" + parent.GetChildCount();
        var view = new CatalogEffectView
        {
            Name = name, _assetId = id, _frames = frames, _fps = fps, _loop = loop,
            Centered = false, RegionEnabled = true, Position = new Vector2(x, y),
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest, Visible = false
        };
        parent.AddChild(view);
        view.SelectFrame(0);
        return view;
    }

    public override void _Ready()
    {
        VisibilityChanged += SyncProcessing;
        SyncProcessing();
    }

    public void SetEnabled(bool enabled, bool animated = true)
    {
        if (_enabled != enabled)
        {
            _elapsed = 0;
            SelectFrame(0);
        }
        _enabled = enabled;
        _animated = animated;
        if (!animated) { _elapsed = 0; SelectFrame(0); }
        Visible = enabled;
        SyncProcessing();
    }

    public void Trigger()
    {
        _elapsed = 0;
        SelectFrame(0);
        _enabled = _animated = true;
        Show();
        SyncProcessing();
    }

    public override void _Process(double delta)
    {
        if (!_enabled || !_animated || !IsVisibleInTree()) return;
        _elapsed += Math.Max(0, delta);
        var frame = (long)Math.Floor((_elapsed + 0.000000001) * _fps);
        if (!_loop && frame >= _frames)
        {
            SetEnabled(false);
            return;
        }
        SelectFrame((int)(frame % _frames));
    }

    private void SelectFrame(int frame)
    {
        if (VisualAssetCatalog.Frame(_assetId, frame) is not { } art) return;
        Texture = art.Sheet;
        RegionRect = art.Source;
        CurrentFrame = frame;
    }

    private void SyncProcessing() => SetProcess(_enabled && _animated && IsVisibleInTree());
}
