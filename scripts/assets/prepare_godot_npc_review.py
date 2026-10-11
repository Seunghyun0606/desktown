"""Isolate approved Town runtime and five candidate NPC sheets for GPU review."""
import argparse,hashlib,json,shutil
from pathlib import Path
from prepare_godot_town_review import prepare as prepare_town, ROOT

def prepare(output):
    prepare_town(output,True)
    output=output.resolve();project=output/'project'
    prepared=ROOT/'docs/art/reviews/2026-10-12/npc-v1/prepared-v2'
    candidates=json.loads((prepared/'preparation.json').read_text(encoding='utf-8'))
    catalog_path=project/'assets/catalogs/asset_catalog.json'
    catalog=json.loads(catalog_path.read_text(encoding='utf-8'));rows={a['id']:a for a in catalog['assets']}
    for clip in candidates['clips']:
        relative='assets/art/npc-review-only/'+clip['file'];target=project/relative;target.parent.mkdir(parents=True,exist_ok=True)
        shutil.copyfile(prepared/clip['file'],target)
        assert hashlib.sha256(target.read_bytes()).hexdigest()==clip['sha256']
        rows[clip['id']].update(status='production',resource='res://'+relative,pivot=[24,44],
            provenance={'creator':'OpenAI image_gen + approved Python normalization','license':'Review-only; NPC delivery pending',
                'source':(prepared/clip['file']).relative_to(ROOT).as_posix(),'importDate':'2026-10-12','manifestRevision':'v0.1'})
    catalog_path.write_text(json.dumps(catalog,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    probe=project/'src/DeskTown.Godot/Bootstrap/NpcArtReview.cs'
    shutil.copyfile(ROOT/'scripts/assets/NpcArtReview.cs.txt',probe)
    scene=project/'scenes/town/ArtReview.tscn'
    scene.write_text('[gd_scene load_steps=2 format=3]\n[ext_resource type="Script" path="res://src/DeskTown.Godot/Bootstrap/NpcArtReview.cs" id="1"]\n[node name="ReviewOnly" type="Node"]\nscript = ExtResource("1")\n',encoding='utf-8')
    report=json.loads((output/'preparation.json').read_text(encoding='utf-8'))
    report.update(reviewKind='npc',expectedCaptures=120,runtimeCatalog=False,candidateClips=candidates['clips'],
        probeSha256=hashlib.sha256(probe.read_bytes()).hexdigest(),
        layoutProposal='Approved Town runtime unchanged; five NPC sheets only in isolated fixture')
    (output/'preparation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('output',type=Path);prepare(parser.parse_args().output)
