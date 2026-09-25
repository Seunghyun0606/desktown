# DeskTown v0.1 — packaging, design integration, and user follow-up

This is an actionable handoff for work after the automated placeholder slice.
An `IN REVIEW` task is not release-complete until its Windows or art gate passes.
The [backlog](../BACKLOG.md) remains the task inventory, and the
[Windows gate](../qa/WINDOWS_GATE.md) records actual exported-build results.

## What the current build proves

- CI builds/tests, imports Godot, exports the Windows folder, launches the
  exported `.exe` headlessly, and creates/recovers/reloads an isolated save in
  three separate processes. This verifies a real process boundary, not only
  in-memory serialization.
- Focus simulation, Energy, Workshop, and pending reveal are independent of
  the presentation. The Town ambient timer and Mina placeholder frame processing
  stop while their views are hidden.
- During Focus the main Town window is hidden. Hidden removes the Companion
  surface. Ghost is disabled for normal Focus until the Windows input gate.

These checks do **not** prove visible focus behavior, cross-application pointer
input, mixed DPI, sleep/lock, normal `user://` save placement, or visual quality.

## Prioritized TODO

| Order | Work | Done when | Dependency |
| --- | --- | --- | --- |
| 1 | Package an unsigned prototype ZIP from the complete Windows export folder, including managed sidecars; add SHA-256, commit, Godot/.NET versions, and short release notes | A clean Windows 11 machine extracts it, launches it, quits, and relaunches without the editor or SDK | Manual clean-machine gate |
| 2 | Define stable Asset Catalog records for the 46 manifest IDs, mapping sprite/audio paths, origin, license, pivot, frame grid, and fallback | Validator rejects missing/invalid dimensions and metadata; scene binding uses IDs and missing assets fall back without changing saves or Energy | Can build schema/validator now; approved production art later |
| 3 | Replace geometric Town buildings and `MinaPlaceholderView`/`TownNpcPlaceholderView` through presentation adapters and catalog resources | All existing snapshot/clip states render without altering domain, events, save schema, or Windows behavior; test each view at target scale | Catalog contract and art gates |
| 4 | Add deterministic demo saves for pre-Workshop, completed Workshop pending reveal, and Railway teaser | Each state opens reproducibly on a clean test account and does not contaminate a personal save | Automated fixture work now; visual sign-off later |
| 5 | Run the Windows and user task matrix below, record issues by build SHA, then address P0/P1 first | Reproduction, severity, owner, fix, regression result, and re-test evidence exist for every issue | Exported Windows environment and users |
| 6 | Decide installer/signing/update path after prototype ZIP validation | Installer or update cannot overwrite the local save; uninstall offers an explicit save-retention choice; rollback is documented | Distribution decision and manual install test |

The current artifact name `desktown-windows-foundation` is historical. Do not
distribute only `DeskTown.exe`; keep the exported directory intact. A ZIP is the
smallest prototype handoff. Do not enable auto-start, background update, or
startup notifications as a packaging shortcut. Version and hash the ZIP; keep
the `user://` save outside the extracted folder. Verify actual save location,
permissions, backup recovery, update-in-place, and deletion on Windows before
an installer is offered. Code signing/reputation and required runtime behavior
must be checked on a clean machine rather than inferred from CI.

## Design replacement assessment

The simulation/persistence boundary is suitable for replacing visuals: scene
views receive Town/Companion/Ghost projections and have no authority to award
Focus Energy. The visual binding is **not yet a plug-in catalog**. Town building
states and positions are currently hard-coded in `TownScene.cs` and its scene;
Mina and ambient NPCs draw geometric rectangles in C#. Replacing sprites today
requires edits to those presentation classes, scene node paths, and possibly
placement. Changing sprite frame sizes or pivots without validation can cause
clipping, jumps, and a click target that no longer matches the artwork.

Keep logical state/clip names and stable manifest IDs; put paths, frame counts,
FPS, pivots, and fallback assets in catalog resources. Swap one visual family at
a time. Validate Mina's six clips/30 frames, Workshop's three states, Companion
props, and shared Mina rendering across Town/Companion/Ghost. Confirm nearest
scaling and hit regions at 75/100/125/150% Companion scale and 40/65/85% Ghost
opacity. Ghost must remain visually test-only until input passthrough is signed.
No art replacement should change a save field or gameplay threshold.

## Work-interruption assessment and user test

Current protections: the Town does not open at Focus completion; a short
completion notice offers explicit Open, with Tray status as fallback; Hidden
does not show Companion/Ghost; the F10 Ghost preview cannot run during Focus.
These are code observations, not proof that real PC work is uninterrupted.

The native completion notice is topmost, 360×110, and visible for six seconds.
It is marked unfocusable but **can receive pointer input** so its Open button
works. It may cover or intercept a click in an underlying app. Companion is
always on top with a draggable strip, and real focus restoration is unverified.
Treat notice pointer interception, Companion focus theft, and any Ghost input
interception as release-blocking until observed on the exported build. Test
Hidden with no visible surfaces and low idle CPU/GPU usage. A setting for
Tray-only completion or a platform notification should be considered if the
notice interrupts work; do not silently sacrifice the Open action.

User test protocol after the Windows system gate:

1. Recruit a small set of Windows 11 users with different DPI/monitor layouts.
   Ask each to complete a normal 25-minute writing/coding/spreadsheet task in
   Companion and Hidden, then try Ghost only as an isolated QA preview.
2. Observe startup/onboarding, optional process selection, mode changes, Tray
   discovery, completion while another app is active, Open action, reveal, and
   quit/restart. Do not record window titles, document contents, typed text, or
   screenshots of private work without explicit consent.
3. Record each unwanted focus change, blocked click/drag/scroll/typing action,
   obscured work area, unexpected sound, and recovery failure with time, mode,
   app category, monitor/DPI, build SHA, and reproduction steps. Ask whether
   Mina was pleasant or distracting and whether Hidden felt truly invisible.
4. Triage P0 for lost progress/input interception; P1 for repeatable focus theft,
   off-screen views, or interruption; P2 for visual/copy polish. Fix one issue,
   add a regression check where feasible, and repeat the affected real-work
   task. Keep a short before/after note; do not collect work content.

Acceptance for the no-interruption claim requires the exported-build matrix:
underlying apps retain typing focus and pointer actions, Town never raises on
passive completion, Companion can be moved/closed without stopping Focus,
Hidden has no DeskTown visual window, and the Ghost QA preview passes every
specified app/input/DPI combination before normal Ghost mode is considered.
