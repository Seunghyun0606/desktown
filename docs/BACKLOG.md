# DeskTown Prototype v0.1 Backlog

Status at creation: all tasks `TODO`. Total: **34 tasks**.

Current Foundation status:

| Task | Status | Note |
| --- | --- | --- |
| DT-E0-001 | IN REVIEW | Build/import/export pass; exported `.exe` launch pending on Windows |
| DT-E0-002 | DONE | Boundaries, composition root, architecture tests, and solution build pass |
| DT-E0-003 | IN REVIEW | CI and Windows artifact pass; manual artifact launch pending |
| DT-E0-004 | DONE | Typed defaults, visible safe fallback, privacy policy, and tests pass |

Current Focus Core status:

| Task | Status | Note |
| --- | --- | --- |
| DT-E1-001 | DONE | Display-independent aggregate and 21 domain tests pass |
| DT-E1-002 | DONE | Monotonic/wall clocks, single-active rule, lifecycle events, and 15 tests pass |
| DT-E1-003 | IN REVIEW | 17 adapter/privacy tests and export pass; real Windows app/idle manual test pending |
| DT-E1-004 | DONE | Tick-precise cumulative/delta Energy contract and 28 domain tests pass |
| DT-E1-005 | IN REVIEW | Coordinator/catalog tests and export pass; Windows process picker and real-session QA await UI wiring |

Current Town Simulation status:

| Task | Status | Note |
| --- | --- | --- |
| DT-E2-001 | DONE | Restore Workshop 25-Focus progression, replay guard, completion-once tests pass |
| DT-E2-002 | DONE | Logical state, presentation intent, one-shot cues, and restore tests pass |
| DT-E2-003 | DONE | Headless absolute-time advancement, immutable mode projections, 60-minute parity test pass |
| DT-E2-004 | TODO | Next Town task: pending Workshop reveal → Railway discovery |

Current Persistence status:

| Task | Status | Note |
| --- | --- | --- |
| DT-E8-001 | DONE | V1 JSON schema/repository, hash, exact restore, privacy tests pass |
| DT-E8-002 | IN REVIEW | Atomic replace, validated backup recovery, migration and concurrency tests pass; Windows file/recovery UI check pending |
| DT-E8-003 | TODO | Depends on E8-002; checkpoint scheduling and restart recovery |

Conventions: `Automated` lists the minimum test; `Manual` is `Yes` only where a
human/exported environment adds information. Expected files are forecasts and
may be adjusted to the final scaffold without changing task responsibility.

## E0 — Project Foundation (4)

### DT-E0-001 — Pin toolchain and create Godot C# solution

- **Purpose:** establish a reproducible Windows-first Godot/.NET project.
- **Expected files:** `project.godot`, `DeskTown.sln`, `src/*/*.csproj`,
  `global.json`, `.gitignore`.
- **Dependencies:** none.
- **Implementation:** pin current stable Godot 4.x .NET and compatible .NET SDK;
  create empty AppRoot launch.
- **Acceptance:** editor and CLI build open/compile; empty exported Windows app
  starts and exits cleanly.
- **Automated:** `dotnet build`; Godot headless import smoke.
- **Manual:** Yes — launch exported `.exe`.
- **Asset dependency:** none.

### DT-E0-002 — Create boundaries, ports, and composition root

- **Purpose:** prevent Godot/Win32 leakage into domain logic.
- **Expected files:** `src/DeskTown.Domain/**`, `DeskTown.Application/**`,
  `DeskTown.Godot/Bootstrap/**`, `DeskTown.Platform.Windows/**`.
- **Dependencies:** DT-E0-001.
- **Implementation:** create projects/references, core ports, AppRoot composition;
  no service locator.
- **Acceptance:** forbidden dependency direction cannot compile; app resolves
  fake services and shows MainWindow shell.
- **Automated:** architecture/reference test; solution build.
- **Manual:** No.
- **Asset dependency:** none.

