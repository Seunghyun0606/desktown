#!/usr/bin/env python3
"""Validate the v0.1 asset catalog against the Markdown manifest and delivered files."""

import argparse
import json
import re
import struct
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MANIFEST = ROOT / "docs/art/ASSET_MANIFEST.md"
CATALOG = ROOT / "assets/catalogs/asset_catalog.json"
PREFIX = {"chr": "character", "bld": "building", "env": "environment",
          "fx": "effect", "prop": "prop", "ui": "ui", "item": "item", "snd": "audio"}
ID = re.compile(r"^(CHR|BLD|ENV|FX|CMP|UI|ITM|SND)-[A-Z]{2,3}-\d{3}$")
FILE = re.compile(r"`([a-z0-9_]+\.(?:png|ogg))`")


def manifest_records():
    records = {}
    for line in MANIFEST.read_text(encoding="utf-8").splitlines():
        if not line.startswith("| "):
            continue
        cells = [cell.strip() for cell in line.strip("|").split("|")]
        if len(cells) != 14 or not ID.fullmatch(cells[0]):
            continue
        asset_id, title, category, usage, dimensions, frames, fps, direction, loop, alpha, priority, required, placeholder, replace = cells
        filename = FILE.search(title)
        if not filename or asset_id in records:
            raise ValueError(f"Invalid/duplicate manifest entry: {asset_id}")
        name = filename.group(1)
        prefix, _, remainder = name.partition("_")
        record = {
            "id": asset_id,
            "key": PREFIX[prefix] + "." + remainder.rsplit(".", 1)[0].replace("_", "."),
            "filename": name,
            "kind": "audio" if name.endswith(".ogg") else "visual",
            "frames": int(frames) if frames.isdigit() else None,
            "fps": int(fps) if fps.isdigit() else None,
            "placeholderAllowed": placeholder == "Yes",
            "replaceBeforeFunding": replace == "Yes",
            "priority": priority,
            "alpha": alpha == "Yes" if name.endswith(".png") else None,
        }
        if record["kind"] == "visual":
            width, height = (int(part) for part in dimensions.split("×"))
            record["size"] = [width, height]
            if width % record["frames"]:
                raise ValueError(f"Non-integral frame width: {asset_id}")
            record["frameSize"] = [width // record["frames"], height]
        records[asset_id] = record
    if len(records) != 46 or sum(r["kind"] == "audio" for r in records.values()) != 8:
        raise ValueError("Manifest count differs from the declared 38 visual + 8 audio assets")
    return records


def new_catalog(records):
    return {"manifestRevision": "v0.1", "assets": [
        {**record, "status": "unassigned", "resource": None, "provenance": None,
         "pivot": [24, 44] if record["key"].startswith("character.") else None,
         "events": []}
        for record in records.values()
    ]}


def validate(catalog, records, require_production=False):
    errors = []
    if catalog.get("manifestRevision") != "v0.1":
        errors.append("Catalog manifest revision must be v0.1")
    assets = catalog.get("assets")
    if not isinstance(assets, list):
        return ["Catalog assets must be an array"]
    ids = [row.get("id") for row in assets if isinstance(row, dict)]
    if len(ids) != len(assets) or len(set(ids)) != len(ids) or set(ids) != set(records):
        errors.append("Catalog IDs must match all 46 unique manifest IDs")
    if len({row.get("key") for row in assets if isinstance(row, dict)}) != len(assets):
        errors.append("Catalog semantic keys must be unique")
    for row in assets:
        if not isinstance(row, dict) or row.get("id") not in records:
            errors.append("Unknown or malformed catalog entry")
            continue
        expected = records[row["id"]]
        for field, value in expected.items():
            if row.get(field) != value:
                errors.append(f"{row['id']}: {field} differs from manifest")
        status, resource = row.get("status"), row.get("resource")
        if status not in ("unassigned", "placeholder", "production"):
            errors.append(f"{row['id']}: invalid status")
            continue
        if status == "unassigned":
            if resource is not None or row.get("provenance") is not None:
                errors.append(f"{row['id']}: unassigned asset must have no file/provenance")
            if require_production:
                errors.append(f"{row['id']}: production asset missing")
            continue
        if status == "placeholder" and not expected["placeholderAllowed"]:
            errors.append(f"{row['id']}: placeholder forbidden by manifest")
        if require_production and status != "production":
            errors.append(f"{row['id']}: production asset required")
        if not isinstance(resource, str) or not resource.startswith("res://assets/art/"):
            errors.append(f"{row['id']}: resource must be in res://assets/art/")
            continue
        relative = resource.removeprefix("res://")
        path = ROOT / relative
        if ".." in Path(relative).parts or path.name != expected["filename"]:
            errors.append(f"{row['id']}: unsafe or unexpected runtime filename")
            continue
        if not path.is_file():
            errors.append(f"{row['id']}: resource file missing")
            continue
        provenance = row.get("provenance")
        if not isinstance(provenance, dict) or any(
            not isinstance(provenance.get(field), str) or not provenance[field].strip()
            for field in ("creator", "license", "source", "importDate", "manifestRevision")
        ):
            errors.append(f"{row['id']}: incomplete provenance")
        if expected["kind"] == "visual":
            if not isinstance(row.get("pivot"), list) or len(row["pivot"]) != 2 or not all(
                isinstance(value, int) for value in row["pivot"]
            ):
                errors.append(f"{row['id']}: integer pivot required")
            try:
                with path.open("rb") as image:
                    header = image.read(26)
                if header[:8] != b"\x89PNG\r\n\x1a\n" or header[12:16] != b"IHDR":
                    raise ValueError("not PNG")
                width, height, depth, color = struct.unpack(">IIBB", header[16:26])
                if ([width, height] != expected["size"] or depth != 8 or
                    color != (6 if expected["alpha"] else 2)):
                    raise ValueError("dimensions or RGB/RGBA8 format differs")
            except (OSError, ValueError, struct.error) as error:
                errors.append(f"{row['id']}: invalid PNG: {error}")
        elif path.open("rb").read(4) != b"OggS":
            errors.append(f"{row['id']}: invalid Ogg header")
        if not isinstance(row.get("events"), list):
            errors.append(f"{row['id']}: events must be an array")
    return errors


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--init", action="store_true", help="create the unassigned catalog once")
    parser.add_argument("--require-production", action="store_true")
    args = parser.parse_args()
    records = manifest_records()
    if args.init:
        CATALOG.parent.mkdir(parents=True, exist_ok=True)
        with CATALOG.open("x", encoding="utf-8") as output:
            json.dump(new_catalog(records), output, indent=2, ensure_ascii=False)
            output.write("\n")
    catalog = json.loads(CATALOG.read_text(encoding="utf-8"))
    errors = validate(catalog, records, args.require_production)
    if errors:
        raise SystemExit("\n".join(errors))
    assigned = sum(row["status"] != "unassigned" for row in catalog["assets"])
    print(f"Asset catalog valid: {len(records)} IDs, {assigned} assigned, "
          f"{len(records) - assigned} awaiting art")


if __name__ == "__main__":
    main()
