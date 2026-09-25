using System.Runtime.InteropServices;

namespace DeskTown.Platform.Windows.Activity;

internal sealed class WindowsActivityNativeApi : IWindowsActivityNativeApi
{
    public bool TryGetForegroundProcessId(out uint processId)
    {
        processId = 0;

        var windowHandle = NativeMethods.GetForegroundWindow();
        if (windowHandle == nint.Zero)
        {
            return false;
        }

        var threadId = NativeMethods.GetWindowThreadProcessId(windowHandle, out processId);
        return threadId != 0 && processId != 0;
    }

    public bool TryGetIdleDuration(out TimeSpan idleDuration)
    {
        var lastInputInfo = new NativeMethods.LastInputInfo
        {
            Size = checked((uint)Marshal.SizeOf<NativeMethods.LastInputInfo>()),
            Time = 0
        };

        if (!NativeMethods.GetLastInputInfo(ref lastInputInfo))
        {
            idleDuration = TimeSpan.Zero;
            return false;
        }

        // GetLastInputInfo and Environment.TickCount64 share the system uptime
        // basis. Comparing as uint intentionally preserves 32-bit wraparound.
        var currentTick = unchecked((uint)Environment.TickCount64);
        var idleMilliseconds = unchecked(currentTick - lastInputInfo.Time);
        idleDuration = TimeSpan.FromMilliseconds(idleMilliseconds);
        return true;
    }
}
