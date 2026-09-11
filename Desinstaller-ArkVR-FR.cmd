@echo off
title Desinstallation de ARK VR (LordMadTrix)
chcp 65001 >nul
cd /d "%~dp0"

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Desinstaller-ArkVR-FR.ps1"

exit /b %ERRORLEVEL%