### DT-E0-003 — Add CI, Windows export, and test skeleton

- **Purpose:** make every later task verifiable.
- **Expected files:** `.github/workflows/ci.yml`, `export_presets.cfg`, `tests/**`,
  test run scripts.
- **Dependencies:** DT-E0-001.
- **Implementation:** build, unit test, Godot import smoke, Windows export artifact;
  choose xUnit consistently.
- **Acceptance:** clean checkout produces test results and Windows artifact.
- **Automated:** CI self-run.
- **Manual:** Yes — download/open one artifact.
- **Asset dependency:** none.

### DT-E0-004 — Add config, structured logging, and privacy defaults

- **Purpose:** centralize thresholds and diagnose without collecting content.
- **Expected files:** `src/DeskTown.Application/Configuration/**`,
  `src/DeskTown.Godot/Bootstrap/LoggingHost.cs`, config resources.
- **Dependencies:** DT-E0-002.
- **Implementation:** typed config for 60/180 idle thresholds, tick/checkpoint,
  FPS; logs exclude title/URL/input/document data.
- **Acceptance:** invalid config falls back visibly; privacy test rejects banned
  fields in serialized logs.
- **Automated:** config and log-redaction tests.
- **Manual:** No.
- **Asset dependency:** none.

## E1 — Focus Core (5)

### DT-E1-001 — Create FocusSession domain model

- **Purpose:** represent one display-independent session.
- **Expected files:** `DeskTown.Domain/Focus/FocusSession.cs`, enums/value objects,
  `DeskTown.Domain.Tests/FocusSessionTests.cs`.
- **Dependencies:** DT-E0-002.
- **Implementation:** ID, UTC start/end, target, status, counted/idle duration,
  intended processes; no DisplayMode.
- **Acceptance:** start/stop/complete/suspend transitions are valid; second start
  and invalid transitions reject; elapsed is calculable.
- **Automated:** exhaustive transition unit tests.
- **Manual:** No.
- **Asset dependency:** none.

### DT-E1-002 — Implement FocusSessionManager with injectable clocks

- **Purpose:** accumulate reliable focus time across ticks and suspension.
- **Expected files:** `DeskTown.Application/Sessions/FocusSessionManager.cs`,
  clock ports/fakes, tests.
- **Dependencies:** DT-E1-001.
- **Implementation:** monotonic elapsed, wall audit time, one active session,
  typed lifecycle events.
- **Acceptance:** 25 fake minutes completes exactly; suspended time excluded;
  mode is absent from calculations.
- **Automated:** fake-clock boundary and duplicate-session tests.
- **Manual:** No.
- **Asset dependency:** none.

### DT-E1-003 — Implement Windows foreground and idle tracker

- **Purpose:** record minimal process/active/idle aggregates.
- **Expected files:** `Platform.Windows/Activity/NativeMethods.cs`,
  `WindowsActivityTracker.cs`, tracker contract tests.
- **Dependencies:** DT-E0-004, DT-E1-001.
- **Implementation:** use `GetForegroundWindow`, `GetWindowThreadProcessId`,
  `GetLastInputInfo`; sample 1 Hz only during Focus.
- **Acceptance:** ProcessName/active/idle aggregate; access failure becomes
  Unknown; no title/URL/content APIs or persisted HWND/PID.
- **Automated:** adapter tests around native seam; privacy serialization test.
- **Manual:** Yes — switch among real apps and idle.
- **Asset dependency:** none.

### DT-E1-004 — Implement session ledger and Energy policy

- **Purpose:** convert counted time into project-safe Focus Energy.
- **Expected files:** `Domain/Focus/SessionLedger.cs`, `IEnergyPolicy.cs`,
  `ElapsedTimeEnergyPolicy.cs`, tests.
- **Dependencies:** DT-E1-002.
- **Implementation:** retain tick-precise elapsed time internally; show
  whole-minute Focus; idle and process choice do not penalize; expose cumulative
  total and unapplied delta explicitly.
