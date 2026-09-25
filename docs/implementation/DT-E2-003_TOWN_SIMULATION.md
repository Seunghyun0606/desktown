# DT-E2-003 — Town logical simulation

`TownSimulation` owns the Restore Workshop project and Mina logical state. An
application caller supplies absolute logical time, cumulative accepted Focus
Energy, current session status, and consecutive idle age. It may call `AdvanceTo`
every second or jump forward after a paused application. Project progression
uses the existing cumulative high-watermark, so replaying the same observation
does not add reward or emit a second completion. There is no Godot frame loop,
window, display mode, platform clock, or renderer in this domain component.

`Snapshot` returns immutable Town, Companion, and Ghost projections of the
same Mina state. Hidden reads no visual projection while the simulation still
advances. The `TownSimulationStep` additionally carries one-time presentation
intent; a renderer must not restart animation from an unchanged steady state.
Completion itself does not show the reward. The future EventSystem delivers a
pending reveal when Town opens, then calls `RevealWorkshopCompletion` and
`AcknowledgeCelebration` after playback.

`CaptureCheckpoint` retains logical time, project progress/high-watermark, and
Mina's one-shot latches. `Restore` reconstructs those aggregates without
replaying elapsed frames. The caller must persist this checkpoint consistently
with the accepted Focus ledger; E8-003 owns checkpoint scheduling and startup
recovery. Scene binders will be added with the actual Town/Companion/Ghost
scenes, so this task exposes projections without creating placeholder visuals.

Tests compare 3,600 one-second observations with a restored long jump, check
display parity, reject decreasing logical time, and verify completion/reveal
idempotency.
