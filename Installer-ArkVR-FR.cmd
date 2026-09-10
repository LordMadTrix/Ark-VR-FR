@echo off
chcp 65001 >nul
title ARK VR - Installation Automatisée (LordMadTrix)
color 0b

echo ====================================================================
echo   🦖 ARK: SURVIVAL EVOLVED VR (ÉDITION FRANÇAISE) - LORDMADTRIX
echo ====================================================================
echo.
echo Détection du dossier de jeu...

set "ARK_DIR=D:\SteamLibrary\steamapps\common\ARK"
if not exist "%ARK_DIR%\ShooterGame\Binaries\Win64\ShooterGame.exe" (
    set "ARK_DIR=C:\Program Files (x86)\Steam\steamapps\common\ARK"
)

if not exist "%ARK_DIR%\ShooterGame\Binaries\Win64\ShooterGame.exe" (
    echo [ERREUR] Impossible de localiser ShooterGame.exe automatiquement.
    echo Veuillez exécuter ArkVR-Setup.exe pour sélectionner votre dossier.
    pause
    exit /b 1
)

echo [OK] Dossier ARK détecté : %ARK_DIR%
echo.
echo Déploiement du runtime UEVR 6DOF...
if not exist "%ARK_DIR%\ShooterGame\Binaries\Win64\uevr" mkdir "%ARK_DIR%\ShooterGame\Binaries\Win64\uevr"
xcopy /s /e /y "payload\uevr\*" "%ARK_DIR%\ShooterGame\Binaries\Win64\uevr\" >nul

echo Déploiement des profils 6DOF et montures de dinosaures...
if not exist "%APPDATA%\uevr\profiles\ShooterGame" mkdir "%APPDATA%\uevr\profiles\ShooterGame"
copy /y "payload\config\ShooterGame_config.txt" "%APPDATA%\uevr\profiles\ShooterGame\config.txt" >nul

echo Application des optimisations FPS (Nuages volumétriques)...
set "ENGINE_INI=%ARK_DIR%\ShooterGame\Saved\Config\WindowsNoEditor\Engine.ini"
if exist "%ENGINE_INI%" (
    findstr /c:"r.VolumetricCloud" "%ENGINE_INI%" >nul
    if errorlevel 1 (
        type "payload\config\VR_Engine_Tweaks.ini" >> "%ENGINE_INI%"
    )
)

echo.
echo ====================================================================
echo   🎉 INSTALLATION TERMINÉE AVEC SUCCÈS !
echo   Lancez SteamVR puis double-cliquez sur Lancer-ArkVR-FR.cmd !
echo ====================================================================
echo.
pause
