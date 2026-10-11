"""Prepare review-only floor/stool/shadow and full Work/Rest contact comparisons.

Run from repository root with Pillow 12.3.0. Never assigns runtime art.
"""
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image, ImageDraw, PngImagePlugin, __version__

SPECS = {
    'floor': ('CMP-FLR-001','prop_companion_floor.png',(360,64),(0,12,360,64)),
    'stool': ('CMP-STL-001','prop_stool.png',(32,32),(7,18,25,32)),
    'shadow': ('CMP-SHD-001','prop_character_shadow.png',(32,16),(3,6,29,11)),
}
PROPS = Path('docs/art/reviews/2026-10-11/companion-props-v1/prepared-v1')
WORK = Path('docs/art/reviews/2026-10-11/palette-v1/prepared-v2/shared32/work-1x.png')
REST = Path('docs/art/reviews/2026-10-11/mina-motion-v1/prepared-v1/chr_mina_rest.png')
PALETTE = Path('docs/art/palettes/desktown-v01-32.json')


def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('review_root',type=Path)
    parser.add_argument('output',type=Path)
    args = parser.parse_args()
    assert __version__ == '12.3.0', 'Use recorded Pillow version.'
    args.output.mkdir(parents=True,exist_ok=False)
    palette = list(map(tuple,json.loads(PALETTE.read_text(encoding='utf-8'))['colorsRGB']))
    inputs, outputs, stats, art = {}, [], [], {}
    info = PngImagePlugin.PngInfo(); info.add(b'sRGB',b'\x00')

    def read(p):
        inputs[p.as_posix()] = sha(p)
        return Image.open(p).convert('RGBA')

    def save(im,name):
        p = args.output/name
        im.save(p,pnginfo=info)
        outputs.append({'file':name,'size':list(im.size),'sha256':sha(p)})

    cache = {}
    for name,(asset_id,filename,size,target) in SPECS.items():
        src = args.review_root/'sources'/f'{name}.png'
        prompt = args.review_root/'sources'/f'{name}-prompt.txt'
        inputs[prompt.as_posix()] = sha(prompt)
        raw = read(src)
        # The shadow generation included an unwanted reference bench above the
        # requested oval. Extract only the visually inspected lower oval region.
        source_roi = (0,690,raw.width,raw.height) if name=='shadow' else (0,0,raw.width,raw.height)
        selected = raw.crop(source_roi)
        mask = selected.getchannel('A').point(lambda x:255 if x>=128 else 0)
        bounds = mask.getbbox()
        assert bounds
        resized = selected.crop(bounds).resize((target[2]-target[0],target[3]-target[1]),Image.Resampling.NEAREST)
        pixels = []
        for p in resized.get_flattened_data():
            if p[3]<128:
                pixels.append((0,0,0,0))
            else:
                if p[:3] not in cache:
                    cache[p[:3]] = min(palette,key=lambda c:sum(w*(a-b)**2 for w,a,b in zip((2,4,3),p[:3],c)))
                pixels.append((*cache[p[:3]],255))
        resized.putdata(pixels)
        native = Image.new('RGBA',size)
        native.paste(resized,target[:2])
        if name == 'floor':
            # A rectangular floor plane must cover all rows below its back edge.
            # Fill sparse transparent pixels introduced by source edge/noise with
            # the same row's nearest opaque color; do not fill top padding.
            for y in range(12,64):
                valid = [x for x in range(360) if native.getpixel((x,y))[3]]
                assert valid, f'Empty floor row {y}'
                for x in range(360):
                    if not native.getpixel((x,y))[3]:
                        native.putpixel((x,y),native.getpixel((min(valid,key=lambda a:abs(a-x)),y)))
        art[name] = native
        for factor in (1,2,3):
            save(native.resize((size[0]*factor,size[1]*factor),Image.Resampling.NEAREST),filename if factor==1 else f'{name}-{factor}x.png')
        save(native.convert('LA').convert('RGBA'),f'{name}-gray.png')
        stats.append({'id':asset_id,'sourceSize':list(raw.size),'sourceROI':source_roi,'sourceAlphaBoundsInROI':bounds,
                      'targetBox':target,'size':size,'alphaBounds':native.getchannel('A').getbbox(),
                      'colorsUsed':len({p[:3] for p in native.get_flattened_data() if p[3]}),
                      'pivot':[size[0]//2,size[1]]})
    art['bench'] = read(PROPS/'prop_workbench.png')
    art['window'] = read(PROPS/'prop_small_window.png')
    art['lamp'] = read(PROPS/'lamp-f01.png')
    sheets = {'work':read(WORK),'rest':read(REST)}
    clips = {k:[sheet.crop((i*48,0,(i+1)*48,48)) for i in range(sheet.width//48)] for k,sheet in sheets.items()}
    inputs[PALETTE.as_posix()] = sha(PALETTE)
    assert len(clips['work'])==6 and len(clips['rest'])==4

    def stage(clip,frame,proposal):
        im = Image.new('RGBA',(360,200),'#3c352f')
        d = ImageDraw.Draw(im)
        d.rectangle((0,0,359,29),fill='#2e2924')
        d.text((10,8),'DeskTown / 100%',fill='#f2d49d')
        # Draw-only header mock; positions match actual input controls.
        for x,mark in ((262,'-'),(295,'+'),(328,'x')):
            d.rectangle((x,3,x+28,28),outline='#caac83')
            d.text((x+10,9),mark,fill='#f2d49d')
        im.alpha_composite(art['window'],(119,10))
        im.alpha_composite(art['lamp'],(282,26))
        im.alpha_composite(art['floor'],(0,136))
        if proposal:
            im.alpha_composite(art['shadow'],(108,141))
        im.alpha_composite(art['bench'],(135 if proposal else 151,91))
        im.alpha_composite(art['stool'],(108,116) if proposal else (270,124))
        d.rectangle((55,126,73,152),fill='#6d6740')
        im.alpha_composite(clips[clip][frame],(100,103))
        d.text((11,180),'Workshop',fill='#f2d49d')
        return im

    stages = {}
    contact = []
    for clip,frames in clips.items():
        for layout in ('current','proposal'):
            images = []
            strip = Image.new('RGBA',(85*len(frames),53))
            for i,mina in enumerate(frames):
                im = stage(clip,i,layout=='proposal')
                images.append(im); stages[(clip,layout,i)] = im
                save(im,f'{clip}-{layout}-f{i:02}.png')
                strip.paste(im.crop((94,106,179,159)),(85*i,0))
                foot_y = mina.getchannel('A').getbbox()[3]-1+103
                assert foot_y==147
                contact.append({'clip':clip,'layout':layout,'frame':i,'minaOrigin':[100,103],
                                'footLastY':foot_y,'floorFirstY':148,
                                'seatOrigin':[108,116] if layout=='proposal' else [269.5,124],
                                'seatTopY':134 if layout=='proposal' else 142,
                                'note':'Geometric reference values; no automatic artistic/contact pass verdict.'})
            save(strip,f'{clip}-{layout}-contact-strip.png')
            save(strip.resize((strip.width*2,strip.height*2),Image.Resampling.NEAREST),f'{clip}-{layout}-contact-strip-2x.png')
            animation = args.output/f'{clip}-{layout}-animated.png'
            images[0].save(animation,save_all=True,append_images=images[1:],duration=167 if clip=='work' else 500,
                           loop=0,disposal=0,blend=0,pnginfo=info)
            outputs.append({'file':animation.name,'size':[360,200],'sha256':sha(animation)})
    board = Image.new('RGBA',(1160,1080),'#f3efe7')
    d = ImageDraw.Draw(board)
    d.text((24,14),'COMPANION FLOOR / STOOL / SHADOW - REVIEW ONLY',fill='#301610')
    for gray,x in ((False,24),(True,406)):
        im = art['floor'].convert('LA').convert('RGBA') if gray else art['floor']
        bg = Image.new('RGBA',(360,64),'#20252b' if not gray else '#e5ded3'); bg.alpha_composite(im)
        board.alpha_composite(bg,(x,50))
    for name,x in (('stool',800),('shadow',936)):
        d.text((x,32),name.upper()+' / 2X',fill='#301610')
        bg = Image.new('RGBA',(100,84),'#20252b')
        shown = art[name].resize((art[name].width*2,art[name].height*2),Image.Resampling.NEAREST)
        bg.alpha_composite(shown,((100-shown.width)//2,(84-shown.height)//2))
        board.alpha_composite(bg,(x,50))
    for row,(clip,layout) in enumerate((('work','current'),('work','proposal'),('rest','current'),('rest','proposal'))):
        y = 152+row*148
        d.text((24,y),f'{clip.upper()} / {layout.upper()} / ALL FRAMES / 2X',fill='#301610')
        strip = Image.open(args.output/f'{clip}-{layout}-contact-strip-2x.png').convert('RGBA')
        board.alpha_composite(strip,(24,y+22))
        for i in range(len(clips[clip])):
            d.text((24+i*170,y+132),f'{i:02}',fill='#301610')
    for layout,x in (('current',24),('proposal',406)):
        d.text((x,780),f'REST / {layout.upper()} / 1X',fill='#301610')
        board.alpha_composite(stages[('rest',layout,0)],(x,804))
    d.text((800,808),'MINA ORIGIN (100,103)',fill='#301610')
    d.text((800,830),'NO SECOND HAMMER',fill='#301610')
    d.text((800,852),'NO RUNTIME ASSIGNMENT',fill='#301610')
    d.text((800,874),'PROPOSAL: STATIC STOOL',fill='#301610')
    d.text((800,896),'SHARED FOOT LINE Y147',fill='#301610')
    save(board,'comparison-board.png')
    report = {'status':'human_review_pending','pillow':__version__,'runtimeAssigned':False,
              'method':'Alpha>=128 bounds fitted by nearest; weighted RGB2/4/3 to approved32; transparent RGB0. Floor holes filled from nearest opaque pixel in same row.',
              'sourceSha256':inputs,'assets':stats,'contactReferences':contact,'outputs':outputs,
              'layout':{'currentWorkbench':[151,91],'proposalWorkbench':[135,91],
                        'currentStool':[269.5,124],'rasterCurrentStool':[270,124],'proposalStool':[108,116],
                        'minaOriginAllClips':[100,103],'floorOrigin':[0,136],'floorVisibleFirstY':148,
                        'oldGeometricFloorFirstY':154,'proposalShadow':[108,141],
                        'shadowRuntimeBindingExists':False,'lampOrigin':[281.5,26],'rasterLamp':[282,26],
                        'drawOrder':'Floor, shadow(proposal), bench, stool, plant, Mina, label; props fixed across clips.',
                        'unresolved':'Tool baked into Work; exact contact/event frame, Rest seat support/occlusion and live whole-pixel binding require human/Godot review.'}}
    (args.output/'preparation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(f'Prepared {len(outputs)} PNGs / 3 new candidates / all 10 Work+Rest frames / no runtime changes.')


if __name__=='__main__':
    main()
