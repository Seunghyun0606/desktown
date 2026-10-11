# Town 아트 11개 · 실제 Godot 배치 검토 — 2026-10-12

[검토 페이지](review.html)에서 Workshop 파손·수리 중·완성, 기존 배치와 월드 2× 배치안, 네 가지 렌더 배율을 비교한다. **납품·런타임 배정·배치 승인은 대기 중**이다. 사용자의 목표는 아트 완성과 최종 화면이며, 이번 묶음은 그 중 Town 건물·환경 단계다.

## 제작 입력과 후보

기존 승인 House·Workshop PNG를 바이트 단위로 보존했다. 승인된 잔디·길·덤불 대표 셀에서 변형을 준비했고, 나무·꽃·울타리·바위·Library는 OpenAI `image_gen`으로 생성했다. 실제 생성 PNG·정확한 프롬프트·도구 출력 경로는 [sources](sources/generation.json)에 보존했다. 승인된 Python 변환으로 알파 기준 128, 최근접 축소, 승인 공통 32색 매핑, 바닥 중앙 정렬을 수행했다. 원본 생성 이미지를 최종 픽셀 시트라고 취급하지 않는다.

| ID | 파일 | 셀·프레임 | 입력 |
| --- | --- | --- | --- |
| BLD-HOU-001 | bld_house.png | 128×128 · 1 | 승인 House 그대로 |
| BLD-WRK-001 | bld_workshop_states.png | 160×128 · 3 | 승인 Broken/Repairing/Complete 그대로 |
| BLD-LCK-001 | bld_library_locked.png | 128×128 · 1 | 새 AI Library, 닫힌 문·책 표식 |
| ENV-GRA-001 | env_grass_tiles.png | 32×32 · 2 | 승인 잔디 + 좌우 반전 |
| ENV-DIR-001 | env_dirt_tile.png | 32×32 · 1 | 승인 잔디 무늬를 승인 흙색으로 매핑 |
| ENV-PTH-001 | env_path_tiles.png | 32×32 · 3 | 승인 가로 길 + 세로 회전·교차 |
| ENV-TRE-001 | env_tree_variants.png | 64×96 · 2 | 새 AI 나무 2종 |
| ENV-BUS-001 | env_bush_variants.png | 32×32 · 2 | 승인 덤불 + 좌우 반전 |
| ENV-FLW-001 | env_flower_variants.png | 32×32 · 2 | 새 AI 꽃 2종 |
| ENV-FEN-001 | env_fence_variants.png | 32×32 · 2 | 새 AI 울타리 2셀 |
| ENV-ROC-001 | env_rock_variants.png | 32×32 · 2 | 새 AI 바위 2종 |

[prepared-v2/preparation.json](prepared-v2/preparation.json)은 원본 해시, 셀·시트 크기, 후보 해시, 피벗과 검사를 기록한다. PNG는 sRGB, 승인 32색, 이진 알파다. 잔디·흙은 불투명이다. 각 다중 셀은 픽셀이 구별되며, 잔디·가로 길의 좌우 끝은 연결된다. 길은 가로·세로·교차 3셀이고 모든 자동 타일 연결 규칙을 제공하지 않는다. `prepared-v1`은 최초 준비 기록이며, 늘어난 길 무늬를 흙으로 사용해 반복 띠가 보였던 문제를 v2에서 수정했다. 현재 검토·렌더 입력은 **v2**다.

## 실제 렌더 검증

`d59cce0`의 `git archive`로 분리 프로젝트를 만들고 후보 11개를 그 프로젝트의 카탈로그에만 임시 배정했다. 분리 카탈로그의 `production` 표시는 검증기용 fixture이며 실제 납품 승인으로 해석하지 않는다. 실제 메인 카탈로그는 **12 production·34 미배정**, 검사 전후 SHA-256이 같다. 개인 저장·AppRoot·Focus 세션 실행 코드는 호출하지 않는다. 엔진 사용자 디렉터리도 `DeskTownArtReview-*`로 분리했다.

Godot 4.7.2 .NET / SDK 8.0.425 / NVIDIA GeForce RTX 4070 SUPER Compatibility GPU로 가져오기·빌드·렌더했다. 기존 `TownScene.tscn`, `TownScene.Bind`, `CatalogRectArt`, Mina 클립 코드를 사용했다. Workshop은 실제 시뮬레이션의 0/10/25분 투영을 전달하고 세 프레임의 RegionRect와 최근접 필터를 검사했다. 애니메이션·주변 타이머는 정지시켜 화면 비교를 고정했다. 보상 연출을 소비하거나 저장을 수정하지 않았다.

