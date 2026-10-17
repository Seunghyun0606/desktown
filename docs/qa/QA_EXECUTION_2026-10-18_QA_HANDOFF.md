# 일상 Focus MVP — Windows QA 전달 갱신 (2026-10-18)

완료된 MVP 다음 단계로 실제 Windows 플레이 검증을 준비했다. 게임 동작·저장 스키마·아트·음향은 변경하지 않는다. 기존 게임 검증은 [2026-10-13 기록](QA_EXECUTION_2026-10-13_DAILY_FOCUS.md)을 따른다.

## 변경

- **Prepare-QA.cmd / prepare-qa.ps1**: ZIP·전체 파일·별도 저장/복구 preflight 성공 후 `qa-reports`에 새 회차를 만든다. 앱 실행 중이면 기존 preflight 규칙에 따라 종료 안내 후 실패한다.
- 결과 양식은 현재 기능에 맞는 26항목이다. 일상 Focus·저장 격리·보상 미중복·음향·효과 감소·PiP·입력·잠금·복구·깨끗한 Windows·일반 저장·업데이트를 검사 ID로 기록한다.
- 모든 수동 항목은 Not tested다. 환경 종류는 검사자의 선언이며 템플릿 생성 시각과 실제 검사 시각을 구분한다. 기존 파일은 원자적 CreateNew로 보호하고 UTF-8로 쓴다.
- 선택적 `qa-handoff.json`으로 원래 패키지·실행 코드·QA 도구 SHA를 구분한다. 구형 CI manifest도 지원하며 존재하지 않는 런타임 SHA는 추정하지 않는다.
- 최신 시작 안내·키트 README·Windows 게이트·백로그를 갱신했다. 예전 무음·아트 미배정·PiP 미수정 설명을 현재 시작 안내에서 제거했다.
- Windows CI는 양식 보존·현재 검사 목록·잘못된 빌드 식별자/혼합 provenance 거부 검사를 PowerShell 7과 5.1에서 실행하도록 연결했다.

## 로컬 검사

Windows PowerShell 5.1에서 양식 관련 14개 검사를 통과했다. 공백이 든 경로, 구형 manifest, 분리된 실행/QA SHA, 수동 Not tested 유지, 비ASCII 내용의 기존 보고서 바이트 보존, 잘못된 metadata 거부를 검사한다. PowerShell 7은 이 PC에 없어 로컬 실행하지 않았다. CI 결과는 원격 실행 후 별도 확인한다.

## 전달 검증

- 전달 ZIP: `build/DeskTown-QA-2026-10-18.zip` — **80,333,268 bytes**.
- SHA-256: `882af8fbaf4e48093859fbadbdd84438c790ccfeaef7ede18718fc77273227d9` (`build/DeskTown-QA-2026-10-18.sha256`에도 기록).
- 원본 `DeskTown-MVP-Daily-2026-10-13.zip`의 기존 SHA-256을 검증하고, 그 안의 게임 ZIP·manifest·체크섬·미리보기·데모를 재사용했다. 게임 ZIP과 manifest는 바이트 그대로이며 196개 파일의 크기·SHA-256이 기존 검증본과 일치한다. 실행 코드는 `b6f62ae8801c79702916b8f54cf4bcb541380730`, 원래 패키지 commit은 `d432c569c71c0e978287bf7dd4dd76bab3927217`, QA 도구 commit은 `2e535358a53aaa21a6b3c5acb9e67592e17d5e5b`다.
- 새 전체 ZIP의 CRC·동일한 안쪽 게임 ZIP·갱신 도구 바이트를 확인했다. `app` 압축 해제본·`qa-sessions`·`qa-reports`는 전달 ZIP에 없다. `qa-handoff.json`에는 현재 도구 파일의 해시와 위 식별자를 기록했다.
- 새 키트에서 **prepare-qa.ps1을 실제로 두 번 실행**했다. 매회 파일 검증과 write/recover/check 세 프로세스 smoke가 통과했다. 서로 다른 보고서 두 개가 생성됐고 첫 보고서의 SHA-256은 유지됐으며 수동 26항목은 모두 Not tested다. 개인 저장은 사용하지 않았다.
- 기존 미추적 162개 파일의 목록과 SHA-256이 이전 보존 기록과 모두 일치한다. 기존 MVP ZIP·게임 진행은 덮어쓰지 않았다.
- 로컬 상세 근거: `build/qa-handoff-2026-10-18-package.json`, `qa-handoff-2026-10-18-integration.json`, `qa-handoff-2026-10-18-preservation.json`.

실행 파일이 같으므로 게임의 기존 .NET 269개·native 창/음향/일상 Focus 76개 검사는 이전 기록을 인용하며 이번에 다시 실행했다고 표시하지 않는다. 실제 입력/DPI·잠금/절전·물리 장치 청취·깨끗한 PC 검사는 이 도구의 합격 범위가 아니다. 다음은 이 키트로 실제 사용자 작업을 수행하고 P0/P1 문제부터 재현·수정하는 단계다.
