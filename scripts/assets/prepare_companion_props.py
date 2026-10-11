"""Prepare Companion prop art for human review; does not assign runtime assets.

Run from repository root with Pillow 12.3.0. Output must be a new directory.
"""
import argparse
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, PngImagePlugin, __version__

SPECS = {
    'workbench': ('CMP-WRK-001', 'prop_workbench.png', (96, 64), 1, (4, 43, 92, 64)),
    'lamp': ('CMP-LMP-001', 'prop_lamp_states.png', (32, 48), 2, (7, 18, 25, 48)),
    'window': ('CMP-WIN-001', 'prop_small_window.png', (96, 80), 1, (15, 29, 81, 80)),
    'tool': ('CMP-TOL-001', 'prop_mina_tool.png', (32, 32), 2, (7, 5, 25, 27)),
}


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('review_root', type=Path)
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    if __version__ != '12.3.0':
        raise RuntimeError('Use Pillow 12.3.0 for the recorded byte reproduction.')
    palette_path = Path('docs/art/palettes/desktown-v01-32.json')
    palette = [tuple(c) for c in json.loads(palette_path.read_text(encoding='utf-8'))['colorsRGB']]
    args.output.mkdir(parents=True, exist_ok=False)
    info = PngImagePlugin.PngInfo()
    info.add(b'sRGB', b'\x00')
    outputs, sources, stats, frames = [], {}, [], {}
    cache = {}

    def save(im, name):
        path = args.output / name
        im.save(path, pnginfo=info)
        outputs.append({'file': name, 'size': list(im.size), 'sha256': sha(path)})

    def normalize(raw):
        pixels = []
        for p in raw.get_flattened_data():
            if p[3] < 128:
                pixels.append((0, 0, 0, 0))
            else:
                if p[:3] not in cache:
                    cache[p[:3]] = min(palette, key=lambda c: sum(w*(a-b)**2 for w,a,b in zip((2,4,3), p[:3],c)))
                pixels.append((*cache[p[:3]], 255))
        result = Image.new('RGBA', raw.size)
        result.putdata(pixels)
        return result

    for name, (asset_id, filename, cell, count, target) in SPECS.items():
        source = args.review_root / 'sources' / (name + '.png')
        prompt = args.review_root / 'sources' / (name + '-prompt.txt')
        sources[source.as_posix()] = sha(source)
        sources[prompt.as_posix()] = sha(prompt)
        raw = Image.open(source).convert('RGBA')
        sheet = Image.new('RGBA', (cell[0]*count, cell[1]))
        frames[name] = []
        for i in range(count):
            # Equal-width source cells are declared by the recorded prompts.
            box = (raw.width*i//count, 0, raw.width*(i+1)//count, raw.height)
            cropped = normalize(raw.crop(box))
            bounds = cropped.getchannel('A').getbbox()
            if not bounds:
                raise ValueError(f'Empty source cell: {name} {i}')
            native = Image.new('RGBA', cell)
            fitted = cropped.crop(bounds).resize((target[2]-target[0], target[3]-target[1]), Image.Resampling.NEAREST)
            native.paste(fitted, target[:2])
            if name == 'lamp' and i == 0:
                # Nearest downsampling dropped portions of the thin hanging cord.
                # Fill only transparent pixels on its centerline; keep cap/shade.
                for y in range(20, 34):
                    if native.getpixel((16, y))[3] == 0:
                        native.putpixel((16, y), (163, 111, 69, 255))
            # Lamp variants must not jump: use the first cell's silhouette and
            # pixel positions; transfer only generated ON colors at shared pixels.
            if name == 'lamp' and i == 1:
                off = frames[name][0]
                native.putdata([b if a[3] and b[3] else a for a,b in zip(off.get_flattened_data(), native.get_flattened_data())])
            colors = {p[:3] for p in native.get_flattened_data() if p[3]}
            assert colors <= set(palette)
            frames[name].append(native)
            sheet.paste(native, (i*cell[0], 0))
            save(native, f'{name}-f{i:02}.png')
            save(native.convert('LA').convert('RGBA'), f'{name}-f{i:02}-gray.png')
            stats.append({'id': asset_id, 'frame': i, 'sourceCell': box, 'sourceAlphaBounds': bounds,
                          'targetBox': target, 'alphaBounds': native.getchannel('A').getbbox(),
                          'colorsUsed': len(colors), 'pivot': [cell[0]//2, cell[1]]})
        save(sheet, filename)
        for scale in (2, 3):
            save(sheet.resize((sheet.width*scale, sheet.height*scale), Image.Resampling.NEAREST), f'{name}-sheet-{scale}x.png')

    shared = Path('docs/art/reviews/2026-10-11/palette-v1/prepared-v2/shared32')
    work_path = shared/'work-1x.png'
    work = Image.open(work_path).convert('RGBA').crop((192, 0, 240, 48))
    sources[work_path.as_posix()] = sha(work_path)
    sources[palette_path.as_posix()] = sha(palette_path)
    # Work frame 4 already contains a hammer. Standalone tool is intentionally
    # shown separately; no second hammer is pasted onto Mina.
    def stage(proposal=False, lamp=0, gray=False):
        im = Image.new('RGBA', (360, 200), '#3c352f')
        d = ImageDraw.Draw(im)
        floor_y = 148 if proposal else 154
        d.rectangle((0, floor_y, 359, 199), fill='#5d493b')
        d.rectangle((0, 0, 359, 29), fill='#2e2924')
        d.text((10, 8), 'DeskTown / 100%', fill='#f2d49d')
        d.rectangle((328, 3, 356, 28), outline='#f2d49d')
        d.text((340, 9), 'x', fill='#f2d49d')
        # Binder native origins; half-pixel current lamp origin is rounded only
        # in this PNG raster reference, not changed in the Godot scene.
        im.alpha_composite(frames['window'][0], (119, 10))
        im.alpha_composite(frames['lamp'][lamp], (282, 26))
        im.alpha_composite(frames['workbench'][0], (135 if proposal else 151, 91))
        d.rectangle((273, 142, 297, 155), fill='#75422a')
        d.rectangle((55, 126, 73, 152), fill='#6d6740')
        im.alpha_composite(work, (100, 103))
        d.text((11, 180), 'Workshop', fill='#f2d49d')
        if gray:
            im = im.convert('LA').convert('RGBA')
        return im

    for proposal in (False, True):
        for lamp in (0, 1):
            name = f'stage-{"proposal" if proposal else "current"}-lamp{lamp}'
            save(stage(proposal, lamp), name+'.png')
            save(stage(proposal, lamp, True), name+'-gray.png')
    board = Image.new('RGBA', (1000, 910), '#f3efe7')
    d = ImageDraw.Draw(board)
    d.text((24, 16), 'COMPANION CORE PROPS / REVIEW ONLY / SHARED 32', fill='#301610')
    for index, (name, spec) in enumerate(SPECS.items()):
        x, y = 24+(index%2)*486, 52+(index//2)*210
        d.text((x, y), spec[0]+' / '+name, fill='#301610')
        sheet = Image.open(args.output/spec[1]).convert('RGBA')
        for gray, offset in ((False,0),(True,245)):
            bg = Image.new('RGBA', (230, 170), '#20252b' if not gray else '#e5ded3')
            shown = sheet.convert('LA').convert('RGBA') if gray else sheet
            shown = shown.resize((sheet.width*2, sheet.height*2), Image.Resampling.NEAREST)
            bg.alpha_composite(shown, ((230-shown.width)//2,(170-shown.height)//2))
            board.alpha_composite(bg, (x+offset,y+22))
    for proposal, x in ((False, 24), (True, 510)):
        y = 478
        d.text((x,y), 'CURRENT COORDINATES' if not proposal else 'CONTACT PROPOSAL - NOT APPLIED', fill='#301610')
        board.alpha_composite(stage(proposal), (x,y+24))
        crop = stage(proposal).crop((95,120,175,161)).resize((320,164), Image.Resampling.NEAREST)
        board.alpha_composite(crop,(x,y+240))
    save(board, 'comparison-board.png')
    report = {'status': 'human_review_pending', 'pillow': __version__, 'palette': 'desktown-v01-32',
              'method': 'Split equal cells, binary alpha at128, weighted RGB 2/4/3, alpha-bounds fit to declared box by nearest neighbor.',
              'lampInvariant': 'OFF alpha mask and pixel positions preserved in ON; overlapping generated ON colors only.',
              'pixelCleanup': 'Reconnect dropped hanging cord only at transparent OFF pixels x16, y20..33; same cord retained in ON.',
              'runtimeAssigned': False, 'sourceSha256': sources, 'frames': stats, 'outputs': outputs,
              'reference': {'viewport': [360,200], 'mina': [100,103], 'workFrame': 4,
                            'currentWorkbench': [151,91], 'proposalWorkbench': [135,91],
                            'currentFloorY':154, 'proposalFloorY':148,
                            'currentLampOrigin': [281.5,26], 'pngLampOrigin': [282,26],
                            'note': 'Review PNG/HTML, not Godot capture; geometric floor/stool/plant are placeholders. No runtime position or binding changes.'}}
    (args.output/'preparation.json').write_text(json.dumps(report, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print(f'Prepared {len(outputs)} PNGs, 4 catalog candidates / 6 cells; runtime unassigned.')


if __name__ == '__main__':
    main()
