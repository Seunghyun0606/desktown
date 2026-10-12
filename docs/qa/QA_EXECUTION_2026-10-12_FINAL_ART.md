# 최종 시각본 Windows QA 키트 준비 — 2026-10-12

## 대상

- 실행 코드 커밋: `06ac9136969f97700a8efb3221d0edf2e337000d`.
- 시각 아트 **38개 production**, 음향 8개 미배정.
- 로컬 키트: `build/qa-kit-final-art-06ac9136`.
- 이동용 전체 ZIP: `build/DeskTown-final-art-QA-06ac9136.zip` (77,677,348 bytes).
- 전체 ZIP SHA-256: `9c4ce1a9ae5e2604ae6b5c9c43d8ec719158d8aadaa36eb501064a2af6c64df7`.
- 안쪽 앱 ZIP SHA-256: `d9571fbcaf678628f407ff8ab880fe5dc61d96bb14dc2b6a2d58ff8ced5e6a1c`.

검증 후 커밋한 코드의 git archive를 새 분리 프로젝트로 내보냈다. 개인 저장·기존 QA 세션·원래 162개 미추적 파일은 패키지에 포함하지 않았다. 앱은 미서명 Windows x64 폴더 전체를 ZIP으로 전달한다. 시스템 SDK·PATH·PowerShell 실행 정책을 변경하지 않았다.

## 자동 확인

| 검사 | 결과 |
| --- | --- |
| .NET 전체 테스트 | 254 passed, failed/skipped 0 |
| 카탈로그·Python 검사 | 46 ID 중 시각 38개 배정; 카탈로그 테스트 3개 통과 |
| 실제 카탈로그 배정·모션 | 204 GPU 캡처, 49,408셀 픽셀; 승인된 개별 아트/Town/Companion 192장 바이트 일치 |
| 모션 경계 | Work 접촉 한 번 재생·같은 intent 재바인딩·종료·숨김 중지·Reduced motion 전환·미완료 연기 숨김 통과 |
| 전체 최종 뷰 | 55 GPU 캡처; 실제 Reward 2초 홀드·정상/효과 감소 완료 콜백 검사 |
| Ghost 알파 | 40/65/85% 레이어 합성 118,800픽셀; 네이티브 입력 검사는 아님 |
| 미배정/잘못된 아트 대체 | 분리된 합성 fixture의 `DESKTOWN_CI_ART_SMOKE_OK`; 원래 카탈로그 유지 |
| Windows 내보내기 | Godot 4.7.2 .NET / SDK 8.0.425, 종료 0; 로그 오류 없음; 196개 배포 파일 |
| 실제 실행 파일 리소스 | 내보낸 EXE에 현재 카탈로그 JSON 바이트가 정확히 포함됨 |
| 앱 ZIP·압축 해제 파일 | 체크섬·196개 크기 및 SHA-256 일치 |
| 키트 preflight -RunSmoke | 내보낸 앱의 write/recover/check 세 프로세스 통과; 개인 저장 미사용 |
| 내보낸 데모 런처 | first-launch/repairing/reveal-pending/railway-teaser 헤드리스 기동 통과 |
| 이름 있는 분리 저장 | 두 번 재열기, 기존 Scenario/저장 덮어쓰기 방지 통과 |
| 이동용 키트 ZIP | 86개 파일 목록·바이트·CRC 검사 통과; app/qa-sessions 제외, 앱은 안쪽 ZIP으로 포함 |
| 결과 양식 | 빌드 SHA·모니터 경계 포함; 새 수동 검사 항목은 모두 Not tested |

렌더 근거는 [최종 뷰](../art/reviews/2026-10-12/final-views-v1/README.md)와 [실제 배정](../art/reviews/2026-10-12/remaining-delivery-v1/README.md)에 있다. 로컬 상세 출력은 `build/test-results-final-art`, `build/final-export-06ac9136/project/build/export-logs`, `build/final-qa-verification.json`에 보존했다. 키트에도 최종 캡처·메타데이터·시작 안내를 넣었다.

## 사용자 전달

[시작 안내](USER_QA_START.md)를 따른다. `art-preview/review.html`은 실행 없이 최종 화면을 보여준다. `Start-Town-Art.cmd`, `Start-Workshop-Reveal.cmd`, `Start-Repairing-Art.cmd`는 새 분리 데모로 시각 상태를 연다. `Start-QA.cmd`는 온보딩/Focus/재시작/복구용 이름 있는 저장을 재사용한다. 다른 DeskTown은 사용자가 먼저 종료한다.

새 빌드의 실제 창·타이핑·포인터·DPI·절전·깨끗한 PC 실행과 일반 저장 위치는 미검증이다. 이전 사용자 QA 보고를 새 빌드의 전체 매트릭스 Pass로 확대하지 않았다. **QA-PIP-001은 사용자 요청대로 TODO**다. 시각 적용은 완료했으나 음향 8개와 출시 검증은 남는다.
