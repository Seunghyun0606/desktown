# DeskTown Prototype v0.1 Backlog

2026-10-13 최신: 사용자 음향·믹스·납품 승인에 따라 시각 38개와 음향 8개 모두 production 적용을 완료했다. 기존 Audio 설정, 보이는 보행/타격, 마을 주변음, 버튼, 완성·발견에 연결했다. 실제 Windows 창 상태 26개·음향 상태 29개를 통과했고 새 [음향 적용 MVP](qa/QA_EXECUTION_2026-10-13_AUDIO_MVP.md)를 제공한다. 아래 음향 미배정/승인 대기는 당시 기록이다. 후속 프로젝트 콘텐츠·실제 장치 청취·전체 출시 검증은 남아 있다.

최신 후속 작업: [음향 8종과 장면 믹스 청취](art/reviews/2026-10-12/audio-v1/review.html)를 준비했다. 기술 검사는 통과했으며 소리·믹스·납품 승인을 기다린다. 승인 후 기존 Audio 설정·화면 숨김·행동 타이밍에 연결하고 새 플레이 빌드를 만든다. 현재 production 카탈로그는 시각 38개·음향 미배정 8개이며 플레이 빌드는 무음이다. [음향 검증 기록](qa/QA_EXECUTION_2026-10-12_AUDIO.md).

최신 사용자 확인: 이전 디자인 MVP를 모두 확인한 뒤 캐릭터 행동의 부자연스러움·패턴 부족을 후속 과제로 지정했다. [행동 개선 기록](qa/QA_EXECUTION_2026-10-12_BEHAVIOR.md)과 [실제 움직임 미리보기](art/reviews/2026-10-12/behavior-v1/review.html)를 제공한다. NPC 연속 보행·개별 동선, Mina 이동·작업 리듬을 연결했고 Focus/보상/저장 규칙은 유지한다. 아래 이전 승인·QA 대기 표기는 해당 빌드 당시 기록이다.

Total: **34 tasks**. The tables and task-level notes below track implementation
separately from exported Windows and production-art review.

## 사용자 QA 후속 TODO — 2026-10-12

- **QA-PIP-001 / P1 / 수정 완료·사용자 재확인 대기:** PiP 중 Companion과 Town 창이 함께 표시되는 문제를 아트 완성 후 실제 Windows 내보내기에서 재현했다. 루트 창 최소화·Unfocusable 처리로 수정했고 26개 native 창 상태 검사와 세션 유지를 확인했다. 사용자 Desktop/작업 표시줄/입력/DPI 재확인은 남아 있다. 최초 사용자 보고 빌드는 `4ba5a2ee`로 추정이며 수정 전 재현 코드는 `327ae13`이다. [MVP 기록](qa/QA_EXECUTION_2026-10-12_MVP.md)을 따른다.
- 사용자는 QA를 모두 확인했다고 보고했다. 전체 DPI·앱 입력 매트릭스의 개별 결과는 제공되지 않았으므로 Ghost 출시 합격으로 확대하지 않는다.
- 현재 우선 목표: 승인된 32색 기준으로 남은 시각 아트를 완성하고 실제 Godot 화면에서 최종 외형을 검토한다. 음향 8항목의 제작·믹스는 별도 상태로 유지한다.
- [Town 실제 배정 검증](art/reviews/2026-10-12/town-delivery-v1/README.md): 사용자 납품·2× 배치 승인에 따라 11개를 적용했다. 실제 카탈로그 23 production, 12개 캡처가 승인본과 바이트 단위 일치. 미배정 바닥·길·소품 대체 표시 검사도 통과했다. Focus·저장·창 정책은 유지한다.
- [NPC 실제 배정 검증](art/reviews/2026-10-12/npc-delivery-v1/README.md): 사용자 승인에 따라 Noah Idle/Walk/Read, Rumi Idle/Walk 5클립을 배정했다. 실제 카탈로그로 GPU 캡처 120장이 승인본과 바이트 단위 일치했고 픽셀 221,184개·NPC 밖 Town 보존을 확인했다. 현재 28 production·시각 10개/음향 8개 미배정. 기존 NPC 이동 스케줄은 유지한다.
- [남은 시각 아트 10개 검토](art/reviews/2026-10-12/remaining-art-v1/review.html): 효과 5개·장식·도구·로고·아이콘·지도 5개를 33셀로 준비했다. 실제 Godot 캡처 204개·픽셀 49,408개와 추가 영역 밖 기존 아트 보존을 확인했다. 외형·납품·배치·연결 승인 후 시각 38개 실제 적용과 전체 최종 화면·새 QA 빌드로 진행한다. 창 정책은 변경하지 않는다.

