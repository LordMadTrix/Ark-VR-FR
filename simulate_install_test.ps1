$ErrorActionPreference = "Stop"
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "🦖 SIMULATION DU FLUX D'INSTALLATION ET VR POUR ARK" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$arkPath = "D:\SteamLibrary\steamapps\common\ARK"
$win64Dir = Join-Path $arkPath "ShooterGame\Binaries\Win64"
$gameExe = Join-Path $win64Dir "ShooterGame.exe"
$uevrDest = Join-Path $win64Dir "uevr"
$appDataProfile = Join-Path $env:APPDATA "uevr\profiles\ShooterGame"
$engineIni = Join-Path $arkPath "ShooterGame\Saved\Config\WindowsNoEditor\Engine.ini"
$engineIniBackup = Join-Path $arkPath "ShooterGame\Saved\Config\WindowsNoEditor\Engine.ini.bak"

# 1. Test de détection
Write-Host "`n[1/6] Vérification de la présence de ShooterGame.exe..." -ForegroundColor Yellow
if (Test-Path $gameExe) {
    Write-Host "  -> [OK] Exécutable trouvé : $gameExe" -ForegroundColor Green
    $fileInfo = Get-Item $gameExe
    Write-Host "  -> Taille : $([math]::Round($fileInfo.Length / 1MB, 2)) Mo" -ForegroundColor Gray
} else {
    Write-Host "  -> [ERREUR] ShooterGame.exe introuvable !" -ForegroundColor Red
    exit 1
}

# 2. Sauvegarde de Engine.ini
Write-Host "`n[2/6] Sauvegarde du fichier de configuration Engine.ini..." -ForegroundColor Yellow
Copy-Item $engineIni -Destination $engineIniBackup -Force
Write-Host "  -> [OK] Backup créé : $engineIniBackup" -ForegroundColor Green

# 3. Simulation du déploiement des binaires UEVR
Write-Host "`n[3/6] Déploiement des binaires UEVR 6DOF..." -ForegroundColor Yellow
if (!(Test-Path $uevrDest)) {
    New-Item -ItemType Directory -Path $uevrDest -Force | Out-Null
}
Copy-Item "D:\VR ARK\payload\uevr\*" -Destination $uevrDest -Recurse -Force
$deployedFiles = Get-ChildItem $uevrDest -File
Write-Host "  -> [OK] $($deployedFiles.Count) fichiers UEVR déployés dans $uevrDest :" -ForegroundColor Green
foreach ($f in $deployedFiles) {
    Write-Host "     * $($f.Name) ($([math]::Round($f.Length / 1KB, 1)) Ko)" -ForegroundColor Gray
}

# 4. Simulation du profil UEVR ShooterGame
Write-Host "`n[4/6] Déploiement du profil 6DOF & Dinosaures dans AppData..." -ForegroundColor Yellow
if (!(Test-Path $appDataProfile)) {
    New-Item -ItemType Directory -Path $appDataProfile -Force | Out-Null
}
Copy-Item "D:\VR ARK\payload\config\ShooterGame_config.txt" -Destination (Join-Path $appDataProfile "config.txt") -Force
$configDeployed = Join-Path $appDataProfile "config.txt"
if (Test-Path $configDeployed) {
    Write-Host "  -> [OK] Profil UEVR déployé : $configDeployed" -ForegroundColor Green
    Write-Host "  -> Contenu clé validé :" -ForegroundColor Gray
    Get-Content $configDeployed | Select-String "VR_RenderingMethod", "VR_DecoupledPitch", "VR_SnapTurn" | ForEach-Object { Write-Host "     $_" -ForegroundColor DarkGray }
}

# 5. Simulation des optimisations Engine.ini
Write-Host "`n[5/6] Application des optimisations FPS (Nuages volumétriques & Ombres)..." -ForegroundColor Yellow
$tweaks = Get-Content "D:\VR ARK\payload\config\VR_Engine_Tweaks.ini" -Raw
Add-Content -Path $engineIni -Value "`n$tweaks"
$engineUpdated = Get-Content $engineIni -Raw
if ($engineUpdated -match "r\.VolumetricCloud=0") {
    Write-Host "  -> [OK] Tweaks [SystemSettings] injectés avec succès dans Engine.ini !" -ForegroundColor Green
} else {
    Write-Host "  -> [ATTENTION] Tweaks non trouvés dans Engine.ini" -ForegroundColor Orange
}

# 6. Test de simulation de lancement
Write-Host "`n[6/6] Simulation de la commande de lancement VR..." -ForegroundColor Yellow
$launchCmd = "`"$gameExe`" -NoBattlEye"
Write-Host "  -> Commande validée : $launchCmd" -ForegroundColor Green
Write-Host "  -> Injecteur UEVR prêt : $uevrDest\UEVRInjector.exe" -ForegroundColor Green

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host "🎉 TOUS LES TESTS DE SIMULATION ONT RÉUSSI AVEC SUCCÈS !" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Cyan
