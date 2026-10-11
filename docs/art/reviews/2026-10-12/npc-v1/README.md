# Noah·Rumi 5클립 / 24프레임 — 검토 후보

[검토 페이지](review.html)에서 실제 Godot 마을 배치와 각 클립 반복을 본다. **외형·동작·최종 납품·실제 카탈로그 배정 승인 대기**다. 기존 승인 Town 11개는 실제 게임에 적용했으며, 이번 NPC 후보는 분리 프로젝트에만 배정했다. Mina는 승인된 공통 아트를 사용한다.

## 입력과 준비

`imagegen` 스킬과 내장 `image_gen`으로 각 클립을 별도 생성했다. Noah/Rumi Idle은 승인 Mina를 스타일·크기 참조로 사용했고, Walk/Read는 같은 캐릭터의 생성된 Idle을 외형 참조로 사용했다. [생성 기록·정확한 프롬프트](sources/generation.json)와 원본 PNG 5개를 보존했다. Noah는 차분한 세로 실루엣·긴 외투·책, Rumi는 단발·짧은 재킷·가방으로 ART_BIBLE의 역할 구분을 따른 후보다.

| ID | 시트 | 프레임 | FPS |
| --- | --- | --- | --- |
| CHR-NOA-001 | chr_noah_idle.png | 4 | 2 |
| CHR-NOA-002 | chr_noah_walk.png | 6 | 8 |
| CHR-NOA-003 | chr_noah_read.png | 4 | 2 |
| CHR-RUM-001 | chr_rumi_idle.png | 4 | 2 |
| CHR-RUM-002 | chr_rumi_walk.png | 6 | 8 |

승인된 Python 픽셀 변환으로 셀별 투명 영역을 잘라 최근접 축소하고, 같은 클립 내 공통 배율·머리 중심·발 접점 y=44에 정렬했다. Noah 기준 높이는 40px, Rumi는 38px다. 48×48·피벗 (24,44)·오른쪽 방향·공통 32색·sRGB·이진 알파·투명 RGB=0을 적용했다. 생성 원본의 반투명 경계는 알파 128에서 분리했고, 정규화 후 떨어져 있는 1~2픽셀 조각을 제거했다. 책·가방과 연결된 팔다리 픽셀은 유지한다.

현재 입력은 **prepared-v2**이며 [프레임별 규격·출처·해시](prepared-v2/preparation.json)를 제공한다. v1은 작은 떠 있는 픽셀 제거 전의 준비 기록이다. 24개 프레임은 모두 구별되며 발 기준 행은 44다. 이 검사는 자연스러운 걷기, 얼굴 일관성, 책 페이지 전환의 품질을 대신하지 않는다. 프레임을 단순히 평행 이동해 걷기로 표기하지 않았고 원본에 생성된 팔다리 자세를 사용했다.

## 실제 Godot 검증

승인된 Town 카탈로그·장면·뷰를 그대로 복사한 분리 프로젝트에 NPC 5개만 임시 배정했다. `production` fixture 표시는 도구 검사 목적이며 실제 납품 승인이 아니다. 실제 메인 카탈로그는 **23 production·23 미배정**을 유지했다. 개인 저장·AppRoot는 실행하지 않았다.

- 24개 프레임 × 75/100/125/150% = **96개 개별 GPU 캡처**.
- 실제 TownScene의 3상태 × Noah Idle/Read × 4배율 = **24개 마을 GPU 캡처**. 기존 Town NPC 뷰의 Idle/Read/Rumi Idle 바인딩을 사용했다.
- 합계 **120장**, 크기·SHA-256 확인. [렌더 목록](godot-v1/captures/result.json), [검증](godot-v1/verification.json).
- 100% 개별 캡처의 **221,184개 픽셀**이 입력 프레임의 2× 최근접 결과와 일치했다(투명 영역은 출력 알파 확인).
- 마을 24장에서 두 NPC 셀 영역을 제외한 출력은 승인된 Town 런타임 캡처와 픽셀 단위로 같다. 건물·지형·Mina·HUD·소품 배치를 바꾸지 않았다.
- Godot 4.7.2 .NET / SDK 8.0.425 / Compatibility GPU, 빌드 오류·경고 0, 캡처 경고·오류 없음. [빌드 로그](godot-v1/build.log), [캡처 로그](godot-v1/capture.log).

최초 검사에서 마을 SubViewport까지 투명 배경으로 만들어 HUD 합성이 승인 캡처와 달랐다. 개별 NPC는 투명, 실제 마을은 기존 불투명 렌더로 수정해 다시 캡처했다. 게시된 결과는 `build/npc-review-v2` 실행이다. PNG를 보정해 검사에 맞추지 않았다.

## 승인 범위와 남은 작업

5클립의 외형·정체성·걷기 반복·책 자세를 함께 검토한다. Walk는 시트 자체를 실제 GPU로 렌더했으며 게임의 기존 5초 주변 위치/읽기 스케줄과 NPC 선택 로직을 바꾸지 않았다. 새로운 연속 보행 경로나 이동 연출을 검증했다고 주장하지 않는다. 기존 Idle/Read 뷰에 배정 가능한 시트와 추후 Walk용 시트를 준비한 것이다.

NPC 5개가 승인·배정되면 **28 production**, 시각 **10개**(효과 5개 + 장식·도구·로고·아이콘·지도 5개)가 남는다. 남은 제작·연결 후 Town/Companion/Reward/Discovery 전체 최종 화면과 새 사용자 QA 빌드로 진행한다. 음향 8개는 별도이고 QA-PIP-001은 요청대로 TODO다. 이번 결과는 네이티브 창·입력·DPI 판정이 아니다.

## 재현

```powershell
.\build\tools\art-python\Scripts\python.exe scripts/assets/prepare_npc_art.py `
  docs/art/reviews/2026-10-12/npc-v1 build/npc-candidates-replay
python scripts/assets/prepare_godot_npc_review.py build/npc-render-replay
python scripts/assets/run_godot_companion_review.py build/npc-render-replay `
  --godot build/tools/godot-4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64.exe `
  --dotnet-root "$env:LOCALAPPDATA/DeskTownBuildTools/dotnet-8.0.425"
.\build\tools\art-python\Scripts\python.exe scripts/assets/audit_godot_npc_review.py build/npc-render-replay
```

생성에는 내장 도구를 사용했다. `sources/*-prompt.txt`가 최종 프롬프트이며, 원본과 검토본은 저장소에 보존된다.
