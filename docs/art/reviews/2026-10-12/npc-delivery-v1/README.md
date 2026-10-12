# Noah·Rumi 실제 배정 검증

사용자 결정 **A: NPC 승인 — 적용 후 나머지 아트 제작**에 따라 prepared-v2의 5개 PNG를 바이트 그대로 실제 `assets/art/`에 복사했다. 카탈로그는 **28 production**, 시각 10개·음향 8개 미배정이다.

분리된 프로젝트에서 실제 카탈로그·Town 장면·NPC 뷰를 사용해 120개 GPU 캡처를 재현했다. 후보 재배정이나 배치 변경 없이 [승인 검토본](../npc-v1/review.html)의 모든 캡처와 바이트 단위로 같았다. 24프레임의 최근접 렌더 픽셀 221,184개와 NPC 영역 밖 Town 보존을 확인했다. [검증 기록](verification.json), [빌드](build.log), [캡처 로그](capture.log).

Godot 4.7.2 .NET / SDK 8.0.425 / Compatibility GPU. 개인 저장·AppRoot를 실행하지 않았다. Walk는 개별 시트 렌더이고 기존 주변 위치·Idle/Read 선택 스케줄을 바꾸지 않았다. 네이티브 창·입력·DPI 검증은 아니다. QA-PIP-001은 TODO다.

재현: `prepare_godot_npc_review.py build/npc-delivery-replay --runtime-catalog` → `run_godot_companion_review.py` → `audit_godot_npc_review.py`. 각 입력과 캡처 SHA-256은 preparation.json과 verification.json에 기록했다.
