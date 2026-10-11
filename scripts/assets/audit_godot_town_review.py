"""Verify isolated Town captures, preserved source hashes, and native nearest pixels."""
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def audit(review):
    review=review.resolve()
    preparation=json.loads((review/'preparation.json').read_text(encoding='utf-8'))
    result=json.loads((review/'captures/result.json').read_text(encoding='utf-8'))
    assert preparation['reviewOnly'] and result['success'] and result['reviewOnly']
    assert preparation['reviewKind']=='town' and result['renderer'].lower()!='dummy'
    assert sha(ROOT/'assets/catalogs/asset_catalog.json')==preparation['runtimeCatalogSha256']
    assert sha(review/'project/src/DeskTown.Godot/Bootstrap/TownArtReview.cs')==preparation['probeSha256']
    sheets={}
    runtime=preparation.get('runtimeCatalog',False)
    for relative,digest in preparation.get('runtimeWorkingTreeFiles',{}).items():
        assert sha(review/'project'/relative)==digest,relative
    for row in preparation['candidateAssets']:
        path=review/'project/assets/art'/('' if runtime else 'town-review-only')/row['file']
        assert sha(path)==row['sha256'],row['id']
        sheets[row['id']]=Image.open(path).convert('RGBA')
    expected={f'existing-{state}-100.png':(1280,720) for state in ('broken','repairing','complete')}
    expected.update({f'proposed-{state}-{scale}.png':(1280*scale//100,720*scale//100)
        for state in ('broken','repairing','complete') for scale in (75,100,125,150)})
    if runtime:
        expected={name:size for name,size in expected.items() if name.startswith('proposed-')}
    assert set(result['captures'])==set(expected)
    hashes={}
    checked=0
    for name,size in expected.items():
        path=review/'captures'/name
        assert Image.open(path).size==size,name
        hashes[name]=sha(path)
    # Unoccluded building pixels above the characters, each source pixel becomes 2x2.
    for frame,state in enumerate(('broken','repairing','complete')):
        rendered=Image.open(review/'captures'/f'proposed-{state}-100.png').convert('RGBA')
        for asset_id,origin,height,index in (
            ('BLD-HOU-001',(104,111),110,0),
            ('BLD-WRK-001',(365,111),110,frame),
            ('BLD-LCK-001',(489,14),128,0)):
            sheet=sheets[asset_id]
            width=160 if asset_id=='BLD-WRK-001' else 128
            for y in range(height):
                for x in range(width):
                    pixel=sheet.getpixel((index*width+x,y))
                    if pixel[3]!=255:
                        continue
                    for dy in range(2):
                        for dx in range(2):
                            assert rendered.getpixel(((origin[0]+x)*2+dx,(origin[1]+y)*2+dy))==pixel,(state,asset_id,x,y)
                            checked+=1
        # An unobstructed 32x32 terrain cell below the HUD, variant 1.
        grass=sheets['ENV-GRA-001']
        for y in range(32):
            for x in range(32):
                for dy in range(2):
                    for dx in range(2):
                        assert rendered.getpixel((x*2+dx,(32+y)*2+dy))==grass.getpixel((32+x,y)),(state,'grass',x,y)
                        checked+=1
    assert len({hashes[f'proposed-{state}-100.png'] for state in ('broken','repairing','complete')})==3
    report={'success':True,'reviewOnly':True,'sourceRevision':preparation['sourceRevision'],
        'engine':result['engine'],'renderer':result['renderer'],'captures':len(expected),'candidateSheets':11,
        'runtimeCatalog':runtime,
        'nearestBuildingAndTerrainPixelsChecked':checked,'mainCatalogUnchanged':True,
        'candidateSourceHashesPreserved':True,'probeHashPreserved':True,'sha256':hashes,
        'limits':'Partial unoccluded pixel checks plus visual review. No native window/DPI/input QA or production assignment.'}
    if runtime:
        approved=ROOT/'docs/art/reviews/2026-10-12/town-complete-v1/godot-v1/captures'
        report['approvedRenderBytesMatch']=all((approved/name).read_bytes()==(review/'captures'/name).read_bytes() for name in expected)
        assert report['approvedRenderBytesMatch'],'Runtime render differs from approved review'
        report['limits']='Approved Town runtime art and layout; NPC/effects/UI unfinished. No native input/DPI QA.'
    (review/'verification.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({k:v for k,v in report.items() if k!='sha256'},ensure_ascii=True))

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('review',type=Path)
    audit(parser.parse_args().review)
