using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DeskTown.Application.Persistence;
using DeskTown.Application.Sessions;
using DeskTown.Domain.Focus;
using DeskTown.Domain.Projects;
using DeskTown.Persistence;

namespace DeskTown.Application.Tests;

public sealed class JsonGameStateStoreTests
{
    private static readonly DateTimeOffset StartedAt = new(2026, 9, 25, 9, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Counted = TimeSpan.FromTicks(TimeSpan.FromSeconds(90).Ticks + 123);

    [Fact]
    public void V1_round_trip_retains_exact_ticks_and_prevents_energy_reapplication()
    {
        var (snapshot, finalized) = Example();
        var bytes = JsonSaveCodec.Encode(snapshot, 42, StartedAt.AddMinutes(3));

        var restored = JsonSaveCodec.Decode(bytes);

        Assert.Equal(42, restored.Revision);
        Assert.Equal(Counted, restored.Snapshot.Focus.Ledger.TotalCountedDuration);
        Assert.Equal(TimeSpan.FromTicks(Counted.Ticks - 7), restored.Snapshot.Focus.Ledger.TotalIdleDuration);
        Assert.Equal(Counted, restored.Snapshot.Town.Project.Progress.CountedDuration);
        Assert.Equal(Counted, restored.Snapshot.Town.Project.LastObservedCumulativeEnergy.CountedDuration);
        Assert.Equal(Counted.Ticks, restored.Snapshot.Player.TotalFocusTicks);
        Assert.False(restored.Snapshot.Focus.Ledger.TryRecord(finalized));
        Assert.Equal(FocusEnergy.Zero, restored.Snapshot.Town.Project.ApplyCumulativeEnergy(
            new ElapsedTimeEnergyPolicy().CalculateTotal(restored.Snapshot.Focus.Ledger)).ObservedDelta);
        Assert.Equal("Ghost", restored.Snapshot.Settings.DisplayMode);
        Assert.Equal(65, restored.Snapshot.Settings.GhostOpacityPercent);
        Assert.Equal("Work", restored.Snapshot.Mina.Activity);
        Assert.True(restored.Snapshot.Mina.ShortIdleStretchPlayed);
        Assert.False(restored.Snapshot.Mina.WorkshopCelebrated);
        Assert.Equal("workshop_complete", Assert.Single(restored.Snapshot.PendingPresentation.PendingIds));
        Assert.Equal(17, restored.Snapshot.Focus.ActiveCheckpoint!.CountedDuration.Ticks);
        Assert.Equal(3, restored.Snapshot.Focus.ActiveCheckpoint.Activity.UnknownDuration.Ticks);
        Assert.Equal("code.exe", Assert.Single(restored.Snapshot.Focus.ActiveCheckpoint.Activity.Processes).ProcessName);
    }

    [Fact]
    public void V1_envelope_has_expected_sections_and_integrity_over_full_payload()
    {
        var (snapshot, _) = Example();
        var root = Parse(JsonSaveCodec.Encode(snapshot, 7, StartedAt));
        Assert.Equal(1, root["schemaVersion"]!.GetValue<int>());
        Assert.Equal(7, root["saveRevision"]!.GetValue<long>());
        foreach (var section in new[] { "settings", "focus", "town", "mina", "player", "pendingPresentation", "integrity" })
            Assert.NotNull(root[section]);

        var savedHash = root["integrity"]!["payloadSha256"]!.GetValue<string>();
        root.Remove("integrity");
        var computed = Convert.ToHexString(SHA256.HashData(
            JsonSerializer.SerializeToUtf8Bytes(root, new JsonSerializerOptions(JsonSerializerDefaults.Web))))
            .ToLowerInvariant();
        Assert.Equal(computed, savedHash);
    }

    [Fact]
    public void Unknown_fields_are_tolerated_when_their_integrity_is_valid()
    {
        var (snapshot, _) = Example();
        var root = Parse(JsonSaveCodec.Encode(snapshot, 1, StartedAt));
        root["futureField"] = new JsonObject { ["next"] = true };
        root["settings"]!["futurePreset"] = 11;

        var restored = JsonSaveCodec.Decode(Rehash(root));

        Assert.Equal("Ghost", restored.Snapshot.Settings.DisplayMode);
    }

    [Fact]
    public void Tampered_payload_is_rejected_without_mutation()
    {
        var (snapshot, _) = Example();
        var root = Parse(JsonSaveCodec.Encode(snapshot, 1, StartedAt));
        root["town"]!["projectProgressTicks"] = 0;
        var before = Encoding.UTF8.GetBytes(root.ToJsonString());

        Assert.Throws<InvalidDataException>(() => JsonSaveCodec.Decode(before));
        Assert.Equal(before, Encoding.UTF8.GetBytes(root.ToJsonString()));
    }

    [Fact]
    public void Unsupported_future_schema_is_rejected_even_with_matching_hash()
    {
        var (snapshot, _) = Example();
        var root = Parse(JsonSaveCodec.Encode(snapshot, 1, StartedAt));
        root["schemaVersion"] = 2;
        Assert.Throws<InvalidDataException>(() => JsonSaveCodec.Decode(Rehash(root)));
    }

    [Fact]
    public void Impossible_high_watermark_is_rejected_even_with_matching_hash()
    {
        var (snapshot, _) = Example();
        var root = Parse(JsonSaveCodec.Encode(snapshot, 1, StartedAt));
        root["town"]!["lastObservedCumulativeEnergyTicks"] = Counted.Ticks + 1;
        Assert.Throws<InvalidDataException>(() => JsonSaveCodec.Decode(Rehash(root)));
    }

    [Fact]
    public void Invalid_Mina_logical_state_is_rejected_even_with_matching_hash()
    {
        var (snapshot, _) = Example();
        var root = Parse(JsonSaveCodec.Encode(snapshot, 1, StartedAt));
        root["mina"]!["activity"] = "Stretch";
        Assert.Throws<InvalidDataException>(() => JsonSaveCodec.Decode(Rehash(root)));
    }

    [Fact]
    public void Save_whitelist_excludes_sensitive_activity_fields()
    {
        var (snapshot, _) = Example();
        var json = Encoding.UTF8.GetString(JsonSaveCodec.Encode(snapshot, 1, StartedAt));

        foreach (var forbidden in new[]
        {
            "windowTitle", "browserUrl", "typedText", "keystrokes", "screenshot",
            "documentContents", "fileContents", "processId", "hwnd"
        })
        {
            Assert.DoesNotContain('"' + forbidden + '"', json, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Repository_reads_writes_configured_path_and_preserves_invalid_file()
    {
        var directory = Path.Combine(Path.GetTempPath(), "desktown-save-test-" + Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "save.json");
        var repository = new JsonGameStateStore(path);
        var (snapshot, _) = Example();

        try
        {
            Assert.Null(await repository.LoadAsync());
            await repository.SaveAsync(snapshot, 9, StartedAt);
            Assert.Equal(9, (await repository.LoadAsync())!.Revision);

            var invalid = Encoding.UTF8.GetBytes("{broken");
            await File.WriteAllBytesAsync(path, invalid);
            await Assert.ThrowsAsync<InvalidDataException>(() => repository.LoadAsync());
            await Assert.ThrowsAsync<InvalidDataException>(() => repository.SaveAsync(snapshot, 10, StartedAt));
            Assert.Equal(invalid, await File.ReadAllBytesAsync(path));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    private static (GameStateSnapshot Snapshot, FocusSession Finalized) Example()
    {
        var finalized = FocusSession.Create(FocusSessionId.From(Guid.Parse("9773f4a3-8f60-435d-81c3-c99c3e4e0f9a")),
            TimeSpan.FromMinutes(25), ["code.exe"]);
        finalized.Start(StartedAt);
        finalized.Accumulate(Counted, TimeSpan.FromTicks(Counted.Ticks - 7));
        finalized.Stop(StartedAt.AddMinutes(2));
        var ledger = new SessionLedger();
        ledger.TryRecord(finalized);
        var project = ProjectSystem.CreateRestoreWorkshop();
        project.ApplyCumulativeEnergy(new ElapsedTimeEnergyPolicy().CalculateTotal(ledger));
        var active = new FocusSessionCheckpoint(
            FocusSessionId.From(Guid.Parse("00a4b324-b444-423b-b60b-698a5906a282")),
            FocusSessionStatus.Running, StartedAt.AddMinutes(3), null,
            TimeSpan.FromMinutes(45), TimeSpan.FromTicks(17), TimeSpan.FromTicks(2),
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "code.exe" },
            new FocusActivitySummary(TimeSpan.FromTicks(12), TimeSpan.FromTicks(2),
                TimeSpan.FromTicks(3), [new ProcessActivityDuration("code.exe", TimeSpan.FromTicks(12))]),
            FocusEnergy.FromCountedDuration(ledger.TotalCountedDuration));
        return (new GameStateSnapshot(
            new SettingsSnapshot("Ghost",
                new WindowPlacementSnapshot("MONITOR-1", "BottomRight", -24, -24),
                new WindowPlacementSnapshot("MONITOR-2", "TopLeft", 8, 8),
                100, 65, true, true, false),
            new FocusSnapshot(ledger, active,
                [new DisplayModeChangeSnapshot(StartedAt, "Companion"),
                    new DisplayModeChangeSnapshot(StartedAt.AddMinutes(1), "Ghost")]),
            new TownSnapshot(project, ["RestoreWorkshop"], ["railway_map"], []),
            new MinaSnapshot("Work", "Workshop", "RestoreWorkshop", true, false),
            new PlayerSnapshot(Counted.Ticks, DateOnly.FromDateTime(StartedAt.UtcDateTime), Counted.Ticks),
            new PendingPresentationSnapshot(["workshop_complete"], [])), finalized);
    }

    private static JsonObject Parse(byte[] data) =>
        (JsonNode.Parse(data) as JsonObject)!;

    private static byte[] Rehash(JsonObject root)
    {
        var payload = (JsonObject)root.DeepClone();
        payload.Remove("integrity");
        var hash = Convert.ToHexString(SHA256.HashData(
            JsonSerializer.SerializeToUtf8Bytes(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web))))
            .ToLowerInvariant();
        root["integrity"] = new JsonObject { ["payloadSha256"] = hash };
        return JsonSerializer.SerializeToUtf8Bytes(root);
    }
}
