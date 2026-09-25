# DT-E8-001 — V1 JSON save contract

This task establishes the wire schema and repository boundary. The application
creates a `GameStateSnapshot`; `SaveStateMapper` explicitly maps it to V1 DTOs;
`JsonSaveCodec` serializes and validates the envelope. `JsonGameStateStore`
receives a concrete filesystem path. The Godot host maps
`user://desktown/save.json` to that path through `GameStatePathResolver`.

## Envelope and precision

The JSON root contains `schemaVersion: 1`, nonnegative `saveRevision`, UTC
`savedAtUtc`, `settings`, `focus`, `town`, `mina`, `player`,
`pendingPresentation`, and `integrity.payloadSha256`. All durations and Focus
Energy values are signed 64-bit `*Ticks` values (100 ns); negative values are
invalid. Focus keeps all terminal ledger entries so a previously recorded ID
remains idempotent after restart. The optional active checkpoint remains data
until E8-003 decides whether to resume or end it. Session activity buckets and
bare process names are descriptive; none change the Energy policy.

Town records project progress and `lastObservedCumulativeEnergyTicks`
separately. The mapper rejects a project high-watermark above the ledger total,
or a player total that differs from the ledger total. Workshop state must match
the state derived from progress. After restoration, applying the same cumulative
Energy is a no-op.

Mina saves `Idle|Work|Rest|Celebrate`, `Home|Workshop`, the current project ID,
and the two one-shot logical latches (`shortIdleStretchPlayed` and
`workshopCelebrated`). `Walk` and `Stretch` remain visual clips, not persisted
logical activities. On integration, these scalar fields map to
`MinaStateCheckpoint` and `MinaStateMachine.Restore`; the mapper already enforces
the same V1 activity/location/latch constraints.

## Integrity and compatibility

SHA-256 covers compact UTF-8 JSON serialization of the entire root object with
the `integrity` property removed. Object property order is preserved by the
codec; formatting whitespace does not participate. Unknown fields remain in
the hashed payload and are ignored by V1 DTO mapping. A future schema version,
missing hash, hash mismatch, malformed JSON, missing required section, or
inconsistent domain state fails load. Failed loads do not modify the source
file. A producer editing unknown fields must recompute the hash.

Sequential writes require an increasing revision and refuse to replace an
unsupported or invalid existing save. This guard does not prevent concurrent
writes or make direct writes crash-safe; E8-002 supplies that protocol.

Only known scalar fields are mapped. Window titles, URLs, typed text, document
contents, screenshots, process IDs, window handles, paths, and visual clips are
not fields in the DTO. Foreground process names must be bare names without a
path, control character, or colon. Settings display mode affects presentation
only; it is absent from Energy and Project computation.

## Scope boundary

The current store writes directly to `save.json`, so an interrupted write is
not yet crash-safe. E8-002 must add temp-file flushing, serialized revisions,
atomic replace, backup recovery, and migrations. E8-003 owns checkpoint timing
and active-session restart decisions. No application bootstrap calls the store
yet; `GameStatePathResolver.CreateDefaultStore()` is the composition entry for
those later tasks.
