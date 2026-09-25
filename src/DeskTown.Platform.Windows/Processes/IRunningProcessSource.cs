namespace DeskTown.Platform.Windows.Processes;

internal interface IRunningProcessSource
{
    IReadOnlyCollection<string> ReadProcessNames();
}
