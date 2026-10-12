# 캐릭터 행동 개선 — 2026-10-12

## 사용자 피드백과 변경

사용자는 이전 디자인 MVP를 모두 확인했고 “캐릭터들의 행동이 부자연스럽고 아직 패턴이 너무 적어”라고 보고했다. 행동 개선을 다음 작업으로 진행했다.

- NPC의 5초 간격 위치 변경을 연속 이동으로 바꿨다. 승인된 Walk 6프레임·8fps, 좌우 방향, 짧은 출발·도착 속도 완화를 연결했다.
- Noah는 울타리 곁 독서 → 책 내려놓기 → Workshop 쪽 산책 → 모닥불 왼편 독서 → House 쪽 산책으로 이어진다. Rumi는 별도 리듬으로 Library 방향 → Workshop 주변 → 모닥불 왼편으로 돌아온다. 잠긴 Library 안으로 들어가거나 게임 진행을 바꾸지 않는다.
- Mina는 Town의 Home/Workshop 위치 변경 때 걷고, 도착 후 실제 논리 상태 클립을 표시한다. Idle에서는 집 앞 짧은 산책을 한다. 캐릭터 앞뒤 순서는 발 높이에 따라 정한다.
- Work의 고정 반복 사이에 2.5/3초 살펴보기와 한 번 스트레칭을 넣었다. Rest·복귀·보상 명령이 이 연출보다 우선한다.
- `Walk → Work`, `Work → Celebrate`가 반복 클립에서 다음 클립으로 넘어가지 못하던 버그를 수정했다. 명시적인 FollowUp은 기본 루프 여부와 무관하게 한 사이클 뒤 진행된다.
- PiP 복귀 Walk에서는 12px을 이동하고, Mina와 그림자가 함께 작업대 위치에 도착한다. 실제 Rest 상태를 Companion에 다시 바인딩해 배율 변경·재열기에서 오래된 Work 상태로 바뀌지 않도록 했다.
- 보이는 동안의 시각 시간만 사용한다. 숨김 중에는 일상 동선 시간이 멈추며 Reduced motion에서는 선택적 이동·스트레칭이 꺼진다. NPC는 독립 프레임 타이머와 Town 타이머를 동시에 돌리지 않는다.

새 아트·논리 상태·보상·저장 필드는 추가하지 않았다. 기존 시각 38개·음향 미배정 8개를 유지한다. 음향과 새 프로젝트 콘텐츠는 별도 후속 작업이다.

## 검증과 미리보기

- 테스트 총 263개 통과: Domain 96, Foundation 30, Windows Platform 37, Application 100. 신규 검사는 반복 클립 후속 전환, 동선 연속성·경계, 작업 리듬·논리 명령 우선순위·프레임 속도 독립성이다.
- [실제 Godot 미리보기](../art/reviews/2026-10-12/behavior-v1/review.html): 741개 GPU 캡처, Town 두 구간과 Companion 한 구간의 무손실 WebP. PNG 입력과 디코딩된 RGB 픽셀 및 원래 8fps 시간을 대조했다.
- 동선 숨김/복귀·Reduced motion, Mina 이동/도착, Walk 후 Work, 그림자, Rest 재바인딩, Work 스트레칭을 분리 프로젝트에서 검사했다. 개인 저장·실제 Focus 시간은 사용하지 않았다.
- 렌더의 발 위치·좌우 방향·가림과 정지 프레임을 직접 확인했다. 추가 손동작·새 캐릭터 포즈의 아트 제작은 포함하지 않았으며 현재 승인된 클립으로 행동의 연결과 리듬을 개선했다.

검토 도구:

```powershell
python scripts/assets/prepare_behavior_review.py build/behavior-review-new
python scripts/assets/run_godot_companion_review.py build/behavior-review-new `
  --godot build/tools/godot-4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64.exe `
  --dotnet-root "$env:LOCALAPPDATA/DeskTownBuildTools/dotnet-8.0.425"
./build/tools/art-python/Scripts/python.exe scripts/assets/publish_behavior_review.py `
  build/behavior-review-new build/behavior-published-new
```

## 플레이 전달

새 키트는 `build/DeskTown-MVP-Behavior-2026-10-12.zip`이다. Start-Play는 `qa-sessions/play`에 저장하며 Start-Quick-Look은 매번 새로운 분리 저장으로 완성 연출을 보여준다. 두 키트의 진행을 합치거나 덮어쓰지 않는다. 정확한 코드 SHA·파일 해시는 키트 manifest/checksum으로 확인한다. 내보내기·분리 저장 검사 결과는 패키징 후 기록한다.