Current Foundation status:

2026-10-12 최신: 마지막 시각 10개의 외형·납품·배치·연결을 사용자가 승인했고 **38개 시각 아트 모두 실제 적용**했다. [전체 최종 화면](art/reviews/2026-10-12/final-views-v1/review.html) 55장과 [실제 배정/모션 검사](art/reviews/2026-10-12/remaining-delivery-v1/README.md) 204장을 제공한다. 이전 승인 대기·28개 집계는 당시 기록이다. Reduced motion은 기존 저장 필드를 트레이에 연결했다. 음향 8개·실제 Windows QA·QA-PIP-001은 남아 있다.

| Task | Status | Note |
| --- | --- | --- |
| DT-E0-001 | IN REVIEW | Build/import/export and CI Windows `.exe` headless launch; interactive Windows 11 launch pending |
| DT-E0-002 | DONE | Boundaries, composition root, architecture tests, and solution build pass |
| DT-E0-003 | IN REVIEW | CI exports and launches the Windows artifact, then checks isolated save/restart/recovery; manual interactive launch pending |
| DT-E0-004 | DONE | Typed defaults, visible safe fallback, privacy policy, and tests pass |

Current Focus Core status:

| Task | Status | Note |
| --- | --- | --- |
| DT-E1-001 | DONE | Display-independent aggregate and 21 domain tests pass |
| DT-E1-002 | DONE | Monotonic/wall clocks, single-active rule, lifecycle events, and 15 tests pass |
| DT-E1-003 | IN REVIEW | 17 adapter/privacy tests and export pass; real Windows app/idle manual test pending |
| DT-E1-004 | DONE | Tick-precise cumulative/delta Energy contract and 28 domain tests pass |
| DT-E1-005 | IN REVIEW | Process picker and real-session UI are wired; exported Windows QA pending |

Current Town Simulation status:

| Task | Status | Note |
| --- | --- | --- |
| DT-E2-001 | DONE | Restore Workshop 25-Focus progression, replay guard, completion-once tests pass |
| DT-E2-002 | DONE | Logical state, presentation intent, one-shot cues, and restore tests pass |
| DT-E2-003 | DONE | Headless absolute-time advancement, immutable mode projections, 60-minute parity test pass |
| DT-E2-004 | DONE | Pending Workshop reveal, Railway discovery/teaser unlock, restart reconciliation tests pass |

Current Persistence status:

| Task | Status | Note |
| --- | --- | --- |
| DT-E8-001 | DONE | V1 JSON schema/repository, hash, exact restore, privacy tests pass |
| DT-E8-002 | IN REVIEW | Atomic replace, validated backup recovery, migration and concurrency tests pass; Windows file/recovery UI check pending |
| DT-E8-003 | IN REVIEW | 15s scheduler, recovery prompt, single-instance forwarding and exported save/restart smoke; Windows sleep/lock/crash gate pending |

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

- **Status:** IN REVIEW — native exported Windows topmost/drag/focus QA pending;
  see `docs/implementation/DT-E3-001_COMPANION_WINDOW.md`.
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

