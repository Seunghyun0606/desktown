# DeskTown Asset Catalog

`asset_catalog.json` is the v0.1 visual/audio delivery contract. Its 46 IDs and
dimensions match `docs/art/ASSET_MANIFEST.md`; the `key` field is a stable
semantic name. The human approved delivery/use and production assignment of
six shared Mina clips and six Companion props on 2026-10-11, followed by eleven
Town building/environment assets and the 2x world layout on 2026-10-12.
Five Noah/Rumi clips were then approved and assigned on 2026-10-12.
The final ten effect/prop/UI/map assets were approved and connected on 2026-10-12.
All 38 visual IDs resolve production art; only 8 audio entries remain unassigned.
See the delivery records for [Mina/Companion](../art/DELIVERY_2026-10-11.md) and
[Town and NPCs](../art/DELIVERY_2026-10-12.md).

To assign an asset, add the file under `assets/art/`, change its entry to
`placeholder` or `production`, set `resource` to its `res://assets/art/...`
path, and fill `provenance` with `creator`, `license`, `source`, `importDate`, and
`manifestRevision`. Keep `pivot` and `events` explicit. Run:

```bash
python scripts/assets/validate_catalog.py
python -m unittest discover -s scripts/assets -p 'test_*.py'
```

The validator checks exact manifest parity, filename, PNG RGBA/RGB8 dimensions,
frame grid, Ogg header, resource presence, provenance, and stable IDs. It does
not judge animation quality, alpha edges, loop seams, license suitability, or
sound mix; those remain art/human gates. Run `--require-production` only for a
funding candidate; it intentionally fails while assets are unassigned.

Mina views in Town, Companion, and Ghost resolve the same six `CHR-MIN-*` IDs.
Town resolves House, Workshop, Library, Tree, Campfire and Noah/Rumi idle/read
art. Companion resolves its Window, Floor, Lamp, Workbench, Stool and Decor.
Companion also resolves the character shadow through a draw-only Sprite2D;
an unassigned or invalid shadow stays hidden. The human-approved integer
Workbench/Stool/Lamp placement and runtime binding reproduce the approved
[Godot captures](../../docs/art/reviews/2026-10-11/companion-runtime-v1/README.md).
Visual delivery/use approval and all 38 visual runtime assignments are complete.
Each binder uses the declared frame size and count, and falls back to geometric
drawing if the file is absent or has the wrong sheet dimensions. The Workshop
sheet cells are Broken, Repairing, Complete, in that order. Lamp and Decor use
cell 0 for their default variants. Noah/Rumi walking sheets are delivered; the
existing ambient schedule continues to select Idle/Read. Town's static
environment tiles use approved integer positions within its 640x360 world at
2x; the HUD remains at 1x. Missing grass retains the geometric ground/path.

Assigned prop art is shown at its native frame size, centered horizontally and
aligned to the bottom of its geometric scene rectangle; the geometric label is
hidden only when that asset loads. Check the placement and legibility against
the three demo states and at each Companion scale before accepting art. These
binders draw only and do not add controls or alter the Focus reward or save.
