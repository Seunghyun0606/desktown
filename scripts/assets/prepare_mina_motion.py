"""Prepare review-only Mina motion grids; requires Pillow 12.3.0.

Retains AI originals, uses the approved shared palette, and never assigns assets.
"""
import argparse
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, PngImagePlugin, __version__

CLIPS = {
    'walk': (3, 6, 8, True, 'CHR-MIN-002'),
    'rest': (2, 4, 2, True, 'CHR-MIN-004'),
    'stretch': (2, 4, 4, False, 'CHR-MIN-005'),
    'celebrate': (3, 6, 8, False, 'CHR-MIN-006'),
}


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def cap_bounds(im):
    bounds = im.getchannel('A').point(lambda a: 255 if a >= 128 else 0).getbbox()
    if not bounds:
        raise ValueError('Blank source cell')
    points = [(x, y) for y in range(bounds[1], bounds[1] + round((bounds[3]-bounds[1])*.30))
              for x in range(bounds[0], bounds[2])
              if (p := im.getpixel((x, y)))[3] >= 128
              and p[0] > 1.65*p[1] and p[1] > 45 and p[0] > 1.7*p[2]]
    if not points:
        raise ValueError('Cannot locate warm cap')
    return (min(x for x, y in points), min(y for x, y in points),
            max(x for x, y in points)+1, max(y for x, y in points)+1)


