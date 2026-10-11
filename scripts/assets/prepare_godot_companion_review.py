"""Prepare an isolated tracked snapshot with approved Companion review art.

Never assigns the working repository's catalog or starts AppRoot/personal saves.
The generated project is disposable review material, not a shipping build.
"""
import argparse
import hashlib
import io
import json
from pathlib import Path
import shutil
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parents[2]
REVIEWS = Path("docs/art/reviews/2026-10-11")
ART = {
    "CHR-MIN-001": "palette-v1/prepared-v2/shared32/idle-1x.png",
    "CHR-MIN-002": "mina-motion-v1/prepared-v1/chr_mina_walk.png",
    "CHR-MIN-003": "palette-v1/prepared-v2/shared32/work-1x.png",
    "CHR-MIN-004": "mina-motion-v1/prepared-v1/chr_mina_rest.png",
    "CHR-MIN-005": "mina-motion-v1/prepared-v1/chr_mina_stretch.png",
    "CHR-MIN-006": "mina-motion-v1/prepared-v1/chr_mina_celebrate.png",
    "CMP-WRK-001": "companion-props-v1/prepared-v1/prop_workbench.png",
    "CMP-LMP-001": "companion-props-v1/prepared-v1/prop_lamp_states.png",
    "CMP-WIN-001": "companion-props-v1/prepared-v1/prop_small_window.png",
    "CMP-TOL-001": "companion-props-v1/prepared-v1/prop_mina_tool.png",
    "CMP-FLR-001": "companion-contact-v1/prepared-v1/prop_companion_floor.png",
    "CMP-STL-001": "companion-contact-v1/prepared-v1/prop_stool.png",
    "CMP-SHD-001": "companion-contact-v1/prepared-v1/prop_character_shadow.png",
}


def prepare(output, runtime_working_tree=False):
    output = output.resolve()
    build = (ROOT / "build").resolve()
    if build not in output.parents or output.exists():
        raise ValueError("Choose a new, nonexistent directory below this repository's build/.")
    revision = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip()
    archive = subprocess.check_output(["git", "archive", "--format=zip", revision], cwd=ROOT)
    output.mkdir(parents=True)
    project = output / "project"
    with zipfile.ZipFile(io.BytesIO(archive)) as bundle:
        bundle.extractall(project)
    runtime_hashes = {}
    if runtime_working_tree:
        # Only explicitly named runtime files are overlaid; never copy user data,
        # build caches, .old backups, or the many editor-generated untracked UIDs.
        for relative in ("scenes/companion/CompanionStage.tscn",
                         "src/DeskTown.Godot/Display/CompanionStage.cs",
                         "src/DeskTown.Godot/Bootstrap/CatalogArtSmoke.cs"):
            shutil.copyfile(ROOT / relative, project / relative)
            runtime_hashes[relative] = hashlib.sha256((ROOT / relative).read_bytes()).hexdigest()
    path = project / "assets/catalogs/asset_catalog.json"
    catalog = json.loads(path.read_text(encoding="utf-8"))
    rows = {row["id"]: row for row in catalog["assets"]}
    missing = set(ART) - rows.keys()
    if missing:
        raise ValueError(f"Unknown catalog IDs: {sorted(missing)}")
    sources = {}
    for asset_id, relative in ART.items():
        source = project / REVIEWS / relative
        row = rows[asset_id]
        resource = "assets/art/review-only/" + row["filename"]
        target = project / resource
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, target)
        row.update(status="production", resource="res://" + resource,
                   provenance={"creator": "OpenAI image generation + approved Python pixel preparation",
                               "license": "Review-only fixture; final delivery license approval pending",
                               "source": (REVIEWS / relative).as_posix(), "importDate": "2026-10-11",
                               "manifestRevision": catalog["manifestRevision"]})
        if row["pivot"] is None:
            row["pivot"] = [row["frameSize"][0] // 2, row["frameSize"][1]]
        sources[asset_id] = {"source": row["provenance"]["source"],
                             "sha256": hashlib.sha256(source.read_bytes()).hexdigest()}
    path.write_text(json.dumps(catalog, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    probe = project / "src/DeskTown.Godot/Bootstrap/CompanionArtReview.cs"
    shutil.copyfile(ROOT / "scripts/assets/CompanionArtReview.cs.txt", probe)
    scene = project / "scenes/companion/ArtReview.tscn"
    scene.write_text('[gd_scene load_steps=2 format=3]\n'
                     '[ext_resource type="Script" path="res://src/DeskTown.Godot/Bootstrap/CompanionArtReview.cs" id="1"]\n'
                     '[node name="ReviewOnly" type="Node"]\nscript = ExtResource("1")\n', encoding="utf-8")
    config = project / "project.godot"
    config.write_text(config.read_text(encoding="utf-8").replace(
        'config/name="DeskTown"', 'config/name="DeskTownArtReview-' + output.name + '"').replace(
        'run/main_scene="res://scenes/app/AppRoot.tscn"',
        'run/main_scene="res://scenes/companion/ArtReview.tscn"'), encoding="utf-8")
    report = {"sourceRevision": revision, "reviewOnly": True, "sources": sources,
              "runtimeWorkingTreeFiles": runtime_hashes,
              "catalogStatusNote": "production is required by the existing P0 validator; isolated fixture only",
              "probeSha256": hashlib.sha256(probe.read_bytes()).hexdigest(),
              "personalSaveAccess": False, "engineUserDirectoryIsolated": True}
    (output / "preparation.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(project)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("output", type=Path)
    parser.add_argument("--runtime-working-tree", action="store_true",
                        help="Overlay only the three named Companion runtime files for pre-commit validation")
    args = parser.parse_args()
    prepare(args.output, args.runtime_working_tree)
