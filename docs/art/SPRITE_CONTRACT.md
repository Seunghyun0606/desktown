# DeskTown Sprite Contract

## 1. Export format

| Property | Contract |
| --- | --- |
| Format | PNG, RGBA 8-bit, sRGB |
| Filtering | Nearest; texture filter disabled |
| Compression | Lossless |
| Character cell | 48 × 48 px |
| Environment tile | 32 × 32 px |
| UI icon cell | 16 × 16 px |
| Background | Fully transparent unless manifest says opaque |
| Padding | 0 px between fixed-grid frames; 1 px extrude only in atlas build step if required |
| Pivot | Character bottom-center at `(24, 44)` unless an approved clip override exists |
| Coordinates | Whole logical pixels only |
| Direction | Author faces right; left uses horizontal flip |

Transparent pixels must have clean RGB edge colors to prevent dark/white matte
fringes. No semi-transparent antialiasing is allowed on core pixel silhouettes;
effects may use controlled alpha.

## 2. Sheet layout

One animation per horizontal sheet:

```text
chr_mina_work.png
┌────48────┬────48────┬────48────┬────48────┬────48────┬────48────┐
│ frame 00 │ frame 01 │ frame 02 │ frame 03 │ frame 04 │ frame 05 │ 48
└──────────┴──────────┴──────────┴──────────┴──────────┴──────────┘
```

Frame numbering starts at 00 in source metadata. PNG filenames do not include
frame numbers when delivered as a sheet. Source-layer exports may use
`chr_mina_work_f00.png` before packing.

## 3. Animation contracts

| Character | Clip | Frames | FPS | Loop | Root movement | Notes |
| --- | --- | ---: | ---: | --- | --- | --- |
| Mina | idle | 4 | 2 | Yes | No | calm breathing/blink |
| Mina | walk | 6 | 8 | Yes | No | movement comes from simulation |
| Mina | work | 6 | 6 | Yes | No | hammer/tool contact on consistent frame |
| Mina | rest | 4 | 2 | Yes | No | seated/tea-compatible |
| Mina | stretch | 4 | 4 | No | No | returns to idle/rest |
| Mina | celebrate | 6 | 8 | No | No | short completion beat |
| Noah | idle | 4 | 2 | Yes | No | restrained |
| Noah | walk | 6 | 8 | Yes | No | right-facing master |
| Noah | read | 4 | 2 | Yes | No | book silhouette readable |
| Rumi | idle | 4 | 2 | Yes | No | curious variation |
| Rumi | walk | 6 | 8 | Yes | No | right-facing master |

Mina total is exactly **30 authored frames**. All prototype characters total
**54 authored frames**. Horizontal flip supplies left-facing movement; no
four- or eight-direction production is allowed for v0.1.

## 4. Animation events

Animation events are presentation-only:

- Mina Work: `tool_contact` on the authored contact frame for sound/spark
- Mina Walk: optional `step_left` and `step_right`
- Mina Celebrate: `celebrate_peak`
- Reward reveal completion is **not** driven by an animation event

Missing events may remove sound/particles but must never block logical progress.

## 5. Bounds and alignment

- Feet/contact point stays within one pixel across Idle, Walk, Work, Stretch,
  and Celebrate.
- Rest may use a clip-specific visual offset but retains the same node origin.
- Tool may extend to cell edge but cannot be clipped.
- Character collision is not required; navigation uses a point/agent radius.
- Companion and Ghost use the same Mina source frames and presentation profile,
  not separately redrawn character sheets.

## 6. Building and tile contract

- Static Town objects use bottom-center pivots and include no cast shadow unless
  the manifest declares it baked.
- Workshop states occupy the same canvas, pivot, door position, and ground
  footprint so state swap does not jump.
- Tiles are seamless at 32 px and tested in at least a 3 × 3 repeat.
- Tall objects may exceed one tile but must declare their footprint in metadata.
- Animated environmental sheets use fixed frame cells listed in the manifest.

## 7. Naming convention

```text
<category>_<subject>_<state-or-action>[_<variant>].png
```

Prefixes:

| Prefix | Category | Example |
| --- | --- | --- |
| `chr` | Character | `chr_mina_work.png` |
| `bld` | Building | `bld_workshop_broken.png` |
| `env` | Environment/tile | `env_path_tiles.png` |
| `prop` | Prop | `prop_workbench.png` |
| `fx` | Effect | `fx_work_spark.png` |
| `ui` | UI/icon | `ui_icon_ghost.png` |
| `item` | Discovery item | `item_old_railway_map.png` |
| `snd` | Audio | `snd_project_complete.ogg` |

Use lowercase ASCII and underscores. State names match domain enums where
applicable. Do not include artist, vendor, date, `final`, or resolution suffixes
in runtime filenames.

## 8. Stable asset references

Runtime code references logical IDs such as `character.mina.work`, never a PNG
path. A resource catalog maps ID to texture, region, timing, pivot, and optional
events. Placeholder and production catalogs implement the same IDs.

```text
character.mina.work
  → res://assets/catalogs/production/characters/mina_work.tres
  → texture res://assets/art/characters/chr_mina_work.png
```

This allows an external pack or new production sheet to replace art without
changing simulation or scene scripts.

## 9. Delivery checklist

- Exact dimensions and frame count match manifest
- Alpha edges inspected over black, white, and mid-gray
- Nearest-neighbor 1×/2×/3× screenshots included for character review
- No duplicate/blank frames unless intentionally documented
- Pivot and contact points validated by overlay
- Loop seam checked at target FPS
- License/provenance recorded
- Asset ID and production/placeholder status updated

