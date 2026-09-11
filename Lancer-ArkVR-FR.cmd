@echo off
chcp 65001 >nul
title ARK VR - Lancement Haute Performance (LordMadTrix)
color 0a

echo ====================================================================
echo   ? ? ? ? ? ? ? / ? ? ?   ? ?  -  LANCEMENT AUTO-INJECTION ZERO-CLIC
echo ====================================================================
echo.

set "ARK_DIR=D:\SteamLibrary\steamapps\common\ARK"
if not exist "%ARK_DIR%\ShooterGame\Binaries\Win64\ShooterGame.exe" (
    set "ARK_DIR=C:\Program Files (x86)\Steam\steamapps\common\ARK"
)

set "WIN64=%ARK_DIR%\ShooterGame\Binaries\Win64"
set "UEVR_DLL=%WIN64%\uevr\UEVRBackend.dll"

:: 1. D?marrage de SteamVR si inactif
tasklist | findstr /i "vrserver.exe" >nul || start "" "steam://run/250820"

:: 2. Lancement de la surveillance d'auto-injection en t?che de fond invisible
if exist "%~dp0AutoInject-ArkVR.ps1" (
    start "" /b powershell.exe -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File "%~dp0AutoInject-ArkVR.ps1" -DllPath "%UEVR_DLL%"
)

:: 3. Lancement de ShooterGame en priorit? ?lev?e
echo Lancement d'ARK: Survival Evolved (-NoBattlEye)...
cd /d "%WIN64%"
start /high "" "ShooterGame.exe" -NoBattlEye

echo.
echo [OK] Auto-injection UEVR arm?e en t?che de fond !
echo Le jeu basculera directement en VR 6DOF dans votre casque.
timeout /t 3 >nul
exit