- **Status:** IN REVIEW — state/clip binding and geometric Work/Rest/Stretch/Walk
  placeholder motion are automated; visual QA and custom Mina art pending.
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

- **Status:** IN REVIEW — four presets, capture, clamp, settings save binding,
  and hashed Windows monitor identity exist; mixed-DPI restore QA pending.
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

- **Status:** IN REVIEW — presentation lifecycle/parity and runtime Focus/Tray
  wiring exist; Town ambient timer and Mina placeholder processing stop while hidden. Windows zero-window/
  performance QA pending.
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

- **Status:** IN REVIEW — native menu, Open/End/Quit and settings commands are
  wired; exported Windows tray and quit-confirmation QA pending.
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

- **Status:** IN REVIEW — hidden transparent QA scene and static interaction
  contract exist; exported Windows transparency/focus/passthrough gate pending.
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

- **Status:** IN REVIEW — isolated HWND style adapter, fake-native tests and F10
  QA preview binding exist; exported focus/taskbar/Alt+Tab validation pending.
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

- **Status:** IN REVIEW — a forced partial-show failure now hides Ghost,
  switches the presentation to Hidden, and keeps Focus active in automated
  tests. The exported-build procedure and result sheet are in
  `docs/qa/WINDOWS_GATE.md`. F10 is a preview only; normal Ghost Focus remains
  disabled until the matrix and end-to-end fallback pass.
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

- **Status:** IN REVIEW — saved opacity/corner/monitor settings now apply live to the F10
  QA preview; pure placement tests cover negative coordinates and missing-monitor
  fallback. Exported mixed-DPI QA and normal Ghost-mode enablement await DT-E5-003.
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

- **Status:** IN REVIEW — saved-world placeholder scene exists; layout/DPI
  review and authored art pending.
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

- **Status:** IN REVIEW — Town now draws three geometric character stand-ins;
  Mina follows the saved logical location and Noah/Rumi follow a deterministic
  visual schedule. Authored sprites and visual QA remain pending.
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

- **Status:** IN REVIEW — privacy screen, process/duration/mode Setup and
  real-session runtime binding exist; exported end-to-end UX QA pending.
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

- **Status:** IN REVIEW — Town stays hidden, short non-focusing passive window
  has an explicit Open action and Tray fallback, and completion persists first;
  exported notification click/focus QA pending.
- **Purpose:** finish work without forcing DeskTown over the user's app.
- **Expected files:** `Application/Sessions/CompletionCoordinator.cs`,
  `Platform.Windows/Notifications/**`, Tray status integration.
- **Dependencies:** DT-E1-005, DT-E2-004, DT-E4-002.
- **Implementation:** commit completion/pending reveal, then passive notification;
  fallback to Tray status.
- **Acceptance:** Main Town remains minimized or fully hidden according to the selected Focus mode; notification failure retains reward;
  Open action focuses existing instance.
- **Automated:** notification success/failure tests.
- **Manual:** Yes — packaged/unpackaged exported build behavior.
- **Asset dependency:** notification icon placeholder allowed.

### DT-E7-002 — Implement crash-safe Workshop reward reveal

- **Status:** IN REVIEW — pending reveal replays after restart and the placeholder
  two-second beat swaps the saved Workshop visual; final motion/effect/art QA pending.
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

- **Status:** IN REVIEW — saved one-shot event, Rumi copy, 0/45 teaser and disabled
  play state exist; narrative/art review pending.
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

