# 실제 카탈로그 배정 검증 — 2026-10-11

사용자가 **A: 납품·사용 승인 — 12개 아트 배정**을 선택했다. [납품 기록](../../../../../assets/art/DELIVERY_2026-10-11.md)과 [배정 목록](../companion-runtime-v1/assignment-plan.json)에 따라 12개 PNG를 실제 경로에 복사하고 `production` 카탈로그에 연결했다. Mina 6클립은 Town·Companion·Ghost 미리보기에 공통 적용된다. Companion 소품은 Workbench/Lamp/Window/Floor/Stool/Shadow다.

## 검증한 실제 입력

메인 `asset_catalog.json`과 실제 배정 PNG 12개를 분리 프로젝트에 **그대로 복사**했다. 카탈로그 상태·리소스 경로·출처를 임시 재배정하지 않았다. 입력은 `724acab` 코드와 현재 승인 배정의 JSON/PNG다. 카탈로그 및 PNG 해시는 [preparation.json](preparation.json)에 기록했다. 검증 루트와 엔진 사용자 디렉터리를 분리했고 개인 저장 코드를 실행하지 않았다.

Godot 4.7.2 .NET, .NET SDK 8.0.425로 가져오기·빌드·실제 Compatibility GPU 렌더를 수행했다. [빌드 로그](build.log)는 오류/경고 0, [캡처 로그](capture.log)는 `DESKTOWN_COMPANION_REVIEW_OK` 종료 0이다. [결과](result.json), [픽셀·해시 검사](verification.json), [렌더 파일 목록](render-result.json)을 보존했다.

## 결과

- 46개 ID 중 **12개 production·34개 미배정**. 카탈로그 계약 검사와 Python 테스트 3개 통과. 원래 미배정 카탈로그를 사용하는 테스트는 명시적으로 미배정 fixture를 생성해 실제 배정 수에 의존하지 않는다.
- Companion 30프레임 × 4배율 + 램프 ON 4장, **124개 캡처 SHA-256이 승인된 캡처와 모두 일치**했다. 원래 배율의 Mina 불투명 픽셀 18,055개와 Work/Rest 접촉 영역 10개도 일치했다. [동일한 캡처 보기](../companion-godot-v1/review.html).
- 실제 TownScene의 Home/Idle Mina 불투명 픽셀이 승인 시트와 정확히 일치했다. 나머지 Town 건물은 미배정 대체 표시다([Town 캡처](town-idle.png)).
- 실제 GhostStage의 Work를 40·65·85% 알파로 렌더하고 캐릭터 불투명 픽셀의 출력 알파를 1/255 오차 이내에서 확인했다. 투명 배경의 [40%](ghost-work-40.png), [65%](ghost-work-65.png), [85%](ghost-work-85.png) 캡처를 보존했다. GhostWindow 네이티브 입력 통과 검사가 아니며 일반 Ghost 모드를 켜지 않았다.
- 별도 프로세스에서 분리 카탈로그를 합성 fixture로 전환해 기존 Town/Companion 및 미배정 Shadow/소품 대체 표시 검사를 수행했다. [합성 검사 로그](fallback-smoke.log) `DESKTOWN_CI_ART_SMOKE_OK`, 종료 0. 메인 카탈로그 해시는 검사 전후 동일하다.

배정한 파일만 사용하며 별도 Tool은 미배정이다. Work의 기존 망치를 중복 표시하지 않는다. 램프는 기본 OFF, ON은 셀 검사만 수행한다. 접촉 사운드·이벤트·빛 정책을 추가하지 않았다. Focus/Energy·저장 스키마·창 입력은 변경하지 않았다. Windows 네이티브 창·입력·DPI QA는 남는다.

## 재현

최신 저장소 루트에서 새 폴더를 선택한다. 실제 카탈로그 검증에는 `--runtime-catalog`를 사용한다. 기존 검토 fixture의 13항목 임시 배정과 구분한다.

```powershell
python scripts/assets/prepare_godot_companion_review.py build/companion-delivery-check-new --runtime-catalog
python scripts/assets/run_godot_companion_review.py build/companion-delivery-check-new `
  --godot build/tools/godot-4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64.exe `
  --dotnet-root "$env:LOCALAPPDATA/DeskTownBuildTools/dotnet-8.0.425"
.\build\tools\art-python\Scripts\python.exe scripts/assets/audit_godot_companion_review.py `
  build/companion-delivery-check-new
```

다음 아트 작업은 Workshop 3상태와 House·환경의 실제 Godot 연결 검토다. 기존 스타일 승인을 보존하고, 남은 납품·배치 승인을 각 항목의 근거와 함께 진행한다. 전체 46항목 완료와 Windows 출시 판정은 별도다.
