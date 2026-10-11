# Mina 외형 시안 v1 — 2026-10-11

## 검토 범위

사용자가 승인한 AI 시안 제작 경로로 생성한 첫 외형 제안이다. 기준 커밋은 `96a291b`다. [검토 페이지](review.html)에서 원본과 밝은/회색/어두운 배경의 48·96·144 CSS px 표시를 비교한다.

- 둥근 주황색 작업 모자, 짧은 갈색 머리, 크림색 셔츠, 올리브색 멜빵, 작은 부츠.
- 오른쪽을 향한 차분한 중립 자세와 낮게 든 작은 망치.
- 얼굴의 세부 묘사보다 모자·머리·몸통 윤곽의 식별성을 먼저 판단한다.
- 인간 빌더 외형과 구체적인 색 배치는 이번 시안의 제안이며 확정된 제품 설정이 아니다.

## 생성 기록

- 방식: 내장 `image_gen.imagegen`, 새 이미지, `transparent_background=true`.
- 프롬프트: [prompt-v1.txt](prompt-v1.txt).
- 원본 복사본: [mina-neutral-v1.png](mina-neutral-v1.png).
- PNG IHDR: 1254×1254, 8-bit RGBA. 원본 파일은 수정하지 않았다.
- 알파 표본: 네 모서리는 0, 중앙 (627,627)은 253. 투명 배경이 있으며 캐릭터 내부 표본도 완전 불투명하지 않아 핵심 실루엣의 알파 계약을 통과한 파일로 볼 수 없다.
- 외부 참고 이미지나 외부 에셋 팩을 사용하지 않았다.
- 출처: 2026-10-11 이 대화의 AI 생성 결과. 아티스트 제작물이나 별도 라이선스가 검증된 납품으로 표기하지 않는다.

## 규격과 검토 한계

이 파일은 **외형 검토용 확대 시안**이다. 48×48 납품 파일이 아니며 브라우저 축소 표시는 원본에서 추출한 정확한 48×48 픽셀 격자를 뜻하지 않는다. 생성 결과에는 색 변화와 불균일한 픽셀 블록이 보인다. 최종 팔레트 수, 알파 가장자리, (24,44) 피벗, 1픽셀 발 접점 계약은 통과 판정을 내리지 않았다. 망치 머리는 회색으로 보여 목재 도구 표현도 후속 검토가 필요하다.

카탈로그·게임 장면에는 연결하지 않았다. 46개 에셋은 미배정 상태이며 실제 애니메이션·Workshop·Noah/Rumi 시트는 만들지 않았다.

외형 방향이 승인되면 정확한 48×48 중립 프레임과 최근접 1×/2×/3× 검토를 먼저 준비한다. 크기·실루엣 게이트를 통과한 뒤 Idle/Work와 Workshop 스타일 테스트로 진행한다. 외형 승인만으로 아트 지침의 전체 첫 게이트나 출시 아트 승인이 완료되는 것은 아니다.

기준: [Art Bible](../../ART_BIBLE.md), [Sprite Contract](../../SPRITE_CONTRACT.md).

## 외형 승인 후 중립 프레임 준비

사용자는 v1 외형 방향을 승인하고 48×48 중립 프레임 준비를 요청했다. 내장 이미지 수정 도구에 v1을 참조하여 단일 48×48 RGBA, 제한 팔레트, 알파 0/255, (24,44) 발 접점을 요청했다.

- 수정 결과: [mina-neutral-edit-v2.png](mina-neutral-edit-v2.png).
- 수정 프롬프트: [prompt-edit-v2.txt](prompt-edit-v2.txt).
- 생성 방식: 내장 `image_gen.imagegen`, v1 파일 참조, 투명 배경 요청.
- 실제 출력: 1254×1254, 8-bit RGBA. **48×48 규격 실패**이며 중립 프레임 완성으로 기록하지 않는다.
- v1 및 수정 결과는 원본 그대로 보존했다. 확대 시안의 승인된 외형 방향은 유지하되, 도구 결과만으로 정확한 픽셀 격자나 팔레트·피벗 승인을 추정하지 않는다.

