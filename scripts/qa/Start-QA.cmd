@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -File ".\preflight.ps1" -KitDirectory "."
if errorlevel 1 goto failed
powershell.exe -NoProfile -File ".\launch-demo.ps1" -Scenario first-launch -WindowsExport ".\app" -DemoSaves ".\demo-saves" -SessionName focus
if errorlevel 1 goto failed
exit /b 0
:failed
echo QA launch failed. See the error above and QA-README.md.
pause
exit /b 1
