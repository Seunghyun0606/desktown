"""Author quiet, sample-free sound candidates and an offline listening review.

Run with the local art Python environment (numpy 2.5.4, soundfile 0.14.0).
Candidates are deliberately separate from the production asset catalog.
"""
import argparse
import hashlib
import html
import json
import math
import subprocess
from pathlib import Path

import numpy as np
import soundfile as sf

ROOT = Path(__file__).resolve().parents[2]
RATE = 44100
SEED = 121026
ASSETS = [
    ("SND-AMB-001", "snd_town_ambient", "마을 공기", 60.0, True, -25, "바람·잎의 낮은 결. 음악과 새소리는 포함하지 않습니다."),
    ("SND-BRD-001", "snd_bird", "작은 새", 1.65, False, -24, "짧은 두 구절. 주변음에 드물게 겹치는 후보입니다."),
    ("SND-STP-001", "snd_footstep", "흙길 발소리", .30, False, -26, "부드러운 발 디딤과 짧은 흙의 결."),
    ("SND-HMR-001", "snd_hammer", "나무 망치", .42, False, -23, "작은 나무 도구의 둔탁한 접촉. 금속성 울림은 줄였습니다."),
    ("SND-FIR-001", "snd_fire_loop", "작은 모닥불", 12.0, True, -28, "낮은 바스락거림과 작은 불씨. 마을 믹스에서는 더 낮춥니다."),
    ("SND-UI-001", "snd_ui_click", "버튼", .13, False, -27, "한 번의 짧고 부드러운 나무 톡."),
    ("SND-CMP-001", "snd_project_complete", "Workshop 완성", 1.75, False, -20, "따뜻한 세 음이 올라간 뒤 가라앉는 완성 신호."),
    ("SND-DSC-001", "snd_discovery", "지도 발견", 1.55, False, -22, "완성과 다른 두 음의 잔향. 짧고 조용한 발견 신호."),
]


def time(seconds):
    return np.arange(round(RATE * seconds), dtype=np.float64) / RATE


def noise(seconds, seed, low, high):
    """Band-limited periodic noise; no recorded samples or plugin instruments."""
    count = round(seconds * RATE)
    frequencies = np.fft.rfftfreq(count, 1 / RATE)
    rng = np.random.default_rng(seed)
    spectrum = rng.normal(size=len(frequencies)) + 1j * rng.normal(size=len(frequencies))
    shape = (1 - np.exp(-(frequencies / low) ** 4)) * np.exp(-(frequencies / high) ** 4)
    spectrum *= shape / np.sqrt(np.maximum(frequencies, low))
    spectrum[0] = spectrum[-1] = 0
    values = np.fft.irfft(spectrum, n=count)
    return values / np.sqrt(np.mean(values * values))


def fade(values, attack=.008, release=.035):
    values = values.copy()
    a = min(round(attack * RATE), len(values))
    r = min(round(release * RATE), len(values))
    attack_curve = np.sin(np.linspace(0, math.pi / 2, a)) ** 2
    release_curve = np.sin(np.linspace(math.pi / 2, 0, r)) ** 2
    values[:a] *= attack_curve[:, None] if values.ndim == 2 else attack_curve
    values[-r:] *= release_curve[:, None] if values.ndim == 2 else release_curve
    return values


def knock(seconds, seed, frequencies):
    t = time(seconds)
    modes = sum(weight * np.sin(2 * math.pi * frequency * t) * np.exp(-t / decay)
                for frequency, weight, decay in frequencies)
    grain = noise(seconds, seed, 170, 1800) * .11 * np.exp(-t / .025)
    return fade(modes + grain, .004, .055)


def add(destination, source, start, gain=1.0, pan=0.0):
    start = round(start * RATE)
    length = min(len(source), len(destination) - start)
    if length <= 0:
        return
    source = source[:length]
    if destination.ndim == 2 and source.ndim == 1:
        source = source[:, None] * np.array([math.sqrt((1 - pan) / 2), math.sqrt((1 + pan) / 2)])
    destination[start:start + length] += source * gain


def chime(seconds, notes):
    out = np.zeros(round(seconds * RATE))
    for start, frequency, gain in notes:
        t = time(seconds - start)
        voice = sum(weight * np.sin(2 * math.pi * frequency * ratio * t) * np.exp(-t / decay)
                    for ratio, weight, decay in [(1, 1, .32), (2.01, .16, .15), (3.99, .035, .08)])
        add(out, fade(voice, .012, .14), start, gain)
    return fade(out, .010, .17)


