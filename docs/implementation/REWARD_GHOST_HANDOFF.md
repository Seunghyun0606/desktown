# Reward and Ghost Windows handoff

This slice connects the existing session and Town simulation to a passive
completion and an on-demand Workshop reveal. It also adds a native Windows
style adapter to the **F10 QA preview**. Normal Ghost Focus selection remains
disabled until the exported Windows interaction matrix passes.

## Reward ordering

1. The Focus tick or End command commits elapsed time, project progress, and
   pending presentation to the JSON save.
2. The Companion window is hidden. A small passive, unfocusable notification
   appears for six seconds; the Tray tooltip is the fallback. The Town window
   is not opened.
3. Open DeskTown binds the committed state with a temporary Repairing visual.
   Mina's final Work beat runs for two seconds (or 0.1 seconds with reduced
   motion), then the visual becomes Complete. The Continue action acknowledges
   the Workshop reveal and queues the Railway discovery.
4. The discovery names the Old Railway Map and Rumi, unlocks the saved teaser,
   and shows `0 / 45 Focus — Coming in the next build`. Continue consumes the
   one-shot event. Railway gameplay is not offered in this prototype.

If the process exits during the reveal, the project remains complete and its
pending reveal is replayed at next Town open. If it exits after Workshop
acknowledgement, the pending discovery resumes instead. Placeholder labels and
colors stand in for the final camera, spark, smoke, sound, and authored assets.
Notification activation into the existing instance is not yet implemented.

## Ghost QA preview boundary

`IGhostWindowPlatform` lives in Application; `WindowsGhostWindowPlatform`
owns all native style operations. It validates an HWND belongs to this process,
adds layered/transparent/no-activate/tool-window bits, asks Windows for topmost
without activation, verifies those bits, and removes only bits it introduced.
F10 uses this adapter outside Focus. A failed apply hides the preview. The
adapter cannot verify cross-application mouse or keyboard behavior, and its
style-bit check must not be used as a release capability verdict.
The preview reads the saved Ghost corner, monitor and opacity preferences; Tray
changes update it while visible. Display selection is populated at startup;
relaunch after changing the monitor configuration. Missing monitors fall back to the first work
area and placement is clamped to its edges. This is still a QA preview, not a
certified Focus display mode.

Before enabling Ghost in Focus Setup or Tray, use the **exported `.exe`** on
Windows 11. Repeat at 100%, 125%, and 150% DPI, single and dual monitors,
windowed and maximized Chrome, VS Code, Excel and Notion. For each combination
record transparency, topmost, Alt+Tab/taskbar presence, keyboard focus, and
click, double-click, right-click, hover, drag, scroll, and text selection
through Mina and the prop. Mark a failure as unavailable and select Hidden
without stopping or changing the session. Do not infer a pass from the editor,
the style adapter, the CI export, or a screenshot.

## Other exported Windows checks

- Finish a 25-minute session across Companion → Hidden → Companion; verify
  passive completion, saved Workshop progress, and no focus-stealing Town.
- Quit after a completion notification, during the Workshop hold, and during
  discovery. Relaunch and verify the correct single pending presentation.
- Verify Today Focus, Tray Open/End/Quit and close-to-focus behavior; test
  sleep/lock and abnormal process termination separately.
- Verify Companion position on multi-monitor mixed DPI and the hidden-mode
  window/rendering behavior in Task Manager.

CI now launches the Godot main scene headlessly in addition to import/export.
This exercises scene paths and bootstrap but does not replace an exported
Windows launch or input test.
