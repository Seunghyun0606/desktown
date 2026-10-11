# Mina Walk·Rest·Stretch·Celebrate — 2026-10-11

## 현재 판정

사용자가 공통 32색을 제작 기준으로 승인하고 다음 제작을 요청하여, 남은 네 가지 Mina 동작의 **20프레임 검토 후보**를 준비했다. [검토 페이지](review.html)는 1×/2×/3×, 개별 프레임, 중립·Idle·Work 비교, 밝고 어두운 배경, 40% 불투명도와 Companion 장면 참조를 제공한다. 2026-10-11 사용자가 네 클립의 동작 방향을 승인하고 Workshop Repairing 제작 진행을 선택했다. 제작용 세부 품질·장면 접촉은 후속 검토로 남는다. 생성 당시 `preparation.json` 판정은 당시 기록으로 보존한다.

| 클립 | 시트 | 프레임 | 속도 | 재생 | 이벤트 후보 |
| --- | --- | --- | --- | --- | --- |
| Walk | [288×48](prepared-v1/chr_mina_walk.png) | 6 | 8 FPS | 반복 | 없음 |
| Rest | [192×48](prepared-v1/chr_mina_rest.png) | 4 | 2 FPS | 반복 | 없음 |
| Stretch | [192×48](prepared-v1/chr_mina_stretch.png) | 4 | 4 FPS | 한 번 | 없음 |
| Celebrate | [288×48](prepared-v1/chr_mina_celebrate.png) | 6 | 8 FPS | 한 번 | frame3 `celebrate_peak` |

전체 프레임은 [3× 비교 보드](prepared-v1/comparison-board.png)에서 볼 수 있다. 각 클립의 개별 PNG, 3× 시트 및 1×/3× APNG도 `prepared-v1/`에 있다. Stretch·Celebrate APNG는 한 번만 재생한다. HTML도 마지막 프레임에서 멈추며 다시 재생할 수 있다. 게임의 Idle/Rest 복귀 로직을 이 페이지가 대신 실행하지 않는다.

## 생성 출처

- 내장 `image_gen.imagegen`으로 네 가지 클립을 각각 생성하고 Walk를 한 번 추가 생성했다. 투명 배경을 요청했다.
- 입력: 승인된 외형을 승인 팔레트로 대응한 [중립 프레임](../palette-v1/prepared-v2/shared32/neutral-1x.png). Walk 재생성에는 최초 Walk 원본도 참고로 제공했다.
- 최초 네 프롬프트는 [prompts.json](sources/prompts.json), Walk 추가 생성은 [walk-retry-prompt.txt](sources/walk-retry-prompt.txt)에 보존했다.
- 원본: [Walk 최초](sources/walk-grid-initial.png), [Walk 재생성](sources/walk-grid.png), [Rest](sources/rest-grid.png), [Stretch](sources/stretch-grid.png), [Celebrate](sources/celebrate-grid.png).
- Walk 두 원본과 Celebrate는 1536×1024, Rest는 1233×1275, Stretch는 1288×1221 RGBA다. 생성된 이미지의 크기는 납품 시트 규격과 다르므로 사용자에게 이미 허용받은 Python 변환·픽셀 정리를 수행했다.
- 외부 에셋 팩이나 참고 이미지는 사용하지 않았다. 최종 제작 납품·출처 승인으로 표기하지 않는다.

## 픽셀 정리

[prepare_mina_motion.py](../../../../../scripts/assets/prepare_mina_motion.py)는 원본을 행 우선 순서로 분리하고, 승인 중립의 모자 폭을 기준으로 최근접 축소한다. 높이를 모두 40px로 늘리지 않아 Rest의 앉은 자세를 유지한다. 발의 마지막 행을 y=44로 맞추고 공통 셀 피벗 (24,44)를 유지한다. Rest의 원본 첫 행 부츠가 이미지 중앙선을 넘어서므로 눈으로 확인한 빈 여백 y=700에서 행을 나눴다. 이 위치와 각 추출 범위는 [preparation.json](prepared-v1/preparation.json)에 기록했다.

