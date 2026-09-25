using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace DeskTown.Platform.Windows.Windows;

/// <summary>Maps a Godot screen rectangle to a monitor interface ID without saving the raw ID.</summary>
public static class WindowsMonitorIdentity
{
    public static string AtOrFallback(int centerX, int centerY, string fallback)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fallback);
        if (!OperatingSystem.IsWindows()) return fallback;
        try
        {
            var monitor = MonitorFromPoint(new Point(centerX, centerY), 2);
            if (monitor == 0) return fallback;
            var info = new MonitorInfoEx { Size = (uint)Marshal.SizeOf<MonitorInfoEx>() };
            if (!GetMonitorInfo(monitor, ref info)) return fallback;
            var device = new DisplayDevice { Size = (uint)Marshal.SizeOf<DisplayDevice>() };
            var source = EnumDisplayDevices(info.DeviceName, 0, ref device, 1)
                && !string.IsNullOrWhiteSpace(device.DeviceId)
                ? device.DeviceId : info.DeviceName;
            if (string.IsNullOrWhiteSpace(source)) return fallback;
            return HashId(source);
        }
        catch (DllNotFoundException) { return fallback; }
        catch (EntryPointNotFoundException) { return fallback; }
    }

    public static string HashId(string deviceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(deviceId.Trim().ToUpperInvariant()));
        return "monitor:" + Convert.ToHexString(bytes)[..16];
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Point
    {
        public readonly int X, Y;
        public Point(int x, int y) { X = x; Y = y; }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MonitorInfoEx
    {
        public uint Size;
        public Rect Monitor, Work;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DisplayDevice
    {
        public uint Size;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        public uint StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    [DllImport("user32.dll")]
    private static extern nint MonitorFromPoint(Point point, uint flags);

    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(nint monitor, ref MonitorInfoEx info);

    [DllImport("user32.dll", EntryPoint = "EnumDisplayDevicesW", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayDevices(string deviceName, uint index,
        ref DisplayDevice device, uint flags);
}
