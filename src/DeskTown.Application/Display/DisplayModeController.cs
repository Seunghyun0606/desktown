namespace DeskTown.Application.Display;

/// <summary>Owns visible surfaces without knowing session, energy, or reward state.</summary>
public sealed class DisplayModeController
{
    private readonly IFocusDisplaySurface _companion;
    private readonly IFocusDisplaySurface? _ghost;

    public DisplayModeController(IFocusDisplaySurface companion, IFocusDisplaySurface? ghost = null)
    {
        _companion = companion ?? throw new ArgumentNullException(nameof(companion));
        _ghost = ghost;
    }

    public DisplayMode Mode { get; private set; } = DisplayMode.Hidden;

    public void SetDisplayMode(DisplayMode mode)
    {
        if (!Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode));
        if (mode == DisplayMode.Ghost && _ghost is null)
            throw new NotSupportedException("Ghost surface is not available yet.");

        if (mode == Mode)
            return;

        _companion.Hide();
        _ghost?.Hide();
        if (mode == DisplayMode.Companion)
            _companion.Show();
        else if (mode == DisplayMode.Ghost)
            _ghost!.Show();
        Mode = mode;
    }

    public void CompanionCloseRequested()
    {
        if (Mode == DisplayMode.Companion)
            SetDisplayMode(DisplayMode.Hidden);
    }
}
