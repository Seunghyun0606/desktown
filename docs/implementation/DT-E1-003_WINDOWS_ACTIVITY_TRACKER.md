# DT-E1-003 — Windows Activity Tracker Contract

## Outcome

The Windows adapter supplies descriptive activity observations without
affecting Focus Energy. It queries the foreground process name and OS idle age,
then returns one already-aggregated interval through `IActivityTracker`.

The default contract is one sample per second. The later Focus coordinator owns
the timer and must call `Sample()` only while a session is Running. The tracker
does not create a background timer and cannot start, stop, or complete a Focus
session.

## Public data boundary

`ActivitySample` contains exactly:

| Field | Meaning |
| --- | --- |
| `ProcessName` | Bare executable process name, or `Unknown` |
| `ActiveDuration` | Duration represented by an active-input sample |
| `IdleDuration` | Duration represented by an idle-input sample |

It contains no native window handle, process identifier, window title, URL,
keystrokes, text, screenshot, path, or document/form content. Native identifiers
are transient parameters inside `DeskTown.Platform.Windows` and are discarded
before a sample crosses into the application layer. Raw one-second observations
do not need to be persisted after a session ledger aggregates them.

If the foreground window or process cannot be inspected, `ProcessName` is
`Unknown` while a valid active/idle bucket is retained. If idle age itself
cannot be read, the whole sample is `Unknown` with zero durations. The later
coordinator can infer that unknown interval from `SampleInterval`; the monotonic
Focus clock remains authoritative, so adapter failure neither loses Focus time
nor claims false activity and cannot interrupt or penalize the session.

## Native boundary

Only `Activity/NativeMethods.cs` declares P/Invoke calls:

- `GetForegroundWindow`
- `GetWindowThreadProcessId`
- `GetLastInputInfo`

`WindowsActivityNativeApi` converts those calls into the narrow
`IWindowsActivityNativeApi` seam. `WindowsActivityTracker` consumes that seam
plus `IProcessNameResolver`; tests replace both with fakes. Domain and
Application contain no P/Invoke and do not reference native identifier types.

## Classification

- idle age below `IdleThreshold`: the configured sample interval is added to
  `ActiveDuration`
- idle age equal to or above `IdleThreshold`: the interval is added to
  `IdleDuration`
- default interval: 1 second, derived from `PrototypeOptions.LogicalTickInterval`
- default idle threshold: 60 seconds, derived from `PrototypeOptions.ShortIdleThreshold`

The idle bucket is descriptive. It is intended for Mina animation variation and
session history, not Productivity scoring or reduced rewards.

## Verification

`DeskTown.Platform.Windows.Tests` covers:

- the default 1 Hz contract
- active and exact-threshold idle classification
- inaccessible/terminated processes becoming `Unknown`
- platform or last-input failure returning a non-throwing unknown sample
- rejection of paths masquerading as process names
- serialized samples exposing only the three approved fields
- presence of the three required Win32 methods on the isolated native adapter

Manual Windows verification remains required: run an exported build, switch
between common applications, wait across the configured idle threshold, and
inspect the eventual session JSON for forbidden content.

Automated verification passed in GitHub Actions run `36085319041`: Release
build, 17 platform adapter tests, Godot headless import, and Windows export.
