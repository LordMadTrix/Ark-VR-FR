$ErrorActionPreference = "Stop"
$PSScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
$payloadDir = Join-Path $PSScriptRoot "payload"
$outputZip = Join-Path $PSScriptRoot "ArkVRInstaller\payload.zip"

if (Test-Path $outputZip) {
    Remove-Item $outputZip -Force
}

Write-Host "Compression du dossier payload vers $outputZip..."
[System.IO.Compression.ZipFile]::CreateFromDirectory($payloadDir, $outputZip, [System.IO.Compression.CompressionLevel]::Optimal, $false)
Write-Host "Compression terminée avec succès !"
Get-Item $outputZip | Select-Object Name, Length
