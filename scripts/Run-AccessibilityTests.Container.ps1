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

# The repository is bind-mounted from the host, so its bin/ and obj/ folders hold build and restore output
# written on the host (Windows paths, Visual Studio's fallback package folder). Building from them inside this
# Linux container fails in ResolvePackageAssets, and building here would also overwrite the host's restore state.
# So build from a clean copy that leaves bin/ and obj/ behind. Reports and the published site still go to
# /workspace/.artifacts, which is the host-visible location.
$source = "/workspace"
$work = "/tmp/cymru-build"
Write-Host "Copying sources to $work (without bin/obj)..."
if (Test-Path $work) { Remove-Item $work -Recurse -Force }
New-Item -ItemType Directory -Path $work -Force | Out-Null
tar -C $source --exclude=./.git --exclude=./.artifacts --exclude=bin --exclude=obj --exclude=node_modules -cf - . | tar -C $work -xf -
if ($LASTEXITCODE -ne 0) { throw "Copying sources failed with exit code $LASTEXITCODE" }
# The browser tests find the repository root by looking for a .git directory, which was left out of the copy.
New-Item -ItemType Directory -Path (Join-Path $work ".git") -Force | Out-Null
Set-Location $work

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
