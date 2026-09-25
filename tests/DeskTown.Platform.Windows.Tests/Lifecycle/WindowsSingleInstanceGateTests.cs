using DeskTown.Platform.Windows.Lifecycle;

namespace DeskTown.Platform.Windows.Tests;

public sealed class WindowsSingleInstanceGateTests
{
    [Fact]
    public async Task Second_launch_requests_open_without_becoming_primary()
    {
        var suffix = Guid.NewGuid().ToString("N");
        using var primary = WindowsSingleInstanceGate.Acquire("DeskTown.Test." + suffix,
            "DeskTown.Test.Open." + suffix);
        using var secondary = WindowsSingleInstanceGate.Acquire("DeskTown.Test." + suffix,
            "DeskTown.Test.Open." + suffix);
        Assert.True(primary.IsPrimary);
        Assert.False(secondary.IsPrimary);

        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var received = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var listener = primary.ListenAsync(() =>
        {
            received.TrySetResult();
            return Task.CompletedTask;
        }, cancellation.Token);

        Assert.True(await secondary.RequestOpenAsync(cancellation.Token));
        await received.Task.WaitAsync(TimeSpan.FromSeconds(3));
        cancellation.Cancel();
        await listener;
    }
}
