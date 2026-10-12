# 마지막 시각 아트 10개 실제 배정·연결

사용자 결정 **A: 10개 아트·배치 승인 — 적용 후 최종 화면 준비**에 따라 [prepared-v2](../remaining-art-v1/prepared-v2/preparation.json) PNG를 바이트 그대로 `assets/art/`에 복사하고 production 카탈로그에 배정했다. 현재 시각 **38개 모두 배정**, 음향 8개 미배정이다.

실제 장면·뷰에서 임시 배치 변경 없이 204개 GPU 캡처를 만들었다. 개별 33셀 132장 + Town 20장 + Companion 40장 = **192개 캡처가 승인본과 바이트 단위로 같다**. UI 12장은 실제 뷰의 로고·지도·아이콘·패널을 렌더했다. 완료 Town의 실제 연기도 UI 배경에 보인다. 입력 49,408개 셀 픽셀과 모든 204개 크기·해시를 확인했다. [검증](verification.json), [빌드](build.log), [캡처](capture.log).

Work 접촉 프레임 2에 8FPS 반짝임이 한 번 시작하고 0.5초 후 숨겨진다. 같은 Mina intent의 재바인딩은 효과를 재시작하지 않는다. Reduced motion은 접촉·김을 숨기고 모닥불을 첫 프레임에 고정한다. Rest에서 김을 다시 켜면 4FPS로 재생된다. 숨겨진 Stage/Town은 효과의 처리·시계를 멈춘다. 미완료 Workshop에는 연기가 없다. 캡처 로그의 `DESKTOWN_RUNTIME_MOTION_OK`가 이 경계 검사를 확인한다.

실제 카탈로그·아트·Godot 소스/장면을 분리 프로젝트로 복사했다. 개인 저장·AppRoot를 실행하지 않았고 검사 전후 메인 카탈로그는 같다. Companion 본문 아트의 추가 영역 밖 보존을 확인했으며 헤더 글꼴은 배율 힌팅 차이 때문에 아트 비교에서 제외했다. 네이티브 포커스·입력·DPI 검사가 아니다.

[최종 전체 화면](../final-views-v1/review.html)은 Reward/Discovery/온보딩/Ghost까지 실제 연결된 결과를 제공한다. QA-PIP-001은 TODO다.
