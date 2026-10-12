"""Exercise real Windows/Godot window transitions using an isolated save and project.

The probe calls the shipped controller commands and reads native window state.
It does not synthesize desktop input or certify typing, DPI or taskbar appearance.
"""
import argparse
import io
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import zipfile

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'scripts'))
from export_windows import run_checked


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', required=True, type=Path)
    parser.add_argument('--godot', required=True, type=Path)
    parser.add_argument('--dotnet-root', required=True, type=Path)
    parser.add_argument('--demo-save', required=True, type=Path, help='first-launch fixture to copy')
    parser.add_argument('--revision', default='HEAD')
    parser.add_argument('--baseline', action='store_true', help='Use committed controller to reproduce the regression')
    args = parser.parse_args()
    output = args.output.resolve()
    if os.name != 'nt' or (ROOT / 'build').resolve() not in output.parents or output.exists():
        parser.error('Choose a new output directory below build/ on Windows')
    output.mkdir(parents=True)
    project = output / 'project'
    archive = subprocess.check_output(['git', 'archive', '--format=zip', args.revision, 'src', 'scenes', 'assets', 'tests', 'tools/DeskTown.DemoSaves',
        'DeskTown.sln', 'DeskTown.csproj', 'project.godot', 'export_presets.cfg', 'global.json', 'Directory.Build.props', 'Directory.Packages.props'], cwd=ROOT)
    with zipfile.ZipFile(io.BytesIO(archive)) as bundle:
        bundle.extractall(project)
    if not args.baseline:
        relative = 'src/DeskTown.Godot/Bootstrap/PrototypeAppController.cs'
        shutil.copyfile(ROOT / relative, project / relative)
    config = project / 'project.godot'
    config.write_text(config.read_text(encoding='utf-8').replace('config/name="DeskTown"',
        'config/name="DeskTownWindowProbe-' + output.name + '"'), encoding='utf-8')
    shutil.copyfile(ROOT / 'scripts/qa/WindowPresentationProbe.cs.txt',
        project / 'src/DeskTown.Godot/Bootstrap/WindowPresentationProbe.cs')
    scene = project / 'scenes/app/AppRoot.tscn'
    content = scene.read_text(encoding='utf-8').replace('load_steps=12', 'load_steps=13')
    content = content.replace('[node name="AppRoot"',
        '[ext_resource type="Script" path="res://src/DeskTown.Godot/Bootstrap/WindowPresentationProbe.cs" id="12_probe"]\n\n[node name="AppRoot"', 1)
    scene.write_text(content + '\n[node name="WindowProbe" type="Node" parent="."]\nscript = ExtResource("12_probe")\n', encoding='utf-8')
    save = output / 'save.json'
    shutil.copyfile(args.demo_save, save)
    env = {key: value for key, value in os.environ.items() if not key.startswith('DESKTOWN_')}
    sdk = args.dotnet_root.resolve()
    env.update(DOTNET_ROOT=str(sdk), PATH=str(sdk) + os.pathsep + env.get('PATH', ''),
        UseSharedCompilation='false', MSBUILDDISABLENODEREUSE='1', DOTNET_CLI_USE_MSBUILD_SERVER='0',
        DESKTOWN_DEMO_SAVE=str(save), DESKTOWN_DAILY_CAPTURE_DIR=str(output / 'captures'))
    godot = args.godot.resolve()
    if godot.name.endswith('_console.exe'):
        godot = godot.with_name(godot.name.replace('_console.exe', '.exe'))
    report = {'baseline': args.baseline, 'revision': args.revision, 'success': False, 'isolatedSave': True}
    try:
        run_checked([str(sdk / 'dotnet.exe'), 'build', 'DeskTown.csproj', '--nologo',
            '-r', 'win-x64', '-p:AppendRuntimeIdentifierToOutputPath=false'], project, env, output / 'build.log', 180)
        run_checked([str(godot), '--headless', '--editor', '--path', str(project), '--import'], project, env, output / 'import.log', 180)
        export = output / 'export'
        export.mkdir()
        run_checked([str(godot), '--headless', '--path', str(project), '--export-release',
            'Windows Desktop', str(export / 'DeskTown.exe')], project, env, output / 'export.log', 180)
        log = run_checked([str(export / 'DeskTown.exe'), '--position', '-10000,-10000',
            '--audio-driver', 'Dummy', '--', '--desktown-demo'], project, env, output / 'probe.log', 60)
        if 'DESKTOWN_WINDOW_PRESENTATION_OK' not in log:
            raise RuntimeError('Missing completion marker')
        report.update(success=True, checks=log.count('WINDOW_CHECK_OK'), audioChecks=log.count('AUDIO_CHECK_OK'),
                      dailyChecks=log.count('DAILY_CHECK_OK'))
    except (OSError, RuntimeError, subprocess.SubprocessError) as error:
        report['error'] = str(error)
    (output / 'result.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(report))
    return 0 if report['success'] else 1


if __name__ == '__main__':
    sys.exit(main())
