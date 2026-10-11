"""Isolate Town candidates and an explicit proposed layout; never assign runtime art."""
import argparse
import hashlib
import io
import json
from pathlib import Path
import shutil
import subprocess
import zipfile

ROOT=Path(__file__).resolve().parents[2]
REVIEW=ROOT/'docs/art/reviews/2026-10-12/town-complete-v1'

def prepare(output, runtime_catalog=False):
    output=output.resolve()
    if (ROOT/'build').resolve() not in output.parents or output.exists():
        raise ValueError('Choose a new directory below build/')
    # The original layout comparison remains replayable after Town delivery.
    revision=subprocess.check_output(['git','rev-parse','HEAD' if runtime_catalog else '8453904'],cwd=ROOT,text=True).strip()
    archive=subprocess.check_output(['git','archive','--format=zip',revision],cwd=ROOT)
    output.mkdir(parents=True)
    project=output/'project'
    with zipfile.ZipFile(io.BytesIO(archive)) as bundle: bundle.extractall(project)
    prepared=REVIEW/'prepared-v2'
    candidates=json.loads((prepared/'preparation.json').read_text(encoding='utf-8'))
    catalog_path=project/'assets/catalogs/asset_catalog.json'
    runtime_hashes={}
    if runtime_catalog:
        for relative in ('scenes/town/TownScene.tscn','src/DeskTown.Godot/Presentation/TownScene.cs',
                         'src/DeskTown.Godot/Display/TownEnvironment.cs','src/DeskTown.Godot/Bootstrap/CatalogArtSmoke.cs',
                         'assets/catalogs/asset_catalog.json'):
            destination=project/relative
            destination.parent.mkdir(parents=True,exist_ok=True)
            shutil.copyfile(ROOT/relative,destination)
            runtime_hashes[relative]=hashlib.sha256(destination.read_bytes()).hexdigest()
    catalog=json.loads(catalog_path.read_text(encoding='utf-8'))
    rows={row['id']:row for row in catalog['assets']}
    for asset in candidates['assets']:
        if runtime_catalog:
            relative=rows[asset['id']]['resource'].removeprefix('res://')
            assert rows[asset['id']]['status']=='production'
            target=project/relative;target.parent.mkdir(parents=True,exist_ok=True)
            shutil.copyfile(ROOT/relative,target)
            assert hashlib.sha256(target.read_bytes()).hexdigest()==asset['sha256']
            continue
        relative='assets/art/town-review-only/'+asset['file']
        target=project/relative; target.parent.mkdir(parents=True,exist_ok=True)
        shutil.copyfile(prepared/asset['file'],target)
        assert hashlib.sha256(target.read_bytes()).hexdigest()==asset['sha256']
        rows[asset['id']].update(status='production',resource='res://'+relative,pivot=asset['pivot'],
            provenance={'creator':'OpenAI image_gen + approved Python pixel preparation',
                        'license':'Isolated review-only fixture; runtime delivery approval pending',
                        'source':(prepared/asset['file']).relative_to(ROOT).as_posix(),
                        'importDate':'2026-10-12','manifestRevision':catalog['manifestRevision']})
    if not runtime_catalog:
        catalog_path.write_text(json.dumps(catalog,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    probe=project/'src/DeskTown.Godot/Bootstrap/TownArtReview.cs'
    shutil.copyfile(ROOT/'scripts/assets/TownArtReview.cs.txt',probe)
    scene=project/'scenes/town/ArtReview.tscn'
    scene.write_text('[gd_scene load_steps=2 format=3]\n[ext_resource type="Script" path="res://src/DeskTown.Godot/Bootstrap/TownArtReview.cs" id="1"]\n[node name="ReviewOnly" type="Node"]\nscript = ExtResource("1")\n',encoding='utf-8')
    config=project/'project.godot'
    config.write_text(config.read_text(encoding='utf-8').replace('config/name="DeskTown"',
        'config/name="DeskTownArtReview-'+output.name+'"').replace('res://scenes/app/AppRoot.tscn','res://scenes/town/ArtReview.tscn'),encoding='utf-8')
    report={'reviewOnly':True,'reviewKind':'town','expectedCaptures':12 if runtime_catalog else 15,'sourceRevision':revision,
            'runtimeCatalog':runtime_catalog,'runtimeWorkingTreeFiles':runtime_hashes,
            'personalSaveAccess':False,'engineUserDirectoryIsolated':True,'candidateAssets':candidates['assets'],
            'runtimeCatalogSha256':hashlib.sha256((ROOT/'assets/catalogs/asset_catalog.json').read_bytes()).hexdigest(),
            'probeSha256':hashlib.sha256(probe.read_bytes()).hexdigest(),
            'layoutProposal':'640x360 logical world at 2x in the 1280x720 Town; HUD stays at 1x. Review-only.'}
    (output/'preparation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(project)

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__); parser.add_argument('output',type=Path)
    parser.add_argument('--runtime-catalog',action='store_true')
    args=parser.parse_args();prepare(args.output,args.runtime_catalog)
