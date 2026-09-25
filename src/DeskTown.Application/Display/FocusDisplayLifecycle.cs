namespace DeskTown.Application.Display;

/// <summary>Presentation lifecycle. The caller retains ownership of the running Focus session.</summary>
public sealed class FocusDisplayLifecycle
{
    private readonly DisplayModeController _modes;
    private readonly IFocusDisplaySurface _main;

    public FocusDisplayLifecycle(DisplayModeController modes, IFocusDisplaySurface main)
    {
        _modes = modes ?? throw new ArgumentNullException(nameof(modes));
        _main = main ?? throw new ArgumentNullException(nameof(main));
    }

    public bool FocusPresentationActive { get; private set; }

    public void EnterFocus(DisplayMode mode)
    {
        if (FocusPresentationActive) throw new InvalidOperationException("Focus presentation already active.");
        _modes.SetDisplayMode(mode);
        _main.Hide();
        FocusPresentationActive = true;
    }

    public void ChangeMode(DisplayMode mode)
    {
        if (!FocusPresentationActive) throw new InvalidOperationException("No focus presentation is active.");
        _modes.SetDisplayMode(mode);
    }

    public void FocusCompleted()
    {
        if (!FocusPresentationActive) return;
        _modes.SetDisplayMode(DisplayMode.Hidden);
        FocusPresentationActive = false;
        // Deliberately leave the Town hidden until the user opens it.
    }

    public void OpenTown()
    {
        _modes.SetDisplayMode(DisplayMode.Hidden);
        FocusPresentationActive = false;
        _main.Show();
    }
}
