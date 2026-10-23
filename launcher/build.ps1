param([switch]$NoCopy)
$ErrorActionPreference = 'Stop'
$dotnet = if (Test-Path 'C:\Program Files\dotnet\dotnet.exe') { 'C:\Program Files\dotnet\dotnet.exe' } else { (Get-Command dotnet).Source }
Push-Location $PSScriptRoot
try {
    & $dotnet publish 'RelayLauncher.csproj' -c Release -o 'publish' --nologo
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }
    if (-not $NoCopy) {
        Copy-Item (Join-Path $PSScriptRoot 'publish\RelayLauncher.exe') (Join-Path (Split-Path $PSScriptRoot -Parent) 'RelayLauncher.exe') -Force
    }
    Write-Host 'Relay launcher rebuilt.'
}
finally {
    Pop-Location
}