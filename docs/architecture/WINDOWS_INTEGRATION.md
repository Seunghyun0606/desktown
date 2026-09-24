# Windows Integration Contract

## 1. Boundary

All Windows-specific code lives under `DeskTown.Platform.Windows` behind ports.
The rest of the application can run with fake adapters for tests.

```text
IActivityTracker          → WindowsActivityTracker
IGhostWindowPlatform      → WindowsGhostWindowPlatform
IWindowPlacementService   → WindowsWindowPlacementService
ITrayService              → GodotStatusIndicatorTrayService
INotificationService      → WindowsNotificationService
ISystemLifecycle          → WindowsLifecycleAdapter
```

No P/Invoke declarations are allowed outside this project.

## 2. Activity tracking

### APIs

- `GetForegroundWindow` obtains the current foreground window.
- `GetWindowThreadProcessId` maps its handle to a process ID.
- `.NET Process.GetProcessById(pid).ProcessName` resolves only the executable
  process name.
- `GetLastInputInfo` provides time since the last input event for the current
  Windows session.

Sampling occurs once per second only while a FocusSession is active. Access
denied, a process exiting between calls, or a zero handle produces `Unknown`
rather than an exception that ends Focus.

### Retained data

```text
ProcessAggregate
  ProcessName
  ForegroundDurationSeconds

SessionActivitySummary
  ActiveDurationSeconds
  IdleDurationSeconds
  UnknownDurationSeconds
```

Raw HWND, PID, and per-second samples are ephemeral. Process names are retained
only in that session's aggregate. Prototype code must not call `GetWindowText`,
inspect browser accessibility trees, install input hooks, or capture screens.

### Idle thresholds

- `<60 s`: User Active
- `60–179 s`: Short Idle; presentation variation only
- `≥180 s`: Long Idle; Mina rests

Idle never reduces v0.1 Focus Energy. `GetLastInputInfo` tick arithmetic must
handle 32-bit tick wrap correctly by using unsigned subtraction.

## 3. Ghost overlay: engine-first, native-verified

### Godot configuration

Ghost is a native `Window` with:

```text
Borderless = true
AlwaysOnTop = true
Transparent = true
Unfocusable = true
MousePassthrough = true
Viewport.TransparentBg = true
ProjectSettings: display/window/per_pixel_transparency/allowed = true
```

Godot documents transparent, no-focus, always-on-top, and mouse-passthrough
flags on native Windows windows. Its mouse-passthrough wording explicitly
mentions an underlying window of the same application, so cross-application
click-through is an exported-build acceptance test rather than an assumption.

### Native adapter

`IGhostWindowPlatform` owns any Win32 reinforcement:

```csharp
public interface IGhostWindowPlatform
{
    GhostCapability Probe(nint nativeHandle);
    GhostApplyResult Apply(nint nativeHandle, GhostWindowOptions options);
    GhostVerification Verify(nint nativeHandle);
    void Remove(nint nativeHandle);
}
```

The adapter may set/preserve extended styles using `GetWindowLongPtr` and
`SetWindowLongPtr`:

- `WS_EX_LAYERED` — layered/alpha window
- `WS_EX_TRANSPARENT` — transparent style reinforcement
- `WS_EX_NOACTIVATE` — showing/clicking does not activate the overlay
- `WS_EX_TOOLWINDOW` — avoids normal taskbar/Alt+Tab presence

It then uses `SetWindowPos(HWND_TOPMOST, ..., SWP_NOACTIVATE | SWP_FRAMECHANGED)`.
Existing style bits must be OR-ed and restored, never overwritten wholesale.

If exported-build testing shows style flags are insufficient, a narrowly scoped
native window-procedure hook may return `HTTRANSPARENT` for `WM_NCHITTEST`. That
is a contingency task, not part of the first implementation, because subclassing
an engine-owned HWND increases lifecycle risk.

### Native handle

