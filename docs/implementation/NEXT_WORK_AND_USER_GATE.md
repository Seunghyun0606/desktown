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
- CI validates the 46-entry asset contract, generates byte-identical demo saves
  for a fixed date, opens all three states in the exported app, and creates an
  unsigned versioned Windows ZIP with a file manifest and SHA-256 checksum.
- CI assembles a laptop QA kit, runs its isolated PowerShell preflight on
  Windows, and tests assigned synthetic art in Town and Companion in a
  separate checkout from the production export.
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
| 1 | Validate the versioned unsigned ZIP on a clean Windows 11 machine | Extract, launch, quit, and relaunch without the editor or SDK; verify save location and update/rollback | ZIP, checksum, manifest and extracted CI startup now automated; manual clean-machine gate |
| 2 | Deliver and approve production art/audio for the 46 Asset Catalog entries | Validator accepts the files, provenance and dimensions; art gates sign off pivots, animation and mix | Contract/validator and main Town/Companion binders implemented; assets remain unassigned |
| 3 | Visually verify the Town/Companion art replacements, then bind remaining scene art as needed | Workshop states and character clips render at target scale; remaining walking/environment/effect/tool usage is approved | Assigned assets and art review |
| 4 | Visually sign off deterministic pre-Workshop, completed Workshop pending reveal, and Railway teaser demos | Each state opens reproducibly without a personal save and the capture matches the narrative/art intent | Generator, byte comparison, isolated launch and exported headless smoke implemented |
| 5 | Run the Windows and user task matrix below, record issues by build SHA, then address P0/P1 first | Reproduction, severity, owner, fix, regression result, and re-test evidence exist for every issue | Exported Windows environment and users |
| 6 | Decide installer/signing/update path after prototype ZIP validation | Installer or update cannot overwrite the local save; uninstall offers an explicit save-retention choice; rollback is documented | Distribution decision and manual install test |

The `desktown-windows-qa-kit` artifact contains the versioned ZIP/checksum,
demo saves, scripts, build manifest, and Windows result sheet. Run its
`preflight.ps1 -RunSmoke` before interactive QA; it never uses the personal
save. The CI artifact `desktown-windows-prototype-package` contains the versioned
ZIP, `SHA256SUMS.txt`, `build-manifest.json`, and `RELEASE-NOTES.txt`. The older
`desktown-windows-foundation` artifact remains a raw export for QA. Do not
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
Focus Energy. The 46-entry catalog validates the manifest. Mina's six clips
resolve from one shared catalog in all three views; Town buildings, Noah/Rumi
idle/read art, and six Companion props also resolve through scene binders with
geometric fallback. All production files are currently unassigned. CI checks
several assigned synthetic sheets in actual Godot scenes in a separate checkout
from the unmodified prototype export. Frame size and count are checked at
runtime, while placement and pivots still need visual sign-off.
Walking sheets, remaining environment/effects/tool art, and audio do not yet
have runtime placement. The scene node bounds are still fixed, so a valid PNG
can be off-center, obscure text, or appear too small at a chosen scale.

Keep logical state/clip names and stable manifest IDs; put paths, frame counts,
FPS, pivots, and fallback assets in catalog resources. Swap one visual family at
a time. Validate Mina's six clips/30 frames, Workshop's three states, Companion
props, and shared Mina rendering across Town/Companion/Ghost. Confirm nearest
scaling and hit regions at 75/100/125/150% Companion scale and 40/65/85% Ghost
opacity. Ghost must remain visually test-only until input passthrough is signed.
No art replacement should change a save field or gameplay threshold.

## Reproducible demo saves

The QA kit includes three isolated demo scenarios; the separate
`desktown-demo-saves` CI artifact contains the same fixtures. The generator
uses a fixed date and session ID, domain transitions, and the production save
codec. Run `dotnet run --project tools/DeskTown.DemoSaves -- <output-dir>
<yyyy-MM-dd>` to generate for a chosen UTC day; without a date it uses
2026-09-25. From the extracted QA kit, after `preflight.ps1`, run:

```powershell
./launch-demo.ps1 -Scenario workshop-reveal-pending `
  -WindowsExport ./app -DemoSaves ./demo-saves
```

Close any existing DeskTown instance first. The script copies the fixture into
a fresh temporary directory and passes `--desktown-demo` to the app. The app
marks its title as an isolated demo, reads/writes only that copy, and refuses to
forward a demo launch to a running personal instance. A fixture generated for
a different UTC day still loads; the Today HUD may then show zero. Do not copy
demo `save.json` into the normal `user://` location.

## Work-interruption assessment and user test

Current protections: the Town does not open at Focus completion; a short
completion notice offers explicit Open, with Tray status as fallback; Hidden
does not show Companion/Ghost; the F10 Ghost preview cannot run during Focus.
These are code observations, not proof that real PC work is uninterrupted.
Catalog sprites are draw-only nodes beneath the existing window boundary and
introduce no pointer controls; the Windows input checks remain necessary.

The native completion notice is topmost, 360×110, and visible for six seconds
when its setting is on (the existing-save and new-save default).
It is marked unfocusable but **can receive pointer input** so its Open button
works. It may cover or intercept a click in an underlying app. Companion is
always on top with a draggable strip, and real focus restoration is unverified.
The Tray offers `Completion notice (otherwise Tray only)` and saves that
preference. Tray-only completion keeps the Work complete status and explicit
Open command without a notice window. Treat notice pointer interception,
Companion focus theft, and any Ghost input interception as release-blocking
until observed on the exported build. Test Hidden with no visible surfaces
and low idle CPU/GPU usage.

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
