using DeskTown.Application.Display;

namespace DeskTown.Application.Tests;

public sealed class FocusDisplayLifecycleTests
{
    [Fact]
    public void Hidden_keeps_all_surfaces_off_and_completion_never_opens_town()
    {
        var main = new Surface();
        var companion = new Surface();
        var ghost = new Surface();
        var mode = new DisplayModeController(companion, ghost);
        var display = new FocusDisplayLifecycle(mode, main);

        display.EnterFocus(DisplayMode.Companion);
        display.ChangeMode(DisplayMode.Hidden);
        Assert.False(main.Visible);
        Assert.False(companion.Visible);
        Assert.False(ghost.Visible);

        display.ChangeMode(DisplayMode.Ghost);
        display.FocusCompleted();
        Assert.False(main.Visible);
        Assert.False(ghost.Visible);
        display.OpenTown();
        Assert.True(main.Visible);
    }

    [Fact]
    public void Ghost_surface_failure_keeps_focus_presentation_active_and_town_hidden()
    {
        var main = new Surface();
        var companion = new Surface();
        var ghost = new Surface { FailOnShow = true };
        var modes = new DisplayModeController(companion, ghost);
        var display = new FocusDisplayLifecycle(modes, main);

        display.EnterFocus(DisplayMode.Companion);
        display.ChangeMode(DisplayMode.Ghost);

        Assert.True(display.FocusPresentationActive);
        Assert.Equal(DisplayMode.Hidden, modes.Mode);
        Assert.False(main.Visible);
        Assert.False(companion.Visible);
        Assert.False(ghost.Visible);
    }

    private sealed class Surface : IFocusDisplaySurface
    {
        public bool Visible { get; private set; }
        public bool FailOnShow { get; set; }
        public void Show()
        {
            Visible = true;
            if (FailOnShow) throw new InvalidOperationException("Native apply failed");
        }
        public void Hide() => Visible = false;
    }
}
