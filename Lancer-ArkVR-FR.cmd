@echo off
chcp 65001 >nul
title ARK VR - Lancement Haute Performance (LordMadTrix)
color 0a

echo ====================================================================
echo   🥽 LANCEMENT D'ARK EN RÉALITÉ VIRTUELLE (LORDMADTRIX)
echo ====================================================================
echo.

set "ARK_DIR=D:\SteamLibrary\steamapps\common\ARK"
if not exist "%ARK_DIR%\ShooterGame\Binaries\Win64\ShooterGame.exe" (
    set "ARK_DIR=C:\Program Files (x86)\Steam\steamapps\common\ARK"
)

set "WIN64=%ARK_DIR%\ShooterGame\Binaries\Win64"
set "INJECTOR=%WIN64%\uevr\UEVRInjector.exe"

if exist "%INJECTOR%" (
    echo [1/2] Démarrage de l'injecteur UEVR OpenXR...
    start "" "%INJECTOR%"
)

echo [2/2] Lancement de ShooterGame.exe en priorité élevée (-NoBattlEye)...
cd /d "%WIN64%"
start /high "" "ShooterGame.exe" -NoBattlEye

echo.
echo Bon jeu en immersion VR dans l'univers d'ARK !
timeout /t 3 >nul
exit
