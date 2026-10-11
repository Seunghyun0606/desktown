"""Publish a verified Town review once, preserving PNG bytes and sanitizing local logs."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil

ROOT=Path(__file__).resolve().parents[2]
DESTINATION=ROOT/'docs/art/reviews/2026-10-12/town-complete-v1/godot-v1'

def publish(review):
    review=review.resolve()
    if (ROOT/'build').resolve() not in review.parents:
        raise ValueError('Review must be below build/')
    report=json.loads((review/'verification.json').read_text(encoding='utf-8'))
    assert report['success'] and report['reviewOnly'] and report['captures']==15
    assert hashlib.sha256((ROOT/'assets/catalogs/asset_catalog.json').read_bytes()).hexdigest()==json.loads(
        (review/'preparation.json').read_text(encoding='utf-8'))['runtimeCatalogSha256']
    logs=[p for p in review.glob('logs-*') if json.loads((p/'result.json').read_text(encoding='utf-8')).get('success')]
    assert len(logs)==1,'Expected one successful capture log set'
    for name,digest in report['sha256'].items():
        assert hashlib.sha256((review/'captures'/name).read_bytes()).hexdigest()==digest,name
    DESTINATION.mkdir(exist_ok=False)
    shutil.copytree(review/'captures',DESTINATION/'captures')
    for name in ('preparation.json','verification.json'):
        shutil.copyfile(review/name,DESTINATION/name)
    sdk=str(Path(os.environ['LOCALAPPDATA'])/'DeskTownBuildTools/dotnet-8.0.425')
    def clean(text):
        for absolute,label in ((str(ROOT),'[workspace]'),(sdk,'[local-sdk]')):
            text=text.replace(absolute,label).replace(absolute.replace('\\','/'),label)
        return text
    for name in ('sdk.log','engine.log','catalog.log','build.log','import.log','capture.log'):
        (DESTINATION/name).write_text(clean((logs[0]/name).read_text(encoding='utf-8')),encoding='utf-8')
    result=json.loads((logs[0]/'result.json').read_text(encoding='utf-8'))
    result['logs']='.'
    (DESTINATION/'result.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(f'Published {len(report["sha256"])} unchanged render PNGs to {DESTINATION.relative_to(ROOT)}')

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('review',type=Path)
    publish(parser.parse_args().review)
