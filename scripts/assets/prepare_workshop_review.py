"""Prepare review-only Workshop Broken/Complete cells. Requires Pillow 12.3.0.

Never assigns runtime catalog assets. Output must be a new directory.
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
    parser.add_argument("source_dir", type=Path)
    parser.add_argument("mina", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    paths = [args.source_dir / f"{state}.png" for state in ("broken", "complete")]
    hashes = [sha256(p) for p in paths]
    sources = [Image.open(p).convert("RGBA") for p in paths]
    if sources[0].size != sources[1].size:
        raise ValueError("Sources must share canvas dimensions; do not independently fit states")
    masks = [im.getchannel("A").point(lambda a: 255 if a >= 128 else 0) for im in sources]
    boxes = [mask.getbbox() for mask in masks]
    if not all(boxes):
        raise ValueError("Blank source")
    crop = (min(b[0] for b in boxes), min(b[1] for b in boxes),
            max(b[2] for b in boxes), max(b[3] for b in boxes))
    factor = min(144 / (crop[2] - crop[0]), 112 / (crop[3] - crop[1]))
    size = (round((crop[2] - crop[0]) * factor), round((crop[3] - crop[1]) * factor))
    offset = (80 - size[0] // 2, 128 - size[1])
    small = [im.crop(crop).resize(size, Image.Resampling.NEAREST) for im in sources]
    visible = [p[:3] for im in small for p in im.get_flattened_data() if p[3] >= 128]
    samples = Image.new("RGB", (len(visible), 1))
    samples.putdata(visible)
    mapped = iter(samples.quantize(32, Image.Quantize.MEDIANCUT,
                                   dither=Image.Dither.NONE).convert("RGB").get_flattened_data())
    frames = []
    for im in small:
        normalized = Image.new("RGBA", size)
        normalized.putdata([(*next(mapped), 255) if p[3] >= 128 else (0, 0, 0, 0)
                            for p in im.get_flattened_data()])
        cell = Image.new("RGBA", (160, 128))
        cell.paste(normalized, offset)
        frames.append(cell)
    # AI editing preserved the broad composition but changed pixels in fixed
    # parts of the facade and extended a loose tool below the foundation.
    # Lock the door/arch and ground band to Complete. Do not independently fit
    # states or shift their structural anchors to compensate for the tool.
    locks = [(65, 78, 94, 125), (0, 120, 160, 128)]
    for box in locks:
        frames[0].paste(frames[1].crop(box), box[:2])
    contact = frames[1].getchannel("A").getbbox()[3]
    shift = 128 - contact
    if shift:
        aligned = []
        for frame in frames:
            moved = Image.new("RGBA", frame.size)
            moved.paste(frame, (0, shift))
            aligned.append(moved)
        frames = aligned
    final_locks = [(l,t+shift,r,min(128,b+shift)) for l,t,r,b in locks]
    assert all(frames[0].crop(b).tobytes() == frames[1].crop(b).tobytes() for b in final_locks)
    assert frames[0].getchannel("A").crop((0,127,160,128)).tobytes() == frames[1].getchannel("A").crop((0,127,160,128)).tobytes()
    assert frames[0].tobytes() != frames[1].tobytes()
    palette = sorted({p[:3] for im in frames for p in im.get_flattened_data() if p[3]})
    assert len(palette) <= 32
    args.output.mkdir(parents=True, exist_ok=False)
    srgb = PngImagePlugin.PngInfo()
    srgb.add(b"sRGB", b"\x00")
    outputs = []

    def save(im, name):
        path = args.output / name
        im.save(path, pnginfo=srgb)
        with Image.open(path) as saved:
            assert saved.size == im.size and saved.info.get("srgb") == 0
            assert saved.tobytes() == im.tobytes()
        outputs.append({"file": name, "size": list(im.size), "sha256": sha256(path)})

    gray_frames = []
    for state, frame in zip(("broken", "complete"), frames):
        pixels = list(frame.get_flattened_data())
        assert {p[3] for p in pixels} == {0, 255}
        assert all(p[:3] == (0, 0, 0) for p in pixels if not p[3])
        gray = frame.convert("L").convert("RGBA")
        gray.putalpha(frame.getchannel("A"))
        gray_frames.append(gray)
        for scale in (1, 2, 3):
            enlarged = frame.resize((160 * scale, 128 * scale), Image.Resampling.NEAREST)
            assert all(enlarged.getpixel((x, y)) == frame.getpixel((x // scale, y // scale))
                       for y in range(enlarged.height) for x in range(enlarged.width))
            save(enlarged, f"workshop-{state}-{scale}x.png")
        save(gray, f"workshop-{state}-gray.png")

    mina = Image.open(args.mina).convert("RGBA")
    assert mina.size == (48, 48)
    board = Image.new("RGB", (704, 656), "#f3efe7")
    draw = ImageDraw.Draw(board)
    draw.text((16, 12), "Workshop style candidate | Broken / Complete | 160x128", fill="#342c27")
    for row, (group, label) in enumerate(((frames, "Color 2x"), (gray_frames, "Grayscale 2x"))):
        top = 48 + row * 300
        draw.text((16, top - 20), label, fill="#342c27")
        for col, frame in enumerate(group):
            x = 16 + col * 352
            draw.rectangle((x, top, x + 319, top + 255), fill="#4c6955")
            im = frame.resize((320, 256), Image.Resampling.NEAREST)
            board.paste(im, (x, top), im)
            draw.text((x, top + 264), ("Broken", "Complete")[col], fill="#342c27")
    save(board, "comparison-board.png")
    # CatalogRectArt uses a native 160x128 frame at host bottom-center:
    # Workshop host (744,272)-(1008,452) => image origin (796,324).
    for state, frame in zip(("broken", "complete"), frames):
        town = Image.new("RGB", (1280, 720), "#4c6955")
        d = ImageDraw.Draw(town)
        for rect, color in [((95,502,1164,547), "#a28e6f"),
                            ((228,278,413,446), "#bf9a77"),
                            ((1015,120,1174,244), "#556259"),
                            ((99,170,159,329), "#2d5242"),
                            ((590,565,614,598), "#eb8f51")]:
            d.rectangle(rect, fill=color)
        town.paste(frame, (796,324), frame)
        town.paste(mina, (680,415), mina)
        d.text((24,22), "Town scene coordinate reference - not a Godot screenshot", fill="#fff9ed")
        save(town, f"town-{state}-1x.png")
    assert [sha256(p) for p in paths] == hashes
    report = {
        "status": "style_candidate_pending_human_approval", "tool": "Python/Pillow",
        "pillowVersion": __version__,
        "sources": [{"file": str(p.as_posix()), "sha256": h, "size": list(im.size)}
                    for p,h,im in zip(paths, hashes, sources)],
        "sourceBounds": boxes, "commonCrop": crop, "resizedSize": size, "offset": offset,
        "identityLocksFromComplete": final_locks, "commonVerticalShift": shift,
        "cell": [160,128], "pivot": [80,128], "resampling": "nearest",
        "alphaThreshold": 128, "paletteRGB": palette, "opaqueColorCount": len(palette),
        "cellBounds": [im.getchannel("A").getbbox() for im in frames],
        "town": {"viewport": [1280,720], "workshopOrigin": [796,324], "minaOrigin": [680,415]},
        "checks": {"dimensions": True, "binaryAlpha": True, "sharedPaletteLimit32": True,
                   "transparentRGBZero": True, "nearestEnlargement": True, "srgb": True,
                   "fixedDoorPixels": True, "fixedFoundationPixels": True,
                   "sameGroundContactRow127": True, "distinctStates": True,
                   "sourcePreserved": True},
        "limits": "Fixed door and foundation pixels do not prove visual approval. "
                  "No Repairing frame, separate smoke, cast shadow, runtime assignment or Windows QA.",
        "files": outputs,
    }
    (args.output / "preparation.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"output": str(args.output), "colors": len(palette), "bounds": report["cellBounds"]}))


if __name__ == "__main__":
    main()
