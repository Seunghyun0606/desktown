"""Publish actual GPU capture sequences as lossless WebP behavior previews."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]

def publish(source, output):
    if output.exists():
        raise ValueError('Choose a new review directory')
    captures = source / 'captures'
    result = json.loads((captures / 'result.json').read_text(encoding='utf-8'))
    assert len(result['captures']) == 741
    output.mkdir(parents=True)
    for prefix, count in [('town-a', 320), ('town-b', 200), ('companion', 200)]:
        frames = [Image.open(captures / f'{prefix}-{i:04d}.png').convert('RGB') for i in range(count)]
        frames[0].save(output / f'{prefix}.webp', save_all=True, append_images=frames[1:], duration=125,
            loop=0, lossless=True, method=4)
        for frame in frames:
            frame.close()
    for name in ['town-reduced.png', 'town-mina-arrived.png', 'town-house-visit.png', 'companion-return-step.png',
        'companion-rest.png', 'companion-stretch.png', 'companion-reduced.png']:
        shutil.copyfile(captures / name, output / name)
    report = {'captures': 741, 'fps': 8, 'actualGpu': True, 'lossless': True,
        'sourceFrames': {name: hashlib.sha256((captures / name).read_bytes()).hexdigest() for name in result['captures']},
        'runtimeWorkingTreeFiles': json.loads((source / 'preparation.json').read_text(encoding='utf-8'))['runtimeWorkingTreeFiles'],
        'checks': result['motionChecks']}
    (output / 'verification.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    html = '''<!doctype html><html lang="ko"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>DeskTown 캐릭터 행동 개선</title><style>
body{background:#24211c;color:#efddbd;font:16px/1.6 system-ui;margin:0}main{max-width:1280px;margin:auto;padding:24px}
h1{font-size:28px}section{margin:32px 0;padding:18px;background:#302a23;border-radius:10px}img{max-width:100%;height:auto;image-rendering:pixelated;display:block;margin:16px auto}
button{background:#756544;color:#fff;border:0;padding:10px 18px;border-radius:5px;cursor:pointer}small{color:#cfbfa2}.companion{width:720px}
</style><main><h1>캐릭터 행동 개선</h1><p>승인된 아트를 사용하는 실제 Godot 렌더입니다. 8fps·원래 시간 속도로 재생됩니다.</p>
<p>Noah는 독서 후 책을 내려놓고 산책하고, Rumi는 Library와 Workshop 주변을 둘러봅니다. Mina는 집 앞을 거닐고 작업 중 짧게 살펴보거나 스트레칭합니다.</p>
<button id="toggle">움직임 멈추기</button>
<section><h2>마을 · 독서와 산책</h2><small>0–40초. 서로 다른 대기 시간, 실제 Walk 6프레임·8fps, 이동 방향을 향하는 캐릭터.</small><img data-motion="town-a.webp" data-still="town-reduced.png" src="town-a.webp"></section>
<section><h2>마을 · 돌아오기와 모닥불 곁 독서</h2><small>42–67초. Noah의 동선은 불을 피해 지나갑니다. 캐릭터는 발 위치에 따라 앞뒤로 그려집니다.</small><img data-motion="town-b.webp" data-still="town-reduced.png" src="town-b.webp"></section>
<section><h2>PiP · 작업 리듬</h2><small>13–38초. 작업 → 짧은 살펴보기 → 작업 → 한 번 스트레칭 → 작업. 실제 Work/Rest 명령이 우선합니다.</small><img class="companion" data-motion="companion.webp" data-still="companion-reduced.png" src="companion.webp"></section>
<section><h2>작업대 복귀·휴식·스트레칭</h2><small>복귀 때 Mina와 그림자가 함께 이동하고 Walk 뒤에 Work가 이어집니다.</small>
<img class="companion" src="companion-return-step.png"><img class="companion" src="companion-rest.png"><img class="companion" src="companion-stretch.png"></section>
<p>숨김 상태에서는 시각 시간이 멈춥니다. Reduced motion에서는 선택적 이동·스트레칭이 꺼집니다. Focus 시간·보상·저장 스키마는 이 연출에 의존하지 않습니다.</p>
</main><script>let running=true;document.querySelector('#toggle').onclick=()=>{running=!running;for(const img of document.querySelectorAll('[data-motion]'))img.src=running?img.dataset.motion:img.dataset.still;document.querySelector('#toggle').textContent=running?'움직임 멈추기':'움직임 재생';};if(matchMedia('(prefers-reduced-motion: reduce)').matches)document.querySelector('#toggle').click();</script></html>'''
    (output / 'review.html').write_text(html, encoding='utf-8')
    print(json.dumps({'published': str(output), 'captures': 741, 'movies': 3}))

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('source', type=Path)
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    publish(args.source, args.output)