- **Acceptance:** equal elapsed sessions produce equal Energy in all modes and
  activity patterns; no rounding drift.
- **Automated:** property/boundary tests.
- **Manual:** No.
- **Asset dependency:** none.

### DT-E1-005 — Add process catalog and Focus orchestration

- **Purpose:** support optional app intent and one complete session command flow.
- **Expected files:** `Application/Ports/IProcessCatalog.cs`,
  `Sessions/FocusSessionCoordinator.cs`, Windows process catalog, tests.
- **Dependencies:** DT-E1-003, DT-E1-004.
- **Implementation:** enumerate process names safely; default Any App; coordinate
  tracker, manager, events, and checkpoints.
- **Acceptance:** session starts when catalog is empty/unavailable; intended apps
  persist as metadata but do not gate Energy.
- **Automated:** coordinator integration tests.
- **Manual:** Yes — process picker sanity check.
- **Asset dependency:** none.

## E2 — Town Simulation (4)

### DT-E2-001 — Implement ProjectSystem and Workshop progression

- **Purpose:** turn Energy into the first visible world state.
- **Expected files:** `Domain/Projects/**`, project definitions/resources, tests.
- **Dependencies:** DT-E1-004.
- **Implementation:** Restore Workshop target 25 Focus; Broken/Repairing/Complete;
  clamp and idempotent completion.
- **Acceptance:** deterministic thresholds; completion emitted once; display mode
  unavailable to this system.
- **Automated:** progression/idempotency tests.
- **Manual:** No.
- **Asset dependency:** none.

### DT-E2-002 — Implement Mina logical state machine

- **Purpose:** make Mina reflect work/rest without evaluating the user.
- **Expected files:** `Domain/Simulation/MinaStateMachine.cs`, states/config, tests.
- **Dependencies:** DT-E1-002, DT-E1-003.
- **Implementation:** active, 60s short idle, 180s long idle, suspend, return,
  completion; logical vs presentation intent separated.
- **Acceptance:** boundary transitions pass; state continues in Hidden; no reward
  mutation.
- **Automated:** transition table tests.
- **Manual:** No.
- **Asset dependency:** none.

### DT-E2-003 — Implement TownSimulation logical tick and snapshots

- **Purpose:** run world state independently of rendering.
- **Expected files:** `Domain/Simulation/TownSimulation.cs`, snapshot records,
  `Godot/Presentation/*SnapshotBinder.cs`.
- **Dependencies:** DT-E2-001, DT-E2-002.
- **Implementation:** 1 Hz/event delta; immutable Town/Companion/Ghost snapshots;
  no frame/physics dependency.
- **Acceptance:** a headless 60-minute fake run equals visible-mode result;
  resume applies delta without frame replay.
- **Automated:** headless parity/soak test.
- **Manual:** No.
- **Asset dependency:** none.

### DT-E2-004 — Implement EventSystem and pending presentations

- **Purpose:** separate logical unlocks from one-shot visual reveals.
- **Expected files:** `Domain/Events/EventSystem.cs`, event IDs/records, tests.
- **Dependencies:** DT-E2-001.
- **Implementation:** Workshop completion → pending reveal → Railway discovery →
  next project unlock; consumed IDs idempotent.
- **Acceptance:** crash/replay cannot duplicate progress or lose an event.
- **Automated:** event ordering/idempotency tests.
- **Manual:** No.
- **Asset dependency:** none.

## E3 — Companion Mode (3)

### DT-E3-001 — Create Companion native window

- **Purpose:** host a quiet 360×200 always-on-top coworker view.
- **Expected files:** `scenes/companion/CompanionWindow.tscn`,
  `Godot/Display/CompanionWindowHost.cs`.
- **Dependencies:** DT-E0-002, DT-E2-003.
- **Implementation:** native borderless Window, 30 FPS cap, close→Hidden, declared
  drag region.
