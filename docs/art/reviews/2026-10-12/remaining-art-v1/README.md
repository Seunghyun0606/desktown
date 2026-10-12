# 남은 시각 아트 10개 — 전체 연결 검토 후보

[검토 페이지](review.html)는 실제 Godot 마을·Companion·First Launch·Discovery·Focus 캡처와 33셀 개별 재생을 제공한다. **외형·최종 납품·배치·런타임 연결 승인 대기**다. 실제 카탈로그는 이미 승인된 Mina/Companion/Town/NPC **28 production**을 유지한다. 새 10개를 승인·연결하면 시각 38개가 갖춰지고 음향 8개는 별도로 남는다. 아직 최종 런타임·사용자 QA 빌드 완료가 아니다.

## 입력과 규격

imagegen 스킬의 **내장 image_gen**으로 9개 원본을 생성했다. 정확한 프롬프트·참조·기본 저장 경로는 [generation.json](sources/generation.json), 개별 `sources/*-prompt.txt`와 원본 PNG에 보존했다. 생성 도중 quota로 중단된 이전 호출에는 저장 결과가 없음을 확인한 뒤 재개했다. Tool은 새로 생성하지 않고 이전 승인된 Companion 소품 외형을 사용했다.

| ID | 후보 | 셀 / 개수 | FPS / 반복 |
| --- | --- | --- | --- |
| FX-SMK-001 | Workshop 연기 | 32×32 / 4 | 4 / 반복 |
| FX-FIR-001 | 모닥불 | 32×32 / 4 | 6 / 반복 |
| FX-GRS-001 | 풀 흔들림 | 32×32 / 2 | 2 / 반복 |
| FX-SPK-001 | 작업 반짝임 | 16×16 / 4 | 8 / 한 번 |
| FX-TEA-001 | 차의 김 | 16×16 / 4 | 4 / 반복 |
| CMP-DEC-001 | 식물 / 상자 | 32×32 / 2 | 정지 변형 |
| CMP-TOL-001 | 도구 2자세 | 32×32 / 2 | 정지 변형 |
| UI-LOG-001 | DeskTown 로고 | 256×96 / 1 | 정지 |
| UI-ICO-001 | 아이콘 9개 | 16×16 / 9 | 정지 |
| ITM-MAP-001 | Old Railway Map | 96×64 / 1 | 정지 |

승인된 Python 변환으로 최근접 축소·정렬·팔레트 양자화를 수행했다. 모든 시트는 RGBA8·sRGB·이진 알파·투명 RGB=0이다. 연기·반짝임·김은 승인 효과 6색, 모닥불은 공통 32색+효과 6색, 나머지는 공통 32색이다. 프레임별 크기·피벗·원본 크롭·해시는 [prepared-v2/preparation.json](prepared-v2/preparation.json)에 있다. v1은 피벗 메타데이터 추가 전이며 픽셀은 같다.

효과 애니메이션은 같은 클립 전체의 공통 크롭·배율을 사용해 반짝임의 최대/소멸 크기와 연기 위상 차이를 유지했다. 모닥불 밑부분 6행과 풀 뿌리 2행은 첫 프레임에 맞춰 고정했다. 아이콘 AI 원본은 요청한 균등 셀 경계와 달랐으므로 실제 알파 띠를 분리해 9개 모두 잘리지 않게 16×16에 넣었다. 순서는 ASSET_MANIFEST의 Focus, Workshop, Companion, Hidden, Ghost, Settings, Audio, Project, Discovery다. Tool은 승인 외형을 32색에 맞췄으며 새 손 소품으로 중복 부착하지 않는다.

## 실제 Godot 검증

실제 승인 28개 카탈로그·아트·장면·뷰를 분리 프로젝트로 복사한 뒤 새 후보 10개만 임시 배정했다. `production` fixture 표시는 검증 목적이며 납품 승인이 아니다. 검사 전후 메인 카탈로그 해시는 같았다. 개인 저장·AppRoot·실제 트레이/창 제어는 실행하지 않았다.

