# DT-E2-004 — Workshop and Railway event contract

`EventSystem` follows the logical `ProjectSystem` result. When Restore Workshop
reaches 25 Focus minutes, it records a pending `workshop_complete` presentation.
Opening Town may start Mina's final work/celebration, but the logical Workshop
state is already Complete. Acknowledging or skipping that presentation consumes
its ID and, in the same logical transition, queues `old_railway_map` discovery
and unlocks the Explore Old Railway **teaser**. Its target is 45 Focus minutes;
the next project's gameplay remains unavailable in v0.1. Acknowledging the
discovery consumes its event ID. Rumi, map art, and timing belong to E7-002/003.

Project progress, presentation markers, discovery markers, and unlock IDs are
separate. Repeated completion/acknowledgement commands are no-ops once recorded.
On restoration, a Complete Workshop with neither pending nor consumed reveal
is reconciled to a pending reveal. This closes the gap if a process stopped
after the project commit and before the event marker was saved. Invalid states
(discovery before reveal, conflicting pending/consumed IDs, unlock before
completion) are rejected.

`TownSimulation` includes event state in its immutable projection/checkpoint
and forwards completion to `EventSystem`. `EventSaveMapper` maps existing V1
`town.unlockedProjectIds`, `town.pendingEventIds`, `town.consumedEventIds`, and
`pendingPresentation` ID sets. It validates known IDs and reconstructs the
logical state. The current playable project remains Restore Workshop; Railway
is an unlocked but unavailable next-build goal. Saving complete progress and
its pending presentation in one JSON envelope is the application caller's
responsibility, with E8-003 scheduling the write.

Tests cover ordering, one-shot delivery, restart reconciliation, repeated
commands, invalid combinations, and three save/restart boundaries through
Workshop reveal and Railway discovery. A crash during animation may replay
the presentation; its consumed marker is written only after acknowledgement.