- **Acceptance:** window does not start/stop session; other apps remain usable;
  close preserves progress.
- **Automated:** surface lifecycle integration test.
- **Manual:** Yes — exported topmost/drag/focus test.
- **Asset dependency:** placeholder Mina/props.

### DT-E3-002 — Build CompanionStage and Mina animator

- **Purpose:** present Work/Rest/Stretch/Walk in the diorama.
- **Expected files:** `scenes/companion/CompanionStage.tscn`, shared Mina view,
  placeholder catalog.
- **Dependencies:** DT-E3-001, DT-E2-002.
- **Implementation:** snapshot binding; animation events only for sound/effects;
  quiet labels.
- **Acceptance:** current state binds after show; no progress bar/judgment copy;
  missing asset cannot block simulation.
- **Automated:** snapshot/clip mapping tests.
- **Manual:** Yes — hierarchy and animation timing.
- **Asset dependency:** contract-correct Mina placeholder, Companion placeholders.

### DT-E3-003 — Persist Companion scale and monitor position

- **Purpose:** restore a usable window across DPI/monitor changes.
- **Expected files:** `Platform.Windows/WindowsWindowPlacementService.cs`,
  Companion settings/view tests.
- **Dependencies:** DT-E3-001, DT-E8-001.
- **Implementation:** monitor key, anchor/offset, 75/100/125/150%, usable-area
  clamp.
- **Acceptance:** restores on same monitor; missing monitor falls back on-screen;
  sprites remain integer-scaled.
- **Automated:** placement math tests.
- **Manual:** Yes — mixed-DPI dual monitor.
- **Asset dependency:** none.

## E4 — Hidden Mode (2)

### DT-E4-001 — Implement Hidden surface lifecycle

- **Purpose:** provide a zero-visual normal play mode with minimal resources.
- **Expected files:** `Godot/Display/HiddenDisplaySurface.cs`, lifecycle tests.
- **Dependencies:** DT-E2-003, DT-E3-001.
- **Implementation:** hide/unload visible focus stages, pause Town rendering, keep
  1 Hz services.
- **Acceptance:** no DeskTown window visible; identical session/Energy/progress;
  no active particles/audio/render surface.
- **Automated:** mode-parity and node lifecycle tests.
- **Manual:** Yes — Task Manager/performance sanity.
- **Asset dependency:** none.

### DT-E4-002 — Implement Tray menu and focus status

- **Purpose:** control a hidden or noninteractive app.
- **Expected files:** `scenes/app/TrayMenu.tres`, `Platform.Windows/Tray/**`, command
  bindings.
- **Dependencies:** DT-E4-001, DT-E1-005.
- **Implementation:** Godot StatusIndicator; open, modes, positions, opacity,
  audio, end, quit; confirmation flows.
- **Acceptance:** tooltip states correct; commands route through coordinators;
  quit never silently loses active Focus.
- **Automated:** command binding tests.
- **Manual:** Yes — Windows tray behavior.
- **Asset dependency:** temporary/final tray icon.

## E5 — Ghost Mode (4)

### DT-E5-001 — Build Godot Ghost overlay spike

- **Purpose:** prove transparent, topmost, no-focus, mouse-passthrough basics
  before full UI work.
- **Expected files:** `scenes/ghost/GhostWindow.tscn`, `GhostStage.tscn`,
  `Godot/Display/GhostWindowHost.cs`, spike notes.
- **Dependencies:** DT-E0-003, DT-E2-003.
- **Implementation:** native 240×180 Window; transparent background; no Control or
  input node; Mina placeholder only.
- **Acceptance:** exported overlay renders alpha/topmost and does not take keyboard
  focus on baseline Windows 11.
- **Automated:** scene contract checks (no focusable/input nodes).
- **Manual:** Yes — exported `.exe`.
- **Asset dependency:** placeholder Mina, prop, shadow.

