# Workshop Repairing·3상태 검토 — 2026-10-11

## 현재 판정

사용자가 Mina Walk·Rest·Stretch·Celebrate의 동작 방향을 승인하고 Workshop Repairing 제작을 선택했다. 기존 Broken·Complete와 연결되는 수리 중 **검토 후보**를 준비했다. [검토 페이지](review.html)는 1×/2×/3×, 컬러·흑백, 밝고 어두운 배경, 문·기초 고정 영역 및 현재 Town 좌표의 세 상태 전환을 제공한다. 사용자가 2026-10-11 **A: 수리 중 외형 승인 — Companion 소품 진행**을 선택하여 Repairing 외형과 세 상태 구분을 승인했다. 생성 당시 변환 기록의 승인 대기 상태는 당시 기록으로 보존한다. 최종 출처·납품 및 런타임 배정은 별도다.

| 항목 | 규격 | 파일 |
| --- | --- | --- |
| Broken·Repairing·Complete | 각 160×128, RGBA8/sRGB | `prepared-v1/workshop-{state}-1x.png` |
| 상태 시트 | 480×128, 3셀, 순서 Broken=0 / Repairing=1 / Complete=2 | [bld_workshop_states.png](prepared-v1/bld_workshop_states.png) |
| 색·흑백 비교 | 최근접 2× | [comparison-board.png](prepared-v1/comparison-board.png) |
| House·Mina·환경 비교 | 384×288 아트 클러스터 | [수리 중 비교](prepared-v1/cluster-repairing-1x.png) |
| Town 좌표 참조 | 1280×720, 상태별 정지 PNG | [수리 중 참조](prepared-v1/town-repairing-1x.png) |

피벗은 (80,128), 불투명 영역은 세 상태 모두 `(8,31)-(152,128)`이다. 기본 색은 [승인된 공통 32색](../../../palettes/README.md)의 부분집합이며 효과 6색을 섞지 않았다. Broken·Complete는 기존 shared32 프레임과 **픽셀이 그대로 일치**한다. 원본·기존 아트 파일을 덮어쓰지 않았다.

## 생성 출처와 정리

- 내장 `image_gen.imagegen`으로 한 번 생성했다. 첫 입력은 승인 팔레트의 [Complete](../palette-v1/prepared-v2/shared32/complete-1x.png) 편집 대상, 두 번째 입력은 [Broken](../palette-v1/prepared-v2/shared32/broken-1x.png) 파손 정도 참고다. 외부 참고 이미지·에셋 팩은 사용하지 않았다.
- [프롬프트](sources/repairing-prompt.txt)와 [1402×1122 RGBA 생성 원본](sources/repairing.png)을 보존했다. 네이티브 160×128을 도구가 납품한 것으로 표기하지 않는다. AI 생성 출처 기록은 최종 제작 납품·출처 승인을 대신하지 않는다.
- 원본은 `.codex/generated_images/01a0e389-e95e-7dc0-8907-5b4fb12a6f3c/exec-097c86d1-8306-422f-8291-0e1012c3121d.png`에서 복사했으며 외부 저장 원본도 그대로 남겼다.
- 사용자에게 허용받은 Python 변환·픽셀 정리 방식으로 [prepare_workshop_repairing.py](../../../../../scripts/assets/prepare_workshop_repairing.py)를 실행했다. 이미지 전체 캔버스를 최근접 160×128로 대응했으며 불투명 영역만 따로 맞추지 않았다. 알파는 128 기준으로 0/255, 투명 RGB는 0, 색은 승인 팔레트에 R/G/B 가중치 2/4/3으로 대응했다.
- AI 수정에서 전체 건물 크기·간판 윤곽이 변한 부분을 발견했다. 따라서 Complete를 바탕으로 **왼쪽 지붕 `(12,37)-(72,77)`, 왼쪽 작업 발판 `(12,74)-(61,123)`, 앞쪽 판자 `(98,79)-(120,123)`**만 새 이미지에서 가져왔다. 나머지는 기존 구조를 보존한다. 생성 원본 오른쪽 가장자리의 지붕은 최종 후보에 사용하지 않았다.
- 문·아치 `(65,81)-(94,128)`와 기초 `(0,123)-(160,128)`를 Complete 픽셀로 고정했다. Repairing의 간판·박공 `(57,58)-(98,86)`도 Complete로 복원하여 패치 경계의 간판 흔들림을 없앴다. 수정 범위·입력 해시·상태별 차이·출력 해시는 [preparation.json](prepared-v1/preparation.json)에 기록했다.
- 별도 연기·반짝임·그림자·작업자는 건물에 넣지 않았다. 수리 중 상태는 미완성 지붕, 노출 서까래, 왼쪽 발판 및 따뜻한 창으로 표현한다.

