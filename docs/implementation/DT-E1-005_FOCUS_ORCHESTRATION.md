# DT-E1-005 — Process Catalog and Focus Orchestration Contract

## Outcome

This task completes the application-level Focus command flow. It connects the
privacy-minimal activity adapter, monotonic `FocusSessionManager`, terminal
`SessionLedger`, and elapsed-time Energy policy without introducing Godot,
display-mode, project, or persistence dependencies.

The Windows process catalog is optional UI metadata. An empty or unavailable
catalog means **Any App** and never blocks Focus.

## Process catalog boundary

`IProcessCatalog.GetRunningProcessNames()` returns a distinct, case-insensitive,
sorted list of bare executable process names. `WindowsProcessCatalog` uses
`.NET Process.GetProcesses()` through an internal test seam and exposes no PID,
HWND, path, title, URL, command line, or document content.

The catalog:

- rejects blank values, paths, and control characters;
- skips processes that exit or become inaccessible during enumeration;
- returns an empty list when enumeration is unavailable;
- performs no background polling and stores nothing.

Selected names are copied into `FocusSession.IntendedProcessNames`. They remain
descriptive intent only. The foreground process does not need to match them for
time or Energy to count.

## Coordinator responsibilities

`FocusSessionCoordinator` owns one command flow:

1. create and start a `FocusSession` with optional intended-process metadata;
2. sample activity only while the session is Running;
3. pass the observed idle state to `FocusSessionManager`, which applies it to
   the actual monotonic interval (including early or delayed ticks);
4. retain per-session active, idle, unknown, and process-name aggregates;
5. record a terminal session idempotently in `SessionLedger`;
6. calculate cumulative Energy from terminal counted time;
7. publish immutable checkpoint and finalized seams for later adapters.

The manager's injected monotonic clock remains authoritative. An unknown sample
or recoverable tracker failure supplies zero idle time, but the elapsed session
interval still counts normally. Activity data never gates, multiplies, or
reduces Energy.

## Activity summary

`FocusActivitySummary` is diagnostic/session-history data:

| Field | Meaning |
| --- | --- |
| `ActiveDuration` | Counted interval observed in an active sample bucket |
| `IdleDuration` | Counted interval observed in an idle sample bucket |
| `UnknownDuration` | Counted interval when tracking was unavailable |
| `Processes` | Applied counted duration grouped by bare process name |

Aggregation uses the manager's applied duration so a final tick clamped at the
target cannot over-report activity. A delayed sample represents at most its
configured observation interval for process attribution; the rest is grouped
under `Unknown`. `Unknown` is also a normal process bucket when a name cannot
be resolved. These totals are not productivity scores.

## Lifecycle and checkpoint seams

`CheckpointAvailable` is an immutable request/snapshot seam emitted after start,
running ticks, suspend, resume, and terminal finalization. It does not write a
file. `SessionFinalized` is emitted only after the terminal session has been
accepted by the ledger and the new cumulative Energy has been calculated.

JSON schema, atomic writes, retries, recovery, checkpoint throttling, and app
restart behavior remain deferred to E8. A persistence adapter may later consume
the checkpoint seam without being referenced by the coordinator.

## Fixed invariants

- No coordinator or Energy API accepts `DisplayMode`.
- Switching Companion, Hidden, or Ghost cannot create or finalize a session.
- Empty catalog and empty intended apps are valid.
- Tracker failure degrades to timer-only Focus and does not end the session.
- Only terminal sessions enter the ledger and cumulative Energy.
- Process selection never changes reward.
- No Godot or Win32 type crosses the application boundary.

## Verification

Automated tests cover Any App startup, intended-app mismatch, unknown and failed
tracking, active/idle aggregation, suspend/resume sampling, early stop,
automatic completion, checkpoint ordering, cumulative Energy retention, and
the absence of display-mode dependencies. Windows adapter tests cover catalog
sanitization, deduplication, sorting, empty results, and recoverable failures.

Manual Windows verification remains required for the process picker: inspect a
representative catalog, confirm inaccessible processes do not break the list,
and start Focus with both Any App and a selected app.
