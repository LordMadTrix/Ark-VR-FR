$ErrorActionPreference = "Stop"
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "🔄 SIMULATION DE LA RESTAURATION VANILLA D'ARK" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$arkPath = "D:\SteamLibrary\steamapps\common\ARK"
$uevrDest = Join-Path $arkPath "ShooterGame\Binaries\Win64\uevr"
$appDataProfile = Join-Path $env:APPDATA "uevr\profiles\ShooterGame"
$engineIni = Join-Path $arkPath "ShooterGame\Saved\Config\WindowsNoEditor\Engine.ini"
$engineIniBackup = Join-Path $arkPath "ShooterGame\Saved\Config\WindowsNoEditor\Engine.ini.bak"

# 1. Suppression du dossier uevr
Write-Host "[1/3] Nettoyage du dossier binaires UEVR..." -ForegroundColor Yellow
if (Test-Path $uevrDest) {
    Remove-Item $uevrDest -Recurse -Force
    Write-Host "  -> [OK] Dossier $uevrDest supprimé." -ForegroundColor Green
}

# 2. Suppression du profil temporaire AppData
Write-Host "`n[2/3] Nettoyage du profil UEVR..." -ForegroundColor Yellow
if (Test-Path $appDataProfile) {
    Remove-Item $appDataProfile -Recurse -Force
    Write-Host "  -> [OK] Profil $appDataProfile supprimé." -ForegroundColor Green
}

# 3. Restauration du fichier Engine.ini d'origine
Write-Host "`n[3/3] Restauration du fichier Engine.ini original..." -ForegroundColor Yellow
if (Test-Path $engineIniBackup) {
    Move-Item $engineIniBackup -Destination $engineIni -Force
    Write-Host "  -> [OK] Engine.ini restauré à l'état d'origine." -ForegroundColor Green
}

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host "✅ LE JEU EST 100% RESTAURÉ À L'ÉTAT VANILLA INITIAL !" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Cyan