Obtain the native handle at the Godot boundary through
`DisplayServer.WindowGetNativeHandle(...)`, validate that it is non-zero and
belongs to the expected window/process, then pass it as opaque `nint` to the
adapter. Never let HWND escape into domain/application services.

### Safety rule

After showing Ghost, verify topmost/no-activate styles and run the platform smoke
check. If capability or application fails, immediately hide Ghost, switch to
Hidden, and retain the FocusSession. An interactive transparent window is not an
acceptable degraded mode.

## 4. Companion window

- Native, borderless, always on top
- 360 × 200 logical canvas at 75/100/125/150% integer content presets
- Drag starts only from declared background region
- Save monitor identity plus normalized anchor/offset, not raw coordinates alone
- After drag, release focus back to the previously foreground application where
  Windows permits; never synthesize input
- Close request maps to `SetDisplayMode(Hidden)`

## 5. Monitor and DPI placement

Store:

```text
MonitorKey
Anchor: TopLeft | TopRight | BottomLeft | BottomRight | Free
OffsetFromAnchorPhysicalPx
LogicalScalePreset
LastKnownWorkArea
```

On restore:

1. Match stable monitor identifier where available.
2. Otherwise choose the primary/current monitor.
3. Recompute from that monitor's usable work area and current DPI.
4. Clamp the full window inside the work area.
5. Snap sprite content to whole rendered pixels.

Never assume the virtual desktop begins at `(0,0)`; a monitor can have negative
coordinates. Test 100%, 125%, and 150% scale and moving between unlike monitors.

## 6. Tray

Primary implementation uses Godot `StatusIndicator`, which is supported on
Windows and can bind a native popup menu. `TrayManager` builds menu commands and
forwards them to application commands; it does not mutate scenes directly.

Required commands:

- Open DeskTown
- Companion / Hidden / Ghost
- Companion scale
- Ghost corner and opacity
- Audio
- End Focus
- Quit

Tooltip is stateful: `DeskTown`, `DeskTown • Focus`, or
`DeskTown • Work complete`.

## 7. Notifications

`INotificationService` exposes a passive notification with optional Open action.
The Windows adapter may use an established Windows notification library/API only
after a packaged and unpackaged export spike confirms app identity behavior.

Fallback order:

1. Windows passive notification
2. Tray notification capability, if available
3. Tray tooltip/status only

Failure never opens Main Town and never consumes a pending reveal.

## 8. Suspend, lock, clock, and restart

- Use a monotonic clock for in-process elapsed time.
- On suspend/lock/deactivation signal, checkpoint and set session Suspended.
- Wall-clock time while suspended is not counted.
- On resume, restart sampling and logical tick; do not replay missed frames.
- Use a named single-instance mutex. A second launch activates/open-requests the
  existing instance instead of starting another session.
- Unexpected shutdown leaves a recoverable active-session checkpoint. Startup
  asks to resume or end at the last checkpoint.

## 9. Manual verification matrix

Every row is tested against an exported `.exe`, not only the editor.

| Area | Matrix |
| --- | --- |
| DPI | 100%, 125%, 150% |
| Monitor | single; dual same DPI; dual mixed DPI; secondary left of primary |
| Underlay apps | Chrome, VS Code, Excel, Notion |
| Underlay state | windowed, maximized |
| Ghost input | click, double-click, right-click, drag, scroll, text selection |
| Focus | typing continues; Alt+Tab order; taskbar entry |
| Lifecycle | sleep/resume, lock/unlock, explorer restart, app crash/restart |

## 10. Technical references

- [Godot Window](https://docs.godotengine.org/en/stable/classes/class_window.html)
- [Godot StatusIndicator](https://docs.godotengine.org/en/stable/classes/class_statusindicator.html)
- [Godot DisplayServer](https://docs.godotengine.org/en/stable/classes/class_displayserver.html)
- [Microsoft Winuser APIs](https://learn.microsoft.com/en-us/windows/win32/api/winuser/)
- [Microsoft Extended Window Styles](https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles)