- **15개 PNG:** 기존 배치 100% 3장 + 배치안 75/100/125/150% × 3상태 12장. [렌더 결과](godot-v1/captures/result.json).
- 모든 캡처 크기와 SHA-256, 후보 11개·검사 코드 해시를 확인했다. 메인 카탈로그는 변경되지 않았다. [검증 JSON](godot-v1/verification.json).
- 배치안 100%에서 캐릭터에 가리지 않은 건물 상부와 잔디 셀의 **275,416개 출력 픽셀**이 입력의 2× 최근접 픽셀과 일치했다. 전체 화면 모든 픽셀 일치를 뜻하지 않는다.
- [빌드 로그](godot-v1/build.log): 오류 0, 경고 0. [캡처 로그](godot-v1/capture.log): `DESKTOWN_TOWN_REVIEW_OK`, 종료 0, Godot 경고/오류 없음. 로컬 경로는 로그에서 치환했다.
- 눈으로 파손/수리/완성 구분, 기존/2× 배치 크기, 75% 가독성, 소품의 HUD 가림을 확인했다. 흙 반복 띠와 오른쪽 아래 버튼 위 소품을 수정하고 다시 렌더했다.

처음 분리 검사는 TextureFilterEnum 한정 이름 누락으로 빌드 실패했다. 이를 수정한 후 렌더 성공했고, 마지막 검사에서는 Control 크기를 고정해 앵커 경고도 제거했다. 실패와 중간 실행은 `build/town-art-review-v1`~`v3`에 남으며, 게시된 결과는 **v4 실행**이다. 미리보기 PNG를 직접 수정하지 않고 검사 코드와 입력을 수정한 후 Godot로 다시 캡처했다.

## 배치안과 승인 경계

Town 1280×720 안에 640×360 논리 월드를 2×로 그린다. 건물·Mina·환경은 2×, HUD·Focus 버튼은 원래 크기다. House/Workshop 출입구와 Library를 길에 연결하고 나무·꽃·덤불·울타리·바위를 배치했다. 정수 좌표를 사용한다. 이 배치는 분리된 검사 코드에서만 적용했다. 실제 Scene/카탈로그·Focus/Energy·저장 스키마·Windows 창 정책은 변경하지 않았다.

Noah/Rumi·모닥불은 아직 기하학적 대체 표시다. Library는 책 표식과 닫힌 문이지만 잠금 UI 아이콘은 후속 UI 제작 범위다. 정지 장면 검토로 걷기 경로, NPC·효과 애니메이션, 보상 연출 연결, Windows 입력·DPI를 증명하지 않는다. 실제 전체 최종 아트 완성이라고 표기하지 않는다.

이번 11개 외형·납품·배치가 승인되면 실제 경로·카탈로그·Town 장면에 옮겨 같은 렌더를 재검증한다. 그 뒤 **NPC 5개, 효과 5개, 소품·UI·지도 5개**를 제작·연결하고 Town/Companion/Reward/Discovery 최종 화면과 새 사용자 QA 빌드를 제공한다. 음향 8개는 별도 제작·믹스·승인 대기다. [QA-PIP-001](../../../../qa/QA_FEEDBACK_2026-10-12.md)은 사용자 요청대로 TODO다.

## 재현

승인 팔레트와 Pillow가 있는 Python을 사용한다. 각 출력은 새 폴더여야 한다. 후보를 재현할 때는 `build/` 출력으로 준비하고 prepared-v2와 PNG 해시를 비교한다. 검토 프로젝트 준비 도구는 저장소의 prepared-v2를 입력으로 사용한다.

```powershell
.\build\tools\art-python\Scripts\python.exe scripts/assets/prepare_town_completion.py `
  docs/art/reviews/2026-10-12/town-complete-v1 build/town-candidates-replay
python scripts/assets/prepare_godot_town_review.py build/town-review-replay
python scripts/assets/run_godot_companion_review.py build/town-review-replay `
  --godot build/tools/godot-4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64.exe `
  --dotnet-root "$env:LOCALAPPDATA/DeskTownBuildTools/dotnet-8.0.425"
.\build\tools\art-python\Scripts\python.exe scripts/assets/audit_godot_town_review.py build/town-review-replay
```

자료 게시 도구 `publish_godot_town_review.py`는 새 `godot-v1` 폴더에 한 번만 게시한다. 기존 승인·검토 증거를 덮어쓰지 않는다. 검토 페이지는 외부 라이브러리 없이 로컬 HTML로 열 수 있다.
