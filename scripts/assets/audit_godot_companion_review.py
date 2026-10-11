"""Check actual Godot capture pixels against approved sheets (requires Pillow)."""
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image
from prepare_godot_companion_review import ART, REVIEWS

CLIPS = {"idle": ("CHR-MIN-001", 4), "walk": ("CHR-MIN-002", 6),
         "work": ("CHR-MIN-003", 6), "rest": ("CHR-MIN-004", 4),
         "stretch": ("CHR-MIN-005", 4), "celebrate": ("CHR-MIN-006", 6)}


def audit(review):
    preparation = json.loads((review / "preparation.json").read_text(encoding="utf-8"))
    result = json.loads((review / "captures/result.json").read_text(encoding="utf-8"))
    assert result["renderer"] != "dummy", "These must be real renderer captures"
    hashes = {}
    pixel_count = 0
    for asset_id, source in preparation["sources"].items():
        data = (review / "project" / source["source"]).read_bytes()
        assert hashlib.sha256(data).hexdigest() == source["sha256"], asset_id
    for clip, (asset_id, frames) in CLIPS.items():
        sheet = Image.open(review / "project" / REVIEWS / ART[asset_id]).convert("RGBA")
        for percent in (75, 100, 125, 150):
            for frame in range(frames):
                name = f"{clip}-{percent}-f{frame:02}.png"
                path = review / "captures" / name
                image = Image.open(path).convert("RGBA")
                assert image.size == (360 * percent // 100, 200 * percent // 100), name
                assert len(image.getcolors(image.width * image.height)) > 32, "Blank or missing UI capture"
                hashes[name] = hashlib.sha256(path.read_bytes()).hexdigest()
                if percent == 100:
                    source = sheet.crop((frame * 48, 0, (frame + 1) * 48, 48))
                    for y in range(48):
                        for x in range(48):
                            expected = source.getpixel((x, y))
                            if expected[3] == 255:
                                assert image.getpixel((100 + x, 103 + y)) == expected, (name, x, y)
                                pixel_count += 1
                    if clip in ("work", "rest"):
                        reference = Image.open(review / "project" / REVIEWS / "companion-contact-v1/prepared-v1"
                                               / f"{clip}-proposal-f{frame:02}.png").convert("RGBA")
                        # Contact pixels, including shadow, stool, bench, floor and Mina.
                        assert image.crop((94, 106, 179, 159)).tobytes() == reference.crop((94, 106, 179, 159)).tobytes(), name
                        # Reference uses lamp ON and a brown mock plant. The real stage
                        # defaults to lamp OFF and retains its green unassigned plant.
                        for y in range(35, 170):
                            for x in range(360):
                                if (282 <= x < 314 and 26 <= y < 74) or (55 <= x < 74 and 126 <= y < 153):
                                    continue
                                assert image.getpixel((x, y)) == reference.getpixel((x, y)), (name, x, y)
        for percent in (75, 100, 125, 150):
            clip_hashes = [hashes[f"{clip}-{percent}-f{frame:02}.png"] for frame in range(frames)]
            assert len(set(clip_hashes)) == frames, f"Frozen or repeated frames: {clip} {percent}"
    for percent in (75, 100, 125, 150):
        name = f"lamp-on-{percent}.png"
        image = Image.open(review / "captures" / name)
        assert image.size == (360 * percent // 100, 200 * percent // 100), name
        assert image.tobytes() != Image.open(review / "captures" / f"celebrate-{percent}-f05.png").tobytes(), name
        hashes[name] = hashlib.sha256((review / "captures" / name).read_bytes()).hexdigest()
    lamp_sheet = Image.open(review / "project" / REVIEWS / ART["CMP-LMP-001"]).convert("RGBA")
    for cell, name in ((0, "celebrate-100-f05.png"), (1, "lamp-on-100.png")):
        rendered = Image.open(review / "captures" / name).convert("RGBA")
        for y in range(48):
            for x in range(32):
                pixel = lamp_sheet.getpixel((cell * 32 + x, y))
                if pixel[3] == 255:
                    assert rendered.getpixel((282 + x, 26 + y)) == pixel, (name, x, y)
    assert set(hashes) == set(result["captures"]) and len(hashes) == 124
    shared_hashes = {}
    if preparation.get("runtimeCatalog"):
        expected_shared = {"town-idle.png": (1280, 720),
                           **{f"ghost-work-{opacity}.png": (220, 180) for opacity in (40, 65, 85)}}
        assert set(result["sharedViews"]) == set(expected_shared)
        for name, size in expected_shared.items():
            path = review / "captures" / name
            assert Image.open(path).size == size, name
            shared_hashes[name] = hashlib.sha256(path.read_bytes()).hexdigest()
    report = {"success": True, "sourceRevision": preparation["sourceRevision"], "engine": result["engine"],
              "renderer": result["renderer"], "captures": len(hashes), "nativeMinaOpaquePixelsMatched": pixel_count,
              "workRestContactCropsMatched": 10, "workRestArtAreasMatchedExceptLampAndPlant": 10,
              "nativeLampCellsMatched": 2, "referenceDifferences": "Lamp OFF vs ON; green stage Plant vs brown mock Plant", "sha256": hashes,
              "sharedViewSha256": shared_hashes,
              "limits": "Pixel/render verification only; no native input/DPI, exact tool event or final delivery approval."}
    (review / "audit.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"PASS: {len(hashes)} captures; {pixel_count} native Mina opaque pixels; 10 exact contact crops")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("review", type=Path)
    audit(parser.parse_args().review.resolve())
