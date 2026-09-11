<#
.SYNOPSIS
    Script de desinstallation et restauration Vanilla pour ARK: Survival Evolved VR.
.DESCRIPTION
    Supprime les modules UEVR injectes, restaure le fichier Engine.ini d'origine
    et supprime les raccourcis bureau. Vos mondes, dinosaures et sauvegardes sont 100% preserves.
.AUTHOR
    LordMadTrix
#>

[CmdletBinding()]
param (
    [string]$GameDirectory = ""
)

$Host.UI.RawUI.WindowTitle = "Desinstallateur ARK VR (Vanilla)"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Clear-Host
Write-Host "====================================================================" -ForegroundColor Red
Write-Host "         RESTAURATION D'ARK: SURVIVAL EVOLVED EN MODE VANILLA       " -ForegroundColor Yellow
Write-Host "====================================================================" -ForegroundColor Red
Write-Host ""
Write-Host "  Ce script restaure ARK dans son etat standard d'origine (ecran plat)." -ForegroundColor Gray
Write-Host "  Vos sauvegardes, dinosaures, tribus et maps sont 100% preserves." -ForegroundColor Green
Write-Host ""

# 1. Detection du dossier ARK
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
    Write-Host "  [X] Dossier ARK invalide (ShooterGame.exe introuvable)." -ForegroundColor Red
    Pause
    exit 1
}

Write-Host "  Dossier cible : $targetDir" -ForegroundColor Cyan
Write-Host ""
$confirm = Read-Host "  Voulez-vous desactiver le mod VR et restaurer ARK en mode classique ? (O/N)"
if ($confirm -notmatch '^[oOyY]') {
    Write-Host "  Restauration annulee." -ForegroundColor Yellow
    Pause
    exit 0
}

Write-Host ""
Write-Host "  [>>>] Restauration en cours..." -ForegroundColor Cyan

# 2. Suppression des modules UEVR
$win64 = Join-Path $targetDir "ShooterGame\Binaries\Win64"
$uevrDir = Join-Path $win64 "uevr"
if (Test-Path $uevrDir) {
    Remove-Item -Path $uevrDir -Recurse -Force
    Write-Host "  [OK] Modules UEVR supprimes de ShooterGame\Binaries\Win64" -ForegroundColor Green
}

# 3. Nettoyage des profils AppData
$appDataUevr = Join-Path ([Environment]::GetFolderPath("ApplicationData")) "uevr\profiles\ShooterGame"
if (Test-Path $appDataUevr) {
    Remove-Item -Path $appDataUevr -Recurse -Force
    Write-Host "  [OK] Profils AppData UEVR supprimes" -ForegroundColor Green
}

# 4. Restauration de la sauvegarde Engine.ini.bak si existante
$savedConfig = Join-Path $targetDir "ShooterGame\Saved\Config\WindowsNoEditor"
$engineIni = Join-Path $savedConfig "Engine.ini"
$engineBak = Join-Path $savedConfig "Engine.ini.bak"
if (Test-Path $engineBak) {
    Copy-Item -Path $engineBak -Destination $engineIni -Force
    Remove-Item -Path $engineBak -Force
    Write-Host "  [OK] Engine.ini restaure depuis la sauvegarde Vanilla (.bak)" -ForegroundColor Green
}

# 5. Nettoyage des raccourcis Bureau
$desktop = [Environment]::GetFolderPath("Desktop")
$shortcuts = @("ARK VR (FR).cmd", "ARK VR (FR).url", "ARK VR.url")
foreach ($s in $shortcuts) {
    $scPath = Join-Path $desktop $s
    if (Test-Path $scPath) {
        Remove-Item -Path $scPath -Force
        Write-Host "  [OK] Raccourci Bureau '$s' supprime" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "====================================================================" -ForegroundColor Green
Write-Host "   ARK EST RESTAURE EN CONFIGURATION ORIGINALE ECRAN PLAT (VANILLA) " -ForegroundColor Green
Write-Host "====================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Vous pouvez desormais lancer ARK normalement via Steam." -ForegroundColor White
Write-Host ""
Pause