당시 Python 변환은 실행하지 않고 imagegen 스킬이 요구하는 명시적 허용을 요청했다. 이후 사용자가 Python 변환과 픽셀 규격 정리를 승인했다. 아래는 해당 승인 후 수행한 결과다.

## 실제 48×48 후보 — 시각 승인 대기

- [실제 프레임 검토 페이지](neutral-48-v1/review.html), [배경별 검토 보드](neutral-48-v1/review-board.png).
- [48×48 PNG](neutral-48-v1/mina-neutral-48-v1-1x.png), [2× PNG](neutral-48-v1/mina-neutral-48-v1-2x.png), [3× PNG](neutral-48-v1/mina-neutral-48-v1-3x.png).
- [변환·검사 기록](neutral-48-v1/preparation.json).
- 변환 입력은 **사용자가 외형을 승인한 v1**이며 수정 도구의 v2는 입력으로 사용하지 않았다.
- 원본 알파 128 이상 영역을 잘라 최근접으로 높이 40px, 폭 23px에 맞추고, 최대 32색으로 디더링 없이 제한했다. 알파는 0/255, 투명 픽셀 RGB는 0으로 정리했다.
- 48×48 셀의 기준점은 (24,44). 실제 불투명 영역은 x=13..35, y=5..44이며 전체 프레임 밖이나 하단 여백으로 도구·발이 잘리지 않는다.
- 저장 형식은 RGBA8 PNG이며 sRGB 청크를 기록했다. 팔레트는 이 중립 후보의 32색이며 전체 프로젝트의 최종 팔레트 확정은 아니다.
- 저장 후 크기·RGBA 모드·sRGB·알파·색 수·투명 RGB·발 접점·최근접 확대의 모든 픽셀·원본 보존을 검사했다. 검사 결과는 위 JSON에 기록했다.
- 검토 보드와 3× PNG를 눈으로 확인했다. 모자·멜빵·오른쪽 자세는 유지되지만 **1×의 망치 세부 가독성은 약하다**. 픽셀 규격 검사는 외형의 사용자 승인이나 도구 가독성 판정을 대신하지 않는다.

### 재현

스크립트는 [prepare_mina_neutral.py](../../../../scripts/assets/prepare_mina_neutral.py)다. 저장소의 무시되는 `build/tools/art-python` 가상환경에 Pillow 12.3.0을 설치해 사용했다. 기존 출력 폴더가 있으면 덮어쓰지 않고 실패한다. 다른 **새 출력 폴더**를 지정해 재현한다.

```powershell
python -m venv build/tools/art-python
.\build\tools\art-python\Scripts\python.exe -m pip install Pillow==12.3.0
.\build\tools\art-python\Scripts\python.exe scripts/assets/prepare_mina_neutral.py `
  docs/art/reviews/2026-10-11/mina-neutral-v1.png build/mina-neutral-check
```

게임 장면·저장 스키마·카탈로그는 변경하지 않았다. 이 후보의 생성 시점에는 중립 프레임 시각 승인을 기다렸다. 이후 사용자가 중립 프레임을 승인하고 Idle·Work 테스트 진행을 선택했다. 생성 당시의 `preparation.json`은 당시 판정을 기록한 원본으로 보존했다.

후속 결과: [Idle·Work 스타일 테스트](idle-work-v1/review.html), [생성·변환 기록](idle-work-v1/README.md). 사용자가 Idle·Work 동작 방향을 승인했고, [Workshop Broken/Complete 스타일 테스트](workshop-v1/review.html)의 스타일을 사용자가 승인했다. 다음 [House·길·식생 테스트](town-cluster-v1/review.html)를 준비했으며 환경 스타일 승인을 기다린다.
