# 음향 검토 준비 — 2026-10-12

## 진행 범위

사용자의 다음 작업 지시에 따라 남은 음향 8종의 청취 후보를 제작했다. 기존 캐릭터 행동 개선과 플레이 패키지는 완료된 상태로 보존했다. 기준 커밋은 `eff41fe`이며 실행 코드 기준은 `105de0f`이다.

[청취 페이지](../art/reviews/2026-10-12/audio-v1/review.html)에 자동 재생 없는 개별 소리 8개와 마을 24초·Companion 20초·완성 5초·발견 5초 믹스를 제공한다. 믹스는 실제 게임 녹음이 아닌 음량·간격 후보다.

로컬 전달 ZIP: `build/DeskTown-Audio-Review-2026-10-12.zip`, 441,252 bytes. SHA-256 `dacbfd17ab4f6adbf0693eb377f0ed07dacb6033d33413ec2b45fe3a82091393`. 압축 파일 CRC와 음원 12개 해시를 확인했다. 풀어서 `review.html`을 직접 연다.

- 마을 공기 60초와 모닥불 12초는 스테레오 반복 후보다.
- 새 1.65초, 발소리 0.30초, 나무 망치 0.42초, 버튼 0.13초는 모노 단발 후보다.
- 완성 1.75초와 발견 1.55초는 서로 다른 모노 신호다.
- 외부 음원·녹음·샘플 없이 수학적 잡음/진동/감쇠로 제작했다. 납품·사용 권한과 청취 품질 판정은 사용자 승인 대기다.

## 기술 검사

- 12파일을 디코딩해 44.1kHz Ogg Vorbis, 정확한 길이, 유한 샘플과 여유 sample peak를 확인했다. 가장 높은 peak는 -19.812 dBFS다. LUFS·인증 true peak를 측정했다고 주장하지 않는다.
- 반복 경계의 샘플 차이가 각 파일의 일반 인접 차이 99백분위 이하인지 확인했다. 마을 경계 -80.908 dBFS, 모닥불 경계 -60.327 dBFS다. 이 수치는 청취상 자연스러운 반복을 보장하지 않는다.
- Godot 4.7.2의 분리 프로젝트와 Dummy audio driver에서 8파일을 직접 디코딩했다. 종료 직전 재생 후 반복 2개는 계속 재생, 단발 6개는 종료됐으며 모두 정지가 가능했다. 실제 스피커 출력 판정은 아니다.
- HTML의 12개 음원 경로·반복 2개·자동 재생 없음과 JavaScript 문법을 검사했다. 로컬 파일을 여는 브라우저 자동 점검은 URL 보안 정책으로 거절돼 진행하지 않았다. 사용자가 직접 로컬 페이지를 열어 재생·음색·반복감·음량을 확인해야 한다.
- Windows libsndfile/Vorbis의 긴 버퍼 쓰기 중 stack overflow를 관측했다. 4096샘플 단위 쓰기로 수정한 뒤 전체 12파일을 생성하고 다시 디코딩했다. 실패 출력은 build 아래에 보존했고 납품 폴더에 복사하지 않았다.
- [파일별 수치와 SHA-256](../art/reviews/2026-10-12/audio-v1/verification.json), [엔진 검사](../art/reviews/2026-10-12/audio-v1/engine-verification.json)에 증거를 기록했다.

실제 카탈로그 SHA-256은 `5476b8505359f88b2616a97ba21b49c0986b06a68401c113e33227489618d4eb`로 유지했다. 검증기는 46항목·38배정·8미배정으로 통과했다. 런타임 코드·개인 저장·스키마·기존 플레이 ZIP을 변경하지 않았다. .NET 테스트는 이 검토 자료 변경에 다시 수행하지 않았다.

## 재현

```powershell
./build/tools/art-python/Scripts/python.exe -m pip install numpy==2.5.4 soundfile==0.14.0
./build/tools/art-python/Scripts/python.exe scripts/assets/prepare_audio_review.py build/audio-review-new
./build/tools/art-python/Scripts/python.exe scripts/assets/check_audio_review.py build/audio-review-new `
  --engine build/tools/godot-4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64.exe
```

## 다음 승인 후 적용안

기존 Audio 설정 하나로 전체 소리를 제어한다. Audio=false 저장은 유지하고, Audio=true 저장과 새 게임은 소리가 켜진다. 화면이 숨겨지면 주변음·발소리·망치를 정지하고 지나간 소리를 몰아서 재생하지 않는다. 완료·발견 신호는 해당 화면을 볼 때 한 번 재생한다. 실제 보행/타격 프레임에 소리를 연결하며 반복 빈도와 중첩 수를 제한한다. Focus Energy·보상·저장 필드는 추가하지 않는다.

이전 무음 MVP의 재생 동작이 바뀌므로 소리·믹스·납품 및 적용안을 인간에게 확인한 후 production 배정과 런타임 연결을 진행한다.
