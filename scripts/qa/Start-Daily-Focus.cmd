@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -File ".\preflight.ps1" -KitDirectory "."
if errorlevel 1 goto failed
powershell.exe -NoProfile -File ".\launch-demo.ps1" -Scenario railway-teaser -WindowsExport ".\app" -DemoSaves ".\demo-saves" -SessionName daily
if errorlevel 1 goto failed
exit /b 0
:failed
echo Launch failed. See PLAY-README.md and the error above.
pause
exit /b 1
