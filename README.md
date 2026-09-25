# DeskTown

DeskTown is a Windows-first cozy desktop companion game. A real focus session
moves Mina and a small town forward without turning activity tracking into
employee monitoring or a productivity score.

This repository contains the Prototype v0.1 design and a playable placeholder
vertical slice. Exported Windows behavior and final art still require review.

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
- [Mina logical state contract](docs/implementation/DT-E2-002_MINA_STATE_MACHINE.md)
- [V1 JSON save contract](docs/implementation/DT-E8-001_JSON_SAVE_CONTRACT.md)
- [Town logical simulation](docs/implementation/DT-E2-003_TOWN_SIMULATION.md)
- [Atomic save and recovery](docs/implementation/DT-E8-002_ATOMIC_SAVE.md)
- [Workshop and Railway event contract](docs/implementation/DT-E2-004_EVENT_SYSTEM.md)
- [Checkpoint and restart recovery contract](docs/implementation/DT-E8-003_LIFECYCLE_RECOVERY.md)
- [Focus UI and runtime integration](docs/implementation/FOCUS_UI_RUNTIME_SLICE.md)
- [Reward and Ghost Windows handoff](docs/implementation/REWARD_GHOST_HANDOFF.md)
- [Exported Windows gate and result sheet](docs/qa/WINDOWS_GATE.md)
- [Packaging, art integration and user test follow-up](docs/implementation/NEXT_WORK_AND_USER_GATE.md)

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

Open `project.godot` with Godot 4.7.2 .NET and run the main scene. A new save
opens First Launch, then the placeholder Town and Focus Setup. Start a Focus
session in Companion or Hidden; the Tray can reopen the Town or end Focus.
The F10 Ghost preview is for isolated QA outside a session. Ghost remains
unavailable in Focus Setup and the Tray until exported Windows input testing.

On Bash-compatible environments, the repository-only validation does not need
Godot or .NET:

```bash
bash scripts/validate-project-structure.sh
```

## Current implementation boundary

The repository currently provides:

- a six-project C# solution with one-way dependency boundaries;
- a Godot AppRoot, Town/Focus Setup, Companion/Hidden presentation, and Windows export preset;
- typed prototype configuration and privacy-safe structured logging;
- a display-independent FocusSession, privacy-minimal Windows activity tracking,
  process catalog, and elapsed-time Focus Energy;
- deterministic Mina/Town simulation and replay-safe Workshop/Railway events;
- versioned atomic JSON save, checkpoints, backup recovery, and restart prompts;
- passive completion notice, on-demand Workshop reveal and Railway teaser;
- an isolated native Windows Ghost QA adapter and saved preview placement/opacity;
- CI for build, tests, headless main-scene launch, Windows export, and exported
  `.exe` save/restart/recovery smoke in an isolated temporary directory.

The exported `.exe` still needs Windows 11 QA for focus/input behavior, Tray,
sleep/crash recovery, mixed-DPI placement, and the Ghost click-through matrix.
Passing CI export does not certify those interactions. Visuals are placeholders;
the custom Mina, Workshop and core prop assets and funding-build polish remain
open in [the backlog](docs/BACKLOG.md).
