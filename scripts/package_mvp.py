"""Package a complete Windows MVP with persistent isolated play and quick art preview."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parents[1]


def digest(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--export', required=True, type=Path)
    parser.add_argument('--demo-saves', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path)
    parser.add_argument('--behavior-preview', type=Path)
    parser.add_argument('--audio-preview', type=Path)
    args = parser.parse_args()
    export = args.export.resolve()
    output = args.output.resolve()
    portable = output.with_suffix('.zip')
    if (ROOT / 'build').resolve() not in output.parents or output.exists() or portable.exists():
        parser.error('Choose a new output directory below build/')
    for relative in ('DeskTown.exe', 'data_DeskTown_windows_x86_64/DeskTown.dll'):
        if not (export / relative).is_file():
            parser.error('Missing export artifact: ' + relative)
    if subprocess.check_output(['git', 'status', '--porcelain', '--untracked-files=no'], cwd=ROOT).strip():
        parser.error('Commit tracked source changes before packaging to identify the exact build')
    revision = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip()
    output.mkdir(parents=True)
    files = [{'path': path.relative_to(export).as_posix(), 'bytes': path.stat().st_size, 'sha256': digest(path)}
        for path in sorted(export.rglob('*')) if path.is_file()]
    app_zip = output / f'DeskTown-v0.1-{revision[:8]}-win-x64.zip'
    with zipfile.ZipFile(app_zip, 'w', zipfile.ZIP_DEFLATED) as bundle:
        for row in files:
            bundle.write(export / row['path'], row['path'])
    with zipfile.ZipFile(app_zip) as bundle:
        for row in files:
            data = bundle.read(row['path'])
            assert len(data) == row['bytes'] and hashlib.sha256(data).hexdigest() == row['sha256'], row['path']
    catalog = json.loads((ROOT / 'assets/catalogs/asset_catalog.json').read_text(encoding='utf-8'))
    audio_count = sum(row['kind'] == 'audio' and row['status'] == 'production' for row in catalog['assets'])
    manifest = {'product': 'DeskTown Prototype v0.1 playable MVP', 'commit': revision,
        'godot': '4.7.2', 'dotnetSdk': '8.0.425', 'signed': False,
        'productionVisualAssets': 38, 'productionAudioAssets': audio_count, 'unassignedAudioAssets': 8 - audio_count,
        'postWorkshopDailyFocus': True,
        'knownIssues': ['Previous visual MVP confirmed by user; new character behavior visual review pending',
            'Clean-PC release QA pending', 'Ghost Focus disabled'], 'files': files}
    (output / 'build-manifest.json').write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
    (output / 'SHA256SUMS.txt').write_text(f'{digest(app_zip)}  {app_zip.name}\n', encoding='ascii')
    for pattern in ('*.ps1', '*.cmd', '*README.md'):
        for path in (ROOT / 'scripts/qa').glob(pattern):
            shutil.copyfile(path, output / path.name)
    shutil.copyfile(ROOT / 'docs/qa/WINDOWS_GATE.md', output / 'WINDOWS_GATE.md')
    for scenario in ('first-launch', 'workshop-repairing', 'workshop-reveal-pending', 'railway-teaser'):
        target = output / 'demo-saves' / scenario / 'save.json'
        target.parent.mkdir(parents=True)
        shutil.copyfile(args.demo_saves / scenario / 'save.json', target)
    source = ROOT / 'docs/art/reviews/2026-10-12/final-views-v1'
    preview = output / 'art-preview'
    preview.mkdir()
    shutil.copyfile(source / 'review.html', preview / 'review.html')
    shutil.copytree(source / 'captures', preview / 'captures')
    if args.behavior_preview:
        shutil.copytree(args.behavior_preview, output / 'behavior-preview')
    if args.audio_preview:
        shutil.copytree(args.audio_preview, output / 'audio-preview')
    with zipfile.ZipFile(portable, 'w', zipfile.ZIP_DEFLATED) as bundle:
        for path in sorted(output.rglob('*')):
            if path.is_file():
                bundle.write(path, path.relative_to(output).as_posix())
    with zipfile.ZipFile(portable) as bundle:
        assert bundle.testzip() is None
        assert not any('qa-sessions/' in name for name in bundle.namelist())
    report = {'commit': revision, 'exportFiles': len(files), 'zip': str(portable), 'sha256': digest(portable)}
    (output.parent / (output.name + '-package.json')).write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(report))


if __name__ == '__main__':
    main()
