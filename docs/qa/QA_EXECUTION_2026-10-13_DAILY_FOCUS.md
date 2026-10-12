# 일상 Focus 반복 플레이 MVP — 2026-10-13

사용자가 Workshop 완료 후 일상 Focus 확장을 승인했다. 기존 시각 38개·음향 8개와 V1 저장을 유지한다. Railway는 다음 빌드 예고다.

## 변경

- Workshop 완성·지도 확인 후 Town의 **Focus with Mina**로 반복 세션을 시작한다. Town은 `Workshop restored`와 Railway 예고를 표시하며 실제로 누적되지 않는 Railway `0 / 45`를 일상 세션 진행처럼 표시하지 않는다.
- 설정 제목·설명·진행·동등한 표시 모드 문구를 완료 전후로 구분했다. 25/45/60분·앱 선택·PiP 크기·세 표시 모드는 유지했다.
- 대기 중 보상/지도, 실제 표시 중 연출/복구, 미해결 체크포인트, 진행 중 세션에서 setup/start를 차단한다. 중복 버튼 요청은 같은 세션을 교체하지 않는다.
- 추가 Focus는 일일/총 시간을 늘린다. Workshop은 25/25로 남고 완성·지도 이벤트를 다시 만들지 않는다. 새 저장 필드·마이그레이션·새 아트는 없다.
- 새 **Start-Daily-Focus.cmd**는 완료된 별도 Town 데모의 `qa-sessions/daily`를 유지한다. 개인 저장·기존 play 세션과 분리된다.

## 검사

.NET 269개 통과: Domain 96·Foundation 30·Windows Platform 37과 Application 106. 신규 6개는 세 모드 × Resume/EndAtCheckpoint의 완료 후 재시작·복구·앱 종료 시간 제외·Workshop 유지·일일/총 기록·소비한 보상 유지다. 새 검사 작성 중 enum 값을 `nameof(mode)`로 비교한 오류를 수정하고 Application 전체를 다시 통과시켰다. 실제 앱 빌드는 경고·오류 0개다.

실제 Windows/Godot 창 26개·음향 29개·일상 Focus 21개 검사가 통과했다. 네이티브 검사 도구의 후반부는 별도 저장과 가짜 시계로 시간을 진행하므로 실제 25분 타이핑이나 OS 잠금/입력/DPI 판정을 대신하지 않는다. 실제 컨트롤의 signal·controller·뷰·표시 모드는 실행하며 개인 진행은 사용하지 않는다.

첫 분리 프로젝트 import가 종료 코드 `3221225477`로 실패했다. 실패 기록을 보존하고 그 단계부터 한 번 재시도해 import·export·native 실행을 통과했다. Root 앱의 내보내기는 별도로 정상 종료했다.

첫 캡처에서 NPC의 발 정렬 Z index가 설정 패널보다 높아 버튼 위에 그려지는 문제를 발견했다. 설정·온보딩·복구·보상·발견 패널을 앞에 배치하고 같은 네이티브 검사를 재실행했다. [최종 실제 렌더 3장](../art/reviews/2026-10-13/daily-focus-v1/review.html)에서 설정 문구·버튼과 25→50분 기록을 확인했다. 최신 검사 결과는 `build/window-probe-daily-2026-10-13/result-v2.json`·`probe-v2.log`이고 최종 실행 코드 기준은 `b6f62ae`다. 캡처는 편집하지 않았다.

## 실행

`build/DeskTown-MVP-Daily-2026-10-13.zip`을 빈 폴더에 풀고 **Start-Play.cmd**로 시작한다. 이미 복구된 마을부터 보려면 **Start-Daily-Focus.cmd**, 완성·지도부터 보려면 **Start-Quick-Look.cmd**를 사용한다. 새로운 일상 데모와 play 진행을 자동으로 합치지 않는다. 이전 play를 이어가려면 앱을 종료하고 새 키트의 같은 위치가 비어 있을 때만 이전 `qa-sessions/play`를 복사한다.

트레이 Audio·Reduced motion·표시 모드와 저장은 그대로 사용한다. 보상·발견 화면을 확인한 뒤 반복 Focus 버튼을 사용한다.
