# DT-E3-001 Companion native window

## Scope

The application owns a native 360 × 200 Godot `Window` with an opaque, quiet
placeholder diorama. It starts hidden, is borderless, non-resizable and always
on top. The top 28 logical pixels, excluding the close control, are its drag
region. The close control and the OS close request change the display mode to
Hidden. The shared `DisplayModeController` owns only surface visibility; it has
no FocusSession, Energy, or Town reference. Ghost is unavailable until its own
window exists, and asking for it leaves the current surface intact.

The placeholder blocks establish scale and hit regions only. Mina animation,
snapshot binding, custom sprites, saved placement, tray access, and a real
Focus Setup are separate backlog tasks.

## Exported Windows QA gate

CI exports a Windows build. Extract the entire artifact folder on Windows 11
and run `DeskTown.exe`. In this Foundation shell, press **F9 while the main
window has focus** to preview or hide the Companion window. This is a temporary
QA entry point; it does not start a Focus session. After closing the Companion,
focus the main window and press F9 to reopen it.

1. Verify the 360 × 200 borderless Companion appears at the bottom right of the
   current main-window monitor and stays above Chrome, VS Code and Excel.
2. Drag only the empty upper strip. Verify the close control is clickable and
   does not initiate a drag. Verify dragging does not prevent normal typing,
   selection, scrolling or clicking in those applications afterward.
3. Click × and reopen with F9; repeat with the OS close request (Alt+F4 while
   the Companion is focused). The main shell remains running; no session or
   progress state is touched.
4. Repeat on a second monitor if available. Record Windows build SHA, Windows
   version, DPI, monitor layout, observed focus behavior, and any failures.

The window lifecycle and display controller can be automated in CI. Native
z-order, drag, and focus handoff require the exported `.exe` on Windows. Until
those are observed, DT-E3-001 remains **in review**. Position persistence and
mixed-DPI clamping belong to DT-E3-003; this preview deliberately starts at
the main monitor's bottom right each time.
