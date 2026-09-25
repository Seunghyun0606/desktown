using System.Runtime.InteropServices;

namespace DeskTown.Platform.Windows.Ghost;

internal sealed class GhostNativeWindowApi : IGhostNativeWindowApi
{
    private const int ExtendedStyleIndex = -20;
    private const uint NoSize = 0x0001;
    private const uint NoMove = 0x0002;
    private const uint NoActivate = 0x0010;
    private const uint FrameChanged = 0x0020;

    public bool IsWindow(nint window) => NativeMethods.IsWindow(window);

    public uint GetOwnerProcessId(nint window) =>
        NativeMethods.GetWindowThreadProcessId(window, out var processId) == 0
            ? 0 : processId;

    public nuint ReadExtendedStyle(nint window)
    {
        Marshal.SetLastPInvokeError(0);
        var result = NativeMethods.GetWindowLongPtr(window, ExtendedStyleIndex);
        if (result == 0 && Marshal.GetLastPInvokeError() != 0)
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError());
        return unchecked((nuint)result);
    }

    public bool WriteExtendedStyle(nint window, nuint style)
    {
        Marshal.SetLastPInvokeError(0);
        var previous = NativeMethods.SetWindowLongPtr(window, ExtendedStyleIndex,
            unchecked((nint)style));
        return previous != 0 || Marshal.GetLastPInvokeError() == 0;
    }

    public bool SetTopmostWithoutFocus(nint window) =>
        NativeMethods.SetWindowPos(window, new nint(-1), 0, 0, 0, 0,
            NoSize | NoMove | NoActivate | FrameChanged);
}
