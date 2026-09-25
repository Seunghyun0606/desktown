namespace DeskTown.Application.Ports;

/// <summary>
/// Supplies privacy-minimal executable process names for optional Focus intent.
/// An empty result means "Any App" and must never prevent a session from starting.
/// </summary>
public interface IProcessCatalog
{
    IReadOnlyList<string> GetRunningProcessNames();
}