- 33셀 × 75/100/125/150% = **132개 개별 GPU 캡처**.
- Town 4효과 위상+효과 없는 정지 × 4배율 = **20개**.
- Companion Work/Rest 각각 4위상+효과 없는 정지 × 4배율 = **40개**.
- First Launch/Discovery/Focus × 4배율 = **12개**.
- 합계 **204개**, 크기·SHA-256 확인. 100% 개별 셀의 **49,408개 픽셀**이 입력과 일치했다.
- 추가한 소품·효과 영역 밖 Town과 Companion 본문 아트는 기존 승인 렌더와 픽셀 단위로 같다. Companion 헤더 30px는 비교에서 제외했다. 150%에서 네이티브 버튼 글꼴 위치가 한 물리 픽셀 달랐으며 이 검사는 헤더·입력·DPI 판정이 아니다.
- Godot 4.7.2 .NET / SDK 8.0.425 / Compatibility GPU. [검증](godot-v1/verification.json), [빌드](godot-v1/build.log), [캡처](godot-v1/capture.log), [204개 렌더 목록](godot-v1/captures/result.json).

최초 준비는 미배정 항목의 피벗이 null인 상태여서 계약 검사에서 중단됐다. 정수 피벗을 선언했다. 첫 렌더의 연기가 굴뚝에서 떨어진 위치에 있었으므로 왼쪽으로 뒤집어 방출점을 굴뚝에 맞췄다. UI 뒤 마을이 글을 방해해 불투명한 패널 배치안을 추가했다. 배율별 헤더 문구도 맞췄다. 게시한 결과는 `build/remaining-art-v4`이며 출력 PNG를 보정하지 않았다.

## 제안하는 연결 범위

- Workshop 완료 상태에서만 연기를 보여주고 Library 가림을 피한다. 모닥불은 기존 주황 대체 표시를 교체한다. 풀 움직임은 두 작은 지점에 제한한다.
- 새 장식은 기존 Plant 바인더로 연결한다. 별도 Tool은 작업대 오른쪽 정지 소품으로 사용한다. Mina의 승인된 손 망치는 유지한다.
- Work 접촉 시 Spark를 한 번, Rest의 찻잔 위에서 작은 김을 재생한다. 검토의 Work/Rest Mina는 고정 프레임이며 실제 접촉 이벤트·재생 타이밍 연결은 아직이다.
- Reduced motion 제안은 연기·풀·Spark·김을 숨기고 모닥불을 첫 프레임에 고정한다. 이 캡처는 실제 설정 저장/전환 검사와 다르다.
- 로고는 기존 First Launch Title을 시각적으로 대체한다. 지도는 기존 Discovery의 문구·버튼 사이에 2×로 넣고 패널 높이를 늘린다. Focus Start와 지원되는 모드 항목에 아이콘을 붙인다. 9개 아이콘 전체의 트레이/설정 사용은 해당 UI가 존재하는 범위에서만 진행한다. 읽기를 위해 세 패널에 따뜻한 어두운 불투명 배경과 여백을 제안한다.

외형·납품·이 배치/연결을 승인받은 뒤 실제 뷰에 적용하고 전체 최종 화면·새 QA 빌드를 준비한다. Focus/Energy·저장 스키마·창 정책을 바꾸지 않는다. PiP 이중 창 QA-PIP-001은 사용자 요청대로 TODO다.

## 재현

```powershell
.\build\tools\art-python\Scripts\python.exe scripts/assets/prepare_remaining_art.py `
  docs/art/reviews/2026-10-12/remaining-art-v1 build/remaining-candidates-replay
python scripts/assets/prepare_godot_remaining_review.py build/remaining-render-replay
python scripts/assets/run_godot_companion_review.py build/remaining-render-replay `
  --godot build/tools/godot-4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64.exe `
  --dotnet-root "$env:LOCALAPPDATA/DeskTownBuildTools/dotnet-8.0.425"
.\build\tools\art-python\Scripts\python.exe scripts/assets/audit_godot_remaining_review.py build/remaining-render-replay
```

새 후보는 외형·납품·배치 승인 전까지 실제 production 카탈로그에 배정하지 않는다.
