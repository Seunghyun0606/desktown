# Companion 분리 Godot 연결 검증 — 2026-10-11

사용자가 **A: 외형·배치안 승인 — 분리된 Godot 연결 검증 진행**을 선택했다. 승인된 바닥·의자·그림자와 가까운 작업대·고정 의자를 실제 Godot 장면에 연결했다. [검토 페이지](review.html)는 PNG 합성이 아닌 **Godot GPU 렌더 캡처**를 제공한다. 실제 렌더의 배치·가림 승인과 런타임 연결 범위 결정은 대기 중이다.

## 분리 범위

- 소스 `84ef643`의 `git archive`를 새 `build/companion-godot-review-v3/project`에 풀었다. 승인 후보 13개 ID를 복사·해시 기록했다. 기존 CompanionStage, CatalogRectArt, VisualAssetCatalog, MinaPlaceholderView와 MinaClipPlayback을 그대로 사용했다.
- [준비 도구](../../../../../scripts/assets/prepare_godot_companion_review.py)는 원본 저장소 카탈로그를 쓰지 않는다. 기존 P0 검증기가 `placeholder`를 금지해 **분리된 카탈로그에서만** 상태명을 `production`으로 지정했다. 출처는 생성·Python 정리 경로와 최종 납품 라이선스 승인 대기를 명시했다. 이는 실제 배포 등급 승인이 아니다.
- [C# 검사 템플릿](../../../../../scripts/assets/CompanionArtReview.cs.txt)은 임시 프로젝트에만 복사되어 컴파일된다. 검사 전용 루트에서 시작해 AppRoot, 세션, 트레이, 개인 저장 코드에 진입하지 않는다. 검사 프로젝트의 이름은 `DeskTownArtReview-companion-godot-review-v3`이며 엔진의 사용자 디렉터리도 DeskTown과 분리됐음을 실제 실행에서 확인했다. 실제 게임 소스·장면·카탈로그의 변경은 없다.
- 그림자는 검사 인스턴스에만 추가했다. 도구 2셀은 카탈로그 로드·범위 검사만 하며 Work에 들어 있는 망치 위에 다시 그리지 않는다. 식물은 미배정 기하학적 대체 표시다.

## 실제 연결한 배치

| 항목 | PNG 원점 | 분리 인스턴스 설정 |
| --- | --- | --- |
| Mina | (100,103) | 기존 그대로, 6클립 30프레임 입력 픽셀 유지 |
| Workbench | (135,91) | 호스트 (130,130), 106×25; 기존보다 16px 왼쪽 |
| Stool | (108,116) | 호스트 (108,116), 32×32; 전 클립 고정, Mina 뒤 |
| Shadow | (108,141) | Centered=false, 32×16; Floor 다음, Workbench 전에 그림 |
| Floor | (0,136) | 기존 노드 유지, 아트 첫 불투명 행 y=148 |
| Window | (119,10) | 기존 그대로 |
| Lamp | (282,26) | 호스트 너비 15→16, 오른쪽 305→306; 반 픽셀 (281.5,26)을 정수로 정렬 |

램프 1px 호스트 확장은 **분리 검증용 정렬**이다. 원본 장면에는 적용하지 않았다. OFF/ON은 같은 위치에서 로드한다. ON을 언제 표시할지 정하는 런타임 정책은 만들지 않았다. 카탈로그 피벗 메타데이터 대신 기존 바인더가 셀의 아래 중앙을 사용하는 사실을 확인했으며, 이를 아키텍처 변경으로 바꾸지 않았다.

## 검사 결과

- .NET SDK **8.0.425**, Godot **4.7.2.stable.mono.official.ed1daf0bf**, Windows Compatibility OpenGL 렌더러(NVIDIA RTX 4070 SUPER)를 사용했다. [빌드 로그](build.log) 오류/경고 0, [카탈로그 검사](catalog.log) 46개 ID 중 임시 배정 13개, [캡처 로그](capture.log) 오류 없이 `DESKTOWN_COMPANION_REVIEW_OK` 종료 0.
- 실제 6클립/30프레임의 모든 카탈로그 영역과 잘못된 프레임의 대체 표시 반환을 검사했다. 75/100/125/150% SubViewport CanvasItems 확대에서 120장, 램프 ON 각 배율 4장을 저장했다. 각 클립의 모든 프레임 캡처가 서로 다르다.
- 기존 재생 코드로 프레임을 진행했다. 자동 `_Process`는 검사 중 끄고 FPS의 프레임 간격을 명시적으로 전달해 캡처 프레임을 고정했다. 실제 렌더 완료 신호 후 ViewportTexture.GetImage로 저장했다. 헤드리스 더미 렌더 결과를 사용하지 않았다.
- [픽셀 검사](verification.json): 124개 PNG 크기·비어 있지 않은 렌더·SHA-256, 승인 입력 13개 해시를 확인했다. 원래 배율에서 **Mina 불투명 픽셀 18,055개**가 승인 시트와 정확히 일치한다. Work 6/Rest 4의 `(94,106)-(179,159)` 접촉 영역은 승인 배치 참조와 바이트 단위로 일치한다. 그림자·의자가 Mina 앞을 가리지 않는다.
- 초기 시도에서 C# Nullable 빌드 오류, 이어서 Task를 Variant로 변환하는 Callable 오류를 발견해 검사 템플릿을 수정했다. v2에서 오류 없이 완료한 뒤 v3에서 프로젝트 이름을 DeskTownArtReview 전용으로 분리해 엔진의 사용자 로그 경로까지 검사했다. 최종 v3는 새 프로젝트에서 빌드·가져오기·렌더를 완료했으며 124개 PNG 해시가 v2와 동일하다. 실패 로그/산출물은 `build/companion-godot-review-v1`에 보존했다.
- Work 04, Rest 00/03, Walk 03, Stretch 03, Celebrate 05의 실제 150% PNG를 직접 열어 배치·그리기 순서를 확인했다. 브라우저 검토 도구는 app-server 시작 경로 오류(`os error 3`)로 실행되지 않았다. HTML의 재생·선택 UI를 브라우저에서 통과했다고 기록하지 않는다.
- PNG 배치 참조는 램프 ON·갈색 식물 목업, 실제 장면은 기본 OFF·기존 녹색 식물 대체 표시다. 두 영역을 제외한 Work/Rest 전체 아트 영역 `(0,35)-(360,170)`도 승인 참조와 일치한다. 램프 OFF/ON의 불투명 픽셀은 각 승인 셀과 따로 일치했다. HTML의 링크·JavaScript 구문과 루프백 페이지 제공을 검사했고 준비 도구의 기존 폴더/저장소 밖 출력 거부를 확인했다.

이 결과는 자연스러운 자세, 정확한 `tool_contact` 이벤트, 최종 아트 납품, Windows 창 포커스·입력·DPI 검증을 대신하지 않는다. 75%에서는 작은 픽셀 특징이 축소되고 125/150%는 비정수 배율이므로 최근접 픽셀 폭이 균일하지 않을 수 있다. 네이티브 CompanionWindow를 띄워 상호작용한 QA는 아니다.

## 재현

현재 저장소 루트에서 새 출력 폴더를 선택한다. 기존 결과는 덮어쓰지 않는다. Windows/Python 3, Pillow 12.3.0, 고정 Godot .NET/SDK가 필요하다.

```powershell
python scripts/assets/prepare_godot_companion_review.py build/companion-godot-check-new
python scripts/assets/run_godot_companion_review.py build/companion-godot-check-new `
  --godot build/tools/godot-4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64.exe `
  --dotnet-root "$env:LOCALAPPDATA/DeskTownBuildTools/dotnet-8.0.425"
.\build\tools\art-python\Scripts\python.exe scripts/assets/audit_godot_companion_review.py `
  build/companion-godot-check-new
```

도구가 창 밖 좌표의 작은 렌더 전용 프로세스를 시작해 완료 후 종료한다. 사용자 화면 캡처나 개인 저장 접근은 없다. 실행별 로그와 성공/실패 판정을 보존한다. 재현 캡처는 GPU/폰트 환경에 따라 달라질 수 있으므로 전체 PNG의 다른 PC 간 해시 일치를 요구하지 않는다. 원래 배율의 승인 스프라이트 픽셀과 접촉 영역을 검사한다.

다음 결정은 실제 렌더 배치·가림을 승인하고 Companion 실제 연결로 진행할지, 수정할지다. 별도 도구 분리, 점등 정책, 최종 출처·납품 승인, Town/효과/음향 연결과 Windows QA는 남는다.
