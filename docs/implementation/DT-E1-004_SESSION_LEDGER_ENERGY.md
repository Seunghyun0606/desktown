# DT-E1-004 Session Ledger and Energy Contract

## Scope

This task converts finalized FocusSession counted time into v0.1 Focus Energy.
It does not apply Energy to a Town project, persist the ledger, orchestrate the
Windows activity tracker, or introduce a display-mode dependency.

## Unit contract

Focus Energy keeps two meanings explicit:

| Value | Meaning |
| --- | --- |
| `CountedDuration` | Authoritative tick-precision progression retained across sessions |
| `CountedSeconds` | Whole-second projection for persistence and reporting |
| `DisplayedWholeMinutes` | Quiet user-facing Focus value, `CountedSeconds / 60` |

The ledger first aggregates exact `TimeSpan` ticks across all finalized
sessions. `ElapsedTimeEnergyPolicy.CalculateTotal` retains that precision in a
cumulative `FocusEnergy`. Presentation rounds down only after aggregation, so
short sessions and sub-second remainders compose without drift.

Sub-second values can exist at the monotonic-clock boundary. They remain in the
ledger and Energy value and can combine across sessions before second/minute
presentation. The prototype's production logical tick remains one second.

The policy returns a cumulative total, not a project-progress increment. A
consumer must apply only `currentTotal.DeltaSince(previouslyAppliedTotal)`. This
explicit contract prevents replaying all historical Energy on every completion.

## Recording and idempotency

`SessionLedger.TryRecord` accepts only terminal `Completed` or `Stopped`
sessions and snapshots their immutable audit metadata.

- the first application of a session ID returns `true`;
- replaying the exact snapshot returns `false` without changing totals;
- replaying an ID with different data throws `SessionLedgerConflictException`;
- Running, Suspended, and Ready sessions are rejected;
- checked arithmetic rejects total-duration overflow before mutation.

This boundary prevents duplicate Energy when a completion event or recovered
save is replayed. Project-level idempotency remains the responsibility of the
later ProjectSystem task.

## Non-punitive policy

Only `CountedDuration` enters the Energy calculation. The following remain
descriptive metadata and never change Energy:

- idle duration;
- intended process names;
- foreground-process samples;
- display mode or mode-change history.

The Energy types have no DisplayMode property or reference. Equal counted time
therefore always produces equal Energy in Companion, Hidden, and Ghost.

## Early stop behavior

An early-stopped session keeps every counted second. The UI shows only complete
minutes, but the remainder carries into later sessions through the ledger. This
implements the existing test-strategy decision that early end retains seconds
and avoids per-session rounding loss.

## Deferred

- applying Energy to Workshop progress;
- JSON serialization and recovery of ledger snapshots;
- bounded session-history retention;
- FocusSessionCoordinator integration;
- activity/process aggregate persistence.
