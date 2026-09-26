using DeskTown.Application.Activity;
using DeskTown.Application.Ports;

namespace DeskTown.Platform.Windows.Processes;

/// <summary>
/// Enumerates only bare executable process names for optional Focus intent.
/// Empty and unavailable catalogs are normal and represent "Any App".
/// </summary>
public sealed class WindowsProcessCatalog : IProcessCatalog
{
    private readonly IRunningProcessSource _processSource;

    public WindowsProcessCatalog()
        : this(new RunningProcessSource())
    {
    }

    internal WindowsProcessCatalog(IRunningProcessSource processSource)
    {
        _processSource = processSource ?? throw new ArgumentNullException(nameof(processSource));
    }

    public IReadOnlyList<string> GetRunningProcessNames()
    {
        try
        {
            return _processSource
                .ReadProcessNames()
                .Where(ProcessNamePolicy.IsPersistable)
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (Exception exception) when (IsRecoverableCatalogFailure(exception))
        {
            return Array.Empty<string>();
        }
    }

    private static bool IsRecoverableCatalogFailure(Exception exception)
    {
        return exception is ArgumentException
            or InvalidOperationException
            or System.ComponentModel.Win32Exception
            or NotSupportedException
            or PlatformNotSupportedException
            or UnauthorizedAccessException;
    }
}
