# Workshop Broken/Complete 스타일 테스트 — 2026-10-11

## 현재 판정

사용자가 Mina Idle·Work 동작 방향을 승인하고 Workshop 스타일 테스트 진행을 선택했다. 아트 지침의 세 번째 게이트인 **Broken/Complete를 Town 크기에서 비교**하는 후보를 준비했다. [검토 페이지](review.html)에서 1×/2×/3×, 컬러/흑백, 밝고 어두운 배경 및 현재 Town 좌표의 상태 전환을 비교한다. 사용자가 Workshop 파손·완성 스타일을 승인하고 House·길·식생 32px 타일 테스트 진행을 선택했다. 생성 당시 preparation.json의 판정은 당시 기록으로 보존한다.

최종 검토 대상은 `prepared-v2/`다. 정렬 전 중간 결과는 무시되는 `build/workshop-intermediate-20261011/`에 보존했다. 카탈로그 배정, 게임 장면, 저장 스키마를 변경하지 않았다. Repairing이 없으므로 3프레임 납품 시트로 취급하지 않는다.

## 생성 출처와 요청

- 내장 `image_gen.imagegen`을 사용했고 투명 배경을 요청했다. 외부 에셋 팩이나 참고 이미지를 사용하지 않았다.
- Complete는 승인된 [Mina 중립 3×](../neutral-48-v1/mina-neutral-48-v1-3x.png)를 재료·팔레트 계열·윤곽의 참고로 사용했다. [프롬프트](sources/complete-prompt.txt), [AI 원본](sources/complete.png).
- Broken은 Complete를 수정 대상으로 삼아 문·벽·기초·굴뚝 위치를 유지하고 지붕 파손, 창·등불 소등, 흩어진 도구를 요청했다. [프롬프트](sources/broken-prompt.txt), [AI 원본](sources/broken.png).
- 두 AI 원본은 모두 **1402×1122 RGBA**다. 도구가 네이티브 160×128 PNG를 납품한 것으로 표기하지 않는다.
- 원본은 `.codex/generated_images/01a0e389-e95e-7dc0-8907-5b4fb12a6f3c/`의 `exec-c9e3800e-6acd-438b-96c3-8261befce81c.png`와 `exec-7d6a037b-2b77-4340-9819-012686225042.png`에서 복사했으며 변경하지 않았다.
- 원본 SHA-256은 [변환 기록](prepared-v2/preparation.json)에 있다. AI 생성 출처 기록이며 최종 제작 에셋의 라이선스·납품 승인을 대신하지 않는다.

## 픽셀 정리와 고정 기준

사용자가 허용한 Python 변환·픽셀 규격 정리 방식으로 [prepare_workshop_review.py](../../../../../scripts/assets/prepare_workshop_review.py)를 실행했다.

1. 두 원본의 알파 128 이상 영역을 합친 **동일한 crop**을 사용하고 하나의 최근접 배율로 정리했다. 상태마다 별도로 크기를 맞추지 않았다.
2. 두 상태의 불투명 픽셀을 함께 양자화해 공통 32색으로 정리했다. 알파는 0/255, 투명 RGB는 0, 출력은 RGBA8/sRGB다.
3. AI 수정 과정에서 문 주변 픽셀 변화와 떨어진 도구의 기초 아래 돌출을 발견했다. Broken의 문·아치 영역 및 하단 기초 띠를 Complete의 동일 좌표 픽셀로 고정했다. 이 처리로 원본의 떨어진 망치 하단 일부는 남지 않는다. 파손 상태는 어긋난 지붕·노출 서까래·어두운 창·기울어진 판자로 전달한다.
4. 두 프레임에 동일한 3px 세로 이동을 적용해 기초 마지막 행을 y=127에 맞췄다. 최종 고정 영역은 문 `(65,81)-(94,128)`, 기초 `(0,123)-(160,128)`의 반열린 좌표다. 두 영역은 바이트가 일치한다. 셀 경계 기준 피벗은 `(80,128)`, 불투명 영역은 두 상태 모두 `(8,31)-(152,128)`이다.

32색은 **Workshop 두 후보의 공통 팔레트**다. Mina를 포함한 프로젝트 전체 24–32색 팔레트로 통합한 것은 아니다. 따뜻한 흙색·크림·올리브 계열을 비교하며 최종 팔레트 정리는 Mina+Workshop 스타일 승인 후 필요하다.

## 출력과 확인

- [Broken 160×128](prepared-v2/workshop-broken-1x.png), [Complete 160×128](prepared-v2/workshop-complete-1x.png), 각각 최근접 2×/3× PNG.
- [컬러·흑백 비교](prepared-v2/comparison-board.png), 상태별 흑백 PNG.
- [Town Broken 좌표 참조](prepared-v2/town-broken-1x.png), [Town Complete 좌표 참조](prepared-v2/town-complete-1x.png), 각각 1280×720.
- 픽셀 검사: 정확한 셀 크기, 알파 0/255, 공통 32색, 투명 RGB 0, 최근접 확대의 픽셀 복제, sRGB, 문·기초 픽셀 일치, 서로 다른 상태, 원본 해시 보존.
- 별도 폴더에 다시 실행하여 **12개 파일 전체의 바이트 일치**를 확인했다. 자동 검사는 스타일·가독성의 사용자 승인을 뜻하지 않는다.
- Chrome에서 1× 흑백 비교, 상태 전환, Canvas 크기 및 PNG 로드를 확인했다. 페이지 오류와 콘솔 오류는 없었다.

## Town 배치와 남은 제작

`TownScene.tscn`의 Workshop 영역은 `(744,272)-(1008,452)`다. `CatalogRectArt.Bind`가 160×128 프레임을 원래 크기로 아래쪽 중앙에 맞추므로 PNG 원점은 `(796,324)`다. Mina 원점 `(680,415)`와 기본 viewport 1280×720도 그대로 참조했다. HTML 부분 화면은 원래 화면의 `(600,254)`에서 440×260을 잘라 비교한다.

이 배치는 건물이 현재 경로 위쪽에 있는 원래 좌표를 보여준다. 도구 접촉, 캐릭터 동선, 전경 가림, House와 전체 마을의 비율은 아직 승인하지 않았다. 비교 화면은 기하학적 배경을 재현한 PNG/Canvas이며 Godot 실행 캡처나 실제 Windows 입력 검증 결과가 아니다.

다음 스타일 게이트는 House·길·식생의 32px 타일 클러스터다. Workshop 제작용 3상태 시트에는 Repairing, 같은 구조 기준점, 별도 굴뚝 연기 효과가 더 필요하다. 최종 아트 카탈로그 배정과 장면 배치 승인도 남는다.

## 재현

Pillow 12.3.0을 사용하는 저장소의 무시되는 가상환경으로 실행했다. 출력은 새 폴더여야 한다.

```powershell
.\build\tools\art-python\Scripts\python.exe scripts/assets/prepare_workshop_review.py `
  docs/art/reviews/2026-10-11/workshop-v1/sources `
  docs/art/reviews/2026-10-11/neutral-48-v1/mina-neutral-48-v1-1x.png `
  build/workshop-check
```

HTML은 파일로 직접 열거나 저장소 루트에서 `python -m http.server 8765 --bind 127.0.0.1`을 실행한 뒤 `http://127.0.0.1:8765/docs/art/reviews/2026-10-11/workshop-v1/review.html`로 연다.
