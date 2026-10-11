# 사용자 QA 시작 — 2026-10-11

Windows 프로토타입의 사용자 QA를 시작할 수 있다. 검사 대상 빌드는 `4ba5a2eeb256f9a7d56748ca0a2912b80b7835f6`이며 Mina 6클립·Companion 소품 6개가 적용됐다. 나머지 아트·음향 34개는 미배정이므로 Town 건물·일부 소품은 대체 표시다. 이번 검사는 기능·창·입력·저장 흐름이며 전체 아트 완성이나 출시 합격 판정은 아니다.

## 현재 PC에서 실행

키트는 저장소의 `build/qa-kit-userqa-4ba5a2ee`에 있다. 편집기와 SDK 없이 실행 가능하다. 다른 DeskTown을 종료하고 다음 순서로 검사한다.

1. 키트 폴더의 **Start-QA.cmd**를 더블클릭한다. ZIP·전체 파일을 확인한 다음 초기 온보딩으로 실행한다.
2. 창 제목의 **[Demo — isolated save]**를 확인한다. 개인 저장 대신 `qa-sessions/focus/save.json`을 사용한다.
3. 아래 검사 후 트레이에서 앱을 종료한다. 같은 버튼을 다시 누르면 기존 QA 저장을 재개한다. 강제 종료·복구 검사에도 같은 버튼을 사용한다.
4. **qa-results.md**의 실제 Windows 배율과 Pass/Fail/Not tested를 채운다. 실패에는 재현 순서, 기대/실제 동작, 발생 모드를 기록한다. 작업 문서·개인 저장·타이핑 내용은 첨부하지 않는다.

이 PC에서 실행 정책은 RemoteSigned였다. 시작 버튼은 실행 정책을 변경하지 않는다. 다른 PC에서 정책이 실행을 차단하면 오류를 기록하고 해당 환경의 허용 절차를 따른다.

## 권장 검사 순서

| 순서 | 검사 | 기대 동작 |
| --- | --- | --- |
| 1 | 온보딩 완료 → Focus 설정 | 개인정보 안내와 세 모드 선택 가능; Start 전에는 Focus 누적 없음 |
| 2 | 25분 PiP Focus → 다른 앱에서 입력 | 작은 창만 남고 작업 앱의 타이핑·클릭·스크롤 유지 |
| 3 | PiP −/+·트레이 배율 변경, 상단 드래그 | 75/100/125/150% 표시와 화면 내 위치 조절; 세션 유지 |
| 4 | 트레이에서 Minimized → Hidden → PiP | 각각 Town 최소화·트레이만·작은 창; 같은 세션과 누적 유지 |
| 5 | 25분 완료 | PiP가 숨겨지고 트레이 완료 표시; Town이 자동으로 열리지 않음 |
| 6 | 트레이 Open → 보상 확인 | Town이 명시적으로 열리고 대기 중인 Workshop/발견 연출이 한 번 표시 |
| 7 | 앱 정상 종료·다시 시작 | 설정과 진행 유지 |
| 8 | 새 Focus 시작 → 15초 이상 대기 → QA 앱만 강제 종료 → 같은 시작 버튼 | 복구 Resume/End 선택; 앱 종료 중 시간 제외; 선택 모드 복귀 |
| 9 | 별도 세션에서 Windows 잠금·절전·복귀 | 중단 시간이 누적되지 않고 재개 가능 |

첫 회에는 한 세션 안에서 모드를 전환하고, 이어 각 모드의 완료와 트레이 알림만 사용한 완료를 검사한다. 첫 회 25분 완료 이후에도 새 Focus를 시작할 수 있다. PiP 크기·이동·입력 검사는 현재 Windows 배율부터 하고 가능하면 125/150%, 보조 모니터에서도 반복한다.

## 빠른 연출 검사

Focus QA 앱을 종료한 뒤 키트 폴더의 PowerShell에서 실행한다. 각 명령은 새 분리 복사본을 만들며 원본 샘플을 바꾸지 않는다.

```powershell
./launch-demo.ps1 -Scenario workshop-repairing -WindowsExport ./app -DemoSaves ./demo-saves
./launch-demo.ps1 -Scenario workshop-reveal-pending -WindowsExport ./app -DemoSaves ./demo-saves
./launch-demo.ps1 -Scenario railway-teaser -WindowsExport ./app -DemoSaves ./demo-saves
```

한 실행을 종료한 뒤 다음 명령을 실행한다. 일반 Ghost Focus는 비활성 상태다. F10은 Focus가 없을 때만 QA 미리보기로 검사하고 [Windows 입력 매트릭스](WINDOWS_GATE.md)를 기록한다.

## 새 검사와 다른 PC

진행을 초기화하려면 기존 세션을 삭제하지 말고 새 이름을 선택한다.

```powershell
./launch-demo.ps1 -Scenario first-launch -WindowsExport ./app -DemoSaves ./demo-saves -SessionName focus2
```

같은 저장을 다시 열려면 같은 SessionName을 사용한다. 일반 저장 위치·설치·업데이트 검사는 별도 계정 또는 저장 백업 후 앱 직접 실행으로 진행한다. QA 런처로는 일반 `user://` 위치를 검증하지 않는다.

다른 PC에는 `build/DeskTown-user-qa-4ba5a2ee.zip` 전체를 전달하고 빈 폴더에 압축을 푼다. 포함된 SHA256SUMS와 preflight로 안쪽 앱 ZIP을 확인한다. 이미 작성한 결과 양식은 덮어쓰지 말고 보존하며 새 회차에는 `collect-evidence.ps1 -Scaling '실제 배율' -OutputPath './qa-results-new.md'`를 사용한다.
