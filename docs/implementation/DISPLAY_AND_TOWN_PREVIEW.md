# Automated display and Town preview slice

This slice prepared DT-E3-002/003, DT-E4-001, DT-E5-001 and DT-E6-001 without
claiming their Windows and art gates. The later Focus UI slice replaced its
temporary Town/Companion shortcuts with actual UI and Tray commands. The two
remaining visual inspection shortcuts work only outside a Focus session:

| Shortcut in main window | QA action |
| --- | --- |
| F8 | Cycle Mina's six presentation clips in the Companion window |
| F10 | Opt in/out of experimental Ghost overlay |

F8/F10 do not start Focus, accrue Energy, or write progress. Ghost is not yet a
selectable gameplay mode: cross-application passthrough and keyboard focus must
be checked on an exported Windows build before enabling it. Its stage contains
only a character, work prop and shadow, with no controls, text or input code.

Companion now owns a separate 360×200 stage. A pure presentation clock follows
the sprite contract's frame counts/FPS; snapshots bind the correct steady clip
on reopening, and one-shot Stretch/Walk can hand off to Work. Its 48×48 Mina
view and stage props are geometric previews, not final authored sprite sheets.
The stage stops processing while hidden. `CompanionPlacement` supports the four
requested window-size presets, negative monitor coordinates and on-screen
clamping; the host captures its position when hidden and persists the setting.
On Windows it maps each screen rectangle to a hashed monitor interface ID and
uses a provisional screen index if the native lookup fails. Mixed-DPI and
physical monitor reordering still require exported Windows validation.

`FocusDisplayLifecycle` models hiding the Main Town during focus, switching
surfaces, and keeping the Town closed after completion until explicitly opened.
It owns no session or reward state. The Town binds a `TownProjection` to distinct
Broken/Repairing/Complete Workshop visuals and a minimal HUD; the later Focus UI
slice connects the saved world and real session.

The follow-up Town pass replaces name-only NPCs with three geometric character
views. Mina reuses the Companion clip clock and moves between Home and Workshop
from the immutable Town snapshot. Noah's reading prop and Noah/Rumi positions
follow the deterministic ambient schedule; their presentation does not change
Focus, project progress or save data. These shapes are replaceable placeholders,
not the production character sheets. Town is hidden while Focus is presented;
its ambient timer stops until the user opens Town again.

## Remaining manual gates

- Companion animation appearance, scale/pixel quality, mixed DPI and position
  restoration on actual Windows monitors.
- Hidden zero-window/performance check once Focus Setup and Tray are wired.
- Ghost exported transparency, focus, Alt+Tab and full cross-app click/drag/
  scroll/text-selection matrix before offering the mode to users.
- Town layout/readability review at target display sizes.
