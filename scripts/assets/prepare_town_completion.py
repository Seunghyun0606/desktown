"""Prepare eleven review-only Town sheets; approved originals stay unchanged."""
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image, PngImagePlugin

ROOT = Path(__file__).resolve().parents[2]
BASE = ROOT / 'docs/art/reviews/2026-10-11/palette-v1/prepared-v2/shared32'
SPECS = {
    'BLD-HOU-001': ('bld_house.png', (128,128), 1),
    'BLD-WRK-001': ('bld_workshop_states.png', (160,128), 3),
    'BLD-LCK-001': ('bld_library_locked.png', (128,128), 1),
    'ENV-GRA-001': ('env_grass_tiles.png', (32,32), 2),
    'ENV-DIR-001': ('env_dirt_tile.png', (32,32), 1),
    'ENV-PTH-001': ('env_path_tiles.png', (32,32), 3),
    'ENV-TRE-001': ('env_tree_variants.png', (64,96), 2),
    'ENV-BUS-001': ('env_bush_variants.png', (32,32), 2),
    'ENV-FLW-001': ('env_flower_variants.png', (32,32), 2),
    'ENV-FEN-001': ('env_fence_variants.png', (32,32), 2),
    'ENV-ROC-001': ('env_rock_variants.png', (32,32), 2),
}

def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()

