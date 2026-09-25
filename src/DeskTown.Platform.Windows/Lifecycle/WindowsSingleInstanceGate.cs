using System.IO.Pipes;
using System.Text;

namespace DeskTown.Platform.Windows.Lifecycle;

/// <summary>
/// Named mutex prevents a second DeskTown session. A local pipe forwards an
/// OPEN request to the existing process; no save or Focus state is shared.
/// </summary>
public sealed class WindowsSingleInstanceGate : IDisposable
{
    public const string DefaultMutexName = @"Local\DeskTown.Prototype.v0.1";
    public const string DefaultPipeName = "DeskTown.Prototype.Open.v0.1";

    private readonly Mutex _mutex;
    private readonly string _pipeName;
    private readonly bool _primary;
    private bool _disposed;

    private WindowsSingleInstanceGate(Mutex mutex, string pipeName, bool primary)
    {
        _mutex = mutex;
        _pipeName = pipeName;
        _primary = primary;
    }

    public bool IsPrimary => _primary;

    public static WindowsSingleInstanceGate Acquire(
        string mutexName = DefaultMutexName, string pipeName = DefaultPipeName)
    {
        if (string.IsNullOrWhiteSpace(mutexName) || string.IsNullOrWhiteSpace(pipeName))
            throw new ArgumentException("Mutex and pipe names are required.");
        // Existence, not thread ownership, is the gate; disposal can occur on
        // a different UI/async thread without abandoning an owned mutex.
        var mutex = new Mutex(false, mutexName, out var createdNew);
        return new WindowsSingleInstanceGate(mutex, pipeName, createdNew);
    }

    public async Task<bool> RequestOpenAsync(CancellationToken cancellationToken = default)
    {
        if (_primary) throw new InvalidOperationException("Primary instance cannot request itself.");
        using var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out,
            PipeOptions.Asynchronous);
        try
        {
            await client.ConnectAsync(1000, cancellationToken);
            using var writer = new StreamWriter(client, Encoding.UTF8, 1024, leaveOpen: true)
            { AutoFlush = true };
            await writer.WriteLineAsync("OPEN");
            return true;
        }
        catch (Exception error) when (error is IOException or TimeoutException)
        {
            return false;
        }
    }

    public async Task ListenAsync(Func<Task> onOpen, CancellationToken cancellationToken)
    {
        if (!_primary) throw new InvalidOperationException("Only the primary instance listens.");
        ArgumentNullException.ThrowIfNull(onOpen);
        while (!cancellationToken.IsCancellationRequested)
        {
            using var server = new NamedPipeServerStream(_pipeName, PipeDirection.In,
                1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            try
            {
                await server.WaitForConnectionAsync(cancellationToken);
                using var reader = new StreamReader(server, Encoding.UTF8, true, 1024, leaveOpen: true);
                if (await reader.ReadLineAsync(cancellationToken) == "OPEN")
                    await onOpen();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _mutex.Dispose();
    }
}
