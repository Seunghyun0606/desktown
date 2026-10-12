"""Audit candidate pixel fidelity, approved scene preservation and 204 GPU capture files."""
import argparse,hashlib,json
from pathlib import Path
from PIL import Image,ImageChops,ImageDraw
from prepare_godot_town_review import ROOT

def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def audit(review):
 prep=json.loads((review/'preparation.json').read_text(encoding='utf-8'));result=json.loads((review/'captures/result.json').read_text(encoding='utf-8'))
 assert result['success'] and result['reviewOnly'] and not result['productionAssignment'] and not result['personalSaveAccess']
 assert result['renderer'].lower()!='dummy' and sha(ROOT/'assets/catalogs/asset_catalog.json')==prep['runtimeCatalogSha256']
 for relative,digest in prep['runtimeWorkingTreeFiles'].items():
  if relative!='assets/catalogs/asset_catalog.json':assert sha(review/'project'/relative)==digest,relative
 assert sha(review/'project/src/DeskTown.Godot/Bootstrap/RemainingArtReview.cs')==prep['probeSha256']
 expected={};pixels=0
 for row in prep['candidateAssets']:
  path=review/'project/assets/art/remaining-review-only'/row['file'];assert sha(path)==row['sha256']
  sheet=Image.open(path).convert('RGBA');w,h=row['frameSize']
  for scale in (75,100,125,150):
   for frame in range(row['frames']):
    name=f'asset-{row["name"]}-{scale}-f{frame:02}.png';expected[name]=((w+16)*scale//100,(h+16)*scale//100)
    if scale!=100:continue
    actual=Image.open(review/'captures'/name).convert('RGBA')
    for y in range(h):
     for x in range(w):
      target=sheet.getpixel((w*frame+x,y));pixel=actual.getpixel((8+x,8+y))
      assert pixel==target if target[3] else pixel[3]==0,(name,x,y);pixels+=1
 def preserve(name,baseline,rectangles,scale):
  actual=Image.open(review/'captures'/name).convert('RGB');approved=Image.open(baseline).convert('RGB')
  difference=ImageChops.difference(actual,approved);draw=ImageDraw.Draw(difference)
  for x,y,w,h in rectangles:draw.rectangle((x*scale//100-2,y*scale//100-2,(x+w)*scale//100+2,(y+h)*scale//100+2),fill=(0,0,0))
  assert difference.getbbox() is None,(name,'Unexpected scene change',difference.getbbox())
 for reduced in (False,True):
  for scale in (75,100,125,150):
   for frame in range(1 if reduced else 4):
    name=f'town-{"reduced" if reduced else "effect"}-{scale}-f{frame:02}.png';expected[name]=(1280*scale//100,720*scale//100)
    preserve(name,ROOT/f'docs/art/reviews/2026-10-12/npc-delivery-v1/captures/town-complete-idle-{scale}.png',[(922,236,64,64),(624,588,64,68),(192,592,64,64),(1128,556,64,64)],scale)
    for pose in ('work','rest'):
     name=f'companion-{pose}-{"reduced" if reduced else "effect"}-{scale}-f{frame:02}.png';expected[name]=(360*scale//100,200*scale//100)
     # Preserve sprite/body art. Header native control glyph hinting is outside
     # this art audit and differs by a physical pixel at 150% in this probe.
     preserve(name,ROOT/f'docs/art/reviews/2026-10-11/companion-godot-v1/captures/{pose}-{scale}-f{"02" if pose=="work" else "00"}.png',[(0,0,360,30),(52,123,38,38),(198,103,32,32),(137,119,16,16) if pose=='work' else (129,116,16,16)],scale)
 for kind in ('first-launch','discovery','focus'):
  for scale in (75,100,125,150):expected[f'ui-{kind}-{scale}.png']=(1280*scale//100,720*scale//100)
 assert set(expected)==set(result['captures']) and len(expected)==204
 hashes={}
 for name,size in expected.items():
  p=review/'captures'/name;assert Image.open(p).size==size,name;hashes[name]=sha(p)
 report={'success':True,'reviewOnly':True,'reviewKind':'remaining','sourceRevision':prep['sourceRevision'],
         'engine':result['engine'],'renderer':result['renderer'],'captures':204,'assets':10,'cells':33,'framePixelsChecked':pixels,
         'mainCatalogUnchanged':True,'approvedTownAndCompanionArtOutsideProposalPreserved':True,
         'companionHeaderControlsExcludedFromArtPreservation':True,'sha256':hashes,
         'limits':'Candidate sheets and draw-only proposals pending art/delivery/placement approval. Not native input, timing, Reduced Motion persistence or DPI QA.'}
 (review/'verification.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8');print(json.dumps({k:v for k,v in report.items() if k!='sha256'},ensure_ascii=True))

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('review',type=Path);audit(p.parse_args().review.resolve())
