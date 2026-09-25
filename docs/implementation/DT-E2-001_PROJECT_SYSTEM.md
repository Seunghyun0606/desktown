# DT-E2-001 Project System Contract

## Scope

This task turns v0.1 Focus Energy into deterministic progress for the only
playable prototype project, `RestoreWorkshop`. It owns logical project progress
and the Workshop's `Broken`, `Repairing`, and `Complete` states.

It does not implement Mina, Town rendering, Railway discovery, a generic event
bus, persistence, or additional playable projects. The completion value emitted
by this boundary is input for the later EventSystem and presentation flow.

## Progress unit and state boundaries

The target is exactly 25 counted minutes, retained as tick-precise
`FocusEnergy`:

| Progress | Workshop state |
| --- | --- |
| exactly zero | `Broken` |
| greater than zero and below 25 minutes | `Repairing` |
| 25 minutes | `Complete` |

Progress clamps at the target. Extra Energy remains visible in the cumulative
high-watermark but is not added beyond 25 minutes. Presentation may show whole
Focus minutes through `DisplayedWholeMinutes`; progression does not round each
session or delta.

## Cumulative Energy boundary

`ProjectSystem.ApplyCumulativeEnergy` accepts the cumulative result of
`IEnergyPolicy.CalculateTotal`, not an unlabelled increment. The ProjectSystem
stores `LastObservedCumulativeEnergy` and calculates:

```text
observed delta = current cumulative total
                 .DeltaSince(last observed cumulative total)
```

This creates three explicit behaviors:

- a larger total applies only the new delta;
- replaying the same total is an idempotent no-op;
- a lower total is stale or corrupt input and is rejected before mutation.

`CreateRestoreWorkshop(baseline)` marks Energy at or before the baseline as
historical. `RestoreWorkshop(progress, highWatermark)` validates and rehydrates
both values without performing I/O. This is the persistence handoff: a later
save schema must store them together rather than replaying the entire
SessionLedger total. Persistence itself is deferred to E8.

The application result separates `ObservedDelta` from `AppliedToProject`.
They differ when an update passes the project target. The entire cumulative
total is still observed, while project progress remains clamped.

## Completion contract

Crossing the exact target returns one `ProjectCompleted(RestoreWorkshop)` domain
value in the application result. Replaying the total, or observing later Energy
after completion, does not return it again. Logical completion is committed by
the ProjectSystem before a later layer dispatches or presents the value.

The result value is intentionally not a C# callback/event. A subscriber failure
therefore cannot interrupt or roll back the already deterministic state change.
The later EventSystem owns durable pending presentation and Railway discovery.

## Architectural invariants

- `ProjectSystem` depends only on the Domain `FocusEnergy` value.
- Display mode is absent from its API and calculations.
- Godot, Win32, activity samples, process names, and visual assets are absent.
- Companion, Hidden, and Ghost therefore apply the same cumulative total and
  produce identical progress.

## Verification

Domain tests cover the exact target, one-tick-below boundary, all Workshop
states, clamping, split versus one-shot updates, same-total replay, decreasing
totals, baseline history, subsecond precision, one-shot completion, and
dependency absence.

Deferred work:

- save DTO and JSON mapping for progress plus high-watermark;
- EventSystem pending/consumed event handling;
- Railway discovery and next-project teaser;
- Town and Workshop state presentation.
