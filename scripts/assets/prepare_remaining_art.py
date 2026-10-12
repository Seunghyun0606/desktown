"""Normalize ten remaining visual candidates; keep production catalog untouched."""
import argparse, hashlib, json, shutil
from pathlib import Path
from PIL import Image, PngImagePlugin

ROOT=Path(__file__).resolve().parents[2]
PIVOTS={'smoke':[3,29],'campfire':[16,29],'grass':[16,29],'spark':[8,8],'steam':[8,13],
        'decor':[16,29],'logo':[0,0],'icons':[8,8],'map':[48,61],'tool':[16,29]}
SPECS={
 'smoke':('FX-SMK-001',4,4,32,32,26,28,4,True,'effects'),
 'campfire':('FX-FIR-001',4,4,32,32,26,27,6,True,'both'),
 'grass':('FX-GRS-001',2,2,32,32,20,14,2,True,'core'),
 'spark':('FX-SPK-001',4,4,16,16,12,12,8,False,'effects'),
 'steam':('FX-TEA-001',4,4,16,16,6,11,4,True,'effects'),
 'decor':('CMP-DEC-001',2,2,32,32,27,27,None,False,'core'),
 'logo':('UI-LOG-001',1,1,256,96,224,72,None,False,'core'),
 'icons':('UI-ICO-001',9,3,16,16,13,13,None,False,'core'),
 'map':('ITM-MAP-001',1,1,96,64,90,58,None,False,'core')}

