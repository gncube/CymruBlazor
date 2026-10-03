[CmdletBinding()]
param(
    [string]$PublishPath = "/workspace/.artifacts/demo-publish",
    [string]$ReportDir = "/workspace/.artifacts/accessibility"
)

$ErrorActionPreference = "Stop"

$mountedHostCa = "/usr/local/share/ca-certificates/cymru-host-root.crt"
if (Test-Path $mountedHostCa) {
    update-ca-certificates | Out-Host
}

Write-Host "Creating report destination directory: $ReportDir"
if (-not (Test-Path $ReportDir)) {
    New-Item -ItemType Directory -Path $ReportDir -Force | Out-Null
}

$env:CYMRU_SMOKE_REPORT_DIR = $ReportDir

Write-Host "Restoring solution dependencies..."
$env:NUGET_CERT_REVOCATION_MODE = "offline"
dotnet restore --verbosity minimal

Write-Host "Publishing CymruBlazor.Demo (isolated to prevent NETSDK1152 duplicate asset collision)..."
dotnet publish src/CymruBlazor.Demo/CymruBlazor.Demo.csproj -c Release -o $PublishPath --no-restore

$wwwroot = Join-Path $PublishPath "wwwroot"
if (-not (Test-Path $wwwroot)) {
    throw "Published wwwroot directory not found at $wwwroot"
}

$env:CYMRU_DEMO_DIR = $wwwroot
$env:CI = "true"

Write-Host "Executing the full accessibility test project with CYMRU_DEMO_DIR=$env:CYMRU_DEMO_DIR..."
dotnet test tests/CymruBlazor.AccessibilityTests/CymruBlazor.AccessibilityTests.csproj `
    -c Release `
    --no-restore `
    --logger "console;verbosity=normal"

$testExitCode = $LASTEXITCODE

if ($testExitCode -ne 0) {
    throw "Accessibility smoke tests failed with exit code $testExitCode"
}

Write-Host "Accessibility test suite completed successfully."
