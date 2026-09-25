# DT-E2-002 Mina Logical State Machine Contract

## Scope and boundaries

`MinaStateMachine` is a deterministic domain component. It reads the current
`FocusSessionStatus` and consecutive user inactivity supplied by its caller,
and exposes `MinaSimulationState(Activity, Location)` plus a separate
`MinaPresentationIntent(PrimaryClip, FollowUpClip?)`.

It does not subscribe to Windows APIs, render a sprite, inspect a process,
choose a display mode, calculate Focus Energy, or update the Workshop. The same
instance continues when Companion and Ghost renderers are hidden. Its cues
can be consumed by a renderer later; `Changed == false` means the steady clip
should not be restarted on every logical tick.

## Transition rules

| Observation | Logical activity/location | One-time presentation on transition |
| --- | --- | --- |
| No running session | Idle / Home | Idle |
| Running, input active | Work / Workshop | Walk then Work when arriving or returning from Rest |
| Running, 60–179 s consecutive idle | Work / Workshop | Stretch then Work once per short-idle episode |
| Running, ≥180 s consecutive idle | Rest / Workshop | Rest |
| Suspended | Rest / Workshop | Rest |
| Project reward reveal | Celebrate / Workshop | Final Work beat then Celebrate |
| Reveal acknowledged | Idle / Home | Idle |

Thresholds come from `MinaThresholds` (60/180 s defaults). The future
composition layer passes `PrototypeOptions.ShortIdleThreshold` and
`LongIdleThreshold`; Domain does not reference Application configuration.
The supplied inactivity is the age of a *continuous* idle episode, not the
session's cumulative idle total. ActivityTracker currently samples idle as
one-second buckets, so DT-E2-003 must derive this episode age, reset it on
observed input, and avoid fabricating activity when tracking is unavailable.
No idle value is used as a productivity grade or reward multiplier.

`ProjectSystem` emits `ProjectCompleted(RestoreWorkshop)` when progression
crosses its target. A later pending-reveal coordinator should deliver it to
`CelebrateProject` when the user opens Town, then call
`AcknowledgeCelebration` after playback. Delivery is idempotent, including
after restoration. The machine holds Celebrate during the reveal instead of
letting the completed session immediately replace it with Idle.

## Save handoff

`CaptureCheckpoint()` returns an immutable `MinaStateCheckpoint`: logical
state, short-idle one-shot latch, and Workshop celebration latch. The JSON
adapter maps these domain values to its versioned DTO and calls `Restore`,
which rejects impossible activity/location combinations and invalid enum
values. Checkpoint creation and restore perform no I/O. The short-idle age is
owned by the future observation coordinator; if restored in a running focus
session, its counter must be restored consistently with the session.

## Verification and limits

Unit tests cover 59/60 and 179/180-second boundaries, custom thresholds,
one-shot Stretch, suspended/return behavior, celebration replay, checkpoint
restore, invalid state, and identical outputs from three independent display
projections. A Godot animator, ActivityTracker-to-Mina adapter, and actual
Windows lock/resume behavior are later tasks.
