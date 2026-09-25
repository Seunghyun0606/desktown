# DT-E8-002 — Atomic JSON save and backup recovery

`JsonGameStateStore` serializes operations for the same local path within the
process and requires a strictly increasing revision. A save is written to
`save.tmp`, flushed, read back and hash/schema validated, then moved to the
primary file. Replacing an existing primary atomically moves its previous
contents to `save.backup.json`. A failed pre-replace write leaves the primary
intact and removes the temp file. The files share one directory/volume.

`LoadAsync` validates the primary first. A future schema version raises
`UnsupportedSaveSchemaException` and is never overwritten or silently replaced
with an older backup. If the primary is damaged, the store validates the backup,
preserves damaged bytes as `save.json.corrupt-<id>`, and restores the backup as
primary. If neither copy is valid, both remain in place and load fails.
`LastLoadStatus` reports `RecoveredFromBackup` or `Migrated` for the later passive
recovery UI. The recovery banner itself belongs to the UI/lifecycle task.

`ISaveMigration` is a pure sequential `JsonNode` transformation. The store
clones the source, verifies each version increment, increments revision, seals
the migrated V1 payload, validates it and atomically installs it while keeping
the pre-migration file as backup. No legacy format has shipped, so production
registers no migration; a V0 fixture adapter tests the mechanism.

The in-process gate does not replace the planned Windows single-instance
guard (E8-003). Actual Windows filesystem, forced termination, and recovery
banner inspection remain manual checks. Automated tests cover interrupted
write, hash/truncation backup recovery, preservation of two bad copies, future
version refusal, migration fixture, and stale concurrent revision rejection.
