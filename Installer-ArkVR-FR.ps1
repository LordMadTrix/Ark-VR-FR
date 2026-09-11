<#
.SYNOPSIS
    Script d'installation automatise pour ARK: Survival Evolved VR (Edition Francaise).
.AUTHOR
    LordMadTrix
#>

[CmdletBinding()]
param (
    [string]$GameDirectory = ""
)

$Host.UI.RawUI.WindowTitle = "Installateur ARK VR - LordMadTrix"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Clear-Host
Write-Host "====================================================================" -ForegroundColor Cyan
Write-Host "       INSTALLATEUR OFFICIEL ARK: SURVIVAL EVOLVED VR - FR          " -ForegroundColor Yellow
Write-Host "                          [v1.4.0-FR]                               " -ForegroundColor Green
Write-Host "====================================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Detection du jeu
$targetDir = $GameDirectory
if ([string]::IsNullOrWhiteSpace($targetDir)) {
    $potentialPaths = @(
        "D:\SteamLibrary\steamapps\common\ARK",
        "C:\Program Files (x86)\Steam\steamapps\common\ARK",
        "C:\Steam\steamapps\common\ARK",
        "E:\SteamLibrary\steamapps\common\ARK"
    )
    foreach ($p in $potentialPaths) {
        if (Test-Path (Join-Path $p "ShooterGame\Binaries\Win64\ShooterGame.exe")) {
            $targetDir = $p
            break
        }
    }
}

if (-not $targetDir -or -not (Test-Path (Join-Path $targetDir "ShooterGame\Binaries\Win64\ShooterGame.exe"))) {
    $targetDir = Read-Host "  Entrez le chemin complet de votre dossier ARK"
}

if (-not (Test-Path (Join-Path $targetDir "ShooterGame\Binaries\Win64\ShooterGame.exe"))) {
    Write-Host "  [X] Dossier ARK invalide." -ForegroundColor Red
    Pause
    exit 1
}

Write-Host "  [OK] Dossier ARK detecte : $targetDir" -ForegroundColor Green
Write-Host ""
Write-Host "  [>>>] Deploiement des composants VR..." -ForegroundColor Cyan

$win64 = Join-Path $targetDir "ShooterGame\Binaries\Win64"
$uevrDir = Join-Path $win64 "uevr"
$null = New-Item -ItemType Directory -Path $uevrDir -Force -ErrorAction SilentlyContinue

# Copie des fichiers UEVR du payload
$sourcePayload = Join-Path $PSScriptRoot "payload\uevr"
if (Test-Path $sourcePayload) {
    Copy-Item -Path "$sourcePayload\*" -Destination $uevrDir -Recurse -Force
    Write-Host "  [OK] Runtime UEVR 6DOF deploye dans ShooterGame\Binaries\Win64\uevr" -ForegroundColor Green
}

# Profils et config UEVR
$appDataUevr = Join-Path ([Environment]::GetFolderPath("ApplicationData")) "uevr\profiles\ShooterGame"
$null = New-Item -ItemType Directory -Path $appDataUevr -Force -ErrorAction SilentlyContinue
$cfgSrc = Join-Path $PSScriptRoot "payload\config\ShooterGame_config.txt"
if (Test-Path $cfgSrc) {
    Copy-Item -Path $cfgSrc -Destination (Join-Path $appDataUevr "config.txt") -Force
    Write-Host "  [OK] Profils 6DOF et camera montures deployes" -ForegroundColor Green
}

# Optimisations Engine.ini
$savedConfig = Join-Path $targetDir "ShooterGame\Saved\Config\WindowsNoEditor"
$null = New-Item -ItemType Directory -Path $savedConfig -Force -ErrorAction SilentlyContinue
$engineIni = Join-Path $savedConfig "Engine.ini"
$engineBak = Join-Path $savedConfig "Engine.ini.bak"

if ((Test-Path $engineIni) -and -not (Test-Path $engineBak)) {
    Copy-Item -Path $engineIni -Destination $engineBak -Force
    Write-Host "  [OK] Sauvegarde Vanilla Engine.ini.bak creee" -ForegroundColor Green
}

$tweaksSrc = Join-Path $PSScriptRoot "payload\config\VR_Engine_Tweaks.ini"
if (Test-Path $tweaksSrc) {
    $tweaks = Get-Content $tweaksSrc -Raw
    if (Test-Path $engineIni) {
        $existing = Get-Content $engineIni -Raw
        if ($existing -notmatch "r\.VolumetricCloud") {
            Add-Content -Path $engineIni -Value "`r`n$tweaks"
            Write-Host "  [OK] Optimisations graphiques VR appliquees a Engine.ini" -ForegroundColor Green
        }
    } else {
        Set-Content -Path $engineIni -Value $tweaks -Encoding UTF8
    }
}

# Raccourcis Bureau
$desktop = [Environment]::GetFolderPath("Desktop")
$bat = "@echo off`r`ntitle ARK VR - LordMadTrix`r`ncd /d ""$win64""`r`nstart """" ""uevr\UEVRInjector.exe""`r`nstart /high """" ""ShooterGame.exe"" -NoBattlEye`r`nexit`r`n"
Set-Content -Path (Join-Path $desktop "ARK VR (FR).cmd") -Value $bat -Encoding ASCII
Write-Host "  [OK] Raccourci Bureau 'ARK VR (FR)' cree" -ForegroundColor Green

Write-Host ""
Write-Host "====================================================================" -ForegroundColor Green
Write-Host "          INSTALLATION DE ARK VR TERMINEE AVEC SUCCES !             " -ForegroundColor Green
Write-Host "====================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Lancez SteamVR puis utilisez le raccourci Bureau pour jouer !" -ForegroundColor White
Write-Host ""
Pause