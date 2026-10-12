# 최종 시각본 사용자 QA 시작 — 2026-10-12

## 최신 일상 Focus MVP — 2026-10-13

승인된 완료 후 반복 Focus가 연결됐다. `build/DeskTown-MVP-Daily-2026-10-13.zip`의 **Start-Play.cmd**로 플레이하거나 **Start-Daily-Focus.cmd**로 완료된 별도 마을에서 바로 시작한다. **Focus with Mina**는 일일·총 시간을 계속 누적하며 Workshop과 지도 보상은 중복되지 않는다. [검증·전달 기록](QA_EXECUTION_2026-10-13_DAILY_FOCUS.md)을 따른다. 아래 음향·아트 키트는 이전 빌드다.

## 최신 음향 적용 MVP — 2026-10-13

사용자가 음향 8종·믹스·사용을 승인했다. 새 키트는 `build/DeskTown-MVP-Audio-2026-10-13.zip`이다. 전부 풀고 **Start-Play.cmd** 또는 **Start-Quick-Look.cmd**로 실행한다. 시각 38개와 음향 8개가 모두 적용됐으며 [새 검증 기록](QA_EXECUTION_2026-10-13_AUDIO_MVP.md)을 따른다. 트레이 **Audio** 체크를 끄면 소리가 멈추고 설정이 저장된다. PiP·Hidden·재열기·완성·발견의 소리와 실제 장치에서의 음량을 확인한다. 기존 진행을 이어가려면 앱을 종료하고 이전 `qa-sessions/play`를 새 키트의 빈 같은 위치로 복사하며 이미 진행이 있으면 덮어쓰지 않는다. 아래 빌드·무음·승인 대기는 이전 기록이다.

## 최신 캐릭터 행동 개선본

사용자가 이전 MVP를 모두 확인했다고 보고했고 캐릭터 행동의 부자연스러움·패턴 부족을 후속 과제로 지정했다. 새 빌드는 `build/DeskTown-MVP-Behavior-2026-10-12.zip`이다. 압축 해제 후 Start-Play/Start-Quick-Look으로 실행한다. [행동 미리보기](../art/reviews/2026-10-12/behavior-v1/review.html)와 [행동 검증 기록](QA_EXECUTION_2026-10-12_BEHAVIOR.md)을 따른다. 기존 플레이를 이어가려면 앱을 종료하고 이전 키트의 `qa-sessions/play` 폴더를 새 키트의 같은 위치에 복사한다. 새 키트에 이미 진행이 있으면 덮어쓰지 말고 기존 키트를 계속 사용한다.

## 최신 플레이용 MVP

아트 완성 후 PiP 메인 창 문제를 수정한 플레이용 키트는 `build/DeskTown-MVP-2026-10-12.zip`이다. 압축을 전부 풀고 **Start-Play.cmd**로 저장을 유지하며 플레이하거나 **Start-Quick-Look.cmd**로 완성 연출부터 확인한다. [플레이 안내](../../scripts/qa/PLAY-README.md)와 [새 창 검사 기록](QA_EXECUTION_2026-10-12_MVP.md)을 따른다. 아래 `06ac9136` 키트는 이전 최종 아트 검증 기록이다.

검사 대상 빌드는 `06ac9136969f97700a8efb3221d0edf2e337000d`다. 승인된 **시각 아트 38개가 모두 적용**됐고 음향 8개는 미배정이다. [전체 최종 화면 55장](../art/reviews/2026-10-12/final-views-v1/review.html)을 먼저 볼 수 있다. 이번 키트는 최종 시각 적용본의 기능·창·입력·저장 검사이며 출시 합격 판정은 아니다. PiP에서 Town 창이 함께 나타나는 **QA-PIP-001은 알려진 TODO**다.

## 현재 PC에서 실행

키트는 저장소의 `build/qa-kit-final-art-06ac9136`에 있다. 이동용 전체 ZIP은 `build/DeskTown-final-art-QA-06ac9136.zip`이다. 편집기와 SDK 없이 실행 가능하다. 다른 DeskTown을 종료하고 다음 순서로 검사한다.

1. 키트 폴더의 **Start-QA.cmd**를 더블클릭한다. ZIP·전체 파일을 확인한 다음 초기 온보딩으로 실행한다.
2. 창 제목의 **[Demo — isolated save]**를 확인한다. 개인 저장 대신 `qa-sessions/focus/save.json`을 사용한다.
3. 아래 검사 후 트레이에서 앱을 종료한다. 같은 버튼을 다시 누르면 기존 QA 저장을 재개한다. 강제 종료·복구 검사에도 같은 버튼을 사용한다.
4. **qa-results.md**의 실제 Windows 배율과 Pass/Fail/Not tested를 채운다. 실패에는 재현 순서, 기대/실제 동작, 발생 모드를 기록한다. 작업 문서·개인 저장·타이핑 내용은 첨부하지 않는다.

이 PC에서 실행 정책은 RemoteSigned였다. 시작 버튼은 실행 정책을 변경하지 않는다. 다른 PC에서 정책이 실행을 차단하면 오류를 기록하고 해당 환경의 허용 절차를 따른다.

## 최종 아트부터 보기

| 키트 파일 | 내용 |
| --- | --- |
| `art-preview/review.html` | 실행 없이 55개 실제 GPU 최종 화면·배율 비교 |
| `Start-Town-Art.cmd` | 복구된 Town·NPC·연기·모닥불·식생을 분리된 Railway 예고 저장으로 실행 |
| `Start-Workshop-Reveal.cmd` | 새 분리 저장으로 수리 중 → 완성 연출 → Old Railway Map 발견을 재현 |
| `Start-Repairing-Art.cmd` | 수리 중 Town에서 시작해 일반 Focus/Companion도 확인 |
| `Start-QA.cmd` | 0분 온보딩·Focus·재시작·복구용 이름 있는 QA 세션 |

각 앱 검사를 마치면 트레이에서 종료한 뒤 다른 버튼을 사용한다. Art 시작 버튼은 매번 새 데모 저장을 사용한다. `Start-QA.cmd`만 `qa-sessions/focus`를 재사용한다. 개인 저장 위치는 사용하지 않는다. 트레이 `Reduced motion`의 효과 감소와 재시작 후 저장 유지도 확인한다.

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

다른 PC에는 `build/DeskTown-final-art-QA-06ac9136.zip` 전체를 전달하고 빈 폴더에 압축을 푼다. 포함된 SHA256SUMS와 preflight로 안쪽 앱 ZIP을 확인한다. 이미 작성한 결과 양식은 덮어쓰지 말고 보존하며 새 회차에는 `collect-evidence.ps1 -Scaling '실제 배율' -OutputPath './qa-results-new.md'`를 사용한다.
