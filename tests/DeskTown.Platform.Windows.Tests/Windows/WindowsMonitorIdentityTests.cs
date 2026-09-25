using DeskTown.Platform.Windows.Windows;

namespace DeskTown.Platform.Windows.Tests.Windows;

public sealed class WindowsMonitorIdentityTests
{
    [Fact]
    public void Hashed_identity_is_stable_without_serializing_raw_device_path()
    {
        const string raw = @"\\?\DISPLAY#ABC123#monitor-instance";
        var key = WindowsMonitorIdentity.HashId(raw);
        Assert.Equal(key, WindowsMonitorIdentity.HashId(raw.ToLowerInvariant()));
        Assert.StartsWith("monitor:", key);
        Assert.DoesNotContain("ABC123", key);
    }

    [Fact]
    public void Non_windows_environment_preserves_provisional_monitor_key()
    {
        if (OperatingSystem.IsWindows()) return;
        Assert.Equal("screen:1", WindowsMonitorIdentity.AtOrFallback(-100, 200, "screen:1"));
    }
}
