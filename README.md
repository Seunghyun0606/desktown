# DeskTown

DeskTown is a Windows-first cozy desktop companion game. A real focus session
moves Mina and a small town forward without turning activity tracking into
employee monitoring or a productivity score.

This repository currently contains the implementation-ready design for
Prototype v0.1. Feature implementation has intentionally not started.

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

## Fixed scope

- Windows 11, Godot 4.x with C#
- Cozy 2D prototype with Mina, Noah, Rumi, one Workshop project, and one discovery
- Local-only persistence
- No AI NPCs, economy, inventory, cloud, accounts, multiplayer, mobile, or macOS

