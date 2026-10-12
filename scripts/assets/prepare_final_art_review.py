"""Capture final production views in a disposable project; never launch personal saves."""
import argparse,hashlib,json,shutil
from pathlib import Path
from prepare_godot_remaining_review import prepare as prepare_runtime, ROOT

def prepare(output):
 prepare_runtime(output,True);output=output.resolve();project=output/'project'
 probe=project/'src/DeskTown.Godot/Bootstrap/FinalArtReview.cs';shutil.copyfile(ROOT/'scripts/assets/FinalArtReview.cs.txt',probe)
 (project/'scenes/town/ArtReview.tscn').write_text('[gd_scene load_steps=2 format=3]\n[ext_resource type="Script" path="res://src/DeskTown.Godot/Bootstrap/FinalArtReview.cs" id="1"]\n[node name="FinalReviewOnly" type="Node"]\nscript = ExtResource("1")\n',encoding='utf-8')
 report=json.loads((output/'preparation.json').read_text(encoding='utf-8'));report.update(reviewKind='final',expectedCaptures=55,probeSha256=hashlib.sha256(probe.read_bytes()).hexdigest(),layoutProposal='Final approved production views, actual Reward timing and shared Ghost art. No personal saves or native input QA.')
 (output/'preparation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8');print(project)

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);prepare(p.parse_args().output)
