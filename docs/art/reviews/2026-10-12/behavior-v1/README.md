# 캐릭터 행동 개선 실제 렌더

[미리보기](review.html)는 실제 Godot 4.7.2 .NET·NVIDIA GPU에서 캡처한 움직임이다. Town 0–40초·42–67초와 Companion 13–38초를 8fps 원래 속도로 재생한다. 정지 화면과 재생 전환을 제공하며 OS의 Reduced motion 선호를 따른다.

741개 원본 PNG의 해시와 시각 코드 해시는 `verification.json`에 기록했다. 세 무손실 WebP의 디코딩 RGB가 입력과 일치하며 전체 시간은 각각 40초·25초·25초다. 새 이미지나 외형을 생성하지 않고 기존 승인된 시각 38개를 사용한다.

실제 플레이는 최신 행동 개선 MVP의 Start-Play 버튼으로 시작한다. Focus 시간·게임 진행·저장·Windows 입력 동작을 이 미리보기로 합격 처리하지 않는다. [변경과 검사 기록](../../../../qa/QA_EXECUTION_2026-10-12_BEHAVIOR.md)을 참고한다.
