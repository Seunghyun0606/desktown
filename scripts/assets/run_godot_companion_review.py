"""Build, import and capture a disposable Companion, Town or NPC project on Windows."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys
import time
import uuid
import xml.etree.ElementTree as ET

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from export_windows import run_checked

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("review", type=Path)
    parser.add_argument("--godot", required=True, type=Path)
    parser.add_argument("--dotnet-root", required=True, type=Path)
    args = parser.parse_args()
    review = args.review.resolve()
    if os.name != "nt":
        parser.error("This capture runner currently supports Windows only.")
    if (ROOT / "build").resolve() not in review.parents:
        parser.error("Review must be below this repository's build/.")
    preparation = json.loads((review / "preparation.json").read_text(encoding="utf-8"))
    if not preparation["reviewOnly"]:
        parser.error("Not an isolated review fixture.")
    if (review / "captures").exists():
        parser.error("Captures already exist; prepare a new review directory.")
    project = review / "project"
    godot = args.godot.resolve()
    if godot.name.endswith("_console.exe"):
        godot = godot.with_name(godot.name.replace("_console.exe", ".exe"))
    sdk = args.dotnet_root.resolve()
    if not str(sdk).isascii():
        parser.error("Use an ASCII SDK installation path.")
    env = os.environ.copy()
    # Prevent inherited smoke/demo switches and keep SDK discovery local to this invocation.
    for key in list(env):
        if key.startswith("DESKTOWN_"):
            del env[key]
    if preparation.get("runtimeCatalog"):
        env["DESKTOWN_ART_REVIEW_RUNTIME_CATALOG"] = "1"
    env.update(DOTNET_ROOT=str(sdk), PATH=str(sdk) + os.pathsep + env.get("PATH", ""),
               UseSharedCompilation="false", MSBUILDDISABLENODEREUSE="1", DOTNET_CLI_USE_MSBUILD_SERVER="0")
    logs = review / ("logs-" + time.strftime("%H%M%S-") + uuid.uuid4().hex[:6])
    logs.mkdir()
    report = {"success": False, "logs": str(logs)}
    try:
        dotnet = str(sdk / "dotnet.exe")
        report["sdk"] = run_checked([dotnet, "--version"], project, env, logs / "sdk.log", 30).strip()
        expected = json.loads((project / "global.json").read_text(encoding="utf-8"))["sdk"]["version"]
        if report["sdk"] != expected:
            raise ValueError(f"Expected SDK {expected}, got {report['sdk']}")
        report["godot"] = run_checked([str(godot), "--version"], project, env, logs / "engine.log", 30).strip()
        engine = ET.parse(project / "DeskTown.csproj").getroot().attrib["Sdk"].split("/")[1]
        if not report["godot"].startswith(engine + ".stable.mono."):
            raise ValueError(f"Expected Godot {engine} .NET, got {report['godot']}")
        run_checked([sys.executable, "scripts/assets/validate_catalog.py"], project, env, logs / "catalog.log", 30)
        run_checked([dotnet, "build", "DeskTown.csproj", "-c", "Debug", "--nologo"], project, env, logs / "build.log", 180)
        run_checked([str(godot), "--headless", "--editor", "--path", str(project), "--import"], project, env, logs / "import.log", 180)
        # A real GPU renderer is required for GetImage. Headless uses the dummy renderer.
        # Keep the small helper window off the desktop; this is not native input QA.
        output = run_checked([str(godot), "--path", str(project), "--position", "-10000,-10000",
                              "--resolution", "64x64", "--audio-driver", "Dummy"],
                             project, env, logs / "capture.log", 90)
        review_kind = preparation.get("reviewKind", "companion")
        marker = {"town":"DESKTOWN_TOWN_REVIEW_OK", "npc":"DESKTOWN_NPC_REVIEW_OK", "remaining":"DESKTOWN_REMAINING_REVIEW_OK", "final":"DESKTOWN_FINAL_REVIEW_OK"}.get(review_kind,"DESKTOWN_COMPANION_REVIEW_OK")
        if marker not in output:
            raise RuntimeError("Missing review completion marker")
        result = json.loads((review / "captures/result.json").read_text(encoding="utf-8"))
        expected_count = preparation.get("expectedCaptures", 124)
        if len(result["captures"]) != expected_count or any(not (review / "captures" / name).is_file() for name in result["captures"]):
            raise RuntimeError(f"Incomplete {review_kind} capture set")
        report.update(success=True, captures=len(result["captures"]), renderer=result["renderer"])
    except (OSError, ValueError, RuntimeError, subprocess.SubprocessError) as error:
        report["error"] = str(error)
    (logs / "result.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=True))
    return 0 if report["success"] else 1


if __name__ == "__main__":
    sys.exit(main())
