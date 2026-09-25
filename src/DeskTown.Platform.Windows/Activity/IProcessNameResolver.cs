namespace DeskTown.Platform.Windows.Activity;

internal interface IProcessNameResolver
{
    bool TryResolve(uint processId, out string processName);
}