### DT-E5-002 — Implement isolated Windows Ghost adapter

- **Purpose:** reinforce and verify native behavior without coupling Godot/domain
  code to Win32.
- **Expected files:** `Application/Ports/IGhostWindowPlatform.cs`,
  `Platform.Windows/Windows/WindowsGhostWindowPlatform.cs`, native methods/tests.
- **Dependencies:** DT-E5-001.
- **Implementation:** obtain validated native handle; preserve/apply/remove
  layered, transparent, no-activate, tool-window, topmost styles.
- **Acceptance:** apply/remove idempotent; original style bits preserved; invalid
  handle fails safely.
- **Automated:** style-calculation and adapter seam tests.
- **Manual:** Yes — inspect focus/taskbar/Alt+Tab.
- **Asset dependency:** none.

### DT-E5-003 — Verify click-through matrix and Hidden fallback

- **Purpose:** enforce interaction-zero as a release gate.
- **Expected files:** Windows QA checklist/results, Ghost capability probe,
  display-controller fallback tests.
- **Dependencies:** DT-E5-002, DT-E4-001.
- **Implementation:** test click/double/right-click/drag/scroll/text selection in
  Chrome, VS Code, Excel, Notion; fail closed to Hidden.
- **Acceptance:** full baseline matrix passes or Ghost is automatically unavailable;
  session/progress never stop.
- **Automated:** forced-failure fallback integration test.
- **Manual:** Yes — release blocking.
- **Asset dependency:** none.

### DT-E5-004 — Add Ghost opacity, corner, DPI, and monitor persistence

- **Purpose:** make a noninteractive overlay configurable from Tray.
- **Expected files:** Ghost settings, placement service additions, Tray bindings,
  tests.
- **Dependencies:** DT-E5-003, DT-E8-001, DT-E4-002.
- **Implementation:** 40/65/85%; four corners; monitor usable area; mixed-DPI
  clamping; no Ghost-side UI.
- **Acceptance:** changes apply live from Tray and restore after restart; overlay
  stays fully on-screen.
- **Automated:** opacity/placement/state tests.
- **Manual:** Yes — 100/125/150% and dual monitor.
- **Asset dependency:** none.

## E6 — Main Town (3)

### DT-E6-001 — Build Main Town scene and world-first HUD

- **Purpose:** create the place where progress becomes emotionally visible.
- **Expected files:** `scenes/town/TownScene.tscn`, TileMap layers, Town snapshot
  binder, placeholder catalog entries.
- **Dependencies:** DT-E2-003, DT-E8-001.
- **Implementation:** House, Workshop, Locked Area, path, campfire, minimal project
  and daily focus text.
- **Acceptance:** 1280×720 minimum; Workshop snapshot shows correct state; Town
  dominates HUD.
- **Automated:** scene/snapshot smoke test.
- **Manual:** Yes — layout and DPI/readability.
- **Asset dependency:** placeholder buildings/environment.

### DT-E6-002 — Add Mina, Noah, Rumi ambient presentation

- **Purpose:** make Town feel alive without AI NPCs.
- **Expected files:** shared character views, `NpcAmbientPresenter.cs`, Town scene
  instances.
- **Dependencies:** DT-E6-001, DT-E2-002.
- **Implementation:** deterministic low-frequency paths/actions; Mina full logical
  binding; Noah/Rumi ambient clips only.
- **Acceptance:** all three display; behavior is state-based/deterministic; no
  gameplay mutation or behavior tree.
- **Automated:** seeded ambient schedule test.
- **Manual:** Yes — collision/readability/visual noise.
- **Asset dependency:** placeholder character sheets.

### DT-E6-003 — Implement First Launch, Main shell, and Focus Setup UX

- **Purpose:** expose trust, project, duration, apps, and equal modes to users.
- **Expected files:** `scenes/app/FirstLaunch.tscn`, `MainShell.tscn`,
  `scenes/focus/FocusSetup.tscn`, controllers.
