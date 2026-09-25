using DeskTown.Platform.Windows.Ghost;

namespace DeskTown.Platform.Windows.Tests.Ghost;

public sealed class WindowsGhostWindowPlatformTests
{
    private static readonly nint Handle = new(42);

    [Fact]
    public void Apply_and_remove_are_idempotent_and_preserve_foreign_bits()
    {
        var native = new FakeNative { Style = 0x40000 | GhostWindowStyles.Layered };
        var platform = new WindowsGhostWindowPlatform(native, native.ProcessId);
        var original = native.Style;

        Assert.True(platform.Apply(Handle).Applied);
        Assert.True(platform.Apply(Handle).Applied);
        Assert.Equal(original | GhostWindowStyles.Required, native.Style);
        Assert.True(platform.Verify(Handle).RequiredStylesPresent);
        Assert.Equal(2, native.TopmostCalls);

        // A separate window owner may add bits while Ghost is visible.
        native.Style |= 0x10000;
        platform.Remove(Handle);
        platform.Remove(Handle);
        Assert.Equal(original | 0x10000, native.Style);
    }

    [Fact]
    public void Rejects_invalid_and_foreign_handles_before_any_style_write()
    {
        var native = new FakeNative { Valid = false };
        var platform = new WindowsGhostWindowPlatform(native, native.ProcessId);
        Assert.False(platform.Apply(Handle).Applied);
        Assert.False(platform.Probe(0).Available);
        native.Valid = true;
        native.ProcessId = 99;
        Assert.False(platform.Apply(Handle).Applied);
        Assert.Equal(0, native.WriteCalls);
    }

    [Fact]
    public void Failed_topmost_restores_only_bits_added_by_our_adapter()
    {
        var native = new FakeNative { Style = 0x40000, TopmostSucceeds = false };
        var platform = new WindowsGhostWindowPlatform(native, native.ProcessId);
        Assert.False(platform.Apply(Handle).Applied);
        Assert.Equal((nuint)0x40000, native.Style);
        Assert.False(platform.Verify(Handle).RequiredStylesPresent);
        platform.Remove(Handle);
        Assert.Equal(2, native.WriteCalls);
    }

    [Fact]
    public void Failed_repeat_apply_does_not_strip_existing_ghost_styles()
    {
        var native = new FakeNative();
        var platform = new WindowsGhostWindowPlatform(native, native.ProcessId);
        Assert.True(platform.Apply(Handle).Applied);
        native.TopmostSucceeds = false;
        Assert.False(platform.Apply(Handle).Applied);
        Assert.True(platform.Verify(Handle).RequiredStylesPresent);
        platform.Remove(Handle);
        Assert.Equal((nuint)0, native.Style);
    }

    private sealed class FakeNative : IGhostNativeWindowApi
    {
        public bool Valid { get; set; } = true;
        public uint ProcessId { get; set; } = 27;
        public nuint Style { get; set; }
        public bool TopmostSucceeds { get; set; } = true;
        public int WriteCalls { get; private set; }
        public int TopmostCalls { get; private set; }
        public bool IsWindow(nint window) => Valid && window == Handle;
        public uint GetOwnerProcessId(nint window) => ProcessId;
        public nuint ReadExtendedStyle(nint window) => Style;
        public bool WriteExtendedStyle(nint window, nuint style)
        {
            Style = style;
            WriteCalls++;
            return true;
        }
        public bool SetTopmostWithoutFocus(nint window)
        {
            TopmostCalls++;
            return TopmostSucceeds;
        }
    }
}