## 검사 결과

- 480×128 시트의 세 셀과 개별 160×128 PNG가 일치하며 서로 다른 세 상태다.
- 모든 기본 프레임은 RGBA8/sRGB, 알파 0/255, 투명 RGB 0이며 승인 32색 안에 있다. Broken·Complete는 각각 22색, Repairing은 31색이다. 흑백 비교 PNG는 색 가독성 검토용이다.
- 세 상태의 문·기초 픽셀이 일치하고 불투명 영역과 바닥 마지막 행 y=127이 같다. Repairing의 Complete 대비 4,274개 변경 픽셀은 지정한 세 영역 안에만 있다.
- 2×/3× 확대의 최근접 픽셀 복제, 기존 두 상태의 픽셀 보존을 별도 검사했다.
- 새 폴더에 재실행하여 **22개 출력 파일 모두 바이트 단위로 일치**했다. 입력 원본 SHA-256도 유지됐다.
- Chrome에서 PNG 로드, 1× 흑백·어두운 배경·고정 영역 표시, 다음 상태 버튼과 Town 상태 선택, 2×/3× 및 밝은 배경 전환을 확인했다. 페이지 콘솔 오류는 없었다.
- 카탈로그 검증과 기존 3개 테스트가 통과했다. 46개 항목은 모두 미배정이며 새 시트를 게임에 배정하지 않았다.

## 남은 시각 검토와 다음 작업

Repairing이 1×·흑백에서 두 상태와 구분되는지, 지붕·목재·발판과 기존 외형이 자연스럽게 연결되는지는 사용자 검토 대상이다. 자동 검사는 이 미술적 판정을 대신하지 않는다. 카탈로그 출처·납품 승인과 실제 장면 배치 검토도 별도로 남는다.

Town 참조는 기존 viewport 1280×720, Workshop 원점 (796,324), Mina 원점 (680,415)을 유지한다. HTML은 (600,254)에서 440×260을 잘라 표시한다. 클러스터는 House (48,32), Workshop (192,32), Mina (168,174)를 사용한 별도 아트 비교다. PNG·Canvas 재현이며 Godot 실행 캡처나 Windows 입력 QA가 아니다. 게임 상태 전환 시점·진행·저장·동선·충돌을 바꾸지 않았다.

다음 제작인 [Companion 핵심 소품 후보](../companion-props-v1/review.html)와 Mina의 도구·발 정렬 비교를 준비했다. 소품 외형 승인과 실제 배치·도구 분리 구현은 후속 검토 대상이다. 전체 효과 시트·Reward 런타임 연결 및 실제 Windows QA도 남는다.

## 재현

Windows·Pillow 12.3.0을 사용한다. 프롬프트의 LF는 이 폴더의 `.gitattributes`로 유지한다. 출력은 새 폴더여야 한다.

```powershell
.\build\tools\art-python\Scripts\python.exe scripts/assets/prepare_workshop_repairing.py `
  docs/art/reviews/2026-10-11/workshop-repairing-v1 `
  build/workshop-repairing-check-new
```

사용자 검토용 Chrome 탭과 루프백 서버를 유지했다:
`http://127.0.0.1:8765/docs/art/reviews/2026-10-11/workshop-repairing-v1/review.html`.
