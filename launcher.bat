@echo off
setlocal
title Mech Desktop App
echo Starting the Mech desktop app...
cd /d "%~dp0"

where node >nul 2>nul
if errorlevel 1 (
  echo Node.js is required. Install Node.js 18 or newer, then try again.
  goto :failed
)
where cargo >nul 2>nul
if errorlevel 1 (
  echo Rust is required to start the desktop app. Install Rust, then try again.
  goto :failed
)
if not exist "node_modules\.bin\tauri.cmd" (
  echo Project dependencies are missing. Run npm install in this folder, then try again.
  goto :failed
)

set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" (
  echo Visual Studio C++ Build Tools are required to compile Mech's Rust desktop core.
  echo Install the C++ build tools, then relaunch Mech.
  goto :failed
)
set "VSROOT="
for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -products * -property installationPath`) do set "VSROOT=%%i"
if not defined VSROOT for %%P in ("%ProgramFiles%\Microsoft Visual Studio\18\Community" "%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools" "%ProgramFiles%\Microsoft Visual Studio\2022\Community") do if not defined VSROOT if exist "%%~P\VC\Auxiliary\Build\vcvars64.bat" set "VSROOT=%%~P"
if not defined VSROOT (
  echo The Visual C++ workload is missing. In Visual Studio Installer, choose Modify and install
  echo Desktop development with C++ and a Windows 10 or 11 SDK, then relaunch Mech.
  goto :failed
)
if not exist "%VSROOT%\VC\Auxiliary\Build\vcvars64.bat" (
  echo Visual Studio was found, but its x64 C++ compiler environment is incomplete.
  echo Repair the Visual C++ workload, then relaunch Mech.
  goto :failed
)

call "%VSROOT%\VC\Auxiliary\Build\vcvars64.bat" >nul
if errorlevel 1 (
  echo Could not initialize the Visual C++ build environment.
  goto :failed
)
where link.exe >nul 2>nul
if errorlevel 1 (
  echo The Visual C++ linker is still missing. Repair the C++ Build Tools installation.
  goto :failed
)

set "CARGO_PROFILE_DEV_DEBUG=0"
echo Launching the native Tauri app. First build can take several minutes.
call npm run desktop
if errorlevel 1 goto :failed
goto :done

:failed
echo.
echo Mech could not start. The diagnostic above identifies the missing requirement.
pause
exit /b 1

:done
endlocal
