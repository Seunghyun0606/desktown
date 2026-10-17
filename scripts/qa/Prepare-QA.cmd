@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -File ".\prepare-qa.ps1" -KitDirectory "."
if errorlevel 1 goto failed
echo See the new report path above and QA-README.md for the test order.
pause
exit /b 0
:failed
echo QA preparation failed. See the error above and QA-README.md.
pause
exit /b 1
