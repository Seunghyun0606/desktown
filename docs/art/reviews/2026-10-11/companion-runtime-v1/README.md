# Companion 실제 장면 연결 — 2026-10-11

사용자가 **A: 렌더 배치 승인 — Companion 런타임 연결 진행**을 선택했다. 승인된 배치와 그림자 연결을 검사 인스턴스에서 실제 `CompanionStage.tscn`/`CompanionStage.cs`로 옮겼다. 장면의 작업대 PNG 원점 (135,91), 의자 (108,116), 램프 (282,26), 그림자 (108,141), Mina (100,103)을 사용한다. Floor/Window 노드는 기존 그대로다.

## 변경과 검증

- 장면에 최근접·왼쪽 위 원점의 `Shadow` Sprite2D를 Floor 다음에 배치했다. 실제 Stage가 `CMP-SHD-001`을 찾아 텍스처·영역을 연결한다. 파일이 미배정이거나 잘못된 규격이면 텍스처를 비우고 숨긴다. 새 입력 컨트롤을 추가하지 않는다.
- 작업대를 16px 왼쪽으로 옮기고 의자를 고정한 32×32 호스트로 연결했다. 램프 호스트 너비 15→16으로 정수 좌표를 유지했다. 그림자와 의자는 Mina 뒤에 그린다.
- 검사 템플릿의 좌표 변경·그림자 생성 코드를 제거했다. 실제 장면 바인딩을 검사하고, 임시 카탈로그에서 아트를 공급한 분리 프로세스로 GPU 렌더했다. 코드/장면 입력 해시는 [preparation.json](preparation.json)에 기록했다.
- Godot 4.7.2 .NET / SDK 8.0.425 빌드 경고·오류 0, 실제 30프레임 × 4배율 + ON 4장 = **124개 캡처가 승인된 분리 검증의 SHA-256과 모두 일치**했다. [동일한 캡처 보기](../companion-godot-v1/review.html), [픽셀 검사](verification.json), [실행 결과](runtime-result.json), [빌드](build.log), [렌더](capture.log).
- 별도 프로세스에서 합성 카탈로그로 전환해 기존 Town/Companion 바인더와 새 미배정 Shadow 숨김을 검사했다. `DESKTOWN_CI_ART_SMOKE_OK`, 종료 0, 오류 없음([로그](fallback-smoke.log)). 합성 준비 도구는 테스트하지 않는 ID를 미배정으로 돌려 향후 실제 아트 배정 이후에도 대체 표시 검사가 유효하다. 원본 카탈로그는 쓰지 않았다.
- 메인 카탈로그 검증/기존 Python 계약 테스트 3개·프로젝트 구조 검사·변경 공백 검사를 통과했다. 개인 저장·Focus 진행·Energy·스키마·창 입력 코드를 바꾸지 않았다. 네이티브 입력/DPI QA는 수행하지 않았다.

## 다음 배정을 위한 구체적인 파일 목록

[assignment-plan.json](assignment-plan.json)에 **12개 대상 파일의 경로·원본 SHA-256·ID·영향 범위**를 준비했다. Mina 6개 클립과 Workbench/Lamp/Window/Floor/Stool/Shadow 6개다. 별도 Tool은 Work에 망치가 이미 들어 있어 배정 대상에서 제외했다. 램프는 기존 기본 셀 0을 사용하며 자동 점등 정책을 추가하지 않는다. 도구 접촉 이벤트도 확정하지 않았다.

Mina는 기존 공통 카탈로그를 사용하므로 6개 클립을 배정하면 **Town·Companion·Ghost 미리보기 모두** 같은 승인 시트를 사용한다. 이 변경을 Companion만의 별도 그림으로 분기하지 않는다.

사용자가 **A: 납품·사용 승인 — 12개 아트 배정**을 선택했다. 준비한 12개 PNG를 실제 배포 아트 경로에 복사하고 메인 production 카탈로그에 배정했다. [납품 기록](../../../../../assets/art/DELIVERY_2026-10-11.md)과 [실제 카탈로그 검증](../companion-delivery-v1/README.md)에 출처·해시·렌더 결과를 기록했다. 일반 실행에서 승인된 Mina와 Companion 소품을 사용한다. 34개 항목은 미배정이다.

통과한 납품 게이트의 근거는 [카탈로그 규칙](../../../../../assets/catalogs/README.md), [검증기](../../../../../scripts/assets/validate_catalog.py)의 P0 `placeholder` 금지, 이전 검토에서 별도로 남긴 최종 납품·출처 판정이다. 새로운 임시 상태나 라이선스를 임의로 만들지 않는다. 사용자의 승인에 따라 준비한 파일과 출처 기록을 카탈로그에 연결했다. 자금 조달용 전체 에셋 완료나 Windows 출시 QA 완료를 의미하지 않는다.

## 재현

커밋 후에는 새 폴더로 기존 Godot 준비/실행/픽셀 검사 명령을 실행하면 된다. 커밋 전의 세 파일만 확인하려면 준비 명령에 `--runtime-working-tree`를 넣는다.

```powershell
python scripts/assets/prepare_godot_companion_review.py build/companion-runtime-check-new
python scripts/assets/run_godot_companion_review.py build/companion-runtime-check-new `
  --godot build/tools/godot-4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64.exe `
  --dotnet-root "$env:LOCALAPPDATA/DeskTownBuildTools/dotnet-8.0.425"
.\build\tools\art-python\Scripts\python.exe scripts/assets/audit_godot_companion_review.py `
  build/companion-runtime-check-new
```

합성 검사 재현은 같은 분리 프로젝트에서 `create_smoke_art.py` 실행 후 다시 가져오고, `DESKTOWN_CI_ART_SMOKE=1`로 `res://scenes/app/AppRoot.tscn`을 헤드리스 실행한다. 앞의 렌더 결과는 합성 전환 전에 검사하고 보존한다. 개인 앱을 실행하지 않는다.
