using global::Godot;
using DeskTown.Presentation.Display;
using DeskTown.Application.Display;

namespace DeskTown.Presentation;

/// <summary>Geometric Town stand-in; ambient actions never affect the simulation.</summary>
public partial class TownNpcPlaceholderView : Node2D
{
    private bool _isRumi;
    private bool _reading;
    private bool _walking;
    private bool _facingLeft;
    private bool _externallyClocked;
    private double _frameElapsed;
    private int _frame;
    public event Action<int>? WalkingFrameChanged;

    public override void _Ready()
    {
        VisibilityChanged += () => SetProcess(!_externallyClocked && IsVisibleInTree());
        SetProcess(IsVisibleInTree());
    }

    public override void _Process(double delta)
    {
        _frameElapsed += delta;
        var period = _walking ? 0.125 : 0.5;
        if (_frameElapsed < period) return;
        var steps = (int)(_frameElapsed / period);
        _frameElapsed %= period;
        _frame = (_frame + steps) % (_walking ? 6 : 4);
        QueueRedraw();
    }

    public void Configure(bool isRumi)
    {
        _isRumi = isRumi;
        QueueRedraw();
    }

    public void SetReading(bool reading)
    {
        _externallyClocked = false;
        SetProcess(IsVisibleInTree());
        if (_reading == reading) return;
        _reading = reading;
        _walking = false;
        _frame = 0;
        _frameElapsed = 0;
        QueueRedraw();
    }

    public void BindPose(NpcAmbientPose pose)
    {
        var previous = (_walking, _frame);
        _externallyClocked = true;
        _reading = pose.Action == NpcAmbientAction.Read;
        _walking = pose.Action == NpcAmbientAction.Walk;
        _facingLeft = pose.FacingLeft;
        _frame = (int)(pose.ClipElapsed.TotalSeconds * (_walking ? 8 : 2)) % (_walking ? 6 : 4);
        // Town owns the shared visible-time clock; don't run a second clock.
        SetProcess(false);
        if (_walking && previous != (_walking, _frame)) WalkingFrameChanged?.Invoke(_frame);
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_facingLeft) DrawSetTransform(new Vector2(48, 0), 0, new Vector2(-1, 1));
        var assetId = _isRumi ? _walking ? "CHR-RUM-002" : "CHR-RUM-001"
            : _walking ? "CHR-NOA-002" : _reading ? "CHR-NOA-003" : "CHR-NOA-001";
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
