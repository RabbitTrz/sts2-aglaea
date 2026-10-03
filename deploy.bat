@echo off
setlocal

rem ============================================================
rem  Aglaea mod one-click build + export + deploy
rem  Usage:
rem    deploy.bat          -> build + export + copy (all)
rem    deploy.bat build    -> only dotnet build
rem    deploy.bat export   -> only godot export pck
rem    deploy.bat copy     -> only copy files to mods dir
rem ============================================================

set "PROJECT_DIR=%~dp0"
if "%PROJECT_DIR:~-1%"=="\" set "PROJECT_DIR=%PROJECT_DIR:~0,-1%"
set "GODOT=C:\Users\28415\Desktop\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64_console.exe"
set "DLL=.godot\mono\temp\bin\ExportRelease\win-x64\Aglaea.dll"
set "DEST=C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\mods\Aglaea"

if "%~1"=="" set "DO_ALL=1"

cd /d "%PROJECT_DIR%"
if errorlevel 1 (
    echo [ERROR] cannot enter project dir: %PROJECT_DIR%
    pause
    exit /b 1
)

if defined DO_ALL goto :BUILD
if /i "%~1"=="build" goto :BUILD
if /i "%~1"=="export" goto :EXPORT
if /i "%~1"=="copy" goto :COPY
echo [ERROR] unknown arg: %~1  (use build / export / copy, or none)
pause
exit /b 1

:BUILD
echo.
echo ===== [1/3] dotnet build =====
dotnet build
if errorlevel 1 (
    echo.
    echo [FAILED] build error. aborted.
    pause
    exit /b 1
)
if defined DO_ALL goto :EXPORT
goto :EOF

:EXPORT
echo.
echo ===== [2/3] godot export Aglaea.pck =====
"%GODOT%" --headless --path "%PROJECT_DIR%" --export-pack "Windows Desktop" Aglaea.pck
if errorlevel 1 (
    echo.
    echo [FAILED] export error. aborted.
    pause
    exit /b 1
)
if defined DO_ALL goto :COPY
goto :EOF

:COPY
echo.
echo ===== [3/3] deploy to mods dir =====
if not exist "%DLL%" (
    echo [ERROR] DLL not found: %PROJECT_DIR%%DLL%
    pause
    exit /b 1
)
if not exist "Aglaea.pck" (
    echo [ERROR] PCK not found: %PROJECT_DIR%Aglaea.pck
    pause
    exit /b 1
)
if not exist "%DEST%" mkdir "%DEST%"
copy /y "%DLL%" "%DEST%\Aglaea.dll" >nul
copy /y "Aglaea.json" "%DEST%\Aglaea.json" >nul
copy /y "Aglaea.pck" "%DEST%\Aglaea.pck" >nul

echo.
echo ============================================================
echo  DEPLOY DONE. Updated:
echo    %DEST%\Aglaea.dll
echo    %DEST%\Aglaea.json
echo    %DEST%\Aglaea.pck
echo ============================================================
echo.
echo Restart the game to reload the mod.
pause
exit /b 0
