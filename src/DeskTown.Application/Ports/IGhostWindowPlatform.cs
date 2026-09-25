namespace DeskTown.Application.Ports;

public interface IGhostWindowPlatform
{
    GhostCapability Probe(nint window);
    GhostApplyResult Apply(nint window);
    GhostVerification Verify(nint window);
    void Remove(nint window);
}

public sealed record GhostCapability(bool Available, string Reason);
public sealed record GhostApplyResult(bool Applied, string Reason);
public sealed record GhostVerification(bool OwnWindow, bool RequiredStylesPresent);
