# DeskTown Prototype v0.1 Asset Manifest

## 1. Field rules

- Dimensions are logical pixels of the delivered file. Multi-frame dimensions
  are `frame width × frame height × frames across` expressed as total sheet size.
- Direction `R+flip` means right-facing art with runtime horizontal flip.
- Priority: P0 blocks the prototype thesis, P1 blocks the vertical slice, P2 is
  polish but still part of the stated v0.1 scope.
- `Placeholder` indicates whether a licensed or geometric placeholder can be
  used before funding polish.
- `Replace` indicates whether that placeholder/draft must be replaced before the
  funding build.

## 2. Visual assets

| Asset ID | Asset name / runtime file | Category | Usage scene | Dimensions | Frames | FPS | Direction | Loop | Alpha | Priority | Required | Placeholder | Replace |
| --- | --- | --- | --- | --- | ---: | ---: | --- | --- | --- | --- | --- | --- | --- |
| CHR-MIN-001 | Mina Idle / `chr_mina_idle.png` | Character | Town, Companion, Ghost | 192×48 | 4 | 2 | R+flip | Yes | Yes | P0 | Yes | No | No |
| CHR-MIN-002 | Mina Walk / `chr_mina_walk.png` | Character | Town, Companion | 288×48 | 6 | 8 | R+flip | Yes | Yes | P0 | Yes | No | No |
| CHR-MIN-003 | Mina Work / `chr_mina_work.png` | Character | Town, Companion, Ghost | 288×48 | 6 | 6 | R+flip | Yes | Yes | P0 | Yes | No | No |
| CHR-MIN-004 | Mina Rest / `chr_mina_rest.png` | Character | Town, Companion, Ghost | 192×48 | 4 | 2 | R+flip | Yes | Yes | P0 | Yes | No | No |
| CHR-MIN-005 | Mina Stretch / `chr_mina_stretch.png` | Character | Companion, Ghost | 192×48 | 4 | 4 | R+flip | No | Yes | P0 | Yes | No | No |
| CHR-MIN-006 | Mina Celebrate / `chr_mina_celebrate.png` | Character | Town reward, Companion | 288×48 | 6 | 8 | R+flip | No | Yes | P0 | Yes | No | No |
| CHR-NOA-001 | Noah Idle / `chr_noah_idle.png` | Character | Town | 192×48 | 4 | 2 | R+flip | Yes | Yes | P2 | Yes | Yes | Yes |
| CHR-NOA-002 | Noah Walk / `chr_noah_walk.png` | Character | Town | 288×48 | 6 | 8 | R+flip | Yes | Yes | P2 | Yes | Yes | Yes |
| CHR-NOA-003 | Noah Read / `chr_noah_read.png` | Character | Town | 192×48 | 4 | 2 | R+flip | Yes | Yes | P2 | Yes | Yes | Yes |
| CHR-RUM-001 | Rumi Idle / `chr_rumi_idle.png` | Character | Town, Discovery | 192×48 | 4 | 2 | R+flip | Yes | Yes | P1 | Yes | Yes | Yes |
| CHR-RUM-002 | Rumi Walk / `chr_rumi_walk.png` | Character | Town, Discovery | 288×48 | 6 | 8 | R+flip | Yes | Yes | P1 | Yes | Yes | Yes |
| BLD-HOU-001 | House / `bld_house.png` | Building | Town | 128×128 | 1 | — | None | No | Yes | P1 | Yes | Yes | Yes |
| BLD-WRK-001 | Workshop states / `bld_workshop_states.png` | Building | Town, Reward | 480×128 | 3 | — | None | No | Yes | P0 | Yes | No | No |
| BLD-LCK-001 | Locked Library/area / `bld_library_locked.png` | Building | Town | 128×128 | 1 | — | None | No | Yes | P2 | Yes | Yes | Yes |
| ENV-GRA-001 | Grass variants / `env_grass_tiles.png` | Tile | Town | 64×32 | 2 | — | None | No | No | P1 | Yes | Yes | Yes |
| ENV-DIR-001 | Dirt tile / `env_dirt_tile.png` | Tile | Town | 32×32 | 1 | — | None | No | No | P1 | Yes | Yes | Yes |
| ENV-PTH-001 | Path tiles / `env_path_tiles.png` | Tile | Town | 96×32 | 3 | — | None | No | Yes | P1 | Yes | Yes | Yes |
| ENV-TRE-001 | Tree variants / `env_tree_variants.png` | Environment | Town | 128×96 | 2 | — | None | No | Yes | P1 | Yes | Yes | Yes |
| ENV-BUS-001 | Bush variants / `env_bush_variants.png` | Environment | Town | 64×32 | 2 | — | None | No | Yes | P2 | Yes | Yes | Yes |
| ENV-FLW-001 | Flower variants / `env_flower_variants.png` | Environment | Town | 64×32 | 2 | — | None | No | Yes | P2 | Yes | Yes | Yes |
| ENV-FEN-001 | Fence variants / `env_fence_variants.png` | Environment | Town | 64×32 | 2 | — | None | No | Yes | P2 | Yes | Yes | Yes |
| ENV-ROC-001 | Rock variants / `env_rock_variants.png` | Environment | Town | 64×32 | 2 | — | None | No | Yes | P2 | Yes | Yes | Yes |
| FX-SMK-001 | Workshop smoke / `fx_workshop_smoke.png` | Effect | Town, Reward | 128×32 | 4 | 4 | None | Yes | Yes | P1 | Yes | Yes | Yes |
| FX-FIR-001 | Campfire / `fx_campfire.png` | Effect | Town | 128×32 | 4 | 6 | None | Yes | Yes | P2 | Yes | Yes | Yes |
| FX-GRS-001 | Grass/leaf movement / `fx_grass_movement.png` | Effect | Town | 64×32 | 2 | 2 | None | Yes | Yes | P2 | Yes | Yes | Yes |
| FX-SPK-001 | Work spark / `fx_work_spark.png` | Effect | Companion, Ghost, Reward | 64×16 | 4 | 8 | None | No | Yes | P1 | Yes | Yes | Yes |
| FX-TEA-001 | Tea steam / `fx_tea_steam.png` | Effect | Companion, Ghost | 64×16 | 4 | 4 | None | Yes | Yes | P2 | Yes | Yes | Yes |
| CMP-FLR-001 | Companion floor / `prop_companion_floor.png` | Companion prop | Companion | 360×64 | 1 | — | None | No | Yes | P1 | Yes | Yes | Yes |
| CMP-WRK-001 | Workbench / `prop_workbench.png` | Companion prop | Companion, Ghost | 96×64 | 1 | — | None | No | Yes | P0 | Yes | No | No |
| CMP-STL-001 | Stool / `prop_stool.png` | Companion prop | Companion | 32×32 | 1 | — | None | No | Yes | P1 | Yes | Yes | Yes |
| CMP-LMP-001 | Lamp states / `prop_lamp_states.png` | Companion prop | Companion | 64×48 | 2 | — | None | No | Yes | P0 | Yes | No | No |
| CMP-WIN-001 | Small window / `prop_small_window.png` | Companion prop | Companion | 96×80 | 1 | — | None | No | Yes | P0 | Yes | No | No |
| CMP-DEC-001 | Plant/box variants / `prop_companion_decor.png` | Companion prop | Companion | 64×32 | 2 | — | None | No | Yes | P2 | Yes | Yes | Yes |
| CMP-TOL-001 | Mina tool / `prop_mina_tool.png` | Companion prop | Companion, Ghost | 64×32 | 2 | — | R+flip | No | Yes | P0 | Yes | No | No |
| CMP-SHD-001 | Contact shadow / `prop_character_shadow.png` | Companion prop | Town, Companion, Ghost | 32×16 | 1 | — | None | No | Yes | P1 | Yes | Yes | Yes |
| UI-LOG-001 | DeskTown logo / `ui_logo_desktown.png` | UI | First Launch, Funding | 256×96 | 1 | — | None | No | Yes | P1 | Yes | Yes | Yes |
| UI-ICO-001 | Prototype icon set / `ui_icons_core.png` | UI | Setup, Settings, Tray | 144×16 | 9 | — | None | No | Yes | P1 | Yes | Yes | Yes |
| ITM-MAP-001 | Old Railway Map / `item_old_railway_map.png` | Discovery item | Discovery | 96×64 | 1 | — | None | No | Yes | P1 | Yes | No | No |

