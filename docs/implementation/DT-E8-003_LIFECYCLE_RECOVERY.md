# DT-E8-003 — Checkpoint and restart recovery contract

`ApplicationLifecycleCoordinator` loads one aggregate save and exposes an
active checkpoint as a pending decision. It never silently resumes. The host
constructs `FocusSessionCoordinator` using the restored ledger, then calls
`ResolveRecovery(Resume|EndAtCheckpoint)`. Either choice first reconstructs
the Focus session as Suspended at its exact counted/idle ticks. Resume starts a
fresh monotonic baseline. End finalizes only the saved ticks. Wall time during
shutdown, lock, or sleep is never applied as Focus Energy.

The `CheckpointScheduler` uses monotonic time: 15 seconds during a running
session, immediate saves at Focus start/end, suspension, display changes,
world/event mutations, recovery decisions, and quit. The host supplies a
consistent `GameStateSnapshot` with the current active checkpoint or terminal
ledger and calls `SaveAsync` with a reason. Failed writes do not advance the
revision or cadence. The local JSON store atomically writes and guards stale
revisions. The caller must advance ProjectSystem/EventSystem and capture them
in the same aggregate after a recovered End; no scene animation controls it.

`WindowsSingleInstanceGate` uses a named mutex object to prevent a second
session and a named pipe carrying only `OPEN` to the existing process. AppRoot
acquires the gate before initializing services; the second instance forwards
the request and exits. An Open request focuses the existing window and does
not create a Focus session.

`RecoveryPrompt.tscn` is a small reusable Resume/End decision view. The current
foundation shell does not yet construct a running Focus UI, so the prompt's
choice event is not bound to the lifecycle coordinator in AppRoot. The
composition hookup and recovery banner belong with Focus Setup/Main shell;
this task remains in review until an exported Windows run covers crash,
sleep/lock, restart, duplicate launch and the actual prompt workflow.

Automated tests cover a 15-second fake-clock cadence, failed save retry,
two-hour offline resume, End-at-checkpoint idempotency, JSON restart
round-trip, and second-instance Open forwarding. A cross-process Windows
test and sleep/lock system gate are still manual.