생성된 모자·얼굴의 크기 흔들림과 잔여 픽셀을 제거하기 위해 머리 영역 전체를 지우고 승인된 중립 머리를 붙였다. Rest는 머리를 4px 낮춘 셀 내부 자세이며 런타임 피벗이나 노드 원점의 새 예외를 추가하지 않았다. 찻잔 및 Stretch·Celebrate의 들어 올린 손은 지정한 영역에서 보존했다. Stretch·Celebrate는 중립 하체를 고정하여 발이 움직이지 않게 했다. Walk의 다리 형상은 생성 원본을 유지하고 두 번째 반주기에서 앞·뒤 다리와 반대 팔의 명암을 승인 팔레트로 정리했다. 비슷한 옆모습 실루엣에서 다른 쪽 다리를 구분하기 위한 픽셀 편집이며, 이것만으로 자연스러운 걷기라고 판정하지 않는다.

불투명 색은 [승인된 공통 32색](../../../palettes/README.md)으로 R/G/B 가중치 2/4/3의 가까운 색에 대응했다. 효과용 별도 6색은 쓰지 않는다. 원본의 흐린 배경·반투명 가장자리는 알파 128 기준으로 정리하여 0/255만 남기고 투명 RGB는 0으로 만들었다. 디더링은 하지 않았다. 원본과 승인 중립은 수정하지 않았다.

## 검사 결과

- 20개 프레임 모두 48×48이며 클립 내부 중복·빈 프레임이 없다.
- 네 가로 시트는 RGBA 8-bit/sRGB이고 개별 프레임과 픽셀이 일치한다.
- 불투명 색은 승인 32색의 부분집합, 알파는 0/255, 투명 RGB는 (0,0,0)이다.
- 모든 프레임의 발 마지막 행은 44이며 셀 끝에 닿거나 잘린 픽셀이 없다. 이는 발바닥의 세부 형태나 보행 접촉의 자연스러움을 승인하는 검사가 아니다.
- APNG의 프레임 수·FPS 타이밍·반복 플래그와 각 확대 프레임의 픽셀 일치를 별도로 확인했다.
- 새 경로에 다시 변환하여 **38개 출력 파일 전체가 바이트 단위로 일치**했다. 입력 SHA-256도 그대로 유지됐다.
- Chrome에서 PNG 로드, Celebrate 마지막 프레임 정지 및 다시 재생, Rest 개별 프레임 정지, 배율 1/2/3, 밝고 어두운 배경, 40% 불투명도 및 Companion 참조 전환을 확인했다. 페이지 콘솔 오류는 없었다.
- 카탈로그 계약 검사 및 기존 3개 테스트를 실행했다. 46개 항목은 모두 미배정이다.

## 남은 검토와 후속 작업

머리 정체성, 팔·도구의 해부학적 연결, Walk 양쪽 다리와 반복 경계, Rest 찻잔의 가독성, Stretch·Celebrate 진폭과 회복 자세는 사용자 시각 검토 대상이다. 자동 검사는 동작 품질을 증명하지 않는다. Rest의 의자·소품은 아직 없으며 앉은 자세의 실제 지지점은 핵심 Companion 소품 제작에서 검토해야 한다.

Companion Mina 원점 (100,103)과 바닥 시작 y=154는 기존 장면 참조를 그대로 사용한다. 마지막 발 픽셀 y=147 뒤에 6개의 빈 행이 남는 기존 정렬 문제는 아직 해결하지 않았다. 이 HTML/PNG 검토는 Godot 장면 실행 또는 실제 Windows 창·입력 QA가 아니다.

승인 후 다음 작업은 Workshop Repairing과 핵심 Companion 소품이다. 기존 Idle·Work 10프레임과 이번 20프레임을 합쳐 Mina 6클립 검토 자료가 준비됐지만, 제작 납품 완료로 판정하지 않는다. 카탈로그 배정·실제 장면 접촉·전체 효과 시트와 Reward 런타임 연결은 후속 작업이다. `celebrate_peak`도 표시용 후보이며 게임 진행·음향·효과에 연결하지 않았다.

## 재현

Windows와 Pillow 12.3.0을 사용한다. 프롬프트는 이 검토 폴더의 `.gitattributes`로 LF 줄바꿈을 유지하여 Git 체크아웃이 기록된 입력 해시를 바꾸지 않게 한다. 새 출력 경로만 허용하므로 원본이나 기존 결과를 덮어쓰지 않는다.

```powershell
.\build\tools\art-python\Scripts\python.exe scripts/assets/prepare_mina_motion.py `
  docs/art/reviews/2026-10-11/mina-motion-v1 `
  build/mina-motion-check-new
```

HTML은 파일로 직접 열 수 있다. 현재 사용자 검토용 Chrome 탭과 루프백 서버를 유지했다:
`http://127.0.0.1:8765/docs/art/reviews/2026-10-11/mina-motion-v1/review.html`.
