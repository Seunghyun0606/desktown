# 디자인 적용 플레이 MVP — 2026-10-12

## 목표와 범위

사용자는 최종 화면 확인 후 빠르게 플레이 가능한 디자인 적용 초기 MVP를 요청했다. Town·Focus 설정·Companion Work·Reward 완료 GPU 화면을 확인했고 기존 승인된 시각 38개를 유지했다. QA-PIP-001을 수정하고 `Start-Play.cmd`(저장 유지)와 `Start-Quick-Look.cmd`(완성 연출 즉시 보기)를 준비했다. 음향·새 NPC 보행 스케줄·일반 Ghost 활성화는 이번 범위에 추가하지 않았다.

## 검증

| 검사 | 결과 |
| --- | --- |
| .NET 전체 테스트 | 254 passed, 실패·건너뜀 0 |
| 수정 전 실제 Windows 내보내기 | `build/window-probe-baseline-v6`: PiP 시작 후 Town의 native minimized 검사 실패; 루트 Visible 변경 불가도 로그 확인 |
| 수정 후 실제 Windows 내보내기 | `build/window-probe-fixed-v3`: 26 checks, `DESKTOWN_WINDOW_PRESENTATION_OK` |
| 검사 경로 | PiP 시작, 세 모드 반복 전환, 트레이 Open/복귀, 75/100/125/150%, 닫기, 세 모드 종료·재열기, 최대화 복원 |
| 세션 경계 | 모드·배율 변경·PiP 닫기 전후 동일 Focus ID, End Focus 후 세션 해제 |

창 검사는 개인 저장을 사용하지 않는 분리 프로젝트·Release Windows EXE에서 실행했다. probe는 실제 컨트롤러 명령을 호출하고 Win32의 `IsIconic`, `IsWindowVisible`, `WS_EX_NOACTIVATE`를 읽는다. 입력 합성은 하지 않는다. probe는 `.cs.txt`로 보관하고 분리 프로젝트에서만 컴파일하므로 플레이 빌드에 포함되지 않는다. 에디터 Debug의 SystemEvents 의존성 탐색 차이를 피하기 위해 실제 내보낸 앱을 검사했다.

재현 명령:

```powershell
python scripts/qa/test_window_presentation.py --output build/window-probe-new `
  --godot build/tools/godot-4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64.exe `
  --dotnet-root "$env:LOCALAPPDATA/DeskTownBuildTools/dotnet-8.0.425" `
  --demo-save build/demo-saves-final-art-06ac9136/first-launch/save.json
```

수정 전 검사에는 `--revision 327ae13 --baseline`을 추가한다. 출력 폴더는 새 이름을 쓴다. 별도의 Desktop/typing/DPI/절전/깨끗한 PC QA는 사용자 재확인 대기다. 기존의 Window Mode·Focus 시간·Energy·저장 스키마는 유지된다.

## 플레이 전달

- 폴더: `build/DeskTown-MVP-2026-10-12`
- 이동용 ZIP: `build/DeskTown-MVP-2026-10-12.zip`
- 실제 코드 SHA·전체 배포 파일 해시는 키트의 `build-manifest.json`과 `SHA256SUMS.txt`로 확인한다.
- 일반 플레이의 첫 Focus는 25분이다. 빠른 완성 확인은 별도 데모 버튼을 사용한다.
- `qa-sessions/play`는 재실행 시 유지하며 개인 저장·기존 `focus` QA 세션과 분리한다. 원래 미추적 백업과 `.uid` 파일은 배포하지 않는다.

내보내기 후 `scripts/package_mvp.py`로 완전한 앱 ZIP과 오프라인 최종 화면을 묶고, `preflight.ps1 -RunSmoke`와 데모 런처 검사 결과를 후속 기록한다. 이전 `06ac9136` 최종 아트 키트는 이전 빌드 증거로 보존한다.
