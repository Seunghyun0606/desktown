"""Export locally on Windows without waiting on the Godot console wrapper's job."""

import argparse
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import time
import uuid
import xml.etree.ElementTree as ET


ROOT = Path(__file__).resolve().parents[1]
ANSI = re.compile(r"\x1b\[[0-9;]*m")
ERROR = re.compile(r"^\s*(?:ERROR:|SCRIPT ERROR:)|\berror [A-Z]+\d+:", re.MULTILINE)


def run_checked(command, cwd, env, log_path, timeout):
    """Keep all output on disk, require termination, and reject error logs at exit 0."""
    with log_path.open("wb") as log:
        process = subprocess.Popen(
            command, cwd=cwd, env=env, stdout=log, stderr=subprocess.STDOUT,
            creationflags=subprocess.CREATE_NO_WINDOW if os.name == "nt" else 0,
        )
        try:
            code = process.wait(timeout=timeout)
        except subprocess.TimeoutExpired:
            # Only terminate this invocation and its descendants, never other builds.
            if os.name == "nt":
                subprocess.run(
                    ["taskkill", "/PID", str(process.pid), "/T", "/F"],
                    stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
                    timeout=10, creationflags=subprocess.CREATE_NO_WINDOW,
                )
            if process.poll() is None:
                process.kill()
            process.wait(timeout=10)
            raise RuntimeError(f"Timed out after {timeout}s. Partial output is not a successful export.")
    output = ANSI.sub("", log_path.read_text(encoding="utf-8", errors="replace"))
    if code != 0:
        raise RuntimeError(f"Command exited with {code}; see {log_path}")
    if ERROR.search(output):
        raise RuntimeError(f"Command logged errors despite exit 0; see {log_path}")
    return output


def prepare_output(directory):
    directory = directory.resolve()
    build = (ROOT / "build").resolve()
    if directory == build or build not in directory.parents:
        raise ValueError("Output must be a subdirectory of this repository's build directory.")
    if directory.exists() and any(directory.iterdir()):
        raise ValueError("Output directory must be empty; choose a new directory.")
    directory.mkdir(parents=True, exist_ok=True)
    return directory


def require_artifacts(directory):
    for relative in ("DeskTown.exe", "data_DeskTown_windows_x86_64/DeskTown.dll"):
        path = directory / relative
        if not path.is_file() or path.stat().st_size == 0:
            raise RuntimeError(f"Export is missing required artifact: {relative}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--godot", required=True, type=Path, help="Godot .NET executable (console or main)")
    parser.add_argument("--dotnet-root", required=True, type=Path, help="SDK installation directory")
    parser.add_argument("--output-directory", required=True, type=Path, help="Empty directory below build/")
    parser.add_argument("--timeout", type=int, default=300, help="Export timeout in seconds")
    args = parser.parse_args()
    if os.name != "nt":
        parser.error("This local exporter requires Windows; Linux CI uses its existing export step.")
    if args.timeout <= 0:
        parser.error("--timeout must be positive")

    log_directory = ROOT / "build/export-logs" / (time.strftime("%Y%m%d-%H%M%S-") + uuid.uuid4().hex[:8])
    log_directory.mkdir(parents=True)
    report = {"success": False, "logs": str(log_directory)}
    started = time.monotonic()
    try:
        godot = args.godot.resolve()
        if godot.name.lower().endswith("_console.exe"):
            godot = godot.with_name(godot.name[:-len("_console.exe")] + ".exe")
        if not godot.is_file():
            raise ValueError(f"Godot main executable not found: {godot}")
        sdk_root = args.dotnet_root.resolve()
        if not str(sdk_root).isascii():
            raise ValueError("Use an ASCII SDK installation path; this Windows .NET/Godot combination misreads non-ASCII SDK paths.")
        dotnet = sdk_root / "dotnet.exe"
        if not dotnet.is_file() or not (sdk_root / "host/fxr").is_dir():
            raise ValueError(f"Incomplete SDK installation: {sdk_root}")
        env = os.environ.copy()
        env.update(DOTNET_ROOT=str(sdk_root), PATH=str(sdk_root) + os.pathsep + env.get("PATH", ""))
        # Avoid leaving this invocation's build workers alive after an export.
        env.update(UseSharedCompilation="false", MSBUILDDISABLENODEREUSE="1", DOTNET_CLI_USE_MSBUILD_SERVER="0")
        sdk_version = json.loads((ROOT / "global.json").read_text(encoding="utf-8"))["sdk"]["version"]
        actual_sdk = run_checked([str(dotnet), "--version"], ROOT, env, log_directory / "sdk.log", 30).strip()
        if actual_sdk != sdk_version:
            raise ValueError(f"Expected SDK {sdk_version}, got {actual_sdk}")
        engine_version = ET.parse(ROOT / "DeskTown.csproj").getroot().attrib["Sdk"].split("/")[1]
        actual_engine = run_checked([str(godot), "--version"], ROOT, env, log_directory / "engine.log", 30).strip()
        if not actual_engine.startswith(engine_version + ".stable.mono."):
            raise ValueError(f"Expected Godot {engine_version} .NET, got {actual_engine}")
        directory = prepare_output(args.output_directory)
        report.update(output=str(directory), godot=actual_engine, dotnetSdk=actual_sdk)
        print(f"Exporting to {directory}\nLogs: {log_directory}", flush=True)
        run_checked(
            [str(godot), "--headless", "--path", str(ROOT), "--export-release", "Windows Desktop", str(directory / "DeskTown.exe")],
            ROOT, env, log_directory / "export.log", args.timeout,
        )
        require_artifacts(directory)
        report.update(success=True, files=sum(path.is_file() for path in directory.rglob("*")))
    except (OSError, ValueError, RuntimeError, subprocess.SubprocessError) as error:
        report["error"] = str(error)
    finally:
        report["seconds"] = round(time.monotonic() - started, 2)
        (log_directory / "result.json").write_text(json.dumps(report, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=True))
    return 0 if report["success"] else 1


if __name__ == "__main__":
    sys.exit(main())
