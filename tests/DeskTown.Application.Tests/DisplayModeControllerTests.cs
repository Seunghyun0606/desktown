using DeskTown.Application.Display;

namespace DeskTown.Application.Tests;

public sealed class DisplayModeControllerTests
{
    [Fact]
    public void Companion_close_hides_surface_without_touching_any_session()
    {
        var companion = new Surface();
        var controller = new DisplayModeController(companion);

        controller.SetDisplayMode(DisplayMode.Companion);
        controller.CompanionCloseRequested();

        Assert.Equal(DisplayMode.Hidden, controller.Mode);
        Assert.False(companion.Visible);
        Assert.Equal(1, companion.ShowCount);
        Assert.Equal(2, companion.HideCount);
    }

    [Fact]
    public void Switching_surfaces_keeps_exactly_one_visible_and_repeated_mode_is_noop()
    {
        var companion = new Surface();
        var ghost = new Surface();
        var controller = new DisplayModeController(companion, ghost);

        controller.SetDisplayMode(DisplayMode.Companion);
        controller.SetDisplayMode(DisplayMode.Ghost);
        controller.SetDisplayMode(DisplayMode.Ghost);
        Assert.False(companion.Visible);
        Assert.True(ghost.Visible);
        Assert.Equal(1, ghost.ShowCount);

        controller.SetDisplayMode(DisplayMode.Hidden);
        Assert.False(companion.Visible);
        Assert.False(ghost.Visible);
    }

    [Fact]
    public void Unavailable_ghost_does_not_hide_current_companion()
    {
        var companion = new Surface();
        var controller = new DisplayModeController(companion);
        controller.SetDisplayMode(DisplayMode.Companion);

        Assert.Throws<NotSupportedException>(() => controller.SetDisplayMode(DisplayMode.Ghost));
        Assert.Equal(DisplayMode.Companion, controller.Mode);
        Assert.True(companion.Visible);
    }

    [Fact]
    public void Partially_shown_ghost_fails_closed_and_remains_unavailable()
    {
        var companion = new Surface();
        var ghost = new Surface { FailOnShow = true };
        var controller = new DisplayModeController(companion, ghost);
        controller.SetDisplayMode(DisplayMode.Companion);

        Assert.Equal(DisplayMode.Hidden, controller.SetDisplayMode(DisplayMode.Ghost));
        Assert.False(companion.Visible);
        Assert.False(ghost.Visible);
        Assert.False(controller.GhostAvailable);
        Assert.Equal(DisplayMode.Hidden, controller.SetDisplayMode(DisplayMode.Ghost));
        Assert.Equal(1, ghost.ShowCount);
    }

    private sealed class Surface : IFocusDisplaySurface
    {
        public bool Visible { get; private set; }
        public int ShowCount { get; private set; }
        public int HideCount { get; private set; }
        public bool FailOnShow { get; set; }
        public void Show()
        {
            Visible = true;
            ShowCount++;
            if (FailOnShow) throw new InvalidOperationException("Native apply failed");
        }
        public void Hide() { Visible = false; HideCount++; }
    }
}