def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def prepare(review,output):
 output.mkdir(parents=True,exist_ok=False)
 core_path=ROOT/'docs/art/palettes/desktown-v01-32.json'
 core=[tuple(c) for c in json.loads(core_path.read_text(encoding='utf-8'))['colorsRGB']]
 effects_path=ROOT/'docs/art/palettes/desktown-v01-effects-6.gpl'
 effects=[tuple(map(int,line.split()[:3])) for line in effects_path.read_text(encoding='utf-8').splitlines() if line.strip() and line.strip()[0].isdigit()]
 srgb=PngImagePlugin.PngInfo();srgb.add(b'sRGB',b'\x00')
 catalog=json.loads((ROOT/'assets/catalogs/asset_catalog.json').read_text(encoding='utf-8'));rows={a['id']:a for a in catalog['assets']}
 report={'reviewOnly':True,'runtimeAssigned':False,'pendingHumanApproval':True,'corePaletteSha256':sha(core_path),'effectPaletteSha256':sha(effects_path),'assets':[]}
 for name,(asset_id,count,columns,cw,ch,mw,mh,fps,loop,pal) in SPECS.items():
  path=review/'sources'/f'{name}.png';source=Image.open(path).convert('RGBA');gridrows=count//columns
  cells=[];boxes=[];bounds=[]
  icon_boxes=None
  if name=='icons':
   # AI source gutters are not guaranteed to equal the requested grid. Split
   # actual alpha bands so an icon cannot be clipped at a guessed cell boundary.
   def bands(projection):
    indices=[i for i,v in enumerate(projection) if v];runs=[];start=previous=indices[0]
    for value in indices[1:]:
     if value-previous>12:runs.append((start,previous+1));start=value
     previous=value
    runs.append((start,previous+1));assert len(runs)==3,runs;return runs
   xs,ys=source.getchannel('A').point(lambda a:255 if a>=128 else 0).getprojection()
   icon_boxes=[(left,top,right,bottom) for top,bottom in bands(ys) for left,right in bands(xs)]
  for i in range(count):
   x=i%columns;y=i//columns;box=icon_boxes[i] if icon_boxes else (x*source.width//columns,y*source.height//gridrows,(x+1)*source.width//columns,(y+1)*source.height//gridrows)
   cell=source.crop(box);mask=cell.getchannel('A').point(lambda a:255 if a>=128 else 0);b=mask.getbbox();assert b,(name,i)
   cells.append(cell);boxes.append(box);bounds.append(b)
  # Animation cells share one crop and scale so peak/fade retain their authored size.
  union=(min(b[0] for b in bounds),min(b[1] for b in bounds),max(b[2] for b in bounds),max(b[3] for b in bounds))
  frames=[];details=[];palette=effects if pal=='effects' else core+effects if pal=='both' else core
  cache={}
  for i,cell in enumerate(cells):
   b=bounds[i] if name in ('icons','logo','map','decor') else union
   crop=cell.crop(b);scale=min(mw/crop.width,mh/crop.height);small=crop.resize((max(1,round(crop.width*scale)),max(1,round(crop.height*scale))),Image.Resampling.NEAREST)
   data=[]
   for r,g,blue,a in small.get_flattened_data():
    if a<128:data.append((0,0,0,0));continue
    rgb=(r,g,blue)
    if rgb not in cache:cache[rgb]=min(palette,key=lambda c:2*(r-c[0])**2+4*(g-c[1])**2+3*(blue-c[2])**2)
    data.append((*cache[rgb],255))
   small.putdata(data);x=(cw-small.width)//2;y=(ch-small.height)//2
   if name in ('campfire','grass','decor','steam'):
    bb=small.getbbox();assert bb
    y=ch-3-bb[3]
    root=small.getchannel('A').crop((0,max(bb[1],bb[3]-2),small.width,bb[3])).getbbox()
    if root:x=round(cw/2-(root[0]+root[2])/2)
   frame=Image.new('RGBA',(cw,ch));frame.paste(small,(x,y))
   assert frame.getbbox() and len({a for *_,a in frame.get_flattened_data()})<=2
   frames.append(frame);details.append({'sourceCell':boxes[i],'sourceCrop':b,'normalizedSize':small.size,'destination':[x,y],'bounds':frame.getbbox()})
  # Lock grounded bases; phase changes remain above these rows.
  base_rows=6 if name=='campfire' else 2 if name=='grass' else 0
  if base_rows:
   strip=frames[0].crop((0,ch-3-base_rows,cw,ch-3))
   for frame in frames[1:]:frame.paste(strip,(0,ch-3-base_rows))
  if fps:assert len({f.tobytes() for f in frames})==count,(name,'Duplicate phases')
  sheet=Image.new('RGBA',(cw*count,ch))
  for i,f in enumerate(frames):
   sheet.paste(f,(cw*i,0));f.save(output/f'{name}-f{i:02}.png',pnginfo=srgb)
  file=rows[asset_id]['filename'];assert sheet.size==tuple(rows[asset_id]['size']);sheet.save(output/file,pnginfo=srgb)
  sheet.resize((sheet.width*3,sheet.height*3),Image.Resampling.NEAREST).save(output/f'{name}-strip-3x.png',pnginfo=srgb)
  if fps:
   for factor in (1,3):
    animation=[f.resize((cw*factor,ch*factor),Image.Resampling.NEAREST) for f in frames]
    animation[0].save(output/f'{name}-loop-{factor}x.png',save_all=True,append_images=animation[1:],duration=round(1000/fps),loop=0 if loop else 1,disposal=0,blend=0,pnginfo=srgb)
  report['assets'].append({'id':asset_id,'name':name,'file':file,'sha256':sha(output/file),'source':path.relative_to(ROOT).as_posix(),'sourceSha256':sha(path),'frames':count,'frameSize':[cw,ch],'pivot':PIVOTS[name],'fps':fps,'loop':loop,'palette':pal,'stableBaseRows':base_rows,'details':details})
 # Previously approved tool directions; palette normalize, do not attach to Mina's baked hammer.
 tool=ROOT/'docs/art/reviews/2026-10-11/companion-props-v1/prepared-v1/prop_mina_tool.png'
 image=Image.open(tool).convert('RGBA');image.putdata([(*min(core,key=lambda c:sum((p[i]-c[i])**2 for i in range(3))),255) if p[3]>=128 else (0,0,0,0) for p in image.get_flattened_data()]);assert image.size==(64,32)
 file='prop_mina_tool.png';image.save(output/file,pnginfo=srgb)
 image.resize((192,96),Image.Resampling.NEAREST).save(output/'tool-strip-3x.png',pnginfo=srgb)
 for i in range(2):image.crop((32*i,0,32*(i+1),32)).save(output/f'tool-f{i:02}.png',pnginfo=srgb)
 report['assets'].append({'id':'CMP-TOL-001','name':'tool','file':file,'sha256':sha(output/file),'source':tool.relative_to(ROOT).as_posix(),'sourceSha256':sha(tool),'frames':2,'frameSize':[32,32],'pivot':PIVOTS['tool'],'fps':None,'loop':False,'palette':'core','note':'Approved prop direction reused; standalone tabletop prop only, no duplicate handheld tool.'})
 assert len(report['assets'])==10
 (output/'preparation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
 print('Prepared 10 candidates, 33 cells; production catalog unchanged.')

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('review',type=Path);p.add_argument('output',type=Path);args=p.parse_args();prepare(args.review.resolve(),args.output.resolve())
