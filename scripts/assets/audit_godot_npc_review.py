"""Check all candidate NPC frame pixels and preserve approved Town outside NPC cells."""
import argparse,hashlib,json
from pathlib import Path
from PIL import Image,ImageChops,ImageDraw
from prepare_godot_town_review import ROOT

def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def audit(review):
    review=review.resolve();preparation=json.loads((review/'preparation.json').read_text(encoding='utf-8'))
    result=json.loads((review/'captures/result.json').read_text(encoding='utf-8'))
    assert result['success'] and result['reviewOnly'] and result['renderer'].lower()!='dummy'
    assert sha(ROOT/'assets/catalogs/asset_catalog.json')==preparation['runtimeCatalogSha256']
    assert sha(review/'project/src/DeskTown.Godot/Bootstrap/NpcArtReview.cs')==preparation['probeSha256']
    for relative,digest in preparation['runtimeWorkingTreeFiles'].items():
        if relative!='assets/catalogs/asset_catalog.json':assert sha(review/'project'/relative)==digest
    expected={};pixels=0
    runtime=preparation.get('runtimeCatalog',False)
    catalog=json.loads((review/'project/assets/catalogs/asset_catalog.json').read_text(encoding='utf-8'))
    rows={row['id']:row for row in catalog['assets']}
    for row in preparation['candidateClips']:
        sheet_path=review/'project'/rows[row['id']]['resource'].removeprefix('res://');assert sha(sheet_path)==row['sha256']
        sheet=Image.open(sheet_path).convert('RGBA');clip=row['file'].removeprefix('chr_').removesuffix('.png').replace('_','-')
        for scale in (75,100,125,150):
            for frame in range(row['frames']):
                name=f'{clip}-{scale}-f{frame:02}.png';expected[name]=(160*scale//100,128*scale//100)
                if scale!=100:continue
                rendered=Image.open(review/'captures'/name).convert('RGBA')
                for y in range(48):
                    for x in range(48):
                        target=sheet.getpixel((frame*48+x,y))
                        for dy in range(2):
                            for dx in range(2):
                                pixel=rendered.getpixel((32+x*2+dx,16+y*2+dy))
                                assert pixel==target if target[3] else pixel[3]==0,(name,x,y)
                                pixels+=1
    for state in ('broken','repairing','complete'):
        for reading in ('idle','read'):
            for scale in (75,100,125,150):
                name=f'town-{state}-{reading}-{scale}.png';expected[name]=(1280*scale//100,720*scale//100)
                actual=Image.open(review/'captures'/name).convert('RGB')
                approved=Image.open(ROOT/f'docs/art/reviews/2026-10-12/town-delivery-v1/captures/proposed-{state}-{scale}.png').convert('RGB')
                difference=ImageChops.difference(actual,approved);draw=ImageDraw.Draw(difference)
                for x,y in ((536,456),(452,564)):
                    draw.rectangle((x*scale//100-2,y*scale//100-2,(x+96)*scale//100+2,(y+96)*scale//100+2),fill=(0,0,0))
                assert difference.getbbox() is None,('Town outside NPC cells changed',name,difference.getbbox())
    assert set(result['captures'])==set(expected) and len(expected)==120
    hashes={}
    for name,size in expected.items():
        path=review/'captures'/name;assert Image.open(path).size==size,name;hashes[name]=sha(path)
        if runtime:
            approved=ROOT/'docs/art/reviews/2026-10-12/npc-v1/godot-v1/captures'/name
            assert hashes[name]==sha(approved),('Approved NPC render changed',name)
    report={'success':True,'reviewOnly':True,'reviewKind':'npc','sourceRevision':preparation['sourceRevision'],'engine':result['engine'],
        'renderer':result['renderer'],'captures':120,'clips':5,'frames':24,'framePixelsChecked':pixels,
        'runtimeCatalog':runtime,'approvedNpcCapturesByteIdentical':runtime,
        'approvedTownOutsideNpcCellsPreserved':True,'mainCatalogUnchanged':True,'sha256':hashes,
        'limits':'Walk sheet review only; current runtime ambient movement is unchanged. No native input/DPI QA.' if runtime else 'Identity/motion/delivery approval pending. Walk sheet review only. No native input/DPI QA.'}
    (review/'verification.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({k:v for k,v in report.items() if k!='sha256'},ensure_ascii=True))

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('review',type=Path);audit(parser.parse_args().review)
