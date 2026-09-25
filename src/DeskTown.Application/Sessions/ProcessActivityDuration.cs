namespace DeskTown.Application.Sessions;

/// <summary>
/// A descriptive foreground duration grouped by bare executable process name.
/// It has no effect on Focus Energy.
/// </summary>
public sealed record ProcessActivityDuration(string ProcessName, TimeSpan Duration);
