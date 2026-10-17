# 최신 MVP 플레이·사용자 QA 시작 — 2026-10-18

**현재 키트: `build/DeskTown-QA-2026-10-18.zip`**

검증된 일상 Focus MVP의 게임 파일을 그대로 사용한다. 시각 38개·음향 8개·개선된 캐릭터 행동·Workshop 완료 후 반복 Focus가 적용됐다. 이번 갱신은 QA 준비 버튼·현재 기능에 맞는 결과 양식·안내다. 검증 근거는 [일상 Focus MVP 기록](QA_EXECUTION_2026-10-13_DAILY_FOCUS.md)과 [이번 QA 전달 기록](QA_EXECUTION_2026-10-18_QA_HANDOFF.md)에서 구분한다.

## 바로 플레이

ZIP을 새 빈 폴더에 **전부** 풀고 다른 DeskTown을 종료한다.

| 버튼 | 시작 위치 / 진행 |
| --- | --- |
| **Start-Play.cmd** | 처음부터, `qa-sessions/play`에 저장 |
| **Start-Daily-Focus.cmd** | Workshop 복구 후, `qa-sessions/daily`에 저장 |
| **Start-Quick-Look.cmd** | 새 분리 데모로 완성 연출 → 지도 발견 |

같은 버튼으로 다시 열면 play/daily 진행을 이어간다. Focus with Mina는 25/45/60분을 반복하며 일일·총 시간만 추가하고 Workshop·지도 보상은 중복하지 않는다. Old Railway는 다음 빌드 예고다. [플레이 안내](../../scripts/qa/PLAY-README.md)를 참고한다.

## 실제 Windows QA

1. 앱을 종료하고 **Prepare-QA.cmd**를 실행한다. ZIP·196개 게임 파일·별도 저장/복구 사전 검사 후 `qa-reports`에 새 보고서가 만들어진다. 기존 진행과 보고서를 덮어쓰지 않는다.
2. 보고서에 실제 Windows 배율·앱 종류·모니터·창 상태를 적는다. 모든 수동 항목은 **Not tested**로 시작한다. 코드와 QA 도구의 SHA는 별도로 기록된다.
3. **Start-QA.cmd**로 온보딩부터 확인하거나 **Start-Daily-Focus.cmd**로 완료 이후를 확인한다. [검사 순서](../../scripts/qa/QA-README.md)에 따라 화면·소리 → 실제 25분 작업 → 모드/완료 → 재시작·복구 순서로 진행한다.
4. 실패에는 보고서의 검사 ID·P0/P1/P2·재현 순서·기대/실제·시각을 적는다. 앱·DPI·모니터·창 상태 조합마다 새 보고서를 사용한다. 상세 통과 조건은 [Windows 게이트](WINDOWS_GATE.md)다.

PiP는 Companion만 남고 Town 창/작업 표시줄 항목은 없어야 한다. 완료하면 PiP가 숨겨지고 트레이에 완료 표시가 생기며 Town이 자동으로 열리지 않아야 한다. QA-PIP-001은 코드 수정/native 검사 후 실제 입력·DPI 재확인 단계다. Audio·Reduced motion은 트레이에서 바꾸며 재시작 후 유지돼야 한다.

개인 저장과 분리된 데모다. 이전 진행을 옮기려면 앱을 모두 종료하고 새 키트의 같은 대상이 비어 있을 때만 `qa-sessions/play` 등을 복사한다. 기존 대상 진행을 덮어쓰지 않는다. 일반 저장·업데이트·롤백은 별도 테스트 계정에서 검사한다.

실제 입력/DPI·Windows 잠금/절전·스피커/헤드폰 청취·편집기/SDK 없는 깨끗한 Windows 11 검증은 남아 있다. Ghost Focus는 계속 비활성이다. 작업 문서·창 제목·입력 내용·개인 저장은 첨부하지 않는다.

## 이전 전달 기록

- [일상 Focus MVP](QA_EXECUTION_2026-10-13_DAILY_FOCUS.md)
- [음향 적용 MVP](QA_EXECUTION_2026-10-13_AUDIO_MVP.md)
- [캐릭터 행동 개선](QA_EXECUTION_2026-10-12_BEHAVIOR.md)
- [PiP 창 수정 MVP](QA_EXECUTION_2026-10-12_MVP.md)
- [최종 시각 아트 QA](QA_EXECUTION_2026-10-12_FINAL_ART.md)
