# DeskTown

DeskTown is a Windows-first cozy desktop companion game. A real focus session
moves Mina and a small town forward without turning activity tracking into
employee monitoring or a productivity score.

This repository contains the implementation-ready design and the M0 foundation
for Prototype v0.1. Gameplay feature implementation has intentionally not
started beyond the walking-skeleton bootstrap.

## Prototype invariant

`Companion`, `Hidden`, and `Ghost` are display preferences over one shared
focus session. They never change Focus Energy, project progress, events, or
rewards.

## Document map

- [UX flow](docs/design/UX_FLOW.md)
- [Screen wireframes](docs/design/WIREFRAMES.md)
- [Art bible](docs/art/ART_BIBLE.md)
- [Asset manifest](docs/art/ASSET_MANIFEST.md)
- [Sprite contract](docs/art/SPRITE_CONTRACT.md)
- [Technical architecture](docs/architecture/ARCHITECTURE.md)
- [Godot scene structure](docs/architecture/GODOT_SCENE_STRUCTURE.md)
- [Windows integration](docs/architecture/WINDOWS_INTEGRATION.md)
- [Persistence](docs/architecture/PERSISTENCE.md)
- [Test strategy](docs/architecture/TEST_STRATEGY.md)
- [Implementation plan](docs/IMPLEMENTATION_PLAN.md)
- [Development backlog](docs/BACKLOG.md)
- [M0 foundation status](docs/implementation/M0_FOUNDATION_STATUS.md)
- [FocusSession domain contract](docs/implementation/DT-E1-001_FOCUS_SESSION.md)
- [FocusSessionManager contract](docs/implementation/DT-E1-002_FOCUS_SESSION_MANAGER.md)
- [Windows activity tracker contract](docs/implementation/DT-E1-003_WINDOWS_ACTIVITY_TRACKER.md)
- [Session ledger and Energy contract](docs/implementation/DT-E1-004_SESSION_LEDGER_ENERGY.md)
- [Focus orchestration contract](docs/implementation/DT-E1-005_FOCUS_ORCHESTRATION.md)
- [Workshop progression contract](docs/implementation/DT-E2-001_PROJECT_SYSTEM.md)

## Fixed scope

- Windows 11, Godot 4.x with C#
- Cozy 2D prototype with Mina, Noah, Rumi, one Workshop project, and one discovery
- Local-only persistence
- No AI NPCs, economy, inventory, cloud, accounts, multiplayer, mobile, or macOS

## Development prerequisites

- Windows 11 x64 for the product/manual platform gates
- [Godot 4.7.2 .NET](https://godotengine.org/download/archive/4.7.2-stable/)
- .NET SDK 8.0.425 x64

The engine and SDK are pinned in `DeskTown.csproj` and `global.json`. Use the
.NET-enabled Godot editor; the standard editor cannot build C# scripts.

## Quick start

```powershell
git clone https://github.com/Seunghyun0606/desktown.git
cd desktown
dotnet restore DeskTown.sln
dotnet build DeskTown.sln
dotnet test DeskTown.sln
```

Open `project.godot` with Godot 4.7.2 .NET and run the main scene. The current
visible result is a deliberately plain `DeskTown / Foundation ready` shell.

On Bash-compatible environments, the repository-only validation does not need
Godot or .NET:

```bash
bash scripts/validate-project-structure.sh
```

## Current implementation boundary

The repository currently provides:

- a six-project C# solution with one-way dependency boundaries;
- an empty Godot AppRoot and Windows export preset;
- typed prototype configuration and privacy-safe structured logging;
- a display-independent FocusSession aggregate and transition tests;
- a monotonic-time FocusSessionManager with typed lifecycle events;
- a privacy-minimal Windows foreground/idle activity adapter;
- an idempotent session ledger and elapsed-time Focus Energy policy;
- a privacy-minimal process catalog and Focus command coordinator;
- deterministic, replay-safe Restore Workshop progression;
- xUnit foundation/architecture tests;
- CI for build, tests, headless import, and Windows export.

Mina/Town simulation, display modes, persistence, Town presentation, and
production assets remain subsequent backlog work. The current Godot scene is
still the Foundation shell; these domain/application features have not yet
been wired into a playable UI.
