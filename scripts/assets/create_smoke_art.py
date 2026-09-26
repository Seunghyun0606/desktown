#!/usr/bin/env python3
"""Create deterministic synthetic art in a disposable CI checkout after export."""

import json
import struct
import zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CATALOG = ROOT / "assets/catalogs/asset_catalog.json"
IDS = {"CHR-MIN-001", "BLD-HOU-001", "BLD-WRK-001", "CMP-WIN-001"}


def chunk(kind, payload):
    return struct.pack(">I", len(payload)) + kind + payload + struct.pack(">I", zlib.crc32(kind + payload))


def png(width, height, frame_width):
    row = bytearray()
    for x in range(width):
        # Each frame has a distinct color so a region mix-up can be seen in CI captures.
        row.extend(((x // frame_width * 75 + 45) % 256, 110, 175, 255))
    scanlines = b"".join(b"\x00" + row for _ in range(height))
    return (b"\x89PNG\r\n\x1a\n"
            + chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0))
            + chunk(b"IDAT", zlib.compress(scanlines)) + chunk(b"IEND", b""))


def main():
    catalog = json.loads(CATALOG.read_text(encoding="utf-8"))
    for row in catalog["assets"]:
        if row["id"] not in IDS:
            continue
        path = ROOT / "assets/art" / row["filename"]
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(png(*row["size"], row["frameSize"][0]))
        row["status"] = "production"
        row["resource"] = "res://assets/art/" + row["filename"]
        row["pivot"] = row["pivot"] or [row["frameSize"][0] // 2, row["frameSize"][1]]
        row["provenance"] = {
            "creator": "DeskTown CI synthetic fixture", "license": "test-only",
            "source": "scripts/assets/create_smoke_art.py", "importDate": "2026-09-26",
            "manifestRevision": "v0.1"
        }
    CATALOG.write_text(json.dumps(catalog, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print("Synthetic art assigned in disposable checkout; never ship these PNGs.")


if __name__ == "__main__":
    main()
