using global::Godot;
using DeskTown.Presentation.Display;

namespace DeskTown.Presentation;

/// <summary>Geometric Town stand-in; ambient actions never affect the simulation.</summary>
public partial class TownNpcPlaceholderView : Node2D
{
    private bool _isRumi;
    private bool _reading;
    private double _frameElapsed;
    private int _frame;

    public override void _Ready()
    {
        VisibilityChanged += () => SetProcess(IsVisibleInTree());
        SetProcess(IsVisibleInTree());
    }

    public override void _Process(double delta)
    {
        _frameElapsed += delta;
        if (_frameElapsed < 0.5) return;
        _frameElapsed %= 0.5;
        _frame = (_frame + 1) % 4;
        QueueRedraw();
    }

    public void Configure(bool isRumi)
    {
        _isRumi = isRumi;
        QueueRedraw();
    }

    public void SetReading(bool reading)
    {
        if (_reading == reading) return;
        _reading = reading;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var assetId = _isRumi ? "CHR-RUM-001" : _reading ? "CHR-NOA-003" : "CHR-NOA-001";
        if (VisualAssetCatalog.Frame(assetId, _frame) is { } image)
        {
            DrawTextureRectRegion(image.Sheet,
                new Rect2(Vector2.Zero, image.Source.Size), image.Source);
            return;
        }

        var coat = Color.FromHtml(_isRumi ? "b87866" : "68889a");
        var hair = Color.FromHtml(_isRumi ? "704a42" : "4c4540");
        DrawRect(new Rect2(9, 40, 29, 3), new Color(0, 0, 0, 0.18f));
        DrawRect(new Rect2(17, 24, 15, 17), coat);
        DrawRect(new Rect2(19, 12, 13, 14), Color.FromHtml("e4ae87"));
        DrawRect(new Rect2(17, 10, 17, 7), hair);
        DrawRect(new Rect2(19, 40, 5, 4), hair);
        DrawRect(new Rect2(27, 40, 5, 4), hair);
        if (_reading)
        {
            DrawRect(new Rect2(11, 29, 13, 9), Color.FromHtml("ead4a9"));
            DrawRect(new Rect2(17, 29, 1, 9), Color.FromHtml("8d694f"));
        }
        else if (_isRumi)
        {
            DrawRect(new Rect2(31, 30, 8, 9), Color.FromHtml("dbc092"));
            DrawRect(new Rect2(31, 29, 8, 2), hair);
        }
    }
}
