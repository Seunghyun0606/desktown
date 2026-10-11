"""Prepare review-only Reward keyframes; preserve approved art and refuse output reuse."""
import argparse
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, PngImagePlugin, __version__


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def effect(source, cell, maximum, colors):
    bounds = source.getchannel("A").point(lambda a: 255 if a >= 128 else 0).getbbox()
    if not bounds:
        raise ValueError("Empty effect")
    crop = source.crop(bounds)
    factor = min(maximum[0] / crop.width, maximum[1] / crop.height)
    size = tuple(max(1, round(v * factor)) for v in crop.size)
    small = crop.resize(size, Image.Resampling.NEAREST)
    visible = [p[:3] for p in small.get_flattened_data() if p[3] >= 128]
    if not visible:
        raise ValueError("Effect disappeared at native size")
    sample = Image.new("RGB", (len(visible), 1))
    sample.putdata(visible)
    palette = iter(sample.quantize(colors, Image.Quantize.MEDIANCUT,
                                  dither=Image.Dither.NONE).convert("RGB").get_flattened_data())
    small.putdata([(*next(palette), 255) if p[3] >= 128 else (0, 0, 0, 0)
                   for p in small.get_flattened_data()])
    result = Image.new("RGBA", cell)
    origin = (cell[0] // 2 - size[0] // 2, cell[1] - size[1])
    result.paste(small, origin)
    return result, {"sourceBounds": bounds, "fitSize": size, "origin": origin,
                    "paletteRGB": sorted({p[:3] for p in small.get_flattened_data() if p[3]})}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source_dir", type=Path)
    parser.add_argument("art_root", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    paths = {n: args.source_dir / (n + ".png") for n in ("smoke", "spark")}
    paths.update({n: args.art_root / "town-cluster-v1/prepared-v3" / (n + "-1x.png")
                  for n in ("grass", "path", "house", "bush")})
    paths.update({n: args.art_root / "workshop-v1/prepared-v2" / ("workshop-" + n + "-1x.png")
                  for n in ("broken", "complete")})
    paths["mina"] = args.art_root / "neutral-48-v1/mina-neutral-48-v1-1x.png"
    hashes = {n: digest(p) for n, p in paths.items()}
    images = {n: Image.open(p).convert("RGBA") for n, p in paths.items()}
    smoke, smoke_info = effect(images["smoke"], (32, 32), (24, 28), 4)
    spark, spark_info = effect(images["spark"], (16, 16), (11, 11), 3)
    stem = [x for x in range(32) if smoke.getpixel((x, 31))[3]]
    assert stem
    smoke_origin = (316 - (min(stem) + max(stem)) // 2, 31)
    smoke_info.update(basePixels=stem, chimneyAnchor=[316, 63], sceneOrigin=smoke_origin)
    for im, size, count in ((smoke, (32, 32), 4), (spark, (16, 16), 3)):
        assert im.size == size
        assert all(p[3] in (0, 255) and (p[3] or p[:3] == (0, 0, 0)) for p in im.get_flattened_data())
        assert len({p[:3] for p in im.get_flattened_data() if p[3]}) <= count
    for name, size in (("grass", (32, 32)), ("path", (32, 32)), ("house", (128, 128)),
                       ("bush", (32, 32)), ("broken", (160, 128)),
                       ("complete", (160, 128)), ("mina", (48, 48))):
        assert images[name].size == size

    def scene(state, show_smoke=False, show_spark=False):
        canvas = Image.new("RGBA", (384, 288))
        for y in range(0, 288, 32):
            for x in range(0, 384, 32):
                canvas.paste(images["grass"], (x, y))
        for x in range(0, 384, 32):
            canvas.alpha_composite(images["path"], (x, 192))
        for x in (96, 256):
            for y in range(160, 224, 32):
                canvas.alpha_composite(images["path"].transpose(Image.Transpose.ROTATE_90), (x, y))
        canvas.alpha_composite(images["house"], (48, 32))
        canvas.alpha_composite(images[state], (192, 32))
        for position in ((16, 144), (160, 144), (336, 144), (32, 224), (304, 224)):
            canvas.alpha_composite(images["bush"], position)
        canvas.alpha_composite(images["mina"], (168, 174))
        if show_smoke:
            canvas.alpha_composite(smoke, smoke_origin)
        if show_spark:
            for position in ((229, 106), (281, 98)):
                canvas.alpha_composite(spark, position)
        return canvas

    frames = {"before": scene("broken"), "beat": scene("complete", True, True),
              "calm": scene("complete", True), "reduced": scene("complete")}
    approved_cluster = Image.open(args.art_root / "town-cluster-v1/prepared-v3/cluster-1x.png").convert("RGBA")
    assert frames["reduced"].tobytes() == approved_cluster.tobytes()
    # All composited changes are restricted to the declared local effect rectangles.
    effect_bounds = {"smoke": [smoke_origin[0], 31, smoke_origin[0] + 32, 63],
                     "spark": [[229, 106, 245, 122], [281, 98, 297, 114]]}
    for y in range(288):
        for x in range(384):
            in_effect = any(a <= x < c and b <= y < d
                            for a, b, c, d in [effect_bounds["smoke"], *effect_bounds["spark"]])
            if not in_effect:
                assert frames["beat"].getpixel((x, y)) == frames["reduced"].getpixel((x, y))
    args.output.mkdir(parents=True, exist_ok=False)
    srgb = PngImagePlugin.PngInfo()
    srgb.add(b"sRGB", b"\x00")
    outputs = []

    def save(im, name):
        target = args.output / name
        im.save(target, pnginfo=srgb)
        with Image.open(target) as checked:
            assert checked.size == im.size and checked.mode == im.mode
            assert checked.tobytes() == im.tobytes() and checked.info.get("srgb") == 0
        outputs.append({"file": name, "size": im.size, "sha256": digest(target)})

    for name, im in {**frames, "smoke": smoke, "spark": spark}.items():
        for scale in (1, 2, 3):
            enlarged = im.resize((im.width * scale, im.height * scale), Image.Resampling.NEAREST)
            assert all(enlarged.getpixel((x, y)) == im.getpixel((x // scale, y // scale))
                       for y in range(enlarged.height) for x in range(enlarged.width))
            save(enlarged, f"{name}-{scale}x.png")
    board = Image.new("RGB", (800, 664), "#f3efe7")
    draw = ImageDraw.Draw(board)
    for index, (name, im) in enumerate(frames.items()):
        x, y = 8 + (index % 2) * 400, 36 + (index // 2) * 324
        gray = im.convert("L").convert("RGBA")
        save(gray, name + "-gray-1x.png")
        draw.text((x, y - 24), {"before": "Before / Broken reference (not Repairing)",
                              "beat": "One local effect beat", "calm": "Calm / smoke only",
                              "reduced": "Reduced motion / no particles"}[name], fill="#342c27")
        board.paste(im, (x, y))
    save(board, "comparison-board.png")
    assert all(digest(p) == hashes[n] for n, p in paths.items())
    report = {"status": "style_candidate_pending_human_approval", "tool": "Python/Pillow",
              "pillowVersion": __version__, "sources": {
                  n: {"file": p.as_posix(), "sha256": hashes[n], "size": images[n].size}
                  for n, p in paths.items()}, "smoke": smoke_info, "spark": spark_info,
              "effectBounds": effect_bounds, "resampling": "nearest", "alphaThreshold": 128,
              "checks": {"sizes": True, "binaryAlpha": True, "transparentRGBZero": True,
                         "smallEffectRamps": True, "srgb": True, "nearestEnlargement": True,
                         "sourcePreserved": True, "reducedEqualsApprovedCluster": True,
                         "changesRestrictedToLocalEffects": True},
              "limits": "Single smoke/spark representatives, not 4-frame catalog sheets. "
                        "Broken is a contrast reference, not the runtime Repairing state. "
                        "Neutral Mina, not new Work/Celebrate art. No timing, sound, camera, "
                        "runtime reduced-motion binding, global palette or catalog assignment.",
              "files": outputs}
    (args.output / "preparation.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"output": str(args.output), "checks": report["checks"]}))


if __name__ == "__main__":
    main()
