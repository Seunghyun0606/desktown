# 승인 Town 11개 실제 배정·장면 검증

사용자가 **Town 아트·납품·2배 배치**를 승인했다. 실제 `assets/art/` PNG와 카탈로그에 배정하고 `TownScene.tscn`, `TownScene.cs`, `TownEnvironment.cs`에 승인 배치를 옮겼다. [납품 기록](../../../../../assets/art/DELIVERY_2026-10-12.md). 실제 카탈로그는 23 production·23 미배정이다.

검사 입력은 `8453904` 아카이브에 [preparation.json](preparation.json)의 명시된 실제 작업 파일과 PNG를 복사한 것이다. 실제 카탈로그는 수정 없이 사용한다. 검토 fixture의 `ApplyProposal`은 호출하지 않는다. HUD·Mina·그림자·환경·소품 위치는 실제 장면과 뷰 코드에서 나온다. 주변 NPC만 `BindAmbient(TimeSpan.Zero)`로 고정해 시간에 따른 캡처 차이를 제거했다.

검사에서는 실제 도메인 API로 파손/수리/완성 상태를 만들고 수리 중 Running, 완성 시 분리 시뮬레이션의 Reveal을 전달했다. 개인 세션·저장을 읽거나 완료 이벤트를 소비하지 않는다. 최초 검사는 세 상태에서 null 세션을 전달해 Mina가 논리적으로 Home에 머물렀고 검토본과 달랐다. 뷰의 저장 위치 로직을 바꾸는 대신 검사 입력을 실제 Work/Celebrate 상태로 맞춰 재검사했다.

- Godot 4.7.2 .NET / SDK 8.0.425 / Compatibility GPU. [빌드](build.log), [캡처](capture.log), [실행 결과](result.json).
- Workshop 세 상태 × 75/100/125/150% = **12개 캡처**. [목록](captures/result.json).
- **12개 PNG 모두 승인된 proposed 캡처와 바이트 단위 일치**, 가려지지 않은 건물·잔디의 출력 275,416개 픽셀 일치. [검증](verification.json).
- 후보 해시·실제 작업 파일 해시·검사 코드 해시 확인, 메인 카탈로그 검사 전후 동일.
- 빌드 오류/경고 0, 캡처 경고/오류 없음. 시각 검사는 네이티브 입력·DPI QA가 아니다.

정적 환경은 `_Ready`에서 한 번 구성하며 반복 Bind는 Workshop·Mina·그림자·HUD만 갱신한다. 환경 픽셀이 없으면 해당 소품을 생략하고, 잔디가 미배정이면 기존 기하학적 바닥·길을 유지한다. 정적 환경은 입력 노드를 추가하지 않는다. Focus/Energy·저장·창 정책과 QA-PIP-001 TODO는 유지한다.

렌더 검증 후 숨겨진 기하학적 Path의 좌표도 새 2× 월드의 (16,264)로 조정했다. 배정된 잔디를 사용하는 실제 화면에서는 이 노드가 숨겨져 출력에 영향을 주지 않는다. 별도 합성 fixture에서 바닥·길의 Visible과 Path 좌표, House/Library/Workshop/Companion 대체 표시를 확인했다. [합성 대체 검사 로그](fallback-smoke.log)의 `DESKTOWN_CI_ART_SMOKE_OK`를 확인했고 메인 카탈로그 해시는 보존했다. 카탈로그 Python 테스트 3개도 통과했다.

## 재현

```powershell
python scripts/assets/prepare_godot_town_review.py build/town-delivery-replay --runtime-catalog
python scripts/assets/run_godot_companion_review.py build/town-delivery-replay `
  --godot build/tools/godot-4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64.exe `
  --dotnet-root "$env:LOCALAPPDATA/DeskTownBuildTools/dotnet-8.0.425"
.\build\tools\art-python\Scripts\python.exe scripts/assets/audit_godot_town_review.py build/town-delivery-replay
```

다음은 [NPC 5클립 후보](../npc-v1/review.html)의 외형·동작 검토와 납품 승인이다. 그 뒤 남은 효과 5개·소품/UI/지도 5개를 제작·연결하고 전체 최종 화면과 새 사용자 QA 빌드를 만든다. 음향 8개는 별도다.