`UI-ICO-001` cells, in order: Focus, Workshop, Companion, Hidden, Ghost,
Settings, Audio, Project, Discovery.

## 3. Audio assets

| Asset ID | Asset name / runtime file | Category | Usage scene | Dimensions | Frames | FPS | Direction | Loop | Alpha | Priority | Required | Placeholder | Replace |
| --- | --- | --- | --- | --- | ---: | ---: | --- | --- | --- | --- | --- | --- | --- |
| SND-AMB-001 | Town ambient / `snd_town_ambient.ogg` | Audio | Town | 45–90 s | — | — | — | Yes | — | P2 | Yes | Yes | Yes |
| SND-BRD-001 | Bird / `snd_bird.ogg` | Audio | Town | <3 s | — | — | — | No | — | P2 | Yes | Yes | Yes |
| SND-STP-001 | Footstep / `snd_footstep.ogg` | Audio | Town | <1 s | — | — | — | No | — | P2 | Yes | Yes | Yes |
| SND-HMR-001 | Hammer / `snd_hammer.ogg` | Audio | Companion, Ghost, Reward | <1 s | — | — | — | No | — | P1 | Yes | Yes | Yes |
| SND-FIR-001 | Fire / `snd_fire_loop.ogg` | Audio | Town | 8–20 s | — | — | — | Yes | — | P2 | Yes | Yes | Yes |
| SND-UI-001 | UI click / `snd_ui_click.ogg` | Audio | UI | <1 s | — | — | — | No | — | P2 | Yes | Yes | Yes |
| SND-CMP-001 | Project complete / `snd_project_complete.ogg` | Audio | Reward | <2 s | — | — | — | No | — | P1 | Yes | Yes | Yes |
| SND-DSC-001 | Discovery / `snd_discovery.ogg` | Audio | Discovery | <2 s | — | — | — | No | — | P1 | Yes | Yes | Yes |