- **Dependencies:** DT-E1-005, DT-E6-001, DT-E4-002.
- **Implementation:** exact privacy lists; 25/45/60; Any App; equal mode cards;
  prevent second session.
- **Acceptance:** tracking begins only after Start; setup remains usable without
  process catalog; copy states equal progress.
- **Automated:** view-model/command tests.
- **Manual:** Yes — end-to-end UX review.
- **Asset dependency:** logo and icons may be placeholders.

## E7 — Reward Reveal (3)

### DT-E7-001 — Complete session passively and notify

- **Purpose:** finish work without forcing DeskTown over the user's app.
- **Expected files:** `Application/Sessions/CompletionCoordinator.cs`,
  `Platform.Windows/Notifications/**`, Tray status integration.
- **Dependencies:** DT-E1-005, DT-E2-004, DT-E4-002.
- **Implementation:** commit completion/pending reveal, then passive notification;
  fallback to Tray status.
- **Acceptance:** Main Town stays hidden; notification failure retains reward;
  Open action focuses existing instance.
- **Automated:** notification success/failure tests.
- **Manual:** Yes — packaged/unpackaged exported build behavior.
- **Asset dependency:** notification icon placeholder allowed.

### DT-E7-002 — Implement crash-safe Workshop reward reveal

- **Purpose:** present the first meaningful world transformation.
- **Expected files:** `scenes/town/RewardReveal.tscn`,
  `Presentation/RewardRevealCoordinator.cs`, timeline resources/tests.
- **Dependencies:** DT-E6-001, DT-E7-001, DT-E8-002.
- **Implementation:** guide camera, final Work, hold, state swap/effect, Celebrate;
  skip after two seconds; reduced-motion path.
- **Acceptance:** final state committed before animation; skip/missing asset/crash
  all end Complete; reveal consumed exactly once.
- **Automated:** timeline command and restart tests.
- **Manual:** Yes — pacing/sound/visual review.
- **Asset dependency:** Workshop states, Mina work/celebrate, spark/smoke.

### DT-E7-003 — Implement Railway discovery and teaser unlock

- **Purpose:** close the loop with a reason for the next Focus.
- **Expected files:** `scenes/town/DiscoveryEvent.tscn`, project definition for
  Railway, Old Map catalog binding.
- **Dependencies:** DT-E7-002, DT-E2-004.
- **Implementation:** While You Were Away, map, Rumi step-in, `0/45`, Coming in
  next build; save consumed/unlocked state.
- **Acceptance:** shown once; Rumi remains existing NPC; next project visible but
  not falsely playable; restart restores it.
- **Automated:** unlock/consumption persistence test.
- **Manual:** Yes — narrative sequence.
- **Asset dependency:** Old Railway Map, Rumi placeholder/production.

## E8 — Persistence (3)

### DT-E8-001 — Define save schema and JSON repository

- **Purpose:** persist settings, Focus, Town, Mina, player, pending presentation.
- **Expected files:** `Application/Persistence/SaveEnvelopeV1.cs`, DTOs/mappers,
  `Persistence/JsonGameStateStore.cs`, fixtures/tests.
- **Dependencies:** DT-E0-002, DT-E1-001, DT-E2-001.
- **Implementation:** version/revision/hash; `user://` path adapter; domain/DTO
  separation; unknown-field tolerance.
- **Acceptance:** round-trip exact; future unknown fields ignored; banned privacy
  fields absent.
- **Automated:** round-trip/golden/privacy tests.
- **Manual:** No.
- **Asset dependency:** none.

### DT-E8-002 — Add atomic write, backup recovery, and migrations

- **Purpose:** avoid corrupt progress and support schema evolution.
- **Expected files:** atomic file writer, `ISaveMigration`, V1 fixtures, recovery
  tests.
- **Dependencies:** DT-E8-001.
- **Implementation:** temp+flush+validate+replace; serialized writes; backup;
  sequential pure migration.
