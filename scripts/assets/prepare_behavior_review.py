"""Capture current character behavior in a disposable project with approved art."""
import argparse
import hashlib
import json
import shutil
from pathlib import Path
from prepare_godot_remaining_review import prepare as prepare_runtime, ROOT

def prepare(output):
    prepare_runtime(output, True)
    output = output.resolve()
    project = output / 'project'
    hashes = {}
    for path in (ROOT / 'src').rglob('*.cs'):
        if any(part in ('bin', 'obj') for part in path.relative_to(ROOT).parts):
            continue
        relative = path.relative_to(ROOT)
        target = project / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(path, target)
        hashes[relative.as_posix()] = hashlib.sha256(target.read_bytes()).hexdigest()
    probe = project / 'src/DeskTown.Godot/Bootstrap/BehaviorReview.cs'
    shutil.copyfile(ROOT / 'scripts/assets/BehaviorReview.cs.txt', probe)
    (project / 'scenes/town/ArtReview.tscn').write_text(
        '[gd_scene load_steps=2 format=3]\n[ext_resource type="Script" path="res://src/DeskTown.Godot/Bootstrap/BehaviorReview.cs" id="1"]\n'
        '[node name="BehaviorReviewOnly" type="Node"]\nscript = ExtResource("1")\n', encoding='utf-8')
    report = json.loads((output / 'preparation.json').read_text(encoding='utf-8'))
    report['runtimeWorkingTreeFiles'].update(hashes)
    report.update(reviewKind='behavior', expectedCaptures=741,
        probeSha256=hashlib.sha256(probe.read_bytes()).hexdigest(),
        layoutProposal='Continuous approved-sprite routes and Work cadence only; no gameplay/save mutation')
    (output / 'preparation.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print(project)

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('output', type=Path)
    prepare(parser.parse_args().output)
