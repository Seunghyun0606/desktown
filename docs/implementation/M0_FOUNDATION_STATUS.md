# M0 Foundation Status

## Scope of this checkpoint

This checkpoint implements the Project Foundation only. It does not implement a
FocusSession, Energy, Workshop progression, persistence, display modes, activity
tracking, or production visuals.

## Toolchain decision

| Tool | Pinned version | Location |
| --- | --- | --- |
| Godot .NET | 4.7.2 | `DeskTown.csproj`, CI |
| .NET SDK | 8.0.425 | `global.json`, CI |
| Target framework | net8.0 | `Directory.Build.props` |
| Test framework | xUnit 2.9.2 | `Directory.Packages.props` |

Godot 4.7.2 is the stable engine baseline. `net8.0` is retained as the documented
Godot baseline for this first build. Because .NET 8 reaches end of support on
2026-11-10, a clean .NET 10 LTS build/import/export spike is required before M2.

## Implemented

- Solution and project boundaries:
  - `DeskTown.Domain` → no project dependency
  - `DeskTown.Application` → Domain only
  - `DeskTown.Platform.Windows` → Application and Domain
  - `DeskTown` Godot host → Application and Windows Platform
  - `DeskTown.Foundation.Tests` → Application and Domain
- `project.godot` and `AppRoot.tscn`
- 1280×720 Compatibility-renderer shell capped at 30 FPS
- native subwindow embedding disabled for later Companion/Ghost windows
- central prototype thresholds: 60s short idle, 180s long idle, 1s logical tick,
  15s checkpoint, 30 FPS maximum
- structured logging boundary that rejects prohibited privacy field names
- architecture, configuration, and privacy policy tests
- Windows export preset
- CI jobs for .NET build/test and Godot headless Windows export
- repository-only validation script

## Validation state

The current agent environment does not provide a .NET SDK or Godot binary, and
network policy prevents downloading them directly. Therefore:

- repository structure validation can run locally;
- C# compile/test and Godot import/export must be confirmed by GitHub Actions;
- Windows `.exe` launch remains the manual part of DT-E0-001 acceptance.

Tasks DT-E0-001 through DT-E0-004 remain `IN REVIEW` until those checks pass.

## Next task after Foundation acceptance

`DT-E1-001 — Create FocusSession domain model` is the next implementation task.
It must remain independent of Godot, Win32, DisplayMode, and persistence DTOs.

