# DeskTown Windows 11 노트북 QA 키트

이 키트에는 버전이 붙은 서명되지 않은 앱 ZIP, SHA-256 체크섬과 파일 목록, 분리된 데모 저장 데이터 세 개, PowerShell 스크립트가 들어 있다. 편집기나 SDK는 필요 없다. ZIP과 파일 목록을 함께 보관한다. 목록에 기록된 빌드 SHA를 이슈 보고의 식별자로 사용한다.

1. GitHub Actions 아티팩트를 빈 폴더에 압축 해제한다. 해당 폴더의 PowerShell에서 `./preflight.ps1 -RunSmoke`를 실행한다. ZIP과 압축 해제된 모든 파일을 확인한 다음 별도 헤드리스 프로세스 세 개에서 폐기 가능한 저장 데이터의 생성·복구·재읽기를 검사한다. 통과하면 실행 파일은 `./app/DeskTown.exe`에 있다.
2. `./collect-evidence.ps1 -Scaling '100%'`를 실행하고 실제 화면 검사 중 생성된 `qa-results.md`를 채운다. 배율에는 Windows의 실제 표시 값을 넣는다. 모니터 해상도·원점과 Windows 버전은 기록하지만 앱·창 제목이나 문서 내용은 수집하지 않는다.
3. 분리된 데모를 실행하기 전에 DeskTown을 종료한다.

   ```powershell
   ./launch-demo.ps1 -Scenario workshop-reveal-pending -WindowsExport ./app -DemoSaves ./demo-saves
   ```

   다른 시나리오는 `workshop-repairing`과 `railway-teaser`다. 실행마다 샘플 데이터를 새 임시 폴더로 복사하고 분리된 데모임을 창 제목에 표시한다. 다음 데모를 실행하기 전에 현재 데모를 정상 종료한다.
4. 실제 Focus를 검사할 때는 데모를 종료한 뒤 `./app/DeskTown.exe`를 실행한다. 파괴적인 복구 검사 전에는 별도 Windows 계정을 쓰거나 기존 DeskTown 저장 데이터를 백업한다. 결과를 `qa-results.md`에 기록하고 `WINDOWS_GATE.md`의 절차를 따른다. 헤드리스 사전 검사는 일반 `user://` 저장 데이터를 건드리지 않지만 실제 앱은 해당 저장 데이터를 사용한다.

Ghost는 Town에서 F10으로 켜는 QA 미리보기이며 일반 Focus에서는 선택할 수 없다. 완료 알림은 현재 기본적으로 켜져 있다. 트레이에서 끄면 알림 창 없이 완료 상태를 표시하며 `Open DeskTown` 명령은 계속 사용할 수 있다.
