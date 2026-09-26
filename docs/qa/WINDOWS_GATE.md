# DeskTown v0.1 — exported Windows gate

Use the exported build. Godot editor behavior, Linux headless CI, and a successful
Windows export do not establish native window/input behavior. Keep Ghost disabled
for normal Focus until the interaction matrix below has been recorded and reviewed.
CI also launches the exported executable headlessly on a Windows runner. That
checks bootstrap and script errors, and uses three separate exported-app
processes to create, recover, and reload an isolated save. It cannot certify
visible window behavior, the normal Windows save directory, or real sleep/lock.

## Obtain and launch the build

1. On the [CI Actions page](https://github.com/Seunghyun0606/desktown/actions/workflows/ci.yml),
   open the latest **successful `main`** run. Record its commit SHA and run URL.
2. Download `desktown-windows-qa-kit`, extract the artifact, and run
   `./preflight.ps1 -RunSmoke` in PowerShell. It checks the versioned ZIP,
   extracts the app, verifies the full file manifest, and tests an isolated
   save/restart/recovery cycle without touching your personal save. Launch
   `./app/DeskTown.exe` on Windows 11. The `desktown-windows-prototype-package`
   artifact remains the distribution ZIP; `desktown-windows-foundation` is the
   raw export. Do not run the executable from inside the ZIP.
3. Run `./collect-evidence.ps1 -Scaling '100%'` with the laptop's actual Windows
   scale. It writes a local result template with build SHA, OS version and
   monitor bounds. Use a separate Windows test account or back up the existing
   DeskTown save before destructive recovery tests. Keep one artifact and one
   save baseline per test pass; do not upload personal saves or work contents.

The first run displays privacy/onboarding copy, then Town. Town opens Focus
Setup. Focus Setup offers Companion or Hidden; Ghost is disabled. The Tray has
Open, mode/settings, End Focus, and Quit. F10 from Town toggles the **Ghost QA
preview only when no Focus session is active**. It does not start a session or
award progress. F8 cycles placeholder Companion clips under the same condition.

## 1. Core loop and recovery

| Check | Procedure | Pass condition | Result / evidence |
| --- | --- | --- | --- |
| First run | Complete onboarding; open Focus Setup | Tracking starts only after Start; process selection is optional | |
| Shared session | Start 25-minute Companion Focus; switch to Hidden and back from Tray | Same elapsed Focus, Energy, and Workshop progress; no duplicate session | |
| Passive completion | Finish 25 minutes while another app is active | Town stays hidden; notice or Tray status appears without stealing focus | |
| Quiet completion | Tray → turn off `Completion notice`, finish another session | No notice window; Tray says Work complete and Open remains available; preference survives restart | |
| Notice Open action | Click `Open DeskTown` on the completion notice | Existing Town opens once, without another process or reward | |
| Deferred reward | Open DeskTown from Tray after completion | Workshop reveal and Railway teaser appear once, in order | |
| Restart at reveal | Exit after completion notice, during reveal, and during discovery in separate runs | Saved state resumes the correct pending step once | |
| Recovery | Start Focus, wait for a checkpoint, terminate process; relaunch | Resume/End choice appears; time away is excluded | |
| Windows lifecycle | Lock/unlock and sleep/resume during separate Focus runs | Suspended time is excluded; state remains recoverable | |
| Duplicate launch | Open another `DeskTown.exe` while first remains active | Existing instance opens; no second Focus/save owner | |
| Hidden resources | Switch to Hidden and inspect windows and Task Manager | No Companion/Ghost surface or active visual effects; timer continues | |

Record the exact elapsed times and save observations for any discrepancy.
If the notice cannot be shown or clicked, use **Tray → Open DeskTown** and record
the failure; the saved reward must remain intact.

## 2. Companion and placement

At 100%, 125%, and 150% Windows scaling, check single and dual monitors,
including unlike DPI and a monitor to the left of the primary display.

| Check | Pass condition | Result / evidence |
| --- | --- | --- |
| Always on top and drag strip | Companion stays visible above ordinary app windows; only its declared upper strip drags | |
| Focus and input | Chrome, VS Code, Excel, and Notion remain usable while Companion is open | |
| Close | Companion closes into Hidden without ending Focus | |
| Scale presets | 75/100/125/150% scale remains legible and on-screen | |
| Restore | Move/close/relaunch, then disconnect the chosen monitor and relaunch | Position restores or clamps into an available usable work area | |

## 3. Ghost interaction gate — release blocking

Use Town F10 to show the isolated preview; set corner, monitor, and 40/65/85%
opacity from Tray. Repeat on Windows 11 at 100%, 125%, and 150% scaling; on
single and dual monitors; and with each application windowed and maximized.
Place the overlay over both visible Mina/prop pixels and transparent pixels.

| App | Click / double / right | Hover / drag | Scroll / text select | Typing focus | Alpha / topmost / Alt+Tab | Result / evidence |
| --- | --- | --- | --- | --- | --- | --- |
| Chrome | | | | | | |
| VS Code | | | | | | |
| Excel | | | | | | |
| Notion | | | | | | |

For every DPI/monitor/window-state combination, record whether all pointer
actions reach the underlying app, typing stays there, Ghost remains transparent
and topmost, and it does not appear as an ordinary taskbar/Alt+Tab work window.
Also verify corner/opacity changes while visible and restoration after restart.

**Decision:** Any intercepted action, focus theft, off-screen overlay, or
unstable native behavior is a failure. Leave Ghost unavailable in Focus Setup
and Tray, use Hidden, and file the exact combination with screenshots/video and
reproduction steps. A passing F10 preview is evidence for the platform spike;
normal Focus-mode fallback still needs an exported end-to-end check before Ghost
can be enabled. Record the reviewer, date, build SHA, and final decision here:

| Reviewer / date | Build SHA / CI run | Ghost decision | Blocking issue links |
| --- | --- | --- | --- |
| | | Pending | |

## 4. Funding-build gate

After the Windows behavior gate, review custom Mina animation, the three
Workshop states, core Companion props, Old Railway Map, audio/effects, reduced
motion, performance, privacy of the local JSON save, and deterministic capture
scenes. These are not certified by the current placeholder export.
