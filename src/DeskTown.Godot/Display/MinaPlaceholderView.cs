using DeskTown.Application.Display;
using DeskTown.Domain.Simulation;
using global::Godot;

namespace DeskTown.Presentation.Display;

/// <summary>48×48 geometric preview; production sheets replace this view through catalog IDs.</summary>
public partial class MinaPlaceholderView : Node2D
{
    private readonly MinaClipPlayback _playback = new();

    public override void _Ready()
    {
        VisibilityChanged += () => SetProcess(IsVisibleInTree());
        SetProcess(IsVisibleInTree());
    }

    public void Bind(MinaSimulationState state) => _playback.Apply(MinaClipPlayback.ForState(state));

    public void Play(MinaPresentationIntent intent) => _playback.Apply(intent);

    public override void _Process(double delta)
    {
        _playback.Advance(TimeSpan.FromSeconds(delta));
        QueueRedraw();
    }

    public override void _Draw()
    {
        var frame = _playback.Frame;
        var assetId = _playback.Clip switch
        {
            MinaAnimationClip.Idle => "CHR-MIN-001",
            MinaAnimationClip.Walk => "CHR-MIN-002",
            MinaAnimationClip.Work => "CHR-MIN-003",
            MinaAnimationClip.Rest => "CHR-MIN-004",
            MinaAnimationClip.Stretch => "CHR-MIN-005",
            MinaAnimationClip.Celebrate => "CHR-MIN-006",
            _ => "CHR-MIN-001"
        };
        if (VisualAssetCatalog.Texture(assetId) is { } sheet && sheet.GetWidth() >= 48)
        {
            var cell = Math.Min(frame, sheet.GetWidth() / 48 - 1);
            DrawTextureRectRegion(sheet, new Rect2(0, 0, 48, 48),
                new Rect2(cell * 48, 0, 48, 48));
            return;
        }

        var bob = _playback.Clip == MinaAnimationClip.Walk ? frame % 2 : 0;
        var apron = Color.FromHtml("947361");
        var face = Color.FromHtml("e9b28b");
        var hair = Color.FromHtml("5d463f");
        DrawRect(new Rect2(12, 40, 25, 3), new Color(0, 0, 0, 0.19f));
        DrawRect(new Rect2(17, 25 - bob, 15, 17), apron);
        DrawRect(new Rect2(19, 13 - bob, 13, 14), face);
        DrawRect(new Rect2(17, 11 - bob, 16, 7), hair);
        DrawRect(new Rect2(19, 40 - bob, 5, 4), hair);
        DrawRect(new Rect2(27, 40 - bob, 5, 4), hair);
        if (_playback.Clip is MinaAnimationClip.Stretch or MinaAnimationClip.Celebrate)
        {
            var reach = frame % 2 == 0 ? 9 : 12;
            DrawRect(new Rect2(11, reach, 6, 4), face);
            DrawRect(new Rect2(32, reach, 6, 4), face);
        }
        else
        {
            DrawRect(new Rect2(12, 27 - bob, 5, 10), face);
            DrawRect(new Rect2(32, 27 - bob, 5, 10), face);
        }
        if (_playback.Clip == MinaAnimationClip.Work)
            DrawRect(new Rect2(34, 17 + frame % 3 * 2, 3, 17), Color.FromHtml("c7a46c"));
        if (_playback.Clip == MinaAnimationClip.Rest)
            DrawRect(new Rect2(33, 30, 7, 5), Color.FromHtml("d6c0a5"));
        if (_playback.Clip == MinaAnimationClip.Celebrate)
            DrawRect(new Rect2(35, 9 + frame % 2, 5, 5), Color.FromHtml("f4d390"));
    }
}
