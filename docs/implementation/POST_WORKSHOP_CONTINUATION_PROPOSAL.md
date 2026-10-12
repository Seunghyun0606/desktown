# Workshop 이후 플레이 — 범위 결정안

상태: **인간 결정 대기**. 기준 코드 `d082435`와 현재 음향 MVP를 검토했으며 실제 게임의 버튼·프로젝트·저장 형식은 변경하지 않았다.

## 지금 플레이가 끝나는 지점

현재 UX 고정 원칙은 완성 가능한 프로젝트 1개와 다음 프로젝트 예고 1개다([UX_FLOW §1](../design/UX_FLOW.md)). Workshop 25 Focus 완료 → 완성 연출 → 지도 발견 → Old Railway `0 / 45` 예고까지 구현돼 있다. `TownScene.Bind`는 Workshop 완료 상태에서 Focus 버튼을 비활성화한다. `EventSystem.IsRailwayPlayable`은 false이며 `ProjectSystem`의 현재 프로젝트는 RestoreWorkshop으로 고정돼 있다. 이는 현재 명세에 맞는 제한이다.

사용자가 다음 작업을 요청했고 이전 목표는 디자인을 입힌 초기 MVP를 빠르게 플레이하는 것이다. 다음 세션을 시작할 수 있게 만드는 확장은 플레이 범위를 바꾸므로 자동으로 해제하지 않는다.

## 선택할 구현 범위

| | A — 완료 후 일상 Focus (추천) | B — Old Railway 실제 프로젝트 |
| --- | --- | --- |
| 플레이 | 복구된 Workshop에서 Mina와 Focus를 반복 | 지도 발견 뒤 45 Focus 탐험을 진행 |
| Town | `Workshop restored` + `Focus with Mina`; Railway는 `Coming in the next build` 예고 | 현재 프로젝트를 Explore Old Railway로 전환, `0 / 45`부터 실제 누적 |
| Focus 설정 | 완료 후 제목 `Focus with Mina`, 설명 `The Workshop is restored. Stay a while.`; 기존 25/45/60분·세 모드 유지 | 탐험 제목·진행·Mina 위치/작업 문맥을 추가 |
| 기록·보상 | 일일/총 Focus 기록은 누적, Workshop 25/25·완성/지도 보상은 재지급하지 않음 | 두 프로젝트 진행·완료·이벤트를 따로 관리해야 함 |
| 아트 | 현재 승인된 38개와 캐릭터 행동·음향 재사용 | Railway 장소·진행 변화·완성 연출의 제작/승인 필요 |
| 저장 | 분리 검사에서 기존 V1 호환을 확인. 새 필드·자동 초기화 불필요 | 현재 단일 Workshop 복원 규칙을 바꿔야 하며 구체 저장/마이그레이션 결정은 구현 전 별도 검토 |
| 목적 | 빠르게 반복 플레이 가능한 MVP 전달 | 추가 개발을 거쳐 세계 진행 콘텐츠 확장 |

### A의 실제 변경 목록

1. `TownScene`에서 복구 완료·보상/지도 확인 이후의 Focus 진입을 열고, 진행 중인 세션의 중복 시작은 기존 controller가 막는다. 대기 중인 연출에서는 진입하지 않는다.
2. Town과 `FocusSetup`의 완료 이후 문구를 구분한다. Railway 예고를 실제 진행처럼 올리지 않는다.
3. 기존 `PrototypeRuntime.StartAsync`·세션 종료·PiP/Hidden/Minimized·Audio·Mina 행동을 사용한다. 추가 Focus를 Workshop 재복구나 새 보상으로 해석하지 않는다.
4. 회귀 조건을 추가한다: 기존 완성 저장에서 진입, 반복 세션의 일일/총 시간, Workshop 유지, 보상 미중복, 저장/재시작/복구, 대기 중 연출 진입 차단, 실제 Windows 새 MVP의 두 번째 세션.
5. 기존 저장을 덮어쓰지 않는 새 플레이 ZIP으로 전달한다.

### B를 선택하면 다음으로 구체화할 것

프로젝트 전환 시점, 과거/초과 Focus의 처리, 두 프로젝트의 독립 진행과 완료 상태, V1 완료·활성·복구 저장의 이전 방법을 먼저 설계한다. 새로운 지역 아트와 완성 보상도 검토본을 준비한다. 현재 V1의 Workshop 진행 필드를 Railway 진행으로 재사용하거나 기존 저장을 초기화하지 않는다. 이번 선택만으로 아직 준비되지 않은 마이그레이션을 실행하지 않는다.

## 안전하게 확인한 기존 런타임

[검사 증거](../qa/POST_WORKSHOP_RUNTIME_ASSESSMENT_2026-10-13.json): 별도 build 콘솔과 가짜 시계로 현재 API를 호출했다. 실제 UI나 개인 저장을 사용하지 않았다.

- 첫 25분 완료와 완성·발견 확인 후, PiP/Minimized/Hidden 모드 값으로 1분씩 추가 세션을 완료할 수 있다.
- 총/일일 Focus 28분, Workshop 진행 25분, 소비한 보상 이벤트는 동일하다.
- V1 저장을 다시 열어 기록을 복원한다. 완료 후 새 세션의 1분 체크포인트를 만들고 60분의 앱 종료 시간을 지난 뒤 EndAtCheckpoint로 복구하면 총 29분이며 보상은 중복되지 않는다.
- 12조건 통과. 이는 기존 런타임·저장 경계의 근거이며 아직 사용자 UI 진입이나 실제 Windows 반복 플레이를 구현/검사했다는 뜻은 아니다.

재현:

```powershell
./build/tools/art-python/Scripts/python.exe scripts/qa/assess_post_workshop.py `
  --output build/post-workshop-assessment-new `
  --dotnet-root "$env:LOCALAPPDATA/DeskTownBuildTools/dotnet-8.0.425"
```

별도로 남아 있는 실제 장치 청취·전체 입력/DPI·깨끗한 Windows PC·설치/서명 검증은 이 결정으로 합격 처리하지 않는다. 현재 음향 MVP와 개인 진행은 그대로 유지한다.
