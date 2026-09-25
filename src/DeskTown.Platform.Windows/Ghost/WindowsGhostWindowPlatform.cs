using DeskTown.Application.Ports;

namespace DeskTown.Platform.Windows.Ghost;

/// <summary>Style reinforcement only; actual cross-app input remains a Windows QA gate.</summary>
public sealed class WindowsGhostWindowPlatform : IGhostWindowPlatform
{
    private readonly IGhostNativeWindowApi _native;
    private readonly uint _processId;
    private readonly Dictionary<nint, nuint> _ownedBits = [];

    public WindowsGhostWindowPlatform() : this(new GhostNativeWindowApi(),
        (uint)Environment.ProcessId) { }

    internal WindowsGhostWindowPlatform(IGhostNativeWindowApi native, uint processId)
    {
        _native = native ?? throw new ArgumentNullException(nameof(native));
        _processId = processId;
    }

    public GhostCapability Probe(nint window)
    {
        if (window == 0 || !_native.IsWindow(window))
            return new GhostCapability(false, "Invalid window handle");
        if (_native.GetOwnerProcessId(window) != _processId)
            return new GhostCapability(false, "Window belongs to a different process");
        return new GhostCapability(true, "Styles can be checked");
    }

    public GhostApplyResult Apply(nint window)
    {
        var capability = Probe(window);
        if (!capability.Available) return new GhostApplyResult(false, capability.Reason);
        var original = _native.ReadExtendedStyle(window);
        var owned = _ownedBits.TryGetValue(window, out var existing)
            ? existing : GhostWindowStyles.AddedByUs(original);
        if (!_native.WriteExtendedStyle(window, GhostWindowStyles.Apply(original)))
            return new GhostApplyResult(false, "Style write failed");
        if (!_native.SetTopmostWithoutFocus(window))
        {
            _native.WriteExtendedStyle(window, GhostWindowStyles.RemoveOwned(
                _native.ReadExtendedStyle(window), GhostWindowStyles.AddedByUs(original)));
            return new GhostApplyResult(false, "Topmost apply failed");
        }
        _ownedBits[window] = owned;
        return new GhostApplyResult(true, "Styles applied; input QA still required");
    }

    public GhostVerification Verify(nint window)
    {
        var own = Probe(window).Available;
        return new GhostVerification(own,
            own && GhostWindowStyles.HasRequired(_native.ReadExtendedStyle(window)));
    }

    public void Remove(nint window)
    {
        if (!_ownedBits.TryGetValue(window, out var owned) || !Probe(window).Available) return;
        if (_native.WriteExtendedStyle(window, GhostWindowStyles.RemoveOwned(
                _native.ReadExtendedStyle(window), owned)))
            _ownedBits.Remove(window);
    }
}
