"""Prepare a disposable project with ten unapproved art candidates and draw-only proposals."""
import argparse,hashlib,json,shutil
from pathlib import Path
from prepare_godot_npc_review import prepare as prepare_npc, ROOT

def prepare(output):
 prepare_npc(output,True);output=output.resolve();project=output/'project'
 prepared=ROOT/'docs/art/reviews/2026-10-12/remaining-art-v1/prepared-v2'
 candidates=json.loads((prepared/'preparation.json').read_text(encoding='utf-8'))
 path=project/'assets/catalogs/asset_catalog.json';catalog=json.loads(path.read_text(encoding='utf-8'));rows={a['id']:a for a in catalog['assets']}
 for asset in candidates['assets']:
  relative='assets/art/remaining-review-only/'+asset['file'];target=project/relative;target.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(prepared/asset['file'],target)
  assert hashlib.sha256(target.read_bytes()).hexdigest()==asset['sha256']
  rows[asset['id']].update(status='production',resource='res://'+relative,pivot=asset['pivot'],provenance={'creator':'OpenAI image_gen + approved Python normalization','license':'Review fixture only; final delivery and runtime placement pending','source':(prepared/asset['file']).relative_to(ROOT).as_posix(),'importDate':'2026-10-12','manifestRevision':'v0.1'})
 path.write_text(json.dumps(catalog,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
 probe=project/'src/DeskTown.Godot/Bootstrap/RemainingArtReview.cs';shutil.copyfile(ROOT/'scripts/assets/RemainingArtReview.cs.txt',probe)
 scene=project/'scenes/town/ArtReview.tscn';scene.write_text('[gd_scene load_steps=2 format=3]\n[ext_resource type="Script" path="res://src/DeskTown.Godot/Bootstrap/RemainingArtReview.cs" id="1"]\n[node name="ReviewOnly" type="Node"]\nscript = ExtResource("1")\n',encoding='utf-8')
 report=json.loads((output/'preparation.json').read_text(encoding='utf-8'));report.update(reviewKind='remaining',expectedCaptures=204,runtimeCatalog=False,candidateAssets=candidates['assets'],probeSha256=hashlib.sha256(probe.read_bytes()).hexdigest(),layoutProposal='Review-only Town effects, Companion contact/steam/tabletop tool, FirstLaunch logo, Discovery map, Focus option icons. No AppRoot, production writes, saves or window/input change.')
 (output/'preparation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8');print(project)

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);prepare(p.parse_args().output)