def nearest_color(pixel, palette):
    return min(palette, key=lambda c: 2*(pixel[0]-c[0])**2
               + 4*(pixel[1]-c[1])**2 + 3*(pixel[2]-c[2])**2)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('review_root', type=Path)
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    neutral_path = Path('docs/art/reviews/2026-10-11/palette-v1/prepared-v2/shared32/neutral-1x.png')
    palette_path = Path('docs/art/palettes/desktown-v01-32.json')
    neutral = Image.open(neutral_path).convert('RGBA')
    palette = [tuple(c) for c in json.loads(palette_path.read_text(encoding='utf-8'))['colorsRGB']]
    target_cap = cap_bounds(neutral)
    hashes = {p.as_posix(): digest(p) for p in (neutral_path, palette_path,
               args.review_root/'sources'/'prompts.json')}
    retry_prompt = args.review_root/'sources'/'walk-retry-prompt.txt'
    if retry_prompt.exists():
        hashes[retry_prompt.as_posix()] = digest(retry_prompt)
    initial_walk = args.review_root/'sources'/'walk-grid-initial.png'
    if initial_walk.exists():
        hashes[initial_walk.as_posix()] = digest(initial_walk)
    srgb = PngImagePlugin.PngInfo()
    srgb.add(b'sRGB', b'\x00')
    args.output.mkdir(parents=True, exist_ok=False)
    report = {'status': 'pending_human_motion_review', 'pillowVersion': __version__,
              'paletteId': 'desktown-v01-32', 'alphaValues': [0, 255],
              'pivot': [24, 44], 'runtimeAssigned': False, 'clips': {}}
    board = Image.new('RGB', (900, 850), '#eee7dc')
    draw = ImageDraw.Draw(board)
    for row_index, (name, (cols, count, fps, loop, asset_id)) in enumerate(CLIPS.items()):
        path = args.review_root/'sources'/f'{name}-grid.png'
        hashes[path.as_posix()] = digest(path)
        source = Image.open(path).convert('RGBA')
        frames, details = [], []
        for index in range(count):
            col, row = index % cols, index // cols
            # Rest's generated first row extends below the nominal half-height.
            # Split through the inspected transparent gutter, preserving both boots.
            split = 700 if name == 'rest' else source.height//2
            box = (col*source.width//cols, 0 if row == 0 else split,
                   (col+1)*source.width//cols, split if row == 0 else source.height)
            cell = source.crop(box)
            mask = cell.getchannel('A').point(lambda a: 255 if a >= 128 else 0)
            bounds = mask.getbbox()
            cap = cap_bounds(cell)
            crop = cell.crop(bounds)
            scale = (target_cap[2]-target_cap[0])/(cap[2]-cap[0])
            size = (round(crop.width*scale), round(crop.height*scale))
            resized = crop.resize(size, Image.Resampling.NEAREST)
            colors = {}
            normalized = Image.new('RGBA', size)
            pixels = []
            for p in resized.get_flattened_data():
                if p[3] >= 128:
                    if p[:3] not in colors:
                        colors[p[:3]] = nearest_color(p, palette)
                    pixels.append((*colors[p[:3]], 255))
                else:
                    pixels.append((0, 0, 0, 0))
            normalized.putdata(pixels)
            x = round((target_cap[0]+target_cap[2]-1)/2
                      - ((cap[0]+cap[2]-1)/2-bounds[0]) * size[0]/crop.width)
            y = 45-size[1]
            if x < 1 or x+size[0] > 47 or y < 1:
                raise ValueError(f'{name} frame{index} cannot fit at approved head scale: {size}, {(x,y)}')
            frame = Image.new('RGBA', (48, 48))
            frame.paste(normalized, (x, y))
            # Lock approved cap/face identity, without freezing moving arms or Walk legs.
            head_y = y+round((cap[1]-bounds[1])*size[1]/crop.height)
            head_offset = head_y-target_cap[1] if name == 'rest' else 0
            # Remove the whole generated head, including cap pixels outside the
            # reference rectangle; retaining those creates a second floating cap.
            raw = frame.copy()
            frame.paste((0, 0, 0, 0), (0, 0, 48, 24+head_offset))
            frame.paste(neutral.crop((0, 0, 48, 24)), (0, head_offset))
            # Retain the tea cup at the face side and the authored raised hands.
            windows = []
            if name == 'rest':
                windows = [(36, 14+head_offset, 42, 24+head_offset)]
            elif name == 'stretch' and index == 2:
                windows = [(10, 13, 14, 28), (35, 17, 39, 29)]
            elif name == 'celebrate' and index == 3:
                windows = [(12, 15, 17, 28)]
            for window in windows:
                frame.paste(raw.crop(window), window[:2])
            if name in ('stretch', 'celebrate'):
                frame.paste(neutral.crop((0, 35, 48, 48)), (0, 35))
            if name == 'walk':
                # Profile walk silhouettes repeat at opposite leg contacts. Give
                # the far limb a dark ramp and the near limb a light ramp so the
                # second half-cycle reads as the other leg, not the same step.
                for py in range(24, 45):
                    for px in range(48):
                        p = frame.getpixel((px, py))
                        if not p[3]:
                            continue
                        factor = 1
                        if py >= 32 and index >= 3:
                            factor = .72 if px >= 24 else 1.20
                        elif 24 <= py < 32 and (px >= 32 if index < 3 else px <= 19):
                            factor = .80
                        if factor != 1:
                            frame.putpixel((px, py), (*nearest_color(tuple(round(c*factor) for c in p[:3]), palette), 255))
            all_pixels = list(frame.get_flattened_data())
            result_bounds = frame.getchannel('A').getbbox()
            assert result_bounds and result_bounds[3] == 45
            assert {p[3] for p in all_pixels} == {0, 255}
            assert {p[:3] for p in all_pixels if p[3]} <= set(palette)
            assert all(p[:3] == (0, 0, 0) for p in all_pixels if not p[3])
            details.append({'index': index, 'sourceCell': list(box), 'sourceBounds': list(bounds),
                            'sourceCapBounds': list(cap), 'scaleFromCapWidth': scale,
                            'destination': [x, y], 'headOffsetY': head_offset,
                            'bounds': list(result_bounds), 'footRow': 44,
                            'pixelSha256': hashlib.sha256(frame.tobytes()).hexdigest()})
            frames.append(frame)
        distinct = len({f.tobytes() for f in frames})
        if distinct != count:
            raise ValueError(f'{name}: {distinct}/{count} distinct frames')
        sheet = Image.new('RGBA', (48*count, 48))
        for i, frame in enumerate(frames):
            sheet.paste(frame, (48*i, 0))
            frame.save(args.output/f'{name}-f{i:02}.png', pnginfo=srgb)
        sheet_name = f'chr_mina_{name}.png'
        sheet.save(args.output/sheet_name, pnginfo=srgb)
        sheet.resize((sheet.width*3, 144), Image.Resampling.NEAREST).save(
            args.output/f'{name}-strip-3x.png', pnginfo=srgb)
        for zoom in (1, 3):
            enlarged = [f.resize((48*zoom,48*zoom), Image.Resampling.NEAREST) for f in frames]
            animation_path = args.output/f'{name}-{zoom}x-animated.png'
            enlarged[0].save(animation_path, save_all=True, append_images=enlarged[1:],
                            duration=1000/fps, loop=0 if loop else 1,
                            disposal=0, blend=0, pnginfo=srgb)
            with Image.open(animation_path) as saved:
                assert saved.n_frames == count and saved.info['loop'] == (0 if loop else 1)
        with Image.open(args.output/sheet_name) as saved:
            assert saved.size == (48*count, 48) and saved.mode == 'RGBA'
            assert saved.info.get('srgb') == 0 and saved.tobytes() == sheet.tobytes()
        changed = [sum(a != b for a, b in zip(frames[i].get_flattened_data(),
                   frames[(i+1) % count].get_flattened_data())) for i in range(count)]
        report['clips'][name] = {'assetId': asset_id, 'file': sheet_name,
            'count': count, 'fps': fps, 'loop': loop, 'frameSize': [48,48],
            'sheetSize': list(sheet.size), 'distinctFrames': distinct, 'footRow': 44,
            'sourceSize': list(source.size), 'sourceRowSplit': split,
            'nextFrameChangedPixelsIncludingSeam': changed, 'frames': details,
            'events': [{'name': 'celebrate_peak', 'frame': 3}] if name == 'celebrate' else [],
            'sha256': digest(args.output/sheet_name)}
        draw.text((16, row_index*208+10), f'{name.upper()}  {count} frames / {fps} FPS / '+('LOOP' if loop else 'ONCE'), fill='#301610')
        for i, frame in enumerate(frames):
            left, top = 16+i*146, row_index*208+36
            draw.rectangle((left,top,left+143,top+143), fill='#d1cdc6')
            enlarged = frame.resize((144,144), Image.Resampling.NEAREST)
            board.paste(enlarged,(left,top),enlarged)
            draw.line((left,top+134,left+143,top+134), fill='#a39e95')
            draw.text((left,top+151), f'f{i:02}', fill='#301610')
    board.save(args.output/'comparison-board.png', pnginfo=srgb)
    assert all(digest(Path(p)) == value for p, value in hashes.items())
    report['sourceHashes'] = hashes
    report['identityCleanup'] = 'cap-width scale; generated upper head cleared, approved head pasted; retained cup/raised-hand windows; planted lower body for Stretch/Celebrate only; Walk near/far limb palette shading alternates after frame2'
    report['limits'] = ['Motion quality and identity need human review.',
                        'Rest uses seated pose without furniture; prop contact remains a follow-up.',
                        'HTML is an art preview, not a Godot or Windows input test.',
                        'No runtime catalog or simulation change.']
    (args.output/'preparation.json').write_text(json.dumps(report, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print(json.dumps({name:{k:v[k] for k in ('count','fps','loop','distinctFrames')} for name,v in report['clips'].items()}, indent=2))


if __name__ == '__main__':
    main()
