using System.Diagnostics;

namespace DeskTown.Platform.Windows.Processes;

internal sealed class RunningProcessSource : IRunningProcessSource
{
    public IReadOnlyCollection<string> ReadProcessNames()
    {
        Process[] processes;
        try
        {
            processes = Process.GetProcesses();
        }
        catch (Exception exception) when (IsExpectedProcessFailure(exception))
        {
            return Array.Empty<string>();
        }

        var names = new List<string>(processes.Length);
        foreach (var process in processes)
        {
            using (process)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(process.ProcessName))
                    {
                        names.Add(process.ProcessName);
                    }
                }
                catch (Exception exception) when (IsExpectedProcessFailure(exception))
                {
                    // A process may exit or become inaccessible between enumeration
                    // and inspection. Other processes remain useful catalog entries.
                }
            }
        }

        return names;
    }

    private static bool IsExpectedProcessFailure(Exception exception)
    {
        return exception is ArgumentException
            or InvalidOperationException
            or System.ComponentModel.Win32Exception
            or NotSupportedException
            or PlatformNotSupportedException
            or UnauthorizedAccessException;
    }
}