- **Status:** IN REVIEW — all 46 manifest IDs have a validated unassigned catalog
  contract and Mina's six runtime clips have catalog/fallback binding. Production
  art, other scene binders, and visual/provenance approval remain open.
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
- **Art review progress (2026-10-11):** Mina's neutral silhouette and actual 48×48
  frame are human-approved. [Idle/Work review candidates](art/reviews/2026-10-11/idle-work-v1/review.html)
  have 4/6 distinct frames, a shared 32-color palette, binary alpha and fixed feet.
  The human approved Idle/Work motion direction and proceeding to Workshop
  Broken/Complete style testing. Companion prop/floor alignment remains open;
  all runtime catalog entries remain unassigned.
  [Workshop Broken/Complete candidates](art/reviews/2026-10-11/workshop-v1/review.html)
  use 160×128 cells, a shared 32-color palette and fixed door/foundation pixels.
  The human approved the Workshop style and proceeding to the 32px tile cluster.
  [House/path/vegetation candidates](art/reviews/2026-10-11/town-cluster-v1/review.html)
  include a 128×128 House, representative 32×32 grass/path/bush cells,
  3×3 repeats and a shared 32-color candidate palette. The human approved the
  environment style and proceeding to Reward color/effect framing.
  [Reward keyframe candidates](art/reviews/2026-10-11/reward-v1/review.html) compare
  Broken reference / one local effect beat / calm / no-particle final state.
  Single 32×32 smoke and 16×16 spark representatives are prepared; the human
  approved Reward color/effect direction and proceeding to shared palette comparison.
  [Shared palette candidates](art/reviews/2026-10-11/palette-v1/review.html) compare
  the original 95-color core union with shared 24/32-color core palettes; the
  approved separate 6-color effect ramp is retained. The human selected shared32
  as the production baseline; exact colors and editor palettes are recorded in
  [Approved palette](art/palettes/README.md).
  [Walk/Rest/Stretch/Celebrate candidates](art/reviews/2026-10-11/mina-motion-v1/review.html)
  now provide the remaining 20 distinct 48×48 frames in the approved palette,
  fixed foot baseline and loop/one-shot previews. Their motion direction was
  human-approved; final visual quality and scene contact remain open.
  [Workshop Repairing / three-state candidate](art/reviews/2026-10-11/workshop-repairing-v1/review.html)
  provides a 480×128 sheet in the approved32 palette, fixed door/foundation,
  unchanged Broken/Complete pixels and color/grayscale/Town references.
  The human approved Repairing appearance and the three-state distinction.
  [Companion core prop candidates](art/reviews/2026-10-11/companion-props-v1/review.html)
  provide Workbench/Lamp/Window/Tool (4 IDs / 6 cells), approved32 and current vs.
  proposed contact references. The human approved core prop appearance.
  [Floor/stool/shadow and full contact review](art/reviews/2026-10-11/companion-contact-v1/review.html)
  adds 3 approved32 candidates and all 6 Work / 4 Rest frames with current vs.
  fixed nearby prop placement. The human approved the new art and placement.
  [Isolated Godot review](art/reviews/2026-10-11/companion-godot-v1/review.html)
  renders 30 frames at four scales plus lamp ON (124 captures); native Mina pixels
  and all 10 contact crops match the approved inputs. Review-only shadow binding
  and integer prop origins pass. The human approved rendered layout and runtime connection.
  [Runtime connection verification](art/reviews/2026-10-11/companion-runtime-v1/README.md)
  moves integer placement/shadow binding into the real scene and reproduces all
  124 capture hashes without test-side layout changes. Missing-shadow fallback passes.
  The human approved delivery/use and 12-ID production assignment (shared Mina
  six clips plus Companion six props). [Actual catalog verification](art/reviews/2026-10-11/companion-delivery-v1/README.md)
  preserves all 124 approved capture hashes and checks Town Mina / Ghost 40/65/85%
  rendering. 34 IDs remain unassigned. Exact tool contact remains open.
  Final Workshop delivery, full effect sheets and runtime reveal integration remain
  future production work. Art direction approvals are not final asset delivery.

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

- **Status:** IN REVIEW — CI creates a versioned unsigned ZIP with hash/build
  manifest and deterministic isolated demo saves; it opens each demo headlessly
  in the exported executable. Windows UI, funding art, and final release review
  remain open.
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
