@echo off
setlocal
cd /d "%~dp0"
if exist "RelayLauncher.exe" (
  start "" "RelayLauncher.exe"
  exit /b 0
)
if exist "MechLauncher.exe" (
  start "" "MechLauncher.exe"
  exit /b 0
)
echo Relay's launcher is not built yet.
echo Run: powershell -ExecutionPolicy Bypass -File "launcher\build.ps1"
pause
exit /b 1