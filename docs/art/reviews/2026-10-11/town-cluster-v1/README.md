# House·길·식생 32px 스타일 테스트 — 2026-10-11

## 현재 판정

사용자가 Workshop 파손·완성 스타일을 승인하고 House·길·식생 테스트 진행을 선택했다. 아트 지침의 네 번째 게이트인 **32px 타일 클러스터**를 준비했다. [검토 페이지](review.html)에서 1×/2×/3×, 컬러/흑백, 32px 격자, 개별 셀 및 3×3 반복을 비교한다. 마을 환경 스타일의 사용자 승인은 대기 중이다.

최종 검토 대상은 `prepared-v3/`다. 잔디 정리 전 결과와 문 앞 길 정렬 전 결과는 무시되는 `build/town-cluster-intermediate-20261011/`에 보존했다. 게임 장면·저장 데이터·런타임 카탈로그는 변경하지 않았다.

## 생성 출처

내장 `image_gen.imagegen`으로 종류마다 별도 생성했다. 승인된 [Workshop 3×](../workshop-v1/prepared-v2/workshop-complete-3x.png)를 빛·재료·팔레트 계열의 참고로 사용했다. House·흙길·덤불은 실제 투명 배경을 요청했고 잔디는 불투명한 바닥 타일을 요청했다. 외부 참고 이미지나 에셋 팩을 사용하지 않았다.

| 후보 | AI 원본 | 실제 원본 크기 | 요청 기록 |
| --- | --- | --- | --- |
| House | [house.png](sources/house.png) | 1402×1122 RGBA | [프롬프트](sources/house-prompt.txt) |
| 잔디 | [grass.png](sources/grass.png) | 1254×1254 RGBA, 알파 255 | [프롬프트](sources/grass-prompt.txt) |
| 흙길 | [path.png](sources/path.png) | 1254×1254 RGBA | [프롬프트](sources/path-prompt.txt) |
| 덤불 | [bush.png](sources/bush.png) | 1402×1122 RGBA | [프롬프트](sources/bush-prompt.txt) |

원본은 `.codex/generated_images/01a0e389-e95e-7dc0-8907-5b4fb12a6f3c/`에서 복사했다. 파일명은 House `exec-547d6282-8678-4729-9417-c833d5678d60.png`, 잔디 `exec-04a5203a-64f7-47d0-96ca-ee6f9ce48003.png`, 길 `exec-a1b45e13-b213-41cd-878a-80443d00f798.png`, 덤불 `exec-462089b6-aced-4aa4-92a9-c737e675852a.png`다. 원본을 수정하지 않았으며 SHA-256은 [변환 기록](prepared-v3/preparation.json)에 있다.

이는 AI 출처 기록이다. 최종 제작 에셋의 라이선스·납품 승인을 대신하지 않는다. 이미지 도구가 실제 32px PNG를 생성했다고 표기하지 않는다.

## 픽셀 정리

사용자가 허용한 Python 변환·픽셀 규격 정리 방식으로 [prepare_town_cluster.py](../../../../../scripts/assets/prepare_town_cluster.py)를 실행했다.

- **House:** 알파 128 이상 영역을 최근접으로 112×81에 맞춰 128×128 셀의 아래쪽 중앙에 놓았다. 셀 기준 피벗 `(64,128)`, 불투명 영역 `(8,47)-(120,128)`이다. 논리 셀은 32px 격자 4×4 크기이며 실제 충돌 영역 정의는 아니다.
- **덤불:** 같은 방식으로 28×16을 32×32 셀에 놓았다. 피벗 `(16,32)`, 불투명 영역 `(2,16)-(30,32)`이다.
- **잔디:** 중앙 정사각형을 32×32로 최근접 변환했다. 최초 결과의 미세한 무늬가 시선을 끌어 16×16으로 다시 뽑고 2×2 픽셀 덩어리로 확대했다. 평균색 `(127,130,76)`에 대한 명암 차이를 35%로 줄인 뒤 공통 팔레트로 양자화했다. 흐림·알파 가장자리 보간은 사용하지 않았다.
- **길:** 원본의 불투명 영역을 32×16 띠로 최근접 변환해 32×32 셀의 `(0,8)`에 놓았다. 위·아래는 투명하며 수평 연결을 비교한다.
- **공통 팔레트:** 새 네 종류의 불투명 픽셀을 함께 32색으로 양자화했다. 알파는 0/255, 투명 RGB는 0, 저장은 RGBA8/sRGB다. 승인된 Mina·Workshop은 다시 양자화하지 않았다. 전체 프로젝트 24–32색 팔레트 확정은 후속 작업이다.
- **반복 경계:** 잔디의 마지막 열/행을 첫 열/행과, 길의 마지막 열을 첫 열과 맞췄다. 경계 색 일치와 3×3 반복 이미지를 확인했다. 색이 같다는 자동 검사만으로 자연스러운 반복의 시각 승인을 추정하지 않는다.