- **Acceptance:** truncation/hash failure recovers backup; stale revision cannot
  overwrite; unsupported future version is preserved/refused.
- **Automated:** fault-injection and migration tests.
- **Manual:** Yes — inspect recovery banner/files once.
- **Asset dependency:** none.

### DT-E8-003 — Implement checkpoints, restart recovery, and single instance

- **Purpose:** preserve session across crash/suspend and prevent duplicates.
- **Expected files:** `ApplicationLifecycleCoordinator.cs`, checkpoint scheduler,
  Windows single-instance adapter, recovery UI.
- **Dependencies:** DT-E1-002, DT-E8-002.
- **Implementation:** 15s/event checkpoints; suspend/lock; Resume/End at startup;
  named mutex + open-existing request.
- **Acceptance:** recovery loses at most checkpoint interval; suspended wall time
  excluded; second launch creates no session/process state conflict.
- **Automated:** fake lifecycle/recovery tests.
- **Manual:** Yes — sleep, lock, crash, duplicate launch.
- **Asset dependency:** none.

## E9 — Funding Polish (3)

### DT-E9-001 — Integrate production Asset Catalog

- **Purpose:** replace placeholders without changing gameplay code.
- **Expected files:** `assets/catalogs/production/**`, imported art/audio, catalog
  validation tool/report.
- **Dependencies:** M4 feature complete; approved Art Gates.
- **Implementation:** stable IDs, import settings, pivots/events, provenance;
  custom Mina/Workshop/core props first.
- **Acceptance:** all required manifest IDs resolve; no runtime vendor filenames;
  Mina 30 frames and Workshop states pass contract.
- **Automated:** catalog completeness/dimension/frame validation.
- **Manual:** Yes — art gates and alpha/pivot review.
- **Asset dependency:** production assets; blocking.

### DT-E9-002 — Add final sound, particles, light, and reduced motion

- **Purpose:** create a cozy, capture-ready experience without distraction.
- **Expected files:** audio buses/resources, particle scenes, lighting profiles,
  reduced-motion bindings.
- **Dependencies:** DT-E9-001, DT-E7-003.
- **Implementation:** eight sound entries, capped effects, warm light; audio toggle;
  optional motion disabled cleanly.
- **Acceptance:** no clipped/looping defects; Hidden stops presentation audio;
  reduced motion preserves state meaning.
- **Automated:** resource/catalog checks.
- **Manual:** Yes — mix, distraction, performance.
- **Asset dependency:** final audio/effects.

### DT-E9-003 — Run release matrix and prepare funding capture build

- **Purpose:** certify feature completeness and reproduce trailer scenes A/B/C.
- **Expected files:** `docs/qa/releases/<build>/`, demo save/launch config, release
  notes/checklists.
- **Dependencies:** DT-E9-002 and every P0/P1 task.
- **Implementation:** full Windows/DPI/monitor/app matrix, performance/privacy
  audit, deterministic capture states, bug fix loop.
- **Acceptance:** zero P0/P1; Ghost gate signed; save/recovery and 60-minute soak
  pass; Work Together/Your Way/World Change captures reproducible.
- **Automated:** full CI and smoke suite.
- **Manual:** Yes — final release gate.
- **Asset dependency:** complete funding set.

## Summary

| Epic | Tasks | Primary milestone |
| --- | ---: | --- |
| E0 Foundation | 4 | M0 |
| E1 Focus Core | 5 | M0–M1 |
| E2 Simulation | 4 | M0–M1 |
| E3 Companion | 3 | M1–M2 |
| E4 Hidden | 2 | M2 |
| E5 Ghost | 4 | M2 |
| E6 Main Town | 3 | M3 |
| E7 Reward | 3 | M4 |
| E8 Persistence | 3 | M0–M4 |
| E9 Funding Polish | 3 | M5 |
| **Total** | **34** | |
