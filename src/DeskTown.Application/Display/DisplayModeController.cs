namespace DeskTown.Application.Display;

/// <summary>Owns visible surfaces without knowing session, energy, or reward state.</summary>
public sealed class DisplayModeController
{
    private readonly IFocusDisplaySurface _companion;
    private readonly IFocusDisplaySurface? _ghost;
    private bool _ghostFailed;

    public DisplayModeController(IFocusDisplaySurface companion, IFocusDisplaySurface? ghost = null)
    {
        _companion = companion ?? throw new ArgumentNullException(nameof(companion));
        _ghost = ghost;
    }

    public DisplayMode Mode { get; private set; } = DisplayMode.Hidden;
    public bool GhostAvailable => _ghost is not null && !_ghostFailed;

    /// <summary>Returns the mode actually shown, which may be Hidden after a Ghost failure.</summary>
    public DisplayMode SetDisplayMode(DisplayMode mode)
    {
        if (!Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode));
        if (mode == DisplayMode.Ghost && _ghost is null)
            throw new NotSupportedException("Ghost surface is not available yet.");

        if (mode == Mode)
            return Mode;

        _companion.Hide();
        _ghost?.Hide();
        if (mode == DisplayMode.Companion)
            _companion.Show();
        else if (mode == DisplayMode.Ghost)
        {
            if (_ghostFailed) return Mode = DisplayMode.Hidden;
            try { _ghost!.Show(); }
            catch (Exception)
            {
                _ghostFailed = true;
                // A partially shown overlay must be removed before Focus continues.
                _ghost!.Hide();
                return Mode = DisplayMode.Hidden;
            }
        }
        Mode = mode;
        return Mode;
    }

    public void CompanionCloseRequested()
    {
        if (Mode == DisplayMode.Companion)
            SetDisplayMode(DisplayMode.Hidden);
    }
}
