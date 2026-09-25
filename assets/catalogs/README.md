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

Mina views in Town, Companion, and Ghost resolve the same six `CHR-MIN-*` IDs
and fall back to geometric drawing if files are missing. Other scene art still
requires presenter/binder replacement before assigned files can appear.
