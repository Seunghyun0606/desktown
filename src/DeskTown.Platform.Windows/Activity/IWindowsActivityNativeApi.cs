namespace DeskTown.Platform.Windows.Activity;

/// <summary>
/// Narrow seam over the native calls needed by activity tracking. Native
/// handles and process identifiers never cross the Platform.Windows assembly.
/// </summary>
internal interface IWindowsActivityNativeApi
{
    bool TryGetForegroundProcessId(out uint processId);

    bool TryGetIdleDuration(out TimeSpan idleDuration);
}