def synthesize():
    ambient_t = time(60)
    left = noise(60, SEED, 65, 1250)
    right = .78 * left + .22 * noise(60, SEED + 1, 65, 1250)
    breeze = .7 + .15 * np.sin(2 * math.pi * ambient_t / 15) + .1 * np.cos(2 * math.pi * ambient_t / 20)
    ambient = np.column_stack((left, right)) * breeze[:, None]

    bird = np.zeros(round(1.65 * RATE))
    for index, (start, duration, frequency) in enumerate([(0.12, .22, 2050), (.41, .16, 2600), (.96, .27, 2280)]):
        t = time(duration)
        phase = 2 * math.pi * (frequency * t + 180 * t * t / duration) + .45 * np.sin(2 * math.pi * 17 * t)
        add(bird, fade(np.sin(phase) + .09 * np.sin(2 * phase), .025, .045), start, 1 - index * .12)
    foot_t = time(.30)
    foot = knock(.30, SEED + 2, [(115, .8, .035), (205, .35, .024)])
    foot += fade(noise(.30, SEED + 3, 450, 2300) * .09 * np.exp(-foot_t / .055), .006, .06)
    hammer = knock(.42, SEED + 4, [(390, 1, .042), (665, .24, .028), (1030, .08, .017)])
    fire_t = time(12)
    fire = noise(12, SEED + 5, 180, 2300) * (.42 + .10 * np.sin(2 * math.pi * fire_t / 3))
    rng = np.random.default_rng(SEED + 6)
    for start in np.linspace(.65, 10.9, 14) + rng.uniform(-.12, .12, 14):
        add(fire, knock(.10, round(start * 1000), [(850, .35, .012), (1300, .12, .007)]), start, .38)
    # Quiet periodic cross-channel texture with the same exact 12-second period.
    fire = np.column_stack((fire, .85 * fire + .15 * noise(12, SEED + 7, 180, 2300)))
    click = knock(.13, SEED + 8, [(520, 1, .012), (780, .12, .009)])
    complete = chime(1.75, [(0, 523.25, .8), (.20, 659.25, .68), (.43, 783.99, .65)])
    discovery = chime(1.55, [(0, 987.77, .7), (.25, 739.99, .65)])
    return [ambient, bird, foot, hammer, fire, click, complete, discovery]


def db(value):
    return round(20 * math.log10(max(float(value), 1e-12)), 3)


