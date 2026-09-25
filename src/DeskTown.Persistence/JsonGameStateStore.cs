using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using DeskTown.Application.Persistence;
using DeskTown.Application.Ports;

namespace DeskTown.Persistence;

/// <summary>Serialized revisions, flushed temp writes, atomic replace, backup and migration.</summary>
public sealed class JsonGameStateStore : IGameStateStore
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly string _saveFilePath;
    private readonly string _backupPath;
    private readonly string _tempPath;
    private readonly SemaphoreSlim _gate;
    private readonly IReadOnlyDictionary<int, ISaveMigration> _migrations;
    private readonly Action? _beforeReplace;

    public JsonGameStateStore(string saveFilePath, IEnumerable<ISaveMigration>? migrations = null)
        : this(saveFilePath, migrations, null) { }

    internal JsonGameStateStore(
        string saveFilePath, IEnumerable<ISaveMigration>? migrations, Action? beforeReplace)
    {
        if (string.IsNullOrWhiteSpace(saveFilePath))
            throw new ArgumentException("A save file path is required.", nameof(saveFilePath));
        _saveFilePath = Path.GetFullPath(saveFilePath);
        _gate = Gates.GetOrAdd(_saveFilePath, _ => new SemaphoreSlim(1, 1));
        var directory = Path.GetDirectoryName(_saveFilePath)!;
        _backupPath = Path.Combine(directory, "save.backup.json");
        _tempPath = Path.Combine(directory, "save.tmp");
        _beforeReplace = beforeReplace;

        var steps = (migrations ?? []).ToArray();
        if (steps.Any(step => step is null || step.FromVersion < 0
                || step.ToVersion != step.FromVersion + 1 || step.ToVersion > 1)
            || steps.GroupBy(step => step.FromVersion).Any(group => group.Count() != 1))
            throw new ArgumentException("Save migrations must be unique, sequential V1 steps.", nameof(migrations));
        _migrations = steps.ToDictionary(step => step.FromVersion);
    }

    public SaveLoadStatus LastLoadStatus { get; private set; } = SaveLoadStatus.NoSave;

    public async Task<StoredGameState?> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try { return await LoadCoreAsync(cancellationToken); }
        finally { _gate.Release(); }
    }

    public async Task SaveAsync(GameStateSnapshot snapshot, long revision,
        DateTimeOffset savedAtUtc, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var bytes = JsonSaveCodec.Encode(snapshot, revision, savedAtUtc);
            var existing = await LoadCoreAsync(cancellationToken);
            if (existing is not null && revision <= existing.Revision)
                throw new InvalidDataException("A save revision must increase.");

            await WriteTempAndValidateAsync(bytes, cancellationToken);
            try
            {
                _beforeReplace?.Invoke();
                if (File.Exists(_saveFilePath))
                    File.Replace(_tempPath, _saveFilePath, _backupPath);
                else
                    File.Move(_tempPath, _saveFilePath);
            }
            finally { DeleteTemp(); }
        }
        finally { _gate.Release(); }
    }

    private async Task<StoredGameState?> LoadCoreAsync(CancellationToken cancellationToken)
    {
        LastLoadStatus = SaveLoadStatus.NoSave;
        if (!File.Exists(_saveFilePath) && !File.Exists(_backupPath)) return null;

        if (File.Exists(_saveFilePath))
        {
            try
            {
                var primary = await ReadVersionedAsync(_saveFilePath, cancellationToken);
                if (primary.MigratedBytes is not null)
                {
                    await WriteTempAndValidateAsync(primary.MigratedBytes, cancellationToken);
                    try { File.Replace(_tempPath, _saveFilePath, _backupPath); }
                    finally { DeleteTemp(); }
                    LastLoadStatus = SaveLoadStatus.Migrated;
                }
                else LastLoadStatus = SaveLoadStatus.Loaded;
                return primary.State;
            }
            catch (UnsupportedSaveSchemaException) { throw; }
            catch (InvalidDataException) { /* Only a validated backup can recover this file. */ }
        }

        if (!File.Exists(_backupPath))
            throw new InvalidDataException("The primary save is invalid and no backup exists.");

        var backup = await ReadVersionedAsync(_backupPath, cancellationToken);
        await WriteTempAndValidateAsync(backup.MigratedBytes
            ?? await File.ReadAllBytesAsync(_backupPath, cancellationToken), cancellationToken);
        try
        {
            if (File.Exists(_saveFilePath))
            {
                var preserved = _saveFilePath + ".corrupt-" + Guid.NewGuid().ToString("N");
                File.Move(_saveFilePath, preserved);
            }
            File.Move(_tempPath, _saveFilePath);
        }
        finally { DeleteTemp(); }
        LastLoadStatus = SaveLoadStatus.RecoveredFromBackup;
        return backup.State;
    }

    private async Task<(StoredGameState State, byte[]? MigratedBytes)> ReadVersionedAsync(
        string path, CancellationToken cancellationToken)
    {
        try
        {
            var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            var version = JsonSaveCodec.ReadSchemaVersion(bytes);
            if (version > 1) throw new UnsupportedSaveSchemaException(version);
            if (version == 1) return (JsonSaveCodec.Decode(bytes), null);

            var source = JsonNode.Parse(bytes) as JsonObject
                ?? throw new InvalidDataException("Migration source must be an object.");
            var oldRevision = source["saveRevision"]?.GetValue<long>()
                ?? throw new InvalidDataException("Migration source revision is missing.");
            if (oldRevision < 0) throw new InvalidDataException("Migration source revision is invalid.");
            while (version < 1)
            {
                if (!_migrations.TryGetValue(version, out var migration))
                    throw new InvalidDataException($"No migration from save schema {version}.");
                source = migration.Apply(source.DeepClone()) as JsonObject
                    ?? throw new InvalidDataException("Migration must return an object.");
                version = JsonSaveCodec.ReadSchemaVersion(
                    System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(source));
                if (version != migration.ToVersion)
                    throw new InvalidDataException("Migration returned an unexpected schema version.");
            }

            source["saveRevision"] = checked(oldRevision + 1);
            var migratedBytes = JsonSaveCodec.SealMigratedPayload(source);
            return (JsonSaveCodec.Decode(migratedBytes), migratedBytes);
        }
        catch (Exception error) when (error is System.Text.Json.JsonException
            or ArgumentException or InvalidOperationException or OverflowException)
        {
            throw new InvalidDataException("Save migration or payload is invalid.", error);
        }
    }

    private async Task WriteTempAndValidateAsync(byte[] bytes, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_saveFilePath)!);
        try
        {
            await using (var stream = new FileStream(_tempPath, FileMode.Create, FileAccess.Write,
                FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await stream.WriteAsync(bytes, cancellationToken);
                stream.Flush(flushToDisk: true);
            }
            _ = JsonSaveCodec.Decode(await File.ReadAllBytesAsync(_tempPath, cancellationToken));
        }
        catch { DeleteTemp(); throw; }
    }

    private void DeleteTemp()
    {
        if (File.Exists(_tempPath)) File.Delete(_tempPath);
    }
}