Audio delivery: Ogg Vorbis, normalized as a coherent set, no clipping, clean
loop points where applicable, and enough headroom for OS/work audio.

## 4. Production totals

| Measure | Total |
| --- | ---: |
| Manifest entries | 46 |
| Visual asset entries | 38 |
| Audio entries | 8 |
| Character animation clips | 11 |
| Animated environment/effect clips | 5 |
| Total animation clips | 16 |
| Mina authored frames | 30 |
| All character authored frames | 54 |
| Static Town environment logical sprites/cells | 16 |
| Building states/sprites | 5 |
| Animated environment/effect frames | 18 |
| Companion prop logical sprites/states | 11 |
| UI logo/icon cells | 10 |

“Environment asset total” can be reported two ways: **16 static Town sprites**
or **21 logical Town environment entries** when 8 static packs, 5 animated
effects, 3 building packs, and 5 major scene-decoration groupings are counted.
The manifest table is authoritative when estimating files and production work.

## 5. Placeholder and custom boundary

**Can start as placeholders:** Noah/Rumi, House, Locked Area, all generic Town
tiles/vegetation, minor Companion props, ambient effects, UI icons, and all
audio during M0–M3.

**Must be custom before testing the funding hypothesis:** Mina's 30 frames,
Workshop three-state sheet, Companion Workbench/Lamp/Window/Tool, Old Railway
Map, logo, project completion sound, and the final warm Town palette pass.

## 6. Catalog and provenance fields

Each imported entry must add a catalog record containing:

- stable Asset ID
- runtime resource path
- placeholder/production status
- source creator/vendor and license
- source URL or delivery reference
- import date and manifest revision
- pivot, frame regions, and animation events
- replacement notes

Scenes bind to the stable Asset ID. Vendor filenames and pack folder structures
must not leak into C# class logic.