## 출력과 검증

| 종류 | 후보 크기 | 카탈로그와의 관계 |
| --- | --- | --- |
| House | [128×128](prepared-v3/house-1x.png) | BLD-HOU-001의 셀 크기, 미배정 |
| 잔디 | [32×32](prepared-v3/grass-1x.png) | ENV-GRA-001의 대표 셀 1개, 2변형 시트 아님 |
| 길 | [32×32](prepared-v3/path-1x.png) | ENV-PTH-001의 대표 셀 1개, 3셀 시트 아님 |
| 덤불 | [32×32](prepared-v3/bush-1x.png) | ENV-BUS-001의 대표 셀 1개, 2변형 시트 아님 |

각 후보에 최근접 2×/3× PNG를 포함했다. [잔디 3×3](prepared-v3/grass-repeat-3x3.png), [길 3×3](prepared-v3/path-repeat-3x3.png), [클러스터 1×](prepared-v3/cluster-1x.png)·[2×](prepared-v3/cluster-2x.png)·[3×](prepared-v3/cluster-3x.png), [컬러·흑백 비교](prepared-v3/comparison-board.png)도 있다.

크기·알파·잔디 불투명·공통 팔레트·투명 RGB·최근접 확대의 픽셀 복제·sRGB·소스 해시 보존·반복 경계 일치를 검사했다. 별도 폴더 재생성 시 **20개 파일 전체의 바이트 일치**를 확인했다. Chrome에서는 1×·흑백·격자 전환 및 Canvas 크기를 확인했고 PNG 로드와 콘솔 오류는 없었다. 사용자 스타일 승인이나 실제 Windows QA 결과는 아니다.

## 구성과 남은 항목

검토 클러스터는 384×288이다. 잔디는 32px 격자, 길은 y=192에서 수평 반복한다. 같은 길 타일을 90도 돌려 x=96/256에서 건물 앞으로 이어지는 구성만 예시했다. House 원점 `(48,32)`, Workshop `(192,32)`, Mina `(168,174)`는 **아트 비교용 배치**다. 현재 Town 좌표를 변경한 것이 아니며 자동 연결 타일, 코너·교차로·통행 영역을 정의하지 않는다.

잔디·길·덤불의 전체 변형, 나무·꽃·울타리·바위·흙 타일, 프로젝트 공통 팔레트, 실제 장면 배치와 음향은 아직 필요하다. 이번에는 식생을 잔디와 낮은 덤불로 대표했다. 다음 아트 지침 게이트는 **Reward 완성 연출의 색·효과 프레임**이다. 환경 스타일 승인만으로 이 후보들을 제작용 카탈로그에 연결하지 않는다.

## 재현

Pillow 12.3.0을 사용하는 저장소의 무시되는 가상환경으로 실행했다. 출력은 새 폴더여야 한다.

```powershell
.\build\tools\art-python\Scripts\python.exe scripts/assets/prepare_town_cluster.py `
  docs/art/reviews/2026-10-11/town-cluster-v1/sources `
  docs/art/reviews/2026-10-11/workshop-v1/prepared-v2/workshop-complete-1x.png `
  docs/art/reviews/2026-10-11/neutral-48-v1/mina-neutral-48-v1-1x.png `
  build/town-cluster-check
```

HTML은 파일로 직접 열 수 있다. 로컬 HTTP 검토는 저장소 루트에서 `python -m http.server 8765 --bind 127.0.0.1`을 실행한 뒤 `http://127.0.0.1:8765/docs/art/reviews/2026-10-11/town-cluster-v1/review.html`로 연다.
