# DeskTown Windows 11 노트북 QA 키트

이 키트에는 버전이 붙은 서명되지 않은 앱 ZIP, SHA-256 체크섬과 파일 목록, 분리된 데모 저장 데이터 네 개, PowerShell 스크립트가 들어 있다. 편집기나 SDK는 필요 없다. ZIP과 파일 목록을 함께 보관한다. 목록에 기록된 빌드 SHA를 이슈 보고의 식별자로 사용한다.

## 개인 저장과 분리한 사용자 QA

사전 검사 후 `Start-QA.cmd`를 실행하거나 키트 폴더의 PowerShell에서 다음 명령을 실행한다. 처음에는 온보딩·Focus 누적 0 상태로 시작하며, 이후 같은 명령은 `qa-sessions/focus/save.json`을 다시 연다. 개인 저장 대신 이 복사본으로 Focus, 모드 전환, 위치·설정 저장, 강제 종료 후 복구를 검사할 수 있다. 강제 종료 후에도 같은 명령으로 다시 열고, 처음부터 검사하려면 새 `SessionName`을 사용한다. 기존 세션을 덮어쓰지 않는다.

```powershell
./launch-demo.ps1 -Scenario first-launch -WindowsExport ./app -DemoSaves ./demo-saves -SessionName focus
```

창 제목의 `[Demo — isolated save]`를 확인한다. QA 시작 시 다른 DeskTown 인스턴스는 닫는다. `qa-sessions`에는 저장과 실행별 로그가 남으므로 개인 자료와 함께 업로드하지 않는다. 데모 시나리오에는 초기 상태 `first-launch`와 기존 세 가지 진행 상태가 있다. 최신 빌드는 승인된 시각 아트 38개를 모두 사용하며 음향 8개는 미배정이다. 이번 QA는 최종 시각 적용본의 기능·창·입력 검사이며 출시 승인과는 다르다. PiP에서 Town 창이 함께 나타나는 QA-PIP-001은 알려진 TODO다.

`Start-QA.cmd`는 실행 정책을 변경하지 않는다. PowerShell 정책으로 스크립트가 차단되면 오류를 기록하고 조직의 허용 절차를 따른다. `app/DeskTown.exe`를 직접 실행하면 일반 개인 저장을 사용하므로 분리 QA에는 위 런처를 사용한다.

1. GitHub Actions 아티팩트를 빈 폴더에 압축 해제한다. 해당 폴더의 PowerShell에서 `./preflight.ps1 -RunSmoke`를 실행한다. ZIP과 압축 해제된 모든 파일을 확인한 다음 별도 헤드리스 프로세스 세 개에서 폐기 가능한 저장 데이터의 생성·복구·재읽기를 검사한다. 통과하면 실행 파일은 `./app/DeskTown.exe`에 있다.
2. `./collect-evidence.ps1 -Scaling '100%'`를 실행하고 실제 화면 검사 중 생성된 `qa-results.md`를 채운다. 배율에는 Windows의 실제 표시 값을 넣는다. 모니터 해상도·원점과 Windows 버전은 기록하지만 앱·창 제목이나 문서 내용은 수집하지 않는다.
3. 분리된 데모를 실행하기 전에 DeskTown을 종료한다.

   ```powershell
   ./launch-demo.ps1 -Scenario workshop-reveal-pending -WindowsExport ./app -DemoSaves ./demo-saves
   ```

   다른 시나리오는 `workshop-repairing`과 `railway-teaser`다. 실행마다 샘플 데이터를 새 임시 폴더로 복사하고 분리된 데모임을 창 제목에 표시한다. 다음 데모를 실행하기 전에 현재 데모를 정상 종료한다.

   런처는 데모 경로를 자식 프로세스에만 전달하고 호출한 PowerShell 환경과 원본 샘플을 유지한다. 종료 코드와 오류 로그를 검사하며, 출력한 임시 폴더에 로그와 분리 저장을 보관한다. 창 조작 없이 기동을 검사하려면 `-Headless`를 추가한다. 이 경우 120프레임 뒤 종료하며 기본 45초 제한 시간을 넘으면 실패다. 헤드리스 통과는 실제 창·입력 검사 결과가 아니다.
4. Focus·재시작·복구는 위의 이름 있는 분리 QA 세션으로 검사한다. 일반 저장 위치를 검사할 때만 런처를 종료한 뒤 `./app/DeskTown.exe`를 직접 실행한다. 일반 저장의 파괴적인 복구 검사 전에는 별도 Windows 계정을 쓰거나 기존 DeskTown 저장 데이터를 백업한다. 결과를 `qa-results.md`에 기록하고 `WINDOWS_GATE.md`의 절차를 따른다.

Focus 중 PiP는 작은 Companion 창만, Minimized는 Town 작업 표시줄 항목만, Hidden은 트레이만 남는다. 세 모드를 전환해도 세션은 유지돼야 한다. Ghost는 Town에서 F10으로 켜는 QA 미리보기이며 일반 Focus에서는 선택할 수 없다. 완료 알림은 현재 기본적으로 켜져 있다. 트레이에서 끄면 알림 창 없이 완료 상태를 표시하며 `Open DeskTown` 명령은 계속 사용할 수 있다.

Focus 완료 시 PiP는 숨겨지고 트레이에 완료 상태를 표시한다. Minimized의 Town은 최소화 유지, Hidden의 Town은 숨김 유지이며 Town을 자동으로 열지 않는다. 다음 세션의 표시 모드 선택은 유지한다. 완료 알림 창은 `Completion notice` 설정에 따른다.

트레이의 `Reduced motion`은 연기·풀·반짝임·김을 끄고 모닥불을 첫 프레임에 고정하며 저장된다. 기본값은 기존 저장 설정에 따른다. 정상 종료·같은 QA 세션 재시작 후 체크와 효과 감소가 유지되는지 확인한다. 시각 미리보기 키트에는 Town/Reward/Discovery용 시작 버튼과 `art-preview/review.html`도 제공한다.