def prepare(review, output):
    review=review.resolve()
    output=output.resolve()
    output.mkdir(parents=True, exist_ok=False)
    palette_path = ROOT / 'docs/art/palettes/desktown-v01-32.json'
    palette = [tuple(c) for c in json.loads(palette_path.read_text(encoding='utf-8'))['colorsRGB']]
    sources = {key: review / 'sources' / (key + '.png') for key in ('tree','flower','fence','rock','library')}
    sources.update({k: BASE / (k+'-1x.png') for k in ('house','grass','path','bush')})
    sources['workshop'] = ROOT / 'docs/art/reviews/2026-10-11/workshop-repairing-v1/prepared-v1/bld_workshop_states.png'
    sources['palette'] = palette_path
    hashes = {k: sha(p) for k,p in sources.items()}
    def quantize(im):
        cache = {}
        def color(p):
            if p[3] < 128: return (0,0,0,0)
            rgb = p[:3]
            if rgb not in cache:
                cache[rgb] = min(palette,key=lambda c:2*(rgb[0]-c[0])**2+4*(rgb[1]-c[1])**2+3*(rgb[2]-c[2])**2)
            return (*cache[rgb],255)
        im.putdata([color(p) for p in im.get_flattened_data()])
        return im
    def read(key): return Image.open(sources[key]).convert('RGBA')
    def fit(im,cell,maximum):
        box=im.getchannel('A').point(lambda a:255 if a>=128 else 0).getbbox()
        if not box: raise ValueError('Empty source')
        im=im.crop(box)
        factor=min(maximum[0]/im.width,maximum[1]/im.height)
        small=quantize(im.resize((round(im.width*factor),round(im.height*factor)),Image.Resampling.NEAREST))
        result=Image.new('RGBA',cell)
        result.alpha_composite(small,(cell[0]//2-small.width//2,cell[1]-small.height))
        return result
    def pair(key,cell,maximum):
        source=read(key); split=source.width//2
        return [fit(source.crop((x,0,end,source.height)),cell,maximum) for x,end in ((0,split),(split,source.width))]
    grass=read('grass'); path=read('path'); bush=read('bush')
    # Reuse the approved grass's sparse, seamless texture without stretching a path edge.
    earth={tuple(palette[14]):palette[19],tuple(palette[15]):palette[19],
           tuple(palette[17]):palette[22],tuple(palette[10]):palette[13],
           tuple(palette[9]):palette[13],tuple(palette[8]):palette[13]}
    dirt=Image.new('RGBA',(32,32))
    dirt.putdata([(*earth.get(p[:3],palette[19]),255) for p in grass.get_flattened_data()])
    cross=path.copy(); cross.alpha_composite(path.transpose(Image.Transpose.ROTATE_90))
    frames={
        'BLD-HOU-001':[read('house')],
        'BLD-WRK-001':[read('workshop').crop((i*160,0,(i+1)*160,128)) for i in range(3)],
        'BLD-LCK-001':[fit(read('library'),(128,128),(112,112))],
        'ENV-GRA-001':[grass,grass.transpose(Image.Transpose.FLIP_LEFT_RIGHT)],
        'ENV-DIR-001':[dirt],
        'ENV-PTH-001':[path,path.transpose(Image.Transpose.ROTATE_90),cross],
        'ENV-TRE-001':pair('tree',(64,96),(60,88)),
        'ENV-BUS-001':[bush,bush.transpose(Image.Transpose.FLIP_LEFT_RIGHT)],
        'ENV-FLW-001':pair('flower',(32,32),(28,24)),
        'ENV-FEN-001':pair('fence',(32,32),(32,24)),
        'ENV-ROC-001':pair('rock',(32,32),(28,20)),
    }
    srgb=PngImagePlugin.PngInfo(); srgb.add(b'sRGB',b'\x00')
    rows=[]
    for asset_id,cells in frames.items():
        name,cell,count=SPECS[asset_id]
        assert len(cells)==count and all(im.size==cell for im in cells)
        if count>1: assert len({im.tobytes() for im in cells})==count,asset_id
        sheet=Image.new('RGBA',(cell[0]*count,cell[1]))
        for i,im in enumerate(cells): sheet.paste(im,(i*cell[0],0))
        pixels=list(sheet.get_flattened_data())
        assert all(p[3] in (0,255) and (p[3] or p[:3]==(0,0,0)) for p in pixels)
        assert {p[:3] for p in pixels if p[3]}<=set(palette),asset_id
        if asset_id in ('ENV-GRA-001','ENV-DIR-001'): assert all(p[3]==255 for p in pixels)
        target=output/name
        if asset_id in ('BLD-HOU-001','BLD-WRK-001'):
            target.write_bytes(sources['house' if asset_id=='BLD-HOU-001' else 'workshop'].read_bytes())
        else:
            delivered=sheet.convert('RGB') if asset_id in ('ENV-GRA-001','ENV-DIR-001') else sheet
            delivered.save(target,pnginfo=srgb)
        assert Image.open(target).convert('RGBA').tobytes()==sheet.tobytes()
        sheet.resize((sheet.width*3,sheet.height*3),Image.Resampling.NEAREST).save(output/(name[:-4]+'-3x.png'),pnginfo=srgb)
        rows.append({'id':asset_id,'file':name,'size':sheet.size,'cell':cell,'frames':count,'pivot':[cell[0]//2,cell[1]],'sha256':sha(target)})
    for key in ('grass','path'):
        tile=frames['ENV-GRA-001' if key=='grass' else 'ENV-PTH-001'][0]
        assert [tile.getpixel((0,y)) for y in range(32)]==[tile.getpixel((31,y)) for y in range(32)]
    assert all(sha(sources[k])==h for k,h in hashes.items())
    report={'reviewOnly':True,'pendingHumanApproval':True,'productionAssigned':False,
            'palette':'desktown-v01-32','sources':{k:{'path':p.relative_to(ROOT).as_posix(),'sha256':hashes[k]} for k,p in sources.items()},
            'assets':rows,'checks':{'dimensions':True,'binaryAlpha':True,'palette32':True,'opaqueGround':True,'distinctCells':True,'approvedHouseWorkshopBytesPreserved':True,'sourceHashesPreserved':True},
            'pathCells':['horizontal','vertical','cross'],'limits':'Three explicit path cells, not an autotile topology; candidate art/layout requires review. Audio, NPCs, UI and effects remain separate.'}
    (output/'preparation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(f'Prepared {len(rows)} Town candidate sheets')

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('review',type=Path); parser.add_argument('output',type=Path)
    args=parser.parse_args(); prepare(args.review,args.output)
