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

키트와 게임 파일 동일성·Prepare-QA 실제 실행의 결과는 패키징 후 이 절에 기록한다. 실제 입력/DPI·잠금/절전·물리 장치 청취·깨끗한 PC 검사는 이 도구의 합격 범위가 아니다.
