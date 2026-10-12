"""Decode and exercise the sound candidates in an isolated, muted Godot project."""
import argparse
import hashlib
import json
import subprocess
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def check(review, engine):
    review = review.resolve()
    record = json.loads((review / "verification.json").read_text(encoding="utf-8"))
    for sound in record["assets"] + record["mixes"]:
        assert hashlib.sha256((review / "audio" / sound["file"]).read_bytes()).hexdigest() == sound["sha256"]
    scratch = ROOT / "build/audio-smoke"
    scratch.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(dir=scratch, prefix="decode-") as temporary:
        project = Path(temporary)
        (project / "project.godot").write_text('[application]\nconfig/name="Isolated Audio Review"\n[rendering]\nrenderer/rendering_method="gl_compatibility"\n', encoding="utf-8")
        probe = ROOT / "scripts/assets/AudioReviewSmoke.gd.txt"
        (project / "smoke.gd").write_bytes(probe.read_bytes())
        result = subprocess.run([str(engine.resolve()), "--headless", "--audio-driver", "Dummy", "--path", str(project),
                                 "--script", "res://smoke.gd", "--", str(review)],
                                capture_output=True, text=True, encoding="utf-8", timeout=30)
        print(result.stdout)
        if result.returncode or "ERROR:" in result.stderr:
            raise SystemExit(result.stderr or f"Godot exited {result.returncode}")
        output = json.loads((project / "engine-verification.json").read_text(encoding="utf-8"))
        output["probeSha256"] = hashlib.sha256(probe.read_bytes()).hexdigest()
        output["candidateFileHashesVerified"] = 12
        (review / "engine-verification.json").write_text(json.dumps(output, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("review", type=Path)
    parser.add_argument("--engine", required=True, type=Path)
    args = parser.parse_args()
    check(args.review, args.engine)
