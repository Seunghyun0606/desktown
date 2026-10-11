# 사용자 QA 키트 준비 — 2026-10-11

## 빌드와 변경 범위

사용자 요청에 따라 최신 승인 아트 12개가 포함된 Windows 프로토타입을 QA 가능한 상태로 준비했다. 앱 런타임·저장 스키마·제품 모드는 변경하지 않았다. `first-launch` 초기 온보딩 fixture, 이름 있는 분리 QA 저장 재열기, 시작 버튼, CI 키트 복사를 추가했다. 기존 세 진행 fixture와 매번 새 복사본을 만드는 런처 기본 동작은 유지한다.

기준 빌드 SHA는 `4ba5a2eeb256f9a7d56748ca0a2912b80b7835f6`이다. Godot 4.7.2 .NET·SDK 8.0.425로 Windows Release를 내보냈다. 내보내기 로그는 `build/export-logs/20261011-191549-1f01b10b`에 있다. 내보내기 직후 QA 변경을 커밋했으며 해당 커밋과 내보내기 입력은 동일하다.

## 자동 검사 결과

| 항목 | 결과 |
| --- | --- |
| Release .NET 테스트 | 254개 통과: Domain 96, Application 91, Foundation 30, Windows 37 |
| Python 테스트 | 카탈로그 3개·Windows 내보내기 7개 통과 |
| 카탈로그 | 46개 ID, 12 production·34 미배정 |
| 네 fixture 생성·재생성 | 고정 날짜 2026-10-11의 save.json 바이트 모두 일치; 초기 fixture 온보딩 미완료·원장 0·Focus 0 확인 |
| Windows 내보내기 | 종료 0·오류 로그 없음·196개 배포 파일 |
| Windows PowerShell 5.1 런처 | 네 시나리오 normal application_started, 종료 0, 원본 해시·호출 환경 유지 |
| 이름 있는 분리 QA 저장 | 같은 저장 재열기 시 다른 Scenario로 덮어쓰지 않음; 두 실행 로그 별도 보존 |
| 앱 ZIP·압축 해제 파일 | 체크섬·196개 파일 크기 및 SHA-256 일치 |
| 키트 preflight -RunSmoke | 내보낸 앱의 write/recover/check 세 프로세스 통과; 개인 저장 사용하지 않음 |
| 키트에 복사된 런처 | first-launch 헤드리스 기동 정상 종료; 사용자의 focus 세션은 아직 만들지 않음 |
| 결과 양식 | 빌드 SHA·Windows 버전·모니터 경계 포함, 모든 수동 항목 Not tested |

앱 ZIP SHA-256: `f2957a3469977842cdef54b8e974187d77258349fd0aeb101a1cdc29ef088a83`.

## 전달과 남은 검사

[사용자 시작 안내](USER_QA_START.md)의 로컬 키트·시작 버튼으로 QA를 진행할 수 있다. 포터블 키트에는 앱 ZIP·체크섬·전체 파일 목록·4개 fixture·실행 스크립트·결과 양식·검사 안내가 들어 있다. SDK·편집기·개인 저장은 포함하지 않는다.

실제 Windows 창·포커스·포인터 입력·DPI·잠금/절전 이벤트, 일반 저장 위치, SDK 없는 깨끗한 PC 실행은 **미검증**이다. 헤드리스 기동과 기존 GPU 렌더 검사를 수동 QA 합격으로 바꾸지 않았다. Ghost는 일반 Focus에서 비활성화하고 F10 QA 미리보기만 유지한다. 남은 34개 아트·음향과 Workshop·환경의 후속 제작/납품 게이트는 별도로 진행한다. 사용자 실패 보고를 받으면 진행 손실·입력 가로채기(P0), 포커스 탈취·화면 밖 표시(P1)부터 수정한다.
