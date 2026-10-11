"""Prepare a review-only three-state Workshop sheet using the approved palette.

Requires Pillow 12.3.0; never writes a runtime catalog entry.
"""
import argparse
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw, PngImagePlugin, __version__

STATES = ('broken', 'repairing', 'complete')
LOCKS = ((65, 81, 94, 128), (0, 123, 160, 128))
REPAIR_DETAIL_LOCKS = ((57, 58, 98, 86),)
# Confine repairs to roof/left staging/front boards. Keep existing architecture.
PATCHES = ((12, 37, 72, 77), (12, 74, 61, 123), (98, 79, 120, 123))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def nearest(pixel, palette):
    return min(palette, key=lambda c: 2*(pixel[0]-c[0])**2
               + 4*(pixel[1]-c[1])**2 + 3*(pixel[2]-c[2])**2)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('review_root', type=Path)
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    palette_root = Path('docs/art/reviews/2026-10-11/palette-v1/prepared-v2/shared32')
    palette_path = Path('docs/art/palettes/desktown-v01-32.json')
    source_path = args.review_root/'sources'/'repairing.png'
    prompt_path = args.review_root/'sources'/'repairing-prompt.txt'
    paths = [source_path, prompt_path, palette_path]+[palette_root/f'{s}-1x.png' for s in ('broken','complete','neutral','house','grass','path')]
    hashes = {p.as_posix(): sha(p) for p in paths}
    palette = [tuple(c) for c in json.loads(palette_path.read_text(encoding='utf-8'))['colorsRGB']]
    broken, complete = [Image.open(palette_root/f'{s}-1x.png').convert('RGBA') for s in ('broken','complete')]
    assert broken.size == complete.size == (160, 128)
    source = Image.open(source_path).convert('RGBA')
    # Use the entire edit canvas, including its transparent margin. Do not
    # independently fit the new state's opaque building bounds.
    resized = source.resize((160,128), Image.Resampling.NEAREST)
    normalized = Image.new('RGBA', (160,128))
    colors = {}
    pixels = []
    for p in resized.get_flattened_data():
        if p[3] >= 128:
            if p[:3] not in colors:
                colors[p[:3]] = nearest(p, palette)
            pixels.append((*colors[p[:3]], 255))
        else:
            pixels.append((0,0,0,0))
    normalized.putdata(pixels)
    repairing = complete.copy()
    for box in PATCHES:
        repairing.paste(normalized.crop(box), box[:2])
    for box in LOCKS:
        repairing.paste(complete.crop(box), box[:2])
    for box in REPAIR_DETAIL_LOCKS:
        repairing.paste(complete.crop(box), box[:2])
    frames = (broken,repairing,complete)
    assert len({f.tobytes() for f in frames}) == 3
    assert all(all(f.crop(box).tobytes()==complete.crop(box).tobytes() for box in LOCKS) for f in frames)
    args.output.mkdir(parents=True, exist_ok=False)
    srgb = PngImagePlugin.PngInfo()
    srgb.add(b'sRGB', b'\x00')
    outputs = []

    def save(im, filename):
        p = args.output/filename
        im.save(p, pnginfo=srgb)
        with Image.open(p) as saved:
            assert saved.tobytes()==im.tobytes() and saved.info.get('srgb')==0
        outputs.append({'file':filename,'size':list(im.size),'sha256':sha(p)})

    sheet = Image.new('RGBA',(480,128))
    gray_frames = []
    stats = []
    for i,(state,frame) in enumerate(zip(STATES,frames)):
        pixels = list(frame.get_flattened_data())
        bounds = frame.getchannel('A').getbbox()
        assert bounds == (8,31,152,128)
        assert {p[3] for p in pixels}=={0,255}
        assert {p[:3] for p in pixels if p[3]} <= set(palette)
        assert all(p[:3]==(0,0,0) for p in pixels if not p[3])
        sheet.paste(frame,(160*i,0))
        for scale in (1,2,3):
            enlarged = frame.resize((160*scale,128*scale), Image.Resampling.NEAREST)
            save(enlarged,f'workshop-{state}-{scale}x.png')
        gray = frame.convert('L').convert('RGBA')
        gray.putalpha(frame.getchannel('A'))
        gray_frames.append(gray)
        save(gray,f'workshop-{state}-gray.png')
        stats.append({'state':state,'index':i,'bounds':list(bounds),
                      'opaqueColors':len({p[:3] for p in pixels if p[3]}),
                      'changedPixelsFromComplete':sum(a!=b for a,b in zip(frame.get_flattened_data(),complete.get_flattened_data()))})
    save(sheet,'bld_workshop_states.png')
    save(normalized,'repairing-normalized-reference.png')
    board = Image.new('RGB',(1024,650),'#f3efe7')
    d = ImageDraw.Draw(board)
    d.text((16,10),'Workshop | Broken / Repairing / Complete | approved32 | REVIEW ONLY',fill='#301610')
    for row, group in enumerate((frames,gray_frames)):
        top = 45+row*300
        for i,frame in enumerate(group):
            left = 16+i*336
            d.rectangle((left,top,left+319,top+255), fill='#4c6955')
            scaled = frame.resize((320,256),Image.Resampling.NEAREST)
            board.paste(scaled,(left,top),scaled)
            d.text((left,top+264),STATES[i]+(' grayscale' if row else ' color'),fill='#301610')
    save(board,'comparison-board.png')
    # Local tile cluster reference, identical origins across state previews.
    mina = Image.open(palette_root/'neutral-1x.png').convert('RGBA')
    house = Image.open(palette_root/'house-1x.png').convert('RGBA')
    grass = Image.open(palette_root/'grass-1x.png').convert('RGBA')
    path = Image.open(palette_root/'path-1x.png').convert('RGBA')
    for s,frame in zip(STATES,frames):
        cluster = Image.new('RGB',(384,288),'#4c6955')
        for y in range(0,288,32):
            for x in range(0,384,32):
                cluster.paste(grass,(x,y),grass)
        for x in range(0,384,32):
            cluster.paste(path,(x,192),path)
        cluster.paste(house,(48,32),house)
        cluster.paste(frame,(192,32),frame)
        cluster.paste(mina,(168,174),mina)
        save(cluster,f'cluster-{s}-1x.png')
        town = Image.new('RGB',(1280,720),'#4c6955')
        d = ImageDraw.Draw(town)
        for rect,color in [((95,502,1164,547),'#a28e6f'),
                           ((228,278,413,446),'#bf9a77'),
                           ((1015,120,1174,244),'#556259'),
                           ((99,170,159,329),'#2d5242'),
                           ((590,565,614,598),'#eb8f51')]:
            d.rectangle(rect,fill=color)
        town.paste(frame,(796,324),frame)
        town.paste(mina,(680,415),mina)
        d.text((24,22),'Town coordinates - art reference, not a Godot screenshot',fill='#fff9ed')
        save(town,f'town-{s}-1x.png')
    assert all(sha(Path(p))==h for p,h in hashes.items())
    report = {'status':'repairing_candidate_pending_human_approval',
        'tool':'Python/Pillow','pillowVersion':__version__,'assetId':'BLD-WRK-001',
        'sourceHashes':hashes,'sourceSize':list(source.size),
        'mapping':'entire generated canvas to160x128, nearest; no alpha-bounds fit',
        'patchesFromGenerated':list(PATCHES),'locksFromComplete':list(LOCKS),
        'repairingSignAndGableLocks':list(REPAIR_DETAIL_LOCKS),
        'unchangedArchitecture':'all pixels outside patch rectangles; door/foundation override patches',
        'sheetSize':[480,128],'frameSize':[160,128],'pivot':[80,128],
        'stateOrder':list(STATES),'states':stats,'paletteId':'desktown-v01-32',
        'town':{'viewport':[1280,720],'workshopOrigin':[796,324],
                'minaOrigin':[680,415],'reviewCrop':[600,254,440,260]},
        'runtimeAssigned':False,'files':outputs,
        'limits':['Human state readability and scaffold/roof continuity approval pending.',
                  'Static PNG/HTML reference; not Godot or native Windows QA.',
                  'No smoke, spark, state timing, collision, navigation or save changes.']}
    (args.output/'preparation.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'states':stats,'files':len(outputs)+1,'sourceSize':source.size},indent=2))


if __name__=='__main__':
    main()
