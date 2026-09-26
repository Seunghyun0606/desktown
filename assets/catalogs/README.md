# DeskTown Asset Catalog

`asset_catalog.json` is the v0.1 visual/audio delivery contract. Its 46 IDs and
dimensions match `docs/art/ASSET_MANIFEST.md`; the `key` field is a stable
semantic name. All entries are initially `unassigned`: the running prototype
continues to use geometric presentation until approved assets are supplied.

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
Each binder uses the declared frame size and count, and falls back to geometric
drawing if the file is absent or has the wrong sheet dimensions. The Workshop
sheet cells are Broken, Repairing, Complete, in that order. Lamp and Decor use
cell 0 until their visual variants are approved. Noah/Rumi walking sheets and
the remaining environment/effect/tool IDs still need scene decisions.

Assigned prop art is shown at its native frame size, centered horizontally and
aligned to the bottom of its geometric scene rectangle; the geometric label is
hidden only when that asset loads. Check the placement and legibility against
the three demo states and at each Companion scale before accepting art. These
binders draw only and do not add controls or alter the Focus reward or save.
