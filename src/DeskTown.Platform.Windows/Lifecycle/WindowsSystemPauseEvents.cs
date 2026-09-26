using System.Runtime.Versioning;
using Microsoft.Win32;
using DeskTown.Application.Lifecycle;

namespace DeskTown.Platform.Windows.Lifecycle;

/// <summary>Reports Windows session and power transitions without touching Godot nodes.</summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsSystemPauseEvents : IDisposable
{
    private bool _disposed;

    public WindowsSystemPauseEvents()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        SystemEvents.SessionSwitch += OnSessionSwitch;
        try { SystemEvents.PowerModeChanged += OnPowerModeChanged; }
        catch
        {
            SystemEvents.SessionSwitch -= OnSessionSwitch;
            throw;
        }
    }

    public event Action<SystemPauseReason, bool>? Changed;

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs args)
    {
        switch (args.Reason)
        {
            case SessionSwitchReason.SessionLock:
                Changed?.Invoke(SystemPauseReason.SessionLocked, true);
                break;
            case SessionSwitchReason.ConsoleDisconnect:
            case SessionSwitchReason.RemoteDisconnect:
                Changed?.Invoke(SystemPauseReason.SessionDisconnected, true);
                break;
            case SessionSwitchReason.SessionUnlock:
                Changed?.Invoke(SystemPauseReason.SessionLocked, false);
                break;
            case SessionSwitchReason.ConsoleConnect:
            case SessionSwitchReason.RemoteConnect:
                Changed?.Invoke(SystemPauseReason.SessionDisconnected, false);
                break;
        }
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs args)
    {
        if (args.Mode == PowerModes.Suspend)
            Changed?.Invoke(SystemPauseReason.PowerSuspended, true);
        else if (args.Mode == PowerModes.Resume)
            Changed?.Invoke(SystemPauseReason.PowerSuspended, false);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
    }
}
