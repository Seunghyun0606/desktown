# Mina Idle·Work 스타일 테스트 — 2026-10-11

## 현재 판정

사용자는 48×48 중립 프레임을 승인하고 Idle·Work 테스트 진행을 선택했다. 이에 따라 두 클립의 **아트 테스트 후보**를 준비했다. [검토 페이지](review.html)에서 재생/정지, 개별 프레임, 1×/2×/3×, Companion 장면 참조 및 밝고 어두운 Ghost 배경과 40% 불투명도를 비교한다. 실제 Windows 창·입력 QA는 수행하지 않았다.

최종 검토 대상은 `prepared-v3/`다. 최초 독립 프레임 변환 `prepared/`와 머리·하체 고정 중간 결과 `prepared-v2/`는 저장소의 무시되는 `build/mina-clips-intermediate-20261011/`로 옮겨 보존했다. AI 원본과 최종 후보는 이 문서 옆에 있다. 2026-10-11 사용자가 Idle·Work 동작 방향을 승인하고 Workshop 스타일 테스트 진행을 선택했다. 장면 배치 및 실제 Windows QA는 후속 검증으로 남는다.

## 생성 출처

- 내장 `image_gen.imagegen`으로 클립마다 별도 생성, 투명 배경 요청.
- 입력: 사용자가 승인한 [중립 프레임의 최근접 3× 확대](../neutral-48-v1/mina-neutral-48-v1-3x.png).
- [Idle 프롬프트](sources/idle-prompt.txt), [Work 프롬프트](sources/work-prompt.txt).
- [Idle 생성 원본](sources/idle-grid.png): 1254×1254 RGBA, 2열×2행.
- [Work 생성 원본](sources/work-grid.png): 1536×1024 RGBA, 3열×2행.
- 외부 참고 이미지나 에셋 팩을 사용하지 않았다. 최종 제작 에셋의 라이선스·납품 승인으로 표기하지 않는다.
- 사용자가 명시적으로 허용한 Python 변환·픽셀 규격 정리 방식으로 추출했다.

## 픽셀 정리

[prepare_mina_clips.py](../../../../../scripts/assets/prepare_mina_clips.py)는 셀을 행 우선 순서로 추출한다. 알파 128 이상 영역을 최근접으로 높이 40px에 맞추고 모자의 중심을 기준으로 가로 배치를 정렬한다. 발의 마지막 행은 y=44다. 모든 불투명 색은 승인된 중립 PNG의 32색 중 가까운 색으로 대응하고 알파는 0/255, 투명 RGB는 0으로 정리한다.

최초 변환에서 모자·얼굴·바지의 불필요한 색 흔들림을 발견했다. Idle은 승인된 중립 프레임을 바탕으로 생성된 가슴/손 영역과 프레임 2의 작은 눈깜빡임 영역만 반영했다. Work는 승인된 머리 실루엣과 하체를 고정하고 생성된 팔·도구 동작을 남겼다. 얼굴 오른쪽의 도구 공간은 유지해 들어 올린 망치가 지워지지 않게 했다. 발 위치와 원본 외형을 유지하기 위한 처리를 변환 기록에 명시했다.

32색은 이번 테스트에 사용한 공통 후보 팔레트이며 프로젝트의 최종 팔레트 확정은 아니다.

## 출력과 검사

| 클립 | 시트 | 프레임 | 속도 | 발 접점 | 이벤트 후보 |
| --- | --- | --- | --- | --- | --- |
| Idle | [192×48 PNG](prepared-v3/chr_mina_idle.png) | 48×48 × 4, 모두 서로 다름 | 2 FPS, 루프 | (24,44), 발 가로 경계 변화 0px | 없음 |
| Work | [288×48 PNG](prepared-v3/chr_mina_work.png) | 48×48 × 6, 모두 서로 다름 | 6 FPS, 루프 | (24,44), 발 가로 경계 변화 0px | frame 4 `tool_contact` |

검사 기록은 [preparation.json](prepared-v3/preparation.json)에 있다. 각 프레임의 크기·알파·팔레트·투명 RGB·발 접점·중복 여부, 저장된 시트의 RGBA/sRGB 및 픽셀 일치, 입력 원본 SHA-256 보존을 검사했다. 이 자동 검사는 동작·루프·제품 정체성을 승인하지 않는다.

- [Idle 전체 프레임 3×](prepared-v3/idle-strip-3x.png).
- [Work 전체 프레임 3×](prepared-v3/work-strip-3x.png).
- 각 프레임 PNG와 배경별 1×/2×/3× APNG도 같은 폴더에 있다. APNG Work는 밀리초 단위 저장 때문에 167ms/프레임이며, 검토 페이지는 시간 기준으로 정확히 6 FPS를 선택한다.
- Chrome에서 PNG 로드, 재생/정지, Work frame4 선택, 1× 표시 및 Ghost 어두운 배경 40% 전환을 확인했고 페이지 오류는 없었다. 시각적 루프와 가독성의 사용자 승인 결과는 아니다.

## 장면 참조와 남은 항목

Companion 배경과 Mina 원점 (100,103)은 현재 `CompanionStage.tscn`을 참조했다. Ghost의 원점 (75,88)과 소품 위치는 `GhostStage.tscn`을 참조하며 단색 밝은/어두운 배경에서 비교한다. 이 페이지는 PNG와 Canvas로 재현한 아트 비교이며 Godot 장면 실행이나 실제 데스크톱의 모든 배경 조합을 검증하지 않는다.

Work frame4의 불투명 영역 오른쪽 끝은 셀 x=41이며 Companion 작업대 왼쪽 x=146까지 5px 간격이 남는다. 자동 테스트를 위해 장면 노드를 옮기지 않았다. Ghost 소품과의 세부 접촉도 후속 장면 배치 검토가 필요하다. 동작 방향은 사용자 승인을 받았으며, 제작용 세부 품질과 장면 접촉은 후속 검토로 남는다.

Companion에서 발 픽셀의 마지막 행은 y=147이고 바닥 시작은 y=154여서 사이에 6개의 빈 행도 남는다. 승인된 프레임의 셀 내부 접점과 실행 장면 바닥 정렬은 별도 문제이며, 현재 테스트는 이를 숨기거나 자동 수정하지 않았다.

게임의 카탈로그 46개 항목은 모두 미배정이며 실행 장면·시뮬레이션·저장 스키마는 변경하지 않았다. `tool_contact`는 이 문서의 표시용 후보이며 게임 진행이나 음향에 연결하지 않았다.

## 재현

Pillow 12.3.0을 사용하는 저장소 내 무시되는 가상환경으로 실행했다. 출력 폴더는 반드시 새 경로여야 한다.

```powershell
.\build\tools\art-python\Scripts\python.exe scripts/assets/prepare_mina_clips.py `
  docs/art/reviews/2026-10-11/idle-work-v1 `
  docs/art/reviews/2026-10-11/neutral-48-v1/mina-neutral-48-v1-1x.png `
  build/mina-clips-check
```

HTML은 직접 파일로 열 수 있다. 로컬 HTTP 검토는 저장소 루트에서 `python -m http.server 8765 --bind 127.0.0.1`을 실행한 뒤 `http://127.0.0.1:8765/docs/art/reviews/2026-10-11/idle-work-v1/review.html`로 연다. 이번 검토에서는 해당 로컬 서버와 Chrome 탭을 사용자 검토용으로 유지했다.
