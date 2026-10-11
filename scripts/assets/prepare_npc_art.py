"""Normalize five review-only NPC clips to 48x48 and the approved 32-color palette."""
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image, PngImagePlugin

ROOT=Path(__file__).resolve().parents[2]
SPECS={'noah-idle':('CHR-NOA-001',4,4,2,40), 'noah-walk':('CHR-NOA-002',6,3,8,40),
       'noah-read':('CHR-NOA-003',4,4,2,40), 'rumi-idle':('CHR-RUM-001',4,4,2,38),
       'rumi-walk':('CHR-RUM-002',6,3,8,38)}

def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()

def prepare(review,output):
    review=review.resolve();output=output.resolve();output.mkdir(parents=True,exist_ok=False)
    palette_path=ROOT/'docs/art/palettes/desktown-v01-32.json'
    palette=[tuple(c) for c in json.loads(palette_path.read_text(encoding='utf-8'))['colorsRGB']]
    srgb=PngImagePlugin.PngInfo();srgb.add(b'sRGB',b'\x00')
    cache={}
    def normalize(pixel):
        if pixel[3]<128:return (0,0,0,0)
        rgb=pixel[:3]
        if rgb not in cache:cache[rgb]=min(palette,key=lambda c:2*(rgb[0]-c[0])**2+4*(rgb[1]-c[1])**2+3*(rgb[2]-c[2])**2)
        return (*cache[rgb],255)
    report={'reviewOnly':True,'runtimeAssigned':False,'pendingHumanApproval':True,'palette':'desktown-v01-32',
        'pivot':[24,44],'paletteSha256':sha(palette_path),'clips':[]}
    for name,(asset_id,count,columns,fps,height) in SPECS.items():
        path=review/'sources'/f'{name}.png';source=Image.open(path).convert('RGBA')
        rows=count//columns;frames=[];details=[];scale=None
        for index in range(count):
            col=index%columns;row=index//columns
            box=(col*source.width//columns,row*source.height//rows,(col+1)*source.width//columns,(row+1)*source.height//rows)
            raw=source.crop(box);mask=raw.getchannel('A').point(lambda a:255 if a>=128 else 0);bounds=mask.getbbox()
            assert bounds,'Blank source cell'
            if scale is None:scale=height/(bounds[3]-bounds[1])
            head=mask.crop((0,bounds[1],raw.width,bounds[1]+round((bounds[3]-bounds[1])*.35))).getbbox()
            assert head,'Missing head silhouette'
            crop=raw.crop(bounds);size=(round(crop.width*scale),round(crop.height*scale))
            small=crop.resize(size,Image.Resampling.NEAREST);small.putdata([normalize(p) for p in small.get_flattened_data()])
            x=round(24-((head[0]+head[2])/2-bounds[0])*size[0]/crop.width);y=45-size[1]
            assert x>=1 and x+size[0]<=47 and y>=1,(name,index,size,x,y)
            frame=Image.new('RGBA',(48,48));frame.paste(small,(x,y))
            # Drop isolated one/two-pixel AI specks; preserve connected limbs and clothing.
            unseen={(px,py) for py in range(48) for px in range(48) if frame.getpixel((px,py))[3]}
            removed=0
            while unseen:
                seed=unseen.pop();component={seed};queue=[seed]
                while queue:
                    px,py=queue.pop()
                    for neighbor in ((px-1,py),(px+1,py),(px,py-1),(px,py+1)):
                        if neighbor in unseen:
                            unseen.remove(neighbor);component.add(neighbor);queue.append(neighbor)
                if len(component)<=2:
                    for point in component:frame.putpixel(point,(0,0,0,0));removed+=1
            pixels=list(frame.get_flattened_data());assert {p[:3] for p in pixels if p[3]}<=set(palette)
            assert {p[3] for p in pixels}=={0,255} and all(p[:3]==(0,0,0) for p in pixels if not p[3])
            assert frame.getchannel('A').getbbox()[3]==45,(name,index,'foot baseline')
            frame.save(output/f'{name}-f{index:02}.png',pnginfo=srgb);frames.append(frame)
            details.append({'frame':index,'sourceCell':box,'sourceBounds':bounds,'scale':scale,'destination':[x,y],
                'bounds':frame.getchannel('A').getbbox(),'footRow':44,'isolatedSpecksRemoved':removed,
                'pixelSha256':hashlib.sha256(frame.tobytes()).hexdigest()})
        assert len({f.tobytes() for f in frames})==count,(name,'Repeated frames')
        sheet=Image.new('RGBA',(48*count,48))
        for i,frame in enumerate(frames):sheet.paste(frame,(i*48,0))
        filename='chr_'+name.replace('-','_')+'.png';sheet.save(output/filename,pnginfo=srgb)
        sheet.resize((48*count*3,144),Image.Resampling.NEAREST).save(output/f'{name}-strip-3x.png',pnginfo=srgb)
        for zoom in (1,3):
            enlarged=[f.resize((48*zoom,48*zoom),Image.Resampling.NEAREST) for f in frames]
            enlarged[0].save(output/f'{name}-{zoom}x-animated.png',save_all=True,append_images=enlarged[1:],
                duration=1000/fps,loop=0,disposal=0,blend=0,pnginfo=srgb)
        report['clips'].append({'id':asset_id,'file':filename,'frames':count,'fps':fps,'loop':True,'cell':[48,48],
            'size':sheet.size,'source':path.relative_to(ROOT).as_posix(),'sourceSha256':sha(path),
            'sha256':sha(output/filename),'details':details})
    report['checks']={'dimensions':True,'palette32':True,'binaryAlpha':True,'cleanTransparentRGB':True,
        'distinct24Frames':True,'footRow44':True,'rightFacingMaster':True}
    report['limits']='AI poses normalized at a shared per-clip scale and head anchor. Motion/identity still require human review; no runtime NPC movement policy change.'
    (output/'preparation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('Prepared 5 NPC clips / 24 distinct frames')

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('review',type=Path);parser.add_argument('output',type=Path)
    args=parser.parse_args();prepare(args.review,args.output)
