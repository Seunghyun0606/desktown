# DeskTown QA 후속 실행 — 2026-09-28

## 기준

`main` HEAD는 `02e176f`이며 아래 결과는 **미커밋 작업 트리**에서 얻었다. 기존 `build/qa-kit-ready`의 ZIP과 매니페스트는 이전 커밋의 빌드이므로 현재 창 변경을 검증하는 배포 산출물로 사용하지 않는다. 현재 소스의 로컬 Windows 내보내기는 무시된 경로 `build/windows-current`에 있으며 공식 QA 키트가 아니다.

최종 사용자 결정은 Focus의 세 가지 배타적 표시 모드다. PiP는 Companion 창만, Minimized는 Town 작업 표시줄 항목만, Hidden은 트레이만 남기도록 연결했다. 앞선 "항상 최소화" 결정은 이 결정으로 대체됐다. Town 장면은 Focus 동안 숨겨 주변 애니메이션을 멈춘다. Minimized에서 작업 표시줄 창을 복원하면 기존 Town을 열고, PiP 닫기는 같은 Focus 세션을 Hidden으로 전환한다. PiP 크기는 Focus 설정과 실행 중 창 버튼 또는 트레이에서 75/100/125/150% 중 고르며, 헤더 드래그로 위치를 옮긴다.

## 자동 검증

| 검사 | 결과 |
| --- | --- |
| 고정 .NET SDK 8.0.425 Release 빌드 | 경고·오류 0 |
| .NET 테스트 | Domain 96, Application 90, Foundation 30, Windows Platform 37 — 총 253개 통과 |
| Godot 4.7.2 .NET Windows Release 내보내기 | `build/windows-current/DeskTown.exe` 생성 |
| 현재 내보내기의 분리된 저장·재시작·복구 | `write`, `recover`, `check` 종료 코드 0 및 성공 마커 확인 |
| 현재 내보내기의 분리된 데모 기동 | `workshop-repairing`, `workshop-reveal-pending`, `railway-teaser` 헤드리스 오류 없음 |
| QA 사전 검사 | Windows PowerShell 5.1에서 기존 키트의 무결성 및 분리된 저장 검사 통과 |
| QA 결과 양식 | 세 표시 모드, 작업 표시줄 복원, PiP 크기·위치 검사 행 생성 확인 |

첫 내보내기 시도는 Godot가 시스템 전역 SDK 9를 선택해 .NET 프로젝트 게시에 실패했다. `DOTNET_ROOT`와 `PATH`를 저장소가 고정한 SDK 8.0.425로 지정한 두 번째 내보내기는 성공했다. Godot 명령의 종료 코드만으로 성공을 판정하지 않고 게시 단계의 오류와 결과 파일을 확인했다.

## 남은 Windows 실사용 검사

현재 내보내기에서 세 모드의 실제 Town 작업 표시줄 항목과 PiP 가시성을 확인한다. Minimized에서 작업 표시줄 Town을 복원할 때 같은 세션이 유지되는지, Town을 다시 닫았을 때 선택한 모드로 돌아가는지 검사한다. PiP 크기 버튼·헤더 드래그·다중 모니터 위치 복원, 다른 앱의 포커스와 입력, 여러 DPI·모니터 구성, 세션 완료 후 Town 열기도 검사한다. 이 창 동작은 헤드리스 검사로 판정할 수 없다. 현재 변경을 커밋한 뒤 새 CI QA 키트의 SHA와 체크섬을 기준으로 [Windows 검증 게이트](WINDOWS_GATE.md)를 기록한다.

Ghost의 일반 Focus 사용과 출시 승인은 계속 보류한다.

## 추가 검증 메모

새 `Minimized` 값은 기존 V1 저장 데이터의 표시 모드 문자열과 변경 이력에 저장된다. 기존 모드 값의 의미를 변경하거나 저장 스키마 버전을 올리지 않았다. 애플리케이션 테스트에는 세 모드의 표시 전환, 복구, V1 직렬화 왕복이 포함된다. Windows Release 내보내기에서 분리된 저장 `write`/`recover`/`check`와 세 데모의 헤드리스 기동이 모두 통과했다. 실제 창 동작의 합격 판정은 위 수동 검사 후에 기록한다.
