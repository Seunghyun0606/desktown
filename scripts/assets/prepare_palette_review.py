"""Compare shared 24/32-color review palettes without changing approved source PNGs."""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, PngImagePlugin, __version__


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def colors(im):
    return Counter(p[:3] for p in im.get_flattened_data() if p[3])


def balanced_sample(images):
    # Equal 4096-pixel budget per asset, deterministic largest-remainder counts.
    # Prevents large repeated ground or long sheets from setting all colors.
    sample_pixels = []
    for im in images.values():
        counts = colors(im)
        total = sum(counts.values())
        allocation = {c: (n * 4096) // total for c, n in counts.items()}
        priority = sorted(counts, key=lambda c: (-((counts[c] * 4096) % total), c))
        for c in priority[:4096 - sum(allocation.values())]:
            allocation[c] += 1
        for c in sorted(counts):
            sample_pixels.extend([c] * allocation[c])
    sample = Image.new("RGB", (len(sample_pixels), 1))
    sample.putdata(sample_pixels)
    return sample


def remap(im, palette):
    mapping = {c: min(palette, key=lambda p: (2*(c[0]-p[0])**2 +
               4*(c[1]-p[1])**2 + 3*(c[2]-p[2])**2, p)) for c in colors(im)}
    result = Image.new("RGBA", im.size)
    pixels = list(im.get_flattened_data())
    result.putdata([(*mapping[p[:3]], p[3]) if p[3] else (0, 0, 0, 0) for p in pixels])
    visible = [p for p in pixels if p[3]]
    changes = sum(mapping[p[:3]] != p[:3] for p in visible)
    error = sum(sum((p[c] - mapping[p[:3]][c])**2 for c in range(3)) for p in visible)
    return result, {"changedOpaquePixelsPercent": round(100*changes/len(visible), 2),
                    "rgbRmsPerChannel": round((error/(len(visible)*3))**.5, 3),
                    "mapping": [{"from": c, "to": mapping[c]} for c in sorted(mapping)]}


def scene(images, state, smoke=None, spark=None):
    result = Image.new("RGBA", (384, 288))
    for y in range(0, 288, 32):
        for x in range(0, 384, 32):
            result.paste(images["grass"], (x, y))
    for x in range(0, 384, 32):
        result.alpha_composite(images["path"], (x, 192))
    for x in (96, 256):
        for y in range(160, 224, 32):
            result.alpha_composite(images["path"].transpose(Image.Transpose.ROTATE_90), (x, y))
    result.alpha_composite(images["house"], (48, 32))
    result.alpha_composite(images[state], (192, 32))
    for pos in ((16, 144), (160, 144), (336, 144), (32, 224), (304, 224)):
        result.alpha_composite(images["bush"], pos)
    result.alpha_composite(images["neutral"], (168, 174))
    if smoke is not None:
        result.alpha_composite(smoke, (310, 31))
    if spark is not None:
        for pos in ((229, 106), (281, 98)):
            result.alpha_composite(spark, pos)
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("art_root", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    paths = {n: args.art_root / "town-cluster-v1/prepared-v3" / (n + "-1x.png")
             for n in ("house", "grass", "path", "bush")}
    paths.update({n: args.art_root / "workshop-v1/prepared-v2" / ("workshop-" + n + "-1x.png")
                  for n in ("broken", "complete")})
    paths["neutral"] = args.art_root / "neutral-48-v1/mina-neutral-48-v1-1x.png"
    paths.update({n: args.art_root / "idle-work-v1/prepared-v3" / ("chr_mina_" + n + ".png")
                  for n in ("idle", "work")})
    effect_paths = {n: args.art_root / "reward-v1/prepared-v2" / (n + "-1x.png")
                    for n in ("smoke", "spark")}
    all_paths = {**paths, **effect_paths}
    hashes = {n: digest(p) for n, p in all_paths.items()}
    images = {n: Image.open(p).convert("RGBA") for n, p in paths.items()}
    effects = {n: Image.open(p).convert("RGBA") for n, p in effect_paths.items()}
    assert all(set(im.getchannel("A").get_flattened_data()) <= {0, 255} for im in images.values())
    sample = balanced_sample(images)
    args.output.mkdir(parents=True, exist_ok=False)
    srgb = PngImagePlugin.PngInfo()
    srgb.add(b"sRGB", b"\x00")
    outputs, modes, preview = [], {}, {}

    def save(im, relative):
        target = args.output / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        im.save(target, pnginfo=srgb)
        with Image.open(target) as check:
            assert check.mode == im.mode and check.size == im.size
            assert check.tobytes() == im.tobytes() and check.info.get("srgb") == 0
        outputs.append({"file": relative, "size": im.size, "sha256": digest(target)})

    for mode, limit in (("original", None), ("shared24", 24), ("shared32", 32)):
        if limit:
            palette = sorted(set(sample.quantize(limit, Image.Quantize.MEDIANCUT,
                                 dither=Image.Dither.NONE).convert("RGB").get_flattened_data()))
            assert len(palette) == limit and (0, 0, 0) not in palette
            mapped, stats = {}, {}
            for n, im in images.items():
                mapped[n], stats[n] = remap(im, palette)
                assert mapped[n].getchannel("A").tobytes() == im.getchannel("A").tobytes()
                assert mapped[n].size == im.size
                assert all(p[3] or p[:3] == (0, 0, 0) for p in mapped[n].get_flattened_data())
            assert set(c for im in mapped.values() for c in colors(im)) <= set(palette)
        else:
            mapped, stats = images, {}
            palette = sorted({c for im in images.values() for c in colors(im)})
        modes[mode] = {"coreColorCount": len(palette), "paletteRGB": palette,
                       "paletteHex": ["#%02x%02x%02x" % c for c in palette], "assets": stats}
        modes[mode]["usedCoreColorCount"] = len({c for im in mapped.values() for c in colors(im)})
        modes[mode]["distinctClipFrames"] = {}
        for clip, count in (("idle", 4), ("work", 6)):
            distinct = len({mapped[clip].crop((x*48, 0, (x+1)*48, 48)).tobytes() for x in range(count)})
            assert distinct == count
            modes[mode]["distinctClipFrames"][clip] = distinct
        if limit:
            swatches = Image.new("RGB", (640, ((limit+7)//8)*64), "#f3efe7")
            draw = ImageDraw.Draw(swatches)
            for i, color in enumerate(palette):
                x, y = (i % 8)*80, (i // 8)*64
                draw.rectangle((x+4, y+4, x+75, y+39), fill=color)
                draw.text((x+4, y+43), "#%02x%02x%02x" % color, fill="#342c27")
            save(swatches, mode + "/swatches.png")
            (args.output / mode / "palette.json").write_text(json.dumps({
                "status": "candidate_pending_human_approval", "colorsRGB": palette,
                "colorsHex": modes[mode]["paletteHex"], "effectRampIncluded": False
            }, indent=2)+"\n", encoding="utf-8")
        for n, im in mapped.items():
            save(im, mode + "/" + n + "-1x.png")
        frames = {"before": scene(mapped, "broken"), "beat": scene(mapped, "complete", **effects),
                  "calm": scene(mapped, "complete", smoke=effects["smoke"]),
                  "reduced": scene(mapped, "complete")}
        if mode == "original":
            for n, im in frames.items():
                reference = Image.open(args.art_root / "reward-v1/prepared-v2" / (n + "-1x.png"))
                assert im.tobytes() == reference.tobytes()
        preview[mode] = frames["beat"]
        for n, im in frames.items():
            for scale in (1, 2, 3):
                enlarged = im.resize((im.width*scale, im.height*scale), Image.Resampling.NEAREST)
                assert enlarged.resize(im.size, Image.Resampling.NEAREST).tobytes() == im.tobytes()
                save(enlarged, mode + "/" + n + f"-{scale}x.png")
        # Unknown-desktop color study: same neutral pixels, 100% and 40% alpha.
        backdrops = Image.new("RGB", (432, 256))
        draw = ImageDraw.Draw(backdrops)
        for col, bg in enumerate(("#ffffff", "#808080", "#20252b")):
            draw.rectangle((col*144, 0, col*144+143, 255), fill=bg)
            for row, opacity in enumerate((1, .4)):
                sprite = mapped["neutral"].copy()
                sprite.putalpha(sprite.getchannel("A").point(lambda a: round(a*opacity)))
                sprite = sprite.resize((96, 96), Image.Resampling.NEAREST)
                backdrops.paste(sprite, (col*144+24, row*128+24), sprite)
        save(backdrops, mode + "/mina-backgrounds.png")
    board = Image.new("RGB", (1200, 324), "#f3efe7")
    draw = ImageDraw.Draw(board)
    for index, (mode, frame) in enumerate(preview.items()):
        draw.text((8+400*index, 8), mode + " | core " + str(modes[mode]["coreColorCount"]), fill="#342c27")
        board.paste(frame, (8+400*index, 28))
    save(board, "comparison-board.png")
    effect_colors = sorted({c for im in effects.values() for c in colors(im)})
    assert all(digest(p) == hashes[n] for n, p in all_paths.items())
    report = {"status": "comparison_pending_human_palette_choice", "tool": "Python/Pillow",
              "pillowVersion": __version__, "sources": {
                  n: {"file": p.as_posix(), "sha256": hashes[n]} for n, p in all_paths.items()},
              "method": "Equal 4096 samples per 9 asset types, largest-remainder allocation, "
                        "median cut without dithering; nearest RGB with weights R2/G4/B3. "
                        "No spatial edits. Effects remain unchanged as a separate ramp.",
              "modes": modes, "effectPaletteRGB": effect_colors, "effectColorCount": len(effect_colors),
              "checks": {"dimensionsAndAlphaMasksPreserved": True, "transparentRGBZero": True,
                         "corePaletteLimits": True, "noPureBlackCore": True, "srgb": True,
                         "sourcePreserved": True, "originalScenesMatchApprovedReward": True,
                         "distinctIdleWorkFramesPreserved": True},
              "limits": "Candidates only, not a final global palette, assigned assets or native "
                        "Ghost QA. New sprites may require palette revisions. Color distances "
                        "and matching masks do not prove perceptual readability.", "files": outputs}
    (args.output / "preparation.json").write_text(json.dumps(report, indent=2)+"\n", encoding="utf-8")
    print(json.dumps({"output": str(args.output), "coreColors": {m: v["coreColorCount"] for m, v in modes.items()},
                      "separateEffectColors": len(effect_colors), "pngCount": len(outputs)}))


if __name__ == "__main__":
    main()