def measure(path, expected_duration, loop=False):
    data, rate = sf.read(path, always_2d=True, dtype="float64")
    info = sf.info(path)
    assert rate == RATE and info.format == "OGG" and info.subtype == "VORBIS", path
    assert len(data) == round(expected_duration * RATE), (path, len(data))
    assert np.isfinite(data).all() and np.max(np.abs(data)) < .30, path
    result = {"file": path.name, "durationSeconds": len(data) / rate, "sampleRate": rate,
              "channels": info.channels, "codec": info.subtype,
              "decodedSamplePeakDbfs": db(np.max(np.abs(data))),
              "decodedRmsDbfs": db(np.sqrt(np.mean(data ** 2))),
              "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}
    if not loop:
        assert np.max(np.abs(data[[0, -1]])) < .0005, path
    else:
        step = np.max(np.abs(data[0] - data[-1]))
        typical = float(np.quantile(np.abs(np.diff(data, axis=0)), .99))
        result.update(wrapStepDbfs=db(step), adjacentStep99Dbfs=db(typical),
                      wrapWithinAdjacent99=bool(step <= typical))
        assert step <= typical, (path, step, typical)
    return result


def write_sound(path, data, duration, loop=False):
    # Bounded writes avoid the Windows Vorbis encoder's large-buffer stack use.
    with sf.SoundFile(path, mode="w", samplerate=RATE,
                      channels=1 if data.ndim == 1 else data.shape[1],
                      format="OGG", subtype="VORBIS", compression_level=.75) as output:
        for start in range(0, len(data), 4096):
            output.write(data[start:start + 4096])
    return measure(path, duration, loop)


def prepare(output):
    output = output.resolve()
    if output.exists():
        raise SystemExit(f"Review exists; choose a new version: {output}")
    output.mkdir(parents=True)
    sounds = output / "audio"
    sounds.mkdir()
    (output / ".gdignore").write_text("", encoding="utf-8")
    source_hash = hashlib.sha256(Path(__file__).read_bytes()).hexdigest()
    catalog_path = ROOT / "assets/catalogs/asset_catalog.json"
    catalog_hash = hashlib.sha256(catalog_path.read_bytes()).hexdigest()
    records, decoded = [], {}
    for (asset_id, name, title, duration, loop, peak, description), source in zip(ASSETS, synthesize()):
        source *= 10 ** (peak / 20) / np.max(np.abs(source))
        result = write_sound(sounds / f"{name}.ogg", source, duration, loop)
        result.update(id=asset_id, title=title, loop=loop, description=description, status="candidate")
        records.append(result)
        decoded[name] = sf.read(sounds / f"{name}.ogg")[0]

    mixes = []
    town = decoded["snd_town_ambient"][:round(24 * RATE)].copy()
    fire = np.tile(decoded["snd_fire_loop"], (2, 1))
    town += fire * .35
    for start in [2.0, 13.7]:
        add(town, decoded["snd_bird"], start, .65, -.35)
    for start in np.arange(7, 10, .55):
        add(town, decoded["snd_footstep"], float(start), .7, .25)
    town = fade(town, .10, .20)
    mixes.append(("mix_town", "마을 · 24초", town, 24, "주변음 + 낮춘 모닥불 + 드문 새 + 한 번의 짧은 걷기."))
    companion = np.zeros((round(20 * RATE), 2))
    for start in [1.5, 3.5, 5.5, 7.5, 13.5, 15.5, 17.5]:
        add(companion, decoded["snd_hammer"], start, .75)
    mixes.append(("mix_companion", "작업대 · 20초", companion, 20, "간격을 둔 망치와 작업 중 쉼. 지속 음악은 없습니다."))
    reward = np.zeros((round(5 * RATE), 2))
    add(reward, decoded["snd_hammer"], .20, .55)
    add(reward, decoded["snd_project_complete"], .65, .9)
    mixes.append(("mix_reward", "완성 · 5초", reward, 5, "마지막 작은 타격 뒤 완성 신호. 반복하지 않습니다."))
    discovery = np.zeros((round(5 * RATE), 2))
    add(discovery, decoded["snd_ui_click"], .15, .75)
    add(discovery, decoded["snd_discovery"], .55, .9)
    mixes.append(("mix_discovery", "발견 · 5초", discovery, 5, "버튼과 지도 발견의 서로 다른 소리."))
    mix_records = []
    for name, title, data, duration, description in mixes:
        result = write_sound(sounds / f"{name}.ogg", data, duration)
        result.update(title=title, description=description)
        mix_records.append(result)

    def card(record):
        loop = ' loop' if record.get("loop") else ''
        return f'<article><h3>{html.escape(record["title"])}</h3><p>{html.escape(record["description"])}</p><audio controls preload="none"{loop} src="audio/{record["file"]}"></audio><p class="meta">{record["durationSeconds"]:g}초 · peak {record["decodedSamplePeakDbfs"]:g} dBFS · {"반복" if loop else "한 번"}</p></article>'
    page = '''<!doctype html><html lang="ko"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>DeskTown 음향 검토</title>
<style>body{margin:0;background:#eee7d5;color:#302b34;font:17px/1.6 system-ui}main{max-width:1000px;margin:auto;padding:40px 24px}h1{font-size:32px}h2{margin-top:42px}article{background:#fffaf0;border:1px solid #d4c3a7;border-radius:12px;padding:20px}h3{margin:0}p{margin:8px 0 16px}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(290px,1fr));gap:16px}audio{width:100%}.meta{font-size:13px;color:#645b59;margin-bottom:0}button{font:inherit;border:1px solid #a99177;background:#fef8e7;border-radius:8px;padding:8px 14px}label{display:block;margin:12px 0}.toolbar{position:sticky;top:0;background:#eee7d5;padding:12px 0;z-index:1}.note{padding:16px;background:#dfdfc8;border-radius:8px}a{color:#455e4c}</style>
<main><h1>DeskTown · 작은 마을의 소리</h1><p>음향 8종 첫 검토본 · 2026-10-12</p><p class="note">재생 버튼을 눌러 들어보세요. 조용한 작업을 방해하지 않는 방향입니다. 현재 게임은 무음이며, 이 페이지는 소리와 믹스를 검토하는 별도 미리보기입니다.</p>
<div class="toolbar"><button id="stop">모두 정지</button><label>미리보기 음량 <input id="volume" type="range" min="0" max="1" step="0.05" value="1"> <output id="level">100%</output></label></div>
<h2>먼저 장면으로 듣기</h2><p>마을 → 작업대 → 완성 → 발견 순서로 들어보세요. 실제 게임 녹음이 아닌 배치·음량 후보입니다.</p><div class="grid">MIXES</div>
<h2>소리 8종</h2><p>마을 공기와 모닥불은 끝에서 반복됩니다. 나머지는 한 번 재생됩니다.</p><div class="grid">ASSETS</div>
<h2>확인할 부분</h2><ul><li>망치·발소리가 너무 딱딱하거나 반복이 거슬리는지</li><li>완성과 발견이 구분되면서 과하지 않은지</li><li>마을과 작업대를 오래 켜둘 수 있을 만큼 조용한지</li></ul>
<p>적용안: 기존 Audio 설정 하나로 제어. 숨긴 화면의 주변음·발소리·망치는 정지하고, 다시 열 때 지나간 소리를 몰아서 재생하지 않습니다. 완료·발견은 보상/발견 화면을 볼 때 한 번 재생합니다. 저장된 Audio=false는 유지합니다. Audio=true인 저장과 새 게임은 적용 승인 후 소리가 켜집니다.</p>
<p class="meta">순수 수학 합성, 외부 녹음·음원 없음. 44.1kHz Ogg Vorbis. 기술 검사와 청취 품질 승인은 구분합니다. <a href="README.md">제작·검증 기록</a> · <a href="verification.json">수치/파일 해시</a></p></main>
<script>const players=[...document.querySelectorAll('audio')];function stop(){players.forEach(p=>{p.pause();p.currentTime=0})}players.forEach(p=>p.addEventListener('play',()=>players.forEach(other=>{if(other!==p){other.pause();other.currentTime=0}})));document.getElementById('stop').onclick=stop;document.getElementById('volume').oninput=e=>{players.forEach(p=>p.volume=Number(e.target.value));document.getElementById('level').value=Math.round(e.target.value*100)+'%'};document.addEventListener('visibilitychange',()=>{if(document.hidden)stop()});</script></html>'''
    (output / "review.html").write_text(page.replace("MIXES", "".join(map(card, mix_records))).replace("ASSETS", "".join(map(card, records))), encoding="utf-8")
    report = {"status": "candidate-awaiting-listening-and-delivery-approval", "seed": SEED,
              "recipe": "scripts/assets/prepare_audio_review.py", "recipeSha256": source_hash,
              "baseCommit": subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip(),
              "catalogSha256BeforeAndAfter": catalog_hash,
              "tools": {"numpy": np.__version__, "soundfile": sf.__version__, "libsndfile": sf.__libsndfile_version__},
              "assets": records, "mixes": mix_records,
              "limits": "Sample-peak/RMS and seam bounds are technical checks, not listening approval, LUFS or certified true-peak measurements."}
    assert hashlib.sha256(catalog_path.read_bytes()).hexdigest() == catalog_hash
    (output / "verification.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    (output / "README.md").write_text(
        "# Quiet sound review v1\n\nOpen [review.html](review.html) locally and press play. No autoplay or CDN. "
        "Only one track plays at once; hiding the page stops all tracks. The volume control affects this review only.\n\n"
        "Eight original procedural candidates plus four offline mix examples. These are NOT game recordings, "
        "NOT assigned production assets, and do not change the silent MVP. They contain no external samples, "
        "recordings or musical works. Delivery and usage approval are pending; no third-party sample license is claimed.\n\n"
        "Recipe: `scripts/assets/prepare_audio_review.py`, deterministic seed 121026. "
        "Install numpy==2.5.4 and soundfile==0.14.0 in a local environment, then run the recipe with a new output directory. "
        "Vorbis container serials need not be byte-identical between encodes. Decoded samples and stored hashes are authoritative.\n\n"
        "Technical checks decode every delivered file: Vorbis, 44.1kHz, exact duration, mono one-shots/stereo loops, "
        "finite samples, ample sample-peak headroom, quiet one-shot endpoints. A loop wrap step must be no greater "
        "than its ordinary 99th-percentile adjacent sample step. This bound catches discontinuities but does not "
        "prove perceptually seamless looping. No LUFS or certified intersample peak measurement is claimed. "
        "Listen through headphones/speakers to decide timbre, repetition and relative level.\n\n"
        "Proposed runtime mix: Town ambience at unity, fire 0.35, bird 0.65, footsteps 0.7; Companion hammer 0.75; "
        "reward/discovery cue 0.9. Event spacing is illustrative. Effects must follow actual visible actions. "
        "Hidden surfaces stop presentation sound; no catch-up playback. Completion/discovery plays when its screen "
        "is viewed, once per reveal. Existing Audio=false is respected; Audio=true will become audible only after approval.\n",
        encoding="utf-8")
    print(json.dumps({"output": str(output), "assets": len(records), "mixes": len(mix_records),
                      "peakDbfsMax": max(r["decodedSamplePeakDbfs"] for r in records + mix_records)}, ensure_ascii=False))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("output", type=Path)
    prepare(parser.parse_args().output)
