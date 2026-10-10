# 로컬 Windows 내보내기

`scripts/export_windows.py`는 현재 저장소의 `global.json`과 `DeskTown.csproj`에 고정된 SDK·Godot .NET 버전을 검사하고 Windows Release 폴더를 생성한다. Python 3와 Godot .NET의 Windows 내보내기 템플릿이 필요하다.

## 실행

.NET SDK는 영문 경로에 설치한다. 이번 PC에서는 한글이 포함된 SDK 경로를 Godot이 `??`로 읽어 MSBuild 초기화가 실패했다. 저장소 경로에는 한글이 있어도 이번 검사는 통과했다. SDK를 분리 설치하려면 [Microsoft 설치 스크립트 안내](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script)에 따라 버전과 설치 폴더를 지정한다. 시스템 SDK·사용자 PATH를 변경할 필요는 없다.

```powershell
python scripts/export_windows.py `
  --godot build/tools/godot-4.7.2/Godot_v4.7.2-stable_mono_win64/Godot_v4.7.2-stable_mono_win64_console.exe `
  --dotnet-root "$env:LOCALAPPDATA/DeskTownBuildTools/dotnet-8.0.425" `
  --output-directory build/windows-local
```

출력 경로는 저장소의 `build/` 아래 빈 폴더여야 한다. 기존 파일이 있으면 새 경로를 선택한다. 스크립트는 기존 산출물을 삭제하거나 덮어쓰지 않는다. 명령은 저장소 밖에서 실행해도 프로젝트 루트를 사용하며, 상대 인수 경로는 현재 작업 폴더 기준이다.

## 종료와 판정

Windows의 Godot 콘솔 래퍼는 본체가 종료해도 자식 .NET 빌드 서버를 기다리며 멈출 수 있다. [Godot 이슈 #110101](https://github.com/godotengine/godot/issues/110101)에 같은 현상이 보고돼 있다. 스크립트는 `_console.exe` 인수를 같은 폴더의 본체 `.exe`로 변환하고 본체 종료를 직접 기다린다. 해당 실행의 컴파일 공유·MSBuild 작업자 재사용도 환경 변수로 끈다. 다른 빌드 서버를 종료하지 않는다.

성공 조건은 종료 코드 0, 로그의 Godot·MSBuild 오류 없음, 비어 있지 않은 `DeskTown.exe`와 배포용 `DeskTown.dll` 존재다. 파일 생성만으로 성공을 판정하지 않는다. 로그와 `result.json`은 실행별 `build/export-logs/` 폴더에 보관하며 배포 폴더에 포함하지 않는다.

기본 제한 시간은 300초이며 `--timeout`으로 조절한다. 시간이 초과되면 해당 프로세스와 하위 프로세스만 종료하고 실패로 기록한다. 부분 산출물과 로그는 보존한다. 원인을 해결한 뒤 새 빈 출력 폴더에서 다시 실행한다.

이 도구의 성공은 내보내기 검사다. 분리된 저장·재시작 검사, 패키지 무결성 검사, 실제 Windows 창·입력 검증은 [Windows 게이트](WINDOWS_GATE.md)의 별도 항목이다. Linux CI의 내보내기 명령은 기존대로 유지한다.
