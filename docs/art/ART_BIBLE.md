# DeskTown Prototype v0.1 Art Bible

## 1. Visual thesis

DeskTown is a warm miniature place that happens to live beside work. It is not
an RPG compressed into a desktop window and not a productivity dashboard with a
character attached.

**Keywords:** cozy, warm, calm, small, handmade, friendly, low-noise, readable.

**Avoid:** heroic fantasy silhouettes, weapons, quest markers, gold/loot cues,
dense HUD chrome, neon gamification, red failure states, hyper-detailed textures,
and animation that competes with the user's real work.

## 2. Visual hierarchy

1. Mina's silhouette and action
2. Workshop state change
3. Warm Town composition
4. Rumi and Noah ambient life
5. Particles and decorative motion
6. UI

When a compromise is required, protect the higher item. Mina must remain
recognizable at 48 × 48 and at 40% Ghost opacity.

## 3. Palette and light

- Warm clay/brown for structures and tools
- Muted moss/olive greens rather than saturated game grass
- Cream window light and pale amber lamp light
- Cool desaturated blue-gray only for shadow separation
- One restrained accent for discoveries, reused consistently
- No pure black outlines; use a dark warm or cool neutral

Target a compact working palette of 24–32 colors for characters and environment,
with shared ramp families. Effects may add a small emissive ramp. Final palette
values are an art-production decision after one Mina + Workshop style test.

## 4. Pixel language

- Character logical cell: 48 × 48 px
- Environment tile: 32 × 32 px
- Nearest-neighbor texture filtering; no subpixel sprite positioning
- One consistent light direction: upper-left/front-left
- Silhouette before interior detail
- Clusters over single-pixel noise; no automatic anti-aliasing
- Character contact shadow is a separate reusable asset

The 48/32 combination is retained. A 48 px character gives enough room for
distinct tool and body poses in the 360 × 200 Companion while 32 px tiles keep
Town construction modular. Their 1.5 ratio is acceptable because scenes place
characters freely rather than snapping character bounds to one tile. Both scale
cleanly at integer multiples.

## 5. Character direction

### Mina — Builder

Mina is the product identity. The silhouette should communicate “small builder”
without relying on tiny facial details.

- Broad, soft shape with one memorable head/ear/hair/hat contour
- One persistent warm accent visible in every animation
- Compact tool silhouette that reads as a hammer, not a weapon
- Work motion is steady and content, not frantic
- Rest poses feel self-directed: tea, sitting, stretching, looking outside
- Celebrate is brief and sincere, not a loot victory pose

Identity locks across all frames:

- head/body proportion
- eye line and face width
- primary accent placement
- hand/foot size
- outline thickness
- tool scale

### Noah

Quieter vertical silhouette. Reading pose is the identity cue. Noah must not
compete with Mina in saturation or animation amplitude.

### Rumi

Curious, open silhouette with a slightly more mobile idle. Rumi owns the railway
discovery beat but remains secondary to Mina.

## 6. Environment direction

Town composition uses breathing room and readable paths. Buildings are small
diorama objects, not large enterable interiors in v0.1.

### Workshop states

The three states must be distinguishable in grayscale silhouette:

- **Broken:** irregular roof/boards, dark window, stopped chimney, scattered tool
- **Repairing:** partial boards/scaffold, one warm work cue, incomplete roofline
- **Complete:** clean roofline, lit window, active chimney smoke, tidy frontage

Do not express state only with particles or color. A screenshot must make the
change obvious.

### Companion Stage

Treat 360 × 200 as a side-view dollhouse corner. The window and lamp provide
light structure; workbench anchors Mina. Props occupy the edges so Mina's
animation bounds stay clear. No meters or floating game icons.

### Ghost Stage

Only Mina, one small work prop, optional contact shadow, and one subtle particle.
The art must survive against unknown light/dark desktop backgrounds. Use a
one-pixel controlled edge/contrast treatment rather than a large glow panel.

## 7. Motion language

- Character animation: 2–8 FPS, stepped pixel motion
- Ambient motion: slow, small amplitude, asynchronously phased
- Work effects: short and local; never continuous screen sparkle
- Reward reveal: one clear anticipation hold, one effect beat, then calm
- Ghost particle count is strictly limited to avoid distraction
- `Reduced motion` disables nonessential ambient/particle motion but preserves
  state communication

## 8. UI direction

- UI is warm neutral paper/wood-toned framing with generous negative space
- Text has priority over decorative borders
- Icons share a 16 × 16 logical grid and can render at 1×/2×
- Avoid RPG frames, rarity colors, badges, streak flames, XP bars, and quest `!`
- Project progress appears as quiet text; Town state carries the emotional reward

## 9. Audio direction

- Soft and close, with low transient harshness
- Hammer sound should feel miniature/wooden rather than metallic construction
- Town ambient has space and does not mask work audio
- Completion cue is under two seconds; discovery cue is distinct but subtle
- All audio obeys one master on/off control in v0.1

## 10. Production gates

Before producing the full set, approve these in order:

1. Mina neutral silhouette at 48 × 48, viewed at 1×/2×/3×
2. Mina Idle and Work against both Companion and Ghost backgrounds
3. Workshop Broken/Complete side-by-side at Town camera scale
4. One 32 px tile cluster with House, path, vegetation
5. Reward reveal color/effect frame

Do not produce Noah/Rumi full sheets until Mina identity and scale pass.

## 11. Placeholder policy

Placeholders may prove layout, state changes, import settings, and animation
timing. They may not be used to judge the central product hypothesis in the
funding build.

Custom production is mandatory for:

- Mina's six animations
- Workshop's three states
- Companion workbench/lamp/window composition
- DeskTown logo and completion/discovery focal art

Environment tiles, secondary NPCs, minor props, UI icons, and early sound can
begin with licensed placeholders if provenance and replacement IDs are tracked.

## 12. External pack compatibility

- Store original source/license beside an asset provenance record, not in code.
- Import through stable Asset IDs; scene scripts never load a vendor filename.
- Replacements must preserve logical bounds, pivot, and animation contract or
  provide a new manifest revision.
- Do not mix packs with conflicting pixel density, outline weight, or light
  direction in the funding build.

