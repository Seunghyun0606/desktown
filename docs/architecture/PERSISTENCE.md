# Persistence Design

## 1. Decision: versioned JSON

Prototype v0.1 uses local JSON rather than SQLite.

| Criterion | JSON | SQLite |
| --- | --- | --- |
| Small aggregate state | Simple | More machinery than needed |
| Human-debuggable prototype | Excellent | Requires query/tooling |
| Schema migrations | Explicit code required | Stronger built-in schema model |
| Atomic write | Temp + replace | Transactional by default |
| Query/history volume | Limited | Strong |

The prototype has one player, one current project, a small settings object, and
compact session summaries. JSON is the smaller reliable choice if writes are
atomic and versioned. Reconsider SQLite only when session history becomes large,
queryable, or cloud/account sync enters scope.

## 2. Files

Under `user://`:

```text
desktown/
├─ save.json
├─ save.backup.json
├─ save.tmp                 # only during write/recovery
└─ logs/
```

Do not store saves under the repository or executable directory.

## 3. Save envelope

```json
{
  "schemaVersion": 1,
  "saveRevision": 42,
  "savedAtUtc": "2026-09-24T12:00:00Z",
  "settings": {},
  "focus": {},
  "town": {},
  "mina": {},
  "player": {},
  "pendingPresentation": {},
  "integrity": {
    "payloadSha256": "..."
  }
}
```

### Settings

- display mode
- Companion monitor, anchor/offset, and scale preset
- Ghost monitor, corner, opacity preset
- audio enabled
- onboarding completed
- reduced motion

### Focus

- at most one recoverable active session checkpoint
- bounded recent session summaries for prototype diagnostics
- start/end UTC, target/count duration, active/idle/unknown duration
- process-name aggregates
- mode-change debug records (never used for reward)

### Town/project

- current project ID
- project progress in seconds/Energy base units
- Workshop state
- unlocked project IDs
- pending and consumed event IDs

### Mina/player

- logical activity/location/project only; visual clip is derived
- total focus seconds and today's date bucket

## 4. Atomic write protocol

1. Serialize a complete immutable snapshot to `save.tmp`.
2. Flush file contents to disk.
3. Validate schema and payload hash by reading the temp representation.
4. Move current `save.json` to `save.backup.json` using replace semantics.
5. Atomically replace `save.json` with the temp file where the platform allows.
6. Only then publish `SaveSucceeded(revision)`.

One `SemaphoreSlim` serializes saves. A later revision cannot be overwritten by
an older queued snapshot.

## 5. Checkpoints

Write at:

- Focus start
- every 15 seconds during active Focus
- display-mode change (settings/debug only)
- OS suspend/lock
- Focus end/complete
- project completion and pending reveal creation
- reveal consumed/discovery unlocked
- orderly quit

The 15-second interval limits recovery loss without writing every logical tick.

## 6. Reward crash safety

Logical completion and visual consumption are separate:

```text
ProjectCompleted
  → WorkshopState = Complete
  → pendingPresentation += workshop_complete
  → save
  → play reveal when Town opens
  → consumedPresentation += workshop_complete
  → unlock railway event/project
  → save
```

If the app crashes during animation, the Workshop remains complete and the
pending reveal may replay. Progress can never roll back because animation did
not finish.

## 7. Load and recovery

1. Load and validate `save.json`.
2. If invalid, load and validate backup.
3. If backup is valid, restore it as current and show a passive recovery banner.
4. If neither is valid, preserve both files for support and offer a new save;
   never silently delete them.
5. If an active checkpoint exists, offer Resume or End at checkpoint.

Unknown JSON fields are ignored. Missing optional fields receive version-specific
defaults. Unsupported future schema versions fail safely without overwrite.

## 8. Migration

```csharp
public interface ISaveMigration
{
    int FromVersion { get; }
    int ToVersion { get; }
    JsonNode Apply(JsonNode source);
}
```

Migrations are sequential, deterministic, pure, and fixture-tested. Migration
creates a new revision and keeps the last pre-migration file as backup.

## 9. Privacy and retention

- No window title, URL, keystroke, screenshot, or document data is serialized.
- Process names are visible to the user in Focus Setup and session summaries.
- Diagnostics redact filesystem username/path where possible.
- A future “clear history” feature is not required for v0.1; save-folder access
  and deleting the local save are sufficient for closed prototype testing.

