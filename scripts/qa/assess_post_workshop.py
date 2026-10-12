"""Read current runtime behavior using a fresh, isolated console assessment."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', required=True, type=Path)
    parser.add_argument('--dotnet-root', required=True, type=Path)
    args = parser.parse_args()
    output = args.output.resolve()
    if (ROOT / 'build').resolve() not in output.parents or output.exists():
        parser.error('Choose a new assessment directory below build/')
    output.mkdir(parents=True)
    project = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    properties = ET.SubElement(project, 'PropertyGroup')
    ET.SubElement(properties, 'OutputType').text = 'Exe'
    references = ET.SubElement(project, 'ItemGroup')
    for name in ('DeskTown.Application', 'DeskTown.Persistence'):
        ET.SubElement(references, 'ProjectReference', Include=str(ROOT / 'src' / name / (name + '.csproj')))
    for name in ('FakeWallClock', 'FakeMonotonicClock', 'FakeActivityTracker'):
        ET.SubElement(references, 'Compile', Include=str(ROOT / 'tests/DeskTown.Application.Tests/Fakes' / (name + '.cs')),
                      Link=name + '.cs')
    ET.ElementTree(project).write(output / 'Assessment.csproj', encoding='utf-8', xml_declaration=True)
    source = ROOT / 'scripts/qa/PostWorkshopAssessment.cs.txt'
    shutil.copyfile(source, output / 'Program.cs')
    sdk = args.dotnet_root.resolve()
    env = {**os.environ, 'DOTNET_ROOT': str(sdk), 'PATH': str(sdk) + os.pathsep + os.environ.get('PATH', '')}
    result = subprocess.run([str(sdk / 'dotnet.exe'), 'run', '--project', str(output / 'Assessment.csproj'),
                             '--', str(output)], cwd=ROOT, env=env, capture_output=True, text=True, encoding='utf-8', timeout=60)
    (output / 'assessment.log').write_text(result.stdout + result.stderr, encoding='utf-8')
    if result.returncode:
        raise SystemExit(result.stdout + result.stderr)
    report_path = output / 'runtime-assessment.json'
    report = json.loads(report_path.read_text(encoding='utf-8'))
    report['baseCommit'] = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip()
    report['probeSha256'] = hashlib.sha256(source.read_bytes()).hexdigest()
    report_path.write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print(result.stdout.strip())


if __name__ == '__main__':
    main()
