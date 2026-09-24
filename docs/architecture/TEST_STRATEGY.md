# Prototype Test Strategy

## 1. Test layers

| Layer | Runtime | Purpose |
| --- | --- | --- |
| Domain unit | `dotnet test` | deterministic focus, energy, project, Mina, events |
| Application unit | `dotnet test` with fakes | orchestration, mode switching, save policy |
| Integration | Godot/headless where possible | scene binding, JSON store, lifecycle adapters |
| Windows system | exported `.exe` | HWND flags, tray, notification, activity APIs |
| Manual visual | exported `.exe` | pixel scaling, cozy hierarchy, reward timing |

Core tests use xUnit (or NUnit if chosen once in E0) and no running Godot editor.
The choice must be singular across the solution; xUnit is the recommendation.

## 2. Unit suites

### Focus and Energy

- start creates one active session and rejects a second
- monotonic ticks accumulate only while Running
- sleep/suspend duration is excluded
- early end retains seconds; display rounds consistently
- idle duration does not reduce v0.1 Energy
- display-mode changes cannot change session totals

### Project and event

- progress applies exactly once per accepted Energy delta
- progress clamps at target
- Workshop transitions Broken → Repairing → Complete at configured boundaries
- completion event emits exactly once
- Railway discovery unlocks only after Workshop completion/reveal flow
- replaying a command/event is idempotent

### Mina

- active, short-idle, long-idle, suspended, return transitions
- threshold boundary tests at 59/60 and 179/180 seconds
- logical state does not depend on rendered surface
- Celebrate is presentation-triggered from completion without owning completion

### Persistence

- round-trip every field
- invalid hash and truncated JSON reject
- backup recovery
- schema migration fixtures
- stale concurrent revision cannot overwrite newer state
- pending reveal survives restart

## 3. Application integration scenarios

1. Town → Focus Setup → start → ticks → progress → complete → save
2. Companion → Hidden → Ghost → Companion with one session ID and equal totals
3. Tracker throws/access denied; Timer-only session completes
4. Ghost probe fails; Hidden activates without session interruption
5. Completion notification fails; pending reveal remains accessible from Tray
6. Crash after project completion but before reveal; restart restores completed
   Workshop and replays/finishes presentation safely
7. Restart mid-session; Resume and End-at-checkpoint paths

## 4. Contract tests for ports

Run the same behavioral contract against fake and real adapters where feasible:

- `IGameStateStore`: revision, atomicity, backup, corruption
- `IActivityTracker`: stable sample model and Unknown behavior
- `IDisplayModeController`: exclusive visible surface and fallback
- `IGhostWindowPlatform`: apply/remove idempotency and style preservation

## 5. Windows manual/system matrix

### Ghost gate — release blocking

On Windows 11 exported `.exe`, test 100/125/150% DPI, single/dual monitors,
Chrome/VS Code/Excel/Notion, windowed/maximized. With the pointer over visible
and transparent Ghost pixels verify:

- left/right/double click reaches underlay
- drag reaches underlay
- wheel scroll reaches underlay
- text selection reaches underlay
- typing focus never leaves underlay
- Ghost does not appear as an ordinary Alt+Tab/taskbar work window
- opacity and corner change from Tray

Any input interception is a **fail** and activates Hidden fallback.

### Companion gate

- topmost but movable
- 360 × 200 logical composition at every scale/DPI
- drag/monitor restore and off-screen clamping
- close means Hidden; session continues
- other applications remain normally usable

### Activity/privacy gate

- process changes aggregate correctly
- idle thresholds reflect `GetLastInputInfo`
- inaccessible/exited process becomes Unknown
- saved JSON contains no title, URL, input, screenshot, document, or HWND/PID

### Lifecycle gate

- sleep/resume and lock/unlock exclude suspended time
- quit confirmation preserves/ends correctly
- crash/restart recovers no more than checkpoint interval
- duplicate launch does not create a second session

## 6. Visual acceptance

- nearest-neighbor scaling, no sprite shimmer or fractional placement
- Mina silhouette readable at 1× and Ghost 40%
- Workshop states distinguishable without color/effects
- Companion is a diorama, not a dashboard
- Reward reveal final state is correct even with particles/reduced motion off
- missing asset fallback never blocks the domain transition

## 7. CI stages

```text
PR / push
  1. formatting and build
  2. domain/application unit tests
  3. persistence fixture tests
  4. Godot project import/headless smoke test
  5. Windows export build

Release candidate (Windows runner)
  6. automated launch/save smoke
  7. signed manual Ghost/DPI/monitor checklist
```

Windows UI behavior cannot be certified by a Linux CI job or Godot editor run.
Store the manual test matrix, build hash, Windows version, GPU, monitor/DPI, and
result with the candidate build.

## 8. Test data and clocks

- Inject fake wall and monotonic clocks; tests never sleep in real time.
- Use fixed GUID/event IDs where snapshot assertions require determinism.
- Use a temporary isolated save directory per test.
- Use placeholder catalog IDs, not production image bytes, in logic tests.
- Keep one golden save for each supported schema version.

## 9. Feature-complete quality gate

- All unit/integration suites pass
- No P0/P1 open bug
- Ghost matrix passes on at least one single- and one dual-monitor Windows 11 setup
- Save recovery and pending reveal restart test pass
- Performance target met: visible ≤30 FPS, Hidden no active rendering surface,
  logical tick stable over a 60-minute soak
- Privacy inspection of persisted data passes

