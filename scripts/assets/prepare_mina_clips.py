"""Extract review-only Mina Idle/Work grids into contract-sized strips.

Requires Pillow 12.3.0. Keeps the approved neutral palette and runtime catalog intact.
"""

import argparse
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, PngImagePlugin, __version__


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def cap_center(image, bounds):
    points = [(x, y) for y in range(bounds[1], bounds[1] + (bounds[3] - bounds[1]) // 2)
              for x in range(bounds[0], bounds[2])
              if (p := image.getpixel((x, y)))[3] >= 128
              and p[0] > 1.65 * p[1] and p[1] > 45 and p[0] > 1.7 * p[2]]
    if not points:
        raise ValueError("Cannot locate approved warm cap for horizontal alignment")
    return (min(x for x, _ in points) + max(x for x, _ in points)) / 2


def color_distance(a, b):
    return sum((a[i] - b[i]) ** 2 for i in range(3))


def frame_from_cell(cell, palette, target_cap_center):
    mask = cell.getchannel("A").point(lambda a: 255 if a >= 128 else 0)
    bounds = mask.getbbox()
    if bounds is None:
        raise ValueError("Blank source pose")
    center = cap_center(cell, bounds)
    cropped = cell.crop(bounds)
    width = round(cropped.width * 40 / cropped.height)
    resized = cropped.resize((width, 40), Image.Resampling.NEAREST)
    normalized = Image.new("RGBA", resized.size)
    normalized.putdata([(*min(palette, key=lambda c: color_distance(pixel, c)), 255)
                        if pixel[3] >= 128 else (0, 0, 0, 0)
                        for pixel in resized.get_flattened_data()])
    scaled_center = (center - bounds[0] + 0.5) * width / cropped.width - 0.5
    x = round(target_cap_center - scaled_center)
    if x < 2 or x + width > 46:
        raise ValueError("Pose/tool cannot fit a 48px cell without clipping")
    frame = Image.new("RGBA", (48, 48))
    frame.paste(normalized, (x, 5))
    return frame, {"sourceBounds": list(bounds), "sourceCapCenter": center,
                   "widthAtHeight40": width, "destination": [x, 5]}


def companion_background():
    # Geometric scene reference only; sourced from CompanionStage.tscn.
    bg = Image.new("RGB", (360, 200), (60, 53, 47))
    draw = ImageDraw.Draw(bg)
    for box, color in [((0, 0, 359, 29), (46, 41, 36)),
                       ((134, 39, 199, 89), (124, 162, 167)),
                       ((0, 154, 359, 199), (93, 73, 59)),
                       ((290, 44, 304, 73), (238, 194, 123)),
                       ((146, 130, 251, 154), (154, 99, 68)),
                       ((273, 142, 297, 155), (141, 94, 70)),
                       ((55, 126, 73, 152), (112, 158, 107))]:
        draw.rectangle(box, fill=color)
    return bg


def lock_identity(frame, neutral, clip, index):
    """Preserve approved head/feet; retain motion from the generated pose grid."""
    if clip == "idle":
        # Only generated chest/near-hand regions change during quiet breathing.
        result = neutral.copy()
        if index:
            for box in ((21, 25, 33, 33), (15, 29, 21, 35)):
                result.paste(frame.crop(box), box[:2])
        if index == 2:
            # The generated blink is a tiny face region, not a new head contour.
            box = (29, 16, 33, 19)
            result.paste(frame.crop(box), box[:2])
        return result
    result = frame.copy()
    # Keep raised hammer pixels to the right of the approved head rectangle.
    for y in range(24):
        for x in range(36):
            approved = neutral.getpixel((x, y))
            if approved[3] or x < 33 or y < 14:
                result.putpixel((x, y), approved)
            # Keep generated raised-tool pixels in the clear space right of the face.
    lower_body = (0, 35, 48, 48)
    result.paste(neutral.crop(lower_body), lower_body[:2])
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("review_root", type=Path)
    parser.add_argument("neutral", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    neutral = Image.open(args.neutral).convert("RGBA")
    if neutral.size != (48, 48):
        raise ValueError("Approved neutral must be 48x48")
    palette = sorted({p[:3] for p in neutral.get_flattened_data() if p[3]})
    assert len(palette) <= 32
    target_center = cap_center(neutral, neutral.getchannel("A").getbbox())
    hashes = {str(args.neutral): digest(args.neutral)}
    clips = {}
    srgb = PngImagePlugin.PngInfo()
    srgb.add(b"sRGB", b"\x00")
    args.output.mkdir(parents=True, exist_ok=False)
    for name, columns, rows, count, fps in [("idle", 2, 2, 4, 2), ("work", 3, 2, 6, 6)]:
        source_path = args.review_root / "sources" / f"{name}-grid.png"
        hashes[str(source_path)] = digest(source_path)
        source = Image.open(source_path).convert("RGBA")
        frames, details = [], []
        for index in range(count):
            col, row = index % columns, index // columns
            box = (col * source.width // columns, row * source.height // rows,
                   (col + 1) * source.width // columns, (row + 1) * source.height // rows)
            frame, info = frame_from_cell(source.crop(box), palette, target_center)
            frame = lock_identity(frame, neutral, name, index)
            pixels = list(frame.get_flattened_data())
            bounds = frame.getchannel("A").getbbox()
            assert bounds and bounds[3] == 45 and bounds[1] >= 3
            assert {p[3] for p in pixels} == {0, 255}
            assert {p[:3] for p in pixels if p[3]} <= set(palette)
            assert all(p[:3] == (0, 0, 0) for p in pixels if p[3] == 0)
            feet = [(x, y) for y in range(40, 45) for x in range(48)
                    if frame.getpixel((x, y))[3]]
            info.update({"index": index, "bounds": list(bounds),
                         "footBoundsX": [min(x for x, _ in feet), max(x for x, _ in feet)],
                         "pixelHash": hashlib.sha256(frame.tobytes()).hexdigest()})
            details.append(info)
            frames.append(frame)
        distinct = len({f.tobytes() for f in frames})
        if distinct != count:
            raise ValueError(f"{name}: only {distinct}/{count} distinct frames")
        sheet = Image.new("RGBA", (48 * count, 48))
        for index, frame in enumerate(frames):
            sheet.paste(frame, (48 * index, 0))
            frame.save(args.output / f"{name}-f{index:02}.png", pnginfo=srgb)
        filename = f"chr_mina_{name}.png"
        sheet.save(args.output / filename, pnginfo=srgb)
        sheet.resize((sheet.width * 3, 144), Image.Resampling.NEAREST).save(
            args.output / f"{name}-strip-3x.png", pnginfo=srgb)
        diffs = [sum(a != b for a, b in zip(frames[i].get_flattened_data(),
                                           frames[(i + 1) % count].get_flattened_data()))
                 for i in range(count)]
        foot_spread = [max(f["footBoundsX"][i] for f in details)
                       - min(f["footBoundsX"][i] for f in details) for i in range(2)]
        assert max(foot_spread) <= 1
        for scale in (1, 2, 3):
            for context, opacity in [("companion", 1), ("ghost-light", 1),
                                     ("ghost-dark", 1), ("ghost-light-40", .4),
                                     ("ghost-dark-40", .4)]:
                previews = []
                for frame in frames:
                    if context == "companion":
                        bg = companion_background()
                        position = (100, 103)
                    else:
                        bg = Image.new("RGB", (240, 160),
                                       "#f3f1ec" if "light" in context else "#20252b")
                        position = (75, 88)
                        draw = ImageDraw.Draw(bg)
                        draw.polygon([(118, 115), (169, 115), (169, 132), (118, 132)],
                                     fill=(149, 98, 72))
                    mask = frame.getchannel("A").point(lambda a: round(a * opacity))
                    bg.paste(frame, position, mask)
                    previews.append(bg.resize((bg.width * scale, bg.height * scale),
                                              Image.Resampling.NEAREST))
                # APNG carries exact 500ms/167ms frame timing; HTML uses PNG strips for 6 FPS.
                previews[0].save(args.output / f"{name}-{context}-{scale}x.png",
                                 save_all=True, append_images=previews[1:],
                                 duration=500 if name == "idle" else 167,
                                 loop=0, disposal=0, blend=0, pnginfo=srgb)
        with Image.open(args.output / filename) as saved:
            assert saved.size == (48 * count, 48) and saved.mode == "RGBA"
            assert saved.info.get("srgb") == 0
            assert saved.tobytes() == sheet.tobytes()
        clips[name] = {"assetId": "CHR-MIN-001" if name == "idle" else "CHR-MIN-003",
                       "file": filename, "sheetSize": [48 * count, 48],
                       "frameSize": [48, 48], "count": count, "fps": fps, "loop": True,
                       "distinctFrames": distinct, "pivot": [24, 44],
                       "footRow": 44, "footEdgeSpreadPixels": foot_spread,
                       "nextFrameChangedPixelsIncludingSeam": diffs,
                       "events": [] if name == "idle" else [{"name": "tool_contact", "frame": 4}],
                       "frames": details, "sha256": digest(args.output / filename)}
    assert all(digest(Path(p)) == value for p, value in hashes.items())
    report = {"status": "style_test_pending_visual_approval", "tool": "Pillow",
              "pillowVersion": __version__, "sourceHashes": hashes,
              "palette": [list(c) for c in palette], "opaqueColorCount": len(palette),
              "alphaValues": [0, 255], "targetCapCenterX": target_center,
              "clips": clips, "sourcePreserved": True,
              "identityLock": {"idle": "neutral with generated chest/hand windows and frame2 blink",
                               "work": "approved head silhouette and lower body; generated arm/tool outside head"},
              "limits": ["Human identity, subtle motion and loop seams are not auto-approved.",
                         "Static scene-background references, not Godot/native-window validation.",
                         "Contact frame is a presentation cue candidate only.",
                         "Fixed scene prop spacing is shown without moving runtime nodes."]}
    (args.output / "preparation.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"clips": {k: {field: v[field] for field in
                                  ("sheetSize", "fps", "distinctFrames", "footEdgeSpreadPixels")}
                                for k, v in clips.items()}, "colors": len(palette)}, indent=2))


if __name__ == "__main__":
    main()
