# DeskTown Prototype v0.1 Wireframes

These wireframes define hierarchy and behavior, not final art. Main Town uses a
16:9 desktop canvas; utility panels are deliberately narrow so the town remains
the focal point.

## A. First Launch

```text
┌──────────────────────────────────────────────────────────────────────┐
│                                                                      │
│                         [ DeskTown logo ]                            │
│                              Mina                                    │
│                                                                      │
│            Your focus helps Mina rebuild a small town.               │
│                                                                      │
│   DURING A FOCUS SESSION              DESKTOWN NEVER RECORDS          │
│   • process name                      • typed text                    │
│   • foreground duration              • screenshots                   │
│   • total idle duration              • files or document content     │
│   • session start and end            • browser URLs or window titles │
│                                      • passwords or forms            │
│                                                                      │
│             Prototype data stays only on this PC.                    │
│                                                                      │
│                         [ Continue ]          Quit                    │
└──────────────────────────────────────────────────────────────────────┘
```

The privacy explanation is the primary content, not a checkbox buried in legal
copy. `Continue` does not start tracking.

## B. Main Town

```text
┌──────────────────────────────────────────────────────────────────────┐
│ Restore Workshop  18/25                              Today 42 min  ⚙ │
│                                                                      │
│                 trees                      [LOCKED AREA]              │
│                                                                      │
│       ┌─────────┐                   ┌─────────────────┐              │
│       │  HOUSE  │       Mina        │ BROKEN WORKSHOP │              │
│       │  light  │        ◕          │   boards/tools  │              │
│       └─────────┘       /|\          └─────────────────┘              │
│            ╲             path                 ╱                       │
│             ╲───────────────────────────────╱                         │
│                 Rumi               Noah            campfire          │
│                  ◕                  ◕                 *              │
│                                                                      │
│                                                        [ Focus ]     │
└──────────────────────────────────────────────────────────────────────┘
```

- Buildings and characters occupy most of the screen.
- Project and daily time are quiet text, never dashboard cards.
- Workshop is both a world object and the entry point to Focus Setup.

## C. Focus Setup

```text
┌──────────────────────────────────────────────────────────────┐
│ RESTORE WORKSHOP                                      [×]    │
│ Mina • Builder                                               │
│ Mina wants to restore the abandoned workshop.                │
│                                                              │
│ Progress                     18 / 25 Focus                    │
│                                                              │
│ Duration                     Apps for this session            │
│ [● 25] [45] [60]            [ Any app                 ▾ ]    │
│                                                              │
│ How should Mina stay with you?                               │
│ ┌────────────┐  ┌────────────┐  ┌────────────┐               │
│ │● Companion │  │○ Hidden    │  │○ Ghost     │               │
│ │small room  │  │no visuals  │  │soft overlay│               │
│ └────────────┘  └────────────┘  └────────────┘               │
│ Same session. Same progress. Choose what feels comfortable.  │
│                                                              │
│                         [ Start Focus ]                       │
└──────────────────────────────────────────────────────────────┘
```

Mode cards use equal size, weight, and neutral descriptions. There is no badge
such as “recommended” or “best rewards.”

## D. Companion Window

Logical size: 360 × 200.

```text
┌──────────────────────────────────────────┐
│       cloud       ┌────────┐        lamp │
│                   │ window │          ●  │
│                   └────────┘             │
│                                          │
│       plant        Mina 🔨               │
│        ♧         ┌──────────┐      stool │
│                  │workbench │        ▱   │
│──────────────────┴──────────┴────────────│
│ Workshop                             18:34│
└──────────────────────────────────────────┘
```

- Remaining time is secondary, small, and non-urgent.
- The empty background is the drag zone.
- Closing the window changes the display to Hidden.

## E. Ghost Overlay

Window size: 240 × 180; visible content about 120 × 100.

```text
        transparent native window boundary
      ┌──────────────────────────────┐
      │                              │
      │                  · spark     │
      │             Mina 🔨          │
      │          ───────────         │
      │             shadow           │
      │                              │
      └──────────────────────────────┘

      no controls • no text • no input
```

The boundary is never rendered. All click, drag, scroll, hover, and keyboard
input belongs to the application underneath.

## F. Hidden / Tray

There is no DeskTown surface on screen.

```text
Windows notification area
┌──────────────────────────────┐
│ DeskTown • Focus             │
│ Open DeskTown                │
│ Display                  ▸   │
│ Companion Scale          ▸   │
│ Ghost Position           ▸   │
│ Ghost Opacity            ▸   │
│ Audio                     ✓  │
│ ───────────────────────────  │
│ End Focus…                   │
│ Quit…                        │
└──────────────────────────────┘
```

## G. Passive completion

```text
┌────────────────────────────────────────┐
│ DeskTown                               │
│ Mina finished her work.                │
│ The Workshop has changed.              │
│                          Open DeskTown  │
└────────────────────────────────────────┘
```

No countdown, urgency, or automatic window opening.

## H. Reward Reveal beats

```text
Beat 1                  Beat 2                  Beat 3
Old Workshop            Mina's last work        Completed Workshop
┌─────────────┐          ┌─────────────┐         ┌─────────────┐
│ loose boards│          │   Mina 🔨   │         │ window  ✦  │
│ dark window │   →      │ hammer × 2  │   →     │ smoke  ~   │
│ no smoke    │          │ then still  │         │ warm light  │
└─────────────┘          └─────────────┘         └─────────────┘
```

The world state is committed before Beat 1. Animation only presents the change.

## I. Discovery Event

```text
┌──────────────────────────────────────────────────────────────┐
│ WHILE YOU WERE AWAY                                         │
│                                                              │
│          [ old railway map ]                                 │
│                                                              │
│ Mina found an old railway map beneath the workshop floor.    │
│                                                              │
│ Rumi: “I wonder where this leads…”                           │
│                                                              │
│ NEW PROJECT                                                  │
│ Explore the Old Railway                         0 / 45 Focus │
│ Coming in the next build                                     │
│                                                [ Continue ]   │
└──────────────────────────────────────────────────────────────┘
```

## J. Settings panel

```text
┌───────────────────────────────────────────┐
│ Settings                              [×] │
│                                           │
│ Display      Companion  Hidden  Ghost     │
│ Companion    Scale 100%   Reset position  │
│ Ghost        Bottom Right   Opacity 65%   │
│ Audio        On                            │
│                                           │
│ Privacy & activity tracking               │
│ View what DeskTown records                 │
│ Open save folder                           │
│                                           │
│                              [ Done ]      │
└───────────────────────────────────────────┘
```

## Responsive and DPI notes

- Main Town supports 1280 × 720 minimum and scales composition while preserving
  the world-first hierarchy.
- Companion uses 360 × 200 logical pixels and integer content scale presets.
- Ghost's pixel art uses integer scale inside a DPI-aware native window; corner
  placement uses each monitor's work area, not the virtual desktop origin.
- At 125% and 150% DPI, UI text may scale smoothly while sprite pixels remain
  nearest-neighbor and aligned to whole rendered pixels.

