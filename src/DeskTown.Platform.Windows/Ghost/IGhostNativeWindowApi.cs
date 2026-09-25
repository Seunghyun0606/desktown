namespace DeskTown.Platform.Windows.Ghost;

internal interface IGhostNativeWindowApi
{
    bool IsWindow(nint window);
    uint GetOwnerProcessId(nint window);
    nuint ReadExtendedStyle(nint window);
    bool WriteExtendedStyle(nint window, nuint style);
    bool SetTopmostWithoutFocus(nint window);
}
