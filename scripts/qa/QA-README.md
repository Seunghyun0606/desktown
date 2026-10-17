# DeskTown Windows 11 플레이·QA 키트

시각 아트 38개·음향 8개와 Workshop 완료 후 일상 Focus가 적용된 MVP다. 전체 ZIP을 **새 빈 폴더**에 풀고 다른 DeskTown을 종료한다. 편집기·SDK는 필요 없다. 미서명 프로토타입이며 실제 Windows 입력·DPI·잠금·장치 청취와 깨끗한 PC 검증은 남아 있다. Ghost Focus는 비활성이다.

## 시작 버튼

| 버튼 | 용도 / 저장 |
| --- | --- |
| **Start-Play.cmd** | 처음부터 플레이, `qa-sessions/play` 유지 |
| **Start-Daily-Focus.cmd** | 복구된 Town에서 반복 Focus, `qa-sessions/daily` 유지 |
| **Start-Quick-Look.cmd** | 새 분리 데모로 Workshop 완성 → 지도 발견 보기 |
| **Start-QA.cmd** | 온보딩·Focus·복구 QA, `qa-sessions/focus` 유지 |
| **Prepare-QA.cmd** | ZIP·전체 파일·분리 저장/복구 사전 검사 후 새 결과 양식 생성 |

플레이만 하려면 Start-Play 또는 Start-Daily-Focus를 사용한다. 조작 안내는 `PLAY-README.md`다. 각 런처는 개인 저장을 사용하지 않으며 창 제목에 `[Demo — isolated save]`가 표시된다. 스크립트는 실행 정책을 변경하지 않는다. 정책이 차단하면 오류와 해당 환경의 허용 절차를 확인한다. `app/DeskTown.exe` 직접 실행은 **일반 개인 저장**을 사용한다.

## QA 준비와 결과 기록

1. 앱을 종료하고 **Prepare-QA.cmd**를 실행한다. `preflight.ps1 -RunSmoke`가 안쪽 앱 ZIP·SHA-256·전체 파일 목록과 별도 세 프로세스의 저장/복구/재읽기를 검사한다. 성공하면 `qa-reports/qa-results-날짜-고유번호.md`가 새로 생긴다. 기존 보고서·진행은 덮어쓰지 않는다.
2. 보고서의 실제 Windows 배율·검사 앱 종류·창 상태·모니터 조합을 채운다. 자동으로 기록하는 항목은 빌드 SHA, 생성 시각, Windows/PowerShell 버전, 모니터 해상도·원점이다. `qa-handoff.json`이 있으면 실행 코드 SHA와 QA 도구 SHA를 구분한다. 환경 종류는 검사자의 선언이며 깨끗한 PC 인증이 아니다.
3. 아래 순서로 검사하고 Pass/Fail/Not tested, 실제 시각, 재현과 이슈 ID를 적는다. **양식 생성은 수동 검사 합격이 아니다.** 앱·DPI·모니터·창 상태 조합마다 새 양식을 쓴다. 자세한 통과 조건은 `WINDOWS_GATE.md`다.

정확한 배율을 넣거나 깨끗한 Windows 11 환경임을 직접 기록하려면 키트 PowerShell에서 다음을 사용한다.

```powershell
./prepare-qa.ps1 -Scaling '125%' -EnvironmentKind CurrentPC
# 편집기·SDK 없는 별도 Windows 11 환경에서만:
./prepare-qa.ps1 -Scaling '100%' -EnvironmentKind CleanWindows11
```

사전 검사를 다시 하지 않고 **새** 결과 양식만 만들 때:

```powershell
./collect-evidence.ps1 -Scaling '150%' -OutputPath './qa-results-run2.md'
```

## 권장 검사 순서

| 단계 | 실행 / 검사 | 기대 동작 |
| --- | --- | --- |
| 빠른 화면·소리 | Start-Quick-Look, 이후 Start-Daily-Focus | 완성·발견이 순서대로 한 번 표시; Town 아트·개별 NPC 동선·완성된 Workshop과 Focus with Mina 표시 |
| 설정 | 트레이 Audio·Reduced motion 변경 후 종료·같은 버튼 재실행 | 음소거/효과 감소가 즉시 반영되고 저장됨; 창 재열기에 소리 몰아 재생 없음 |
| 실제 25분 Focus | Start-Daily-Focus → 25분·Any App·PiP → 평소 작업 | PiP만 남고 Town 창/작업 표시줄 항목 없음; 타이핑·클릭·드래그·스크롤 유지 |
| 모드·위치 | 같은 세션에서 Minimized → Hidden → PiP, 크기·상단 드래그 | 같은 세션 유지; Minimized는 Town 작업 표시줄만, Hidden은 트레이만; 크기·위치 저장 |
| 완료·반복 | 완료 후 트레이 Open, 다음 세션 시작 | PiP 숨김·트레이 완료 표시; Town 자동 열림 없음; 일일/총 기록 증가, Workshop 유지, 보상·지도 재등장 없음 |
| 복구 | 새 분리 Focus → 체크포인트 15초 이상 → QA 앱만 강제 종료 → 같은 버튼 | Resume/End 선택, 종료 중 시간 제외; PiP/Minimized/Hidden 각각 확인 |
| 수명주기·장치 | 별도 세션에서 실제 잠금·절전·복귀, 실제 스피커/헤드폰 청취 | 중단 중 시간·소리 제외; 재개 시 누락·중복·소리 폭주 없음 |

빠른 확인과 실제 25분 완료를 구분한다. 45/60분 프리셋·세 모드의 완료·알림 on/off·100/125/150%·단일/듀얼 모니터·각 앱 입력 매트릭스는 `WINDOWS_GATE.md`에서 별도로 기록한다. QA-PIP-001은 코드 수정과 native 26개 검사를 마쳤으며 실제 Desktop/입력/DPI 재확인은 이 회차에서 수행한다. 자동 검사로 실제 입력이나 물리적 청취를 Pass로 채우지 않는다.

## 진행 보존과 추가 회차

`play`, `daily`, `focus`는 서로 다른 저장이다. 같은 버튼은 같은 저장을 재개한다. 초기화하려고 폴더를 지우지 말고 새 세션 이름을 사용한다.

```powershell
./launch-demo.ps1 -Scenario first-launch -WindowsExport ./app -DemoSaves ./demo-saves -SessionName focus2
```

그 밖의 fixture는 `workshop-repairing`, `workshop-reveal-pending`, `railway-teaser`다. 이름 없는 실행은 매번 새 분리 복사본을 만들고 원본 샘플을 바꾸지 않는다. `-Headless`는 기동 확인용이며 실제 UI 합격이 아니다.

이전 키트의 플레이를 옮기려면 앱을 모두 종료하고 새 키트의 동일 대상 폴더가 비어 있을 때만 `qa-sessions/play` 등을 복사한다. 대상에 이미 진행이 있으면 덮어쓰지 않는다. 일반 저장 위치·업데이트·롤백 검사는 별도 Windows 테스트 계정에서 진행하고 기존 저장을 먼저 백업한다. QA 런처의 분리 저장으로 일반 `user://` 검증을 대신하지 않는다.

보고서에는 P0(진행 손실·입력 차단), P1(반복 포커스 탈취·화면 밖 표시·작업 중단), P2(시각·문구)를 기록한다. 작업 문서·창 제목·입력 내용·브라우저 URL·개인 저장 또는 `qa-sessions` 전체를 첨부하지 않는다.
