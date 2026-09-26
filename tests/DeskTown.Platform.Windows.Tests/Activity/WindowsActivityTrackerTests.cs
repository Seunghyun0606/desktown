using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using DeskTown.Application.Activity;
using DeskTown.Application.Configuration;
using DeskTown.Platform.Windows.Activity;

namespace DeskTown.Platform.Windows.Tests.Activity;

public sealed class WindowsActivityTrackerTests
{
    [Fact]
    public void DefaultContractSamplesAtOneHertz()
    {
        Assert.Equal(TimeSpan.FromSeconds(1), ActivityTrackingOptions.Default.SampleInterval);

        var tracker = CreateTracker();

        Assert.Equal(TimeSpan.FromSeconds(1), tracker.SampleInterval);
    }

    [Fact]
    public void ActivityOptionsDeriveFromTheSharedPrototypeConfiguration()
    {
        var prototypeOptions = PrototypeOptions.Default with
        {
            LogicalTickInterval = TimeSpan.FromSeconds(2),
            ShortIdleThreshold = TimeSpan.FromSeconds(90),
            LongIdleThreshold = TimeSpan.FromSeconds(180)
        };

        var activityOptions = ActivityTrackingOptions.FromPrototypeOptions(prototypeOptions);

        Assert.Equal(prototypeOptions.LogicalTickInterval, activityOptions.SampleInterval);
        Assert.Equal(prototypeOptions.ShortIdleThreshold, activityOptions.IdleThreshold);
    }

    [Fact]
    public void SampleAggregatesActiveIntervalForForegroundProcess()
    {
        var native = new FakeWindowsActivityNativeApi
        {
            ForegroundProcessId = 314,
            IdleDuration = TimeSpan.FromSeconds(59)
        };
        var resolver = new FakeProcessNameResolver { ProcessName = "code" };
        var tracker = CreateTracker(native, resolver);

        var sample = tracker.Sample();

        Assert.Equal("code", sample.ProcessName);
        Assert.Equal(TimeSpan.FromSeconds(1), sample.ActiveDuration);
        Assert.Equal(TimeSpan.Zero, sample.IdleDuration);
        Assert.Equal((uint)314, resolver.LastProcessId);
        Assert.Equal(1, native.IdleReadCount);
        Assert.Equal(1, native.ForegroundReadCount);
    }

    [Fact]
    public void SampleTreatsThresholdBoundaryAsIdle()
    {
        var native = new FakeWindowsActivityNativeApi
        {
            IdleDuration = TimeSpan.FromSeconds(60)
        };
        var tracker = CreateTracker(native);

        var sample = tracker.Sample();

        Assert.Equal(TimeSpan.Zero, sample.ActiveDuration);
        Assert.Equal(TimeSpan.FromSeconds(1), sample.IdleDuration);
    }

    [Fact]
    public void ForegroundAccessFailureUsesUnknownWithoutLosingActivityBucket()
    {
        var native = new FakeWindowsActivityNativeApi
        {
            ForegroundAvailable = false,
            IdleDuration = TimeSpan.Zero
        };
        var resolver = new FakeProcessNameResolver();
        var tracker = CreateTracker(native, resolver);

        var sample = tracker.Sample();

        Assert.Equal(ActivitySample.UnknownProcessName, sample.ProcessName);
        Assert.Equal(TimeSpan.FromSeconds(1), sample.ActiveDuration);
        Assert.Equal(0, resolver.ResolveCount);
    }

    [Fact]
    public void ProcessAccessFailureUsesUnknown()
    {
        var resolver = new FakeProcessNameResolver
        {
            Exception = new UnauthorizedAccessException()
        };
        var tracker = CreateTracker(processNameResolver: resolver);

        var sample = tracker.Sample();

        Assert.Equal(ActivitySample.UnknownProcessName, sample.ProcessName);
        Assert.Equal(TimeSpan.FromSeconds(1), sample.ActiveDuration);
    }

    [Theory]
    [InlineData(@"C:\Program Files\Code.exe")]
    [InlineData("/usr/bin/code")]
    [InlineData("   ")]
    public void NonBareProcessValueIsNotCollected(string candidate)
    {
        var resolver = new FakeProcessNameResolver { ProcessName = candidate };
        var tracker = CreateTracker(processNameResolver: resolver);

        var sample = tracker.Sample();

        Assert.Equal(ActivitySample.UnknownProcessName, sample.ProcessName);
    }

    [Fact]
    public void LongForegroundNameFallsBackToUnknownBeforeSave()
    {
        var resolver = new FakeProcessNameResolver { ProcessName = new string('x', 129) };
        var sample = CreateTracker(processNameResolver: resolver).Sample();

        Assert.Equal(ActivitySample.UnknownProcessName, sample.ProcessName);
    }

    [Fact]
    public void LastInputFailureReturnsUnknownAndSkipsForegroundRead()
    {
        var native = new FakeWindowsActivityNativeApi { IdleAvailable = false };
        var resolver = new FakeProcessNameResolver();
        var tracker = CreateTracker(native, resolver);

        var sample = tracker.Sample();

        Assert.Equal(ActivitySample.Unknown, sample);
        Assert.Equal(0, native.ForegroundReadCount);
        Assert.Equal(0, resolver.ResolveCount);
    }

    [Fact]
    public void PlatformFailureReturnsUnknownInsteadOfInterruptingFocus()
    {
        var native = new FakeWindowsActivityNativeApi
        {
            IdleException = new PlatformNotSupportedException()
        };
        var tracker = CreateTracker(native);

        var sample = tracker.Sample();

        Assert.Equal(ActivitySample.Unknown, sample);
    }

    [Fact]
    public void InvalidOptionsAreRejectedAtCompositionBoundary()
    {
        var options = new ActivityTrackingOptions(
            TimeSpan.Zero,
            TimeSpan.FromSeconds(60));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateTracker(options: options));
    }

    [Fact]
    public void SampleRejectsSimultaneousActiveAndIdleBuckets()
    {
        Assert.Throws<ArgumentException>(() => new ActivitySample(
            "code",
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void SerializedSampleContainsOnlyApprovedAggregateFields()
    {
        var sample = CreateTracker().Sample();

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(sample));
        var fieldNames = document.RootElement.EnumerateObject()
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[] { "ActiveDuration", "IdleDuration", "ProcessName" },
            fieldNames);

        var publicProperties = typeof(ActivitySample)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(fieldNames, publicProperties);
    }

    [Theory]
    [InlineData(nameof(NativeMethods.GetForegroundWindow))]
    [InlineData(nameof(NativeMethods.GetWindowThreadProcessId))]
    [InlineData(nameof(NativeMethods.GetLastInputInfo))]
    public void RequiredWin32CallsRemainInsideNativeAdapter(string methodName)
    {
        var method = typeof(NativeMethods).GetMethod(
            methodName,
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<DllImportAttribute>());
    }

    private static WindowsActivityTracker CreateTracker(
        FakeWindowsActivityNativeApi? nativeApi = null,
        FakeProcessNameResolver? processNameResolver = null,
        ActivityTrackingOptions? options = null)
    {
        return new WindowsActivityTracker(
            options ?? ActivityTrackingOptions.Default,
            nativeApi ?? new FakeWindowsActivityNativeApi(),
            processNameResolver ?? new FakeProcessNameResolver());
    }
}
