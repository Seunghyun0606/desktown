"""Prepare a review-only neutral frame from the approved AI Mina concept.

Requires Pillow 12.3.0. Does not update the runtime asset catalog.
"""

import argparse
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, PngImagePlugin, __version__


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    source_hash = sha256(args.source)
    with Image.open(args.source) as opened:
        source = opened.convert("RGBA")
    source_mask = source.getchannel("A").point(lambda a: 255 if a >= 128 else 0)
    bounds = source_mask.getbbox()
    if bounds is None:
        raise ValueError("Source has no visible character")
    cropped = source.crop(bounds)
    width = round(cropped.width * 40 / cropped.height)
    if not 1 <= width <= 42:
        raise ValueError("Source silhouette cannot fit the required margins")
    small = cropped.resize((width, 40), Image.Resampling.NEAREST)
    samples = list(small.get_flattened_data())
    visible = [pixel[:3] for pixel in samples if pixel[3] >= 128]
    colors = Image.new("RGB", (len(visible), 1))
    colors.putdata(visible)
    limited = iter(colors.quantize(32, Image.Quantize.MEDIANCUT,
                                  dither=Image.Dither.NONE).convert("RGB").get_flattened_data())
    normalized = Image.new("RGBA", small.size)
    normalized.putdata([(*next(limited), 255) if pixel[3] >= 128
                        else (0, 0, 0, 0) for pixel in samples])
    frame = Image.new("RGBA", (48, 48))
    frame.paste(normalized, (24 - width // 2, 5))
    pixels = list(frame.get_flattened_data())
    palette = sorted({pixel[:3] for pixel in pixels if pixel[3]})
    frame_bounds = frame.getchannel("A").getbbox()
    assert frame_bounds and frame_bounds[3] == 45, "Foot contact must be on row 44"
    assert frame_bounds[0] >= 3 and frame_bounds[2] <= 45 and frame_bounds[1] >= 3
    assert {pixel[3] for pixel in pixels} == {0, 255}
    assert len(palette) <= 32
    assert all(pixel[:3] == (0, 0, 0) for pixel in pixels if pixel[3] == 0)
    assert sha256(args.source) == source_hash
    args.output.mkdir(parents=True, exist_ok=False)
    srgb = PngImagePlugin.PngInfo()
    srgb.add(b"sRGB", b"\x00")
    checks = []
    for scale in (1, 2, 3):
        image = frame.resize((48 * scale, 48 * scale), Image.Resampling.NEAREST)
        target = args.output / f"mina-neutral-48-v1-{scale}x.png"
        image.save(target, pnginfo=srgb)
        with Image.open(target) as saved:
            assert saved.mode == "RGBA" and saved.size == image.size
            assert saved.info.get("srgb") == 0
            for y in range(saved.height):
                for x in range(saved.width):
                    assert saved.getpixel((x, y)) == frame.getpixel((x // scale, y // scale))
        checks.append({"file": target.name, "size": list(image.size), "sha256": sha256(target)})
    board = Image.new("RGB", (484, 602), "#f3efe7")
    draw = ImageDraw.Draw(board)
    draw.text((16, 12), "Mina neutral candidate | exact 48x48 source", fill="#342c27")
    for row, (label, bg, fg) in enumerate([
            ("Light", "#ffffff", "#342c27"),
            ("Mid-gray", "#808080", "#ffffff"),
            ("Dark", "#20252b", "#ffffff")]):
        top = 40 + row * 184
        draw.rectangle((12, top, 472, top + 176), fill=bg)
        draw.text((24, top + 8), label, fill=fg)
        for scale, x in ((1, 32), (2, 124), (3, 276)):
            image = frame.resize((48 * scale, 48 * scale), Image.Resampling.NEAREST)
            board.paste(image, (x, top + 160 - image.height), image)
            draw.text((x, top + 162), f"{scale}x ({48 * scale}px)", fill=fg)
    board.save(args.output / "review-board.png", pnginfo=srgb)
    report = {
        "status": "candidate_pending_visual_approval",
        "source": str(args.source.as_posix()), "sourceSha256": source_hash,
        "tool": "Python/Pillow", "pillowVersion": __version__,
        "sourceBoundsAtAlpha128": list(bounds), "targetBounds": list(frame_bounds),
        "fit": {"height": 40, "width": width, "top": 5},
        "resampling": "nearest", "alphaThreshold": 128,
        "alphaValues": [0, 255], "opaqueColorCount": len(palette),
        "paletteRGB": [list(color) for color in palette],
        "pivot": [24, 44], "footContactRow": 44,
        "checks": {"dimensions": True, "binaryAlpha": True, "paletteLimit": True,
                   "transparentRGBZero": True, "footContact": True,
                   "scalePixelReplication": True, "srgbChunk": True,
                   "sourcePreserved": sha256(args.source) == source_hash},
        "files": checks,
        "limits": "Visual identity, hammer readability and animation continuity require review."
    }
    (args.output / "preparation.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"output": str(args.output), "colors": len(palette),
                      "bounds": frame_bounds, "checks": report["checks"]}, indent=2))


if __name__ == "__main__":
    main()
