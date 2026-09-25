using System.Diagnostics;

namespace DeskTown.Platform.Windows.Activity;

internal sealed class ProcessNameResolver : IProcessNameResolver
{
    public bool TryResolve(uint processId, out string processName)
    {
        processName = string.Empty;

        try
        {
            using var process = Process.GetProcessById(checked((int)processId));
            var candidate = process.ProcessName;
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return false;
            }

            processName = candidate.Trim();
            return true;
        }
        catch (Exception exception) when (
            exception is ArgumentException
                or InvalidOperationException
                or System.ComponentModel.Win32Exception
                or NotSupportedException
                or OverflowException)
        {
            return false;
        }
    }
}
