using DeskTown.Application.Activity;
using DeskTown.Application.Activity;
using DeskTown.Application.Ports;

namespace DeskTown.Platform.Windows.Activity;

public sealed class WindowsActivityTracker : IActivityTracker
{
    private readonly ActivityTrackingOptions _options;
    private readonly IWindowsActivityNativeApi _nativeApi;
    private readonly IProcessNameResolver _processNameResolver;

    public WindowsActivityTracker(ActivityTrackingOptions? options = null)
        : this(
            options ?? ActivityTrackingOptions.Default,
            new WindowsActivityNativeApi(),
            new ProcessNameResolver())
    {
    }

    internal WindowsActivityTracker(
        ActivityTrackingOptions options,
        IWindowsActivityNativeApi nativeApi,
        IProcessNameResolver processNameResolver)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(nativeApi);
        ArgumentNullException.ThrowIfNull(processNameResolver);
        options.EnsureValid();

        _options = options;
        _nativeApi = nativeApi;
        _processNameResolver = processNameResolver;
    }

    public TimeSpan SampleInterval => _options.SampleInterval;

    /// <summary>Ephemeral continuous idle age for Mina presentation only.</summary>
    public TimeSpan LastIdleAge { get; private set; }

    public ActivitySample Sample()
    {
        if (!TryReadIdleDuration(out var idleDuration))
        {
            LastIdleAge = TimeSpan.Zero;
            return ActivitySample.Unknown;
        }

        LastIdleAge = idleDuration;
        var processName = ReadProcessName();
        return idleDuration >= _options.IdleThreshold
            ? ActivitySample.Idle(processName, _options.SampleInterval)
            : ActivitySample.Active(processName, _options.SampleInterval);
    }

    private string ReadProcessName()
    {
        try
        {
            if (!_nativeApi.TryGetForegroundProcessId(out var processId)
                || !_processNameResolver.TryResolve(processId, out var processName)
                || !ProcessNamePolicy.IsPersistable(processName))
            {
                return ActivitySample.UnknownProcessName;
            }

            return processName.Trim();
        }
        catch (Exception exception) when (IsExpectedAccessFailure(exception))
        {
            return ActivitySample.UnknownProcessName;
        }
    }

    private bool TryReadIdleDuration(out TimeSpan idleDuration)
    {
        try
        {
            return _nativeApi.TryGetIdleDuration(out idleDuration)
                && idleDuration >= TimeSpan.Zero;
        }
        catch (Exception exception) when (IsExpectedAccessFailure(exception))
        {
            idleDuration = TimeSpan.Zero;
            return false;
        }
    }

    private static bool IsExpectedAccessFailure(Exception exception)
    {
        return exception is ArgumentException
            or InvalidOperationException
            or System.ComponentModel.Win32Exception
            or DllNotFoundException
            or EntryPointNotFoundException
            or PlatformNotSupportedException
            or UnauthorizedAccessException;
    }
}
