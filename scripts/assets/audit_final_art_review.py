"""Verify production inputs, final GPU view dimensions/hashes and reward callback checks."""
import argparse,hashlib,json
from pathlib import Path
from PIL import Image
from prepare_godot_town_review import ROOT
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def audit(review):
 prep=json.loads((review/'preparation.json').read_text(encoding='utf-8'));result=json.loads((review/'captures/result.json').read_text(encoding='utf-8'))
 assert result['success'] and result['runtimeCatalog'] and result['rewardNormalAndReducedChecksPassed'] and not result['personalSaveAccess']
 assert result['renderer'].lower()!='dummy' and sha(ROOT/'assets/catalogs/asset_catalog.json')==prep['runtimeCatalogSha256']
 for relative,digest in prep['runtimeWorkingTreeFiles'].items():assert sha(review/'project'/relative)==digest,relative
 assert sha(review/'project/src/DeskTown.Godot/Bootstrap/FinalArtReview.cs')==prep['probeSha256']
 catalog=json.loads((review/'project/assets/catalogs/asset_catalog.json').read_text(encoding='utf-8'));visual=[a for a in catalog['assets'] if a['kind']=='visual']
 assert len(visual)==38 and all(a['status']=='production' for a in visual)
 for a in visual:assert sha(review/'project'/a['resource'].removeprefix('res://'))==a['provenance']['sha256'],a['id']
 expected={}
 for scale in (75,100,125,150):
  for name in ('town-broken','town-repairing','town-complete','town-reduced','ui-first-launch','ui-focus','ui-discovery','reward-before','reward-after'):expected[f'{name}-{scale}.png']=(1280*scale//100,720*scale//100)
  for pose in ('work','rest'):
   for motion in ('normal','reduced'):expected[f'companion-{pose}-{motion}-{scale}.png']=(360*scale//100,200*scale//100)
 for opacity in (40,65,85):expected[f'ghost-{opacity}.png']=(220,180)
 assert set(result['captures'])==set(expected) and len(expected)==55
 hashes={}
 for name,size in expected.items():
  path=review/'captures'/name;assert Image.open(path).size==size,name;hashes[name]=sha(path)
 # Root modulation applies to each draw. Overlaps blend normally, so verify
 # the whole composite alpha instead of claiming uniform window opacity.
 def art(name,box=None):
  im=Image.open(review/'project/assets/art'/name).convert('RGBA');return im.crop(box) if box else im
 layers=[(art('prop_character_shadow.png'),83,126),
         (art('prop_workbench.png'),110,76),
         (art('chr_mina_work.png',(96,0,144,48)),75,88),
         (art('fx_work_spark.png',(16,0,32,16)),112,104)]
 pixels=0
 for opacity in (40,65,85):
  actual=Image.open(review/'captures'/f'ghost-{opacity}.png').convert('RGBA')
  for y in range(180):
   for x in range(220):
    alpha=0.0
    for image,ox,oy in layers:
     if 0<=x-ox<image.width and 0<=y-oy<image.height:
      layer=image.getpixel((x-ox,y-oy))[3]/255*opacity/100
      alpha=layer+alpha*(1-layer)
    assert abs(actual.getpixel((x,y))[3]-round(255*alpha))<=1,(opacity,x,y)
    pixels+=1
 report={'success':True,'reviewOnly':True,'reviewKind':'final','runtimeCatalog':True,'sourceRevision':prep['sourceRevision'],
         'engine':result['engine'],'renderer':result['renderer'],'captures':55,'productionVisuals':38,
         'rewardNormalAndReducedChecksPassed':True,'ghostCompositeAlphaPixelsChecked':pixels,'mainCatalogUnchanged':True,'sha256':hashes,
         'limits':'Actual production views in isolated SubViewports; not native input, DPI, Windows lifecycle or release approval. Audio 8 unassigned; QA-PIP-001 deferred.'}
 (review/'verification.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8');print(json.dumps({k:v for k,v in report.items() if k!='sha256'},ensure_ascii=True))
if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('review',type=Path);audit(p.parse_args().review.resolve())
