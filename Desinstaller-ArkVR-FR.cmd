@echo off
chcp 65001 >nul
title ARK VR - Désinstallation Vanilla (LordMadTrix)
color 0c

echo ====================================================================
echo   🔄 RESTAURATION VANILLA D'ARK: SURVIVAL EVOLVED (LORDMADTRIX)
echo ====================================================================
echo.
echo Ce script supprime les fichiers de configuration VR.
echo Vos sauvegardes, mondes, tribus et dinosaures sont 100%% préservés.
echo.
set /p CONFIRM="Voulez-vous restaurer ARK en mode classique écran plat ? (O/N) : "
if /i not "%CONFIRM%"=="O" (
    echo Opération annulée.
    pause
    exit /b 0
)

set "ARK_DIR=D:\SteamLibrary\steamapps\common\ARK"
if not exist "%ARK_DIR%\ShooterGame\Binaries\Win64\ShooterGame.exe" (
    set "ARK_DIR=C:\Program Files (x86)\Steam\steamapps\common\ARK"
)

echo.
echo Suppression des modules UEVR...
if exist "%ARK_DIR%\ShooterGame\Binaries\Win64\uevr" (
    rmdir /s /q "%ARK_DIR%\ShooterGame\Binaries\Win64\uevr"
)

echo Nettoyage des profils AppData...
if exist "%APPDATA%\uevr\profiles\ShooterGame" (
    rmdir /s /q "%APPDATA%\uevr\profiles\ShooterGame"
)

echo.
echo ====================================================================
echo   ✅ ARK EST 100%% RESTAURÉ À SON ÉTAT D'ORIGINE VANILLA !
echo ====================================================================
echo.
pause
