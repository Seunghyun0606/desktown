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

    private sealed class Surface : IFocusDisplaySurface
    {
        public bool Visible { get; private set; }
        public void Show() => Visible = true;
        public void Hide() => Visible = false;
    }
}
