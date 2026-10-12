# 최종 시각 적용본 — 38개 production

[최종 화면](review.html)에서 Town 세 상태, 효과 감소, First Launch, Focus, Workshop 완성 전/후, 발견, Companion Work/Rest 및 Ghost 미리보기를 볼 수 있다. 승인된 마지막 10개를 실제 카탈로그와 화면에 적용한 **55개 GPU 캡처**다. 개인 저장·AppRoot·실제 창 입력을 실행하지 않았다.

38개 시각 항목 모두 production이며 원본·프롬프트·승인·해시는 [납품 기록](../../../../../assets/art/DELIVERY_2026-10-12.md)과 카탈로그에 있다. 음향 8개는 별도다. [실제 배정·모션 검사](../remaining-delivery-v1/README.md)는 204개 캡처와 Work 접촉 한 번 재생·숨김 중지·Reduced motion 전환을 확인했다.

[검증](verification.json)은 55장 크기·SHA-256, 실제 38개 리소스 해시와 코드/장면 해시를 확인한다. Ghost 40/65/85%에서 118,800개 출력 알파를 원본 레이어 합성식과 비교했다. 각 레이어에 기존 root modulate를 적용하므로 겹친 그림자·작업대·캐릭터 픽셀의 합성 알파는 단일 레이어 값보다 높다. 균일한 창 전체 알파를 주장하지 않는다.

Reward는 실제 `PlayAsync`의 2초 홀드와 상태 교체 콜백을 사용해 전/후를 캡처했다. Continue가 활성화되고 Finish를 두 번 호출해도 완료 콜백은 한 번이다. Reduced motion의 짧은 완료도 교체/완료가 한 번임을 확인했다. 이 검사는 진행도 부여나 저장을 연출 프레임에 연결하지 않는다.

Godot 4.7.2 .NET / SDK 8.0.425 / Compatibility GPU. [빌드](build.log), [캡처](capture.log), [렌더 목록](captures/result.json). 입력은 preparation.json의 기준 커밋과 명시적으로 복사한 작업 트리 파일 해시로 기록했다. 캡처는 보정하지 않았다. 온보딩의 Town 배경 숨김 등 기존 앱 표시 정책은 유지했다. Ghost에는 입력 노드를 추가하지 않았으며 일반 Focus는 여전히 비활성화다.

시각 적용을 완료했으며 네이티브 입력·DPI·시스템 이벤트·출시 검사는 [사용자 QA](../../../../qa/USER_QA_START.md)에 남는다. QA-PIP-001은 TODO다.

재현: `prepare_final_art_review.py build/final-render-replay` → `run_godot_companion_review.py` → `audit_final_art_review.py`. SDK·Godot 인수는 다른 실제 렌더 검토와 같다.
