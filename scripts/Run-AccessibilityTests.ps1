[CmdletBinding()]
param(
    [string]$ImageTag = "cymrublazor-playwright:1.62.0"
)

$ErrorActionPreference = "Stop"

Write-Host "Checking Docker daemon status..."
try {
    docker info | Out-Null
}
catch {
    Write-Error "Docker daemon is not running or accessible. Please start Docker Desktop or dockerd."
    exit 1
}

$repoRoot = (Resolve-Path "$PSScriptRoot/..").Path
$artifactDir = Join-Path $repoRoot ".artifacts/accessibility"
$hostCaCertificateDer = Join-Path $env:TEMP "cymrublazor-host-ca.cer"
$hostCaCertificate = Join-Path $env:TEMP "cymrublazor-host-ca.crt"
$trustedHostCas = @(
    Get-ChildItem Cert:\CurrentUser\Root, Cert:\CurrentUser\CA, Cert:\LocalMachine\Root, Cert:\LocalMachine\CA |
        Where-Object { $_.Subject -like "*Zscaler*" -or $_.Issuer -like "*Zscaler*" } |
        Sort-Object Thumbprint -Unique
)

if ($trustedHostCas.Count -gt 0) {
    Remove-Item $hostCaCertificate -ErrorAction SilentlyContinue
    foreach ($trustedHostCa in $trustedHostCas) {
        Export-Certificate -Cert $trustedHostCa -FilePath $hostCaCertificateDer -Type CERT -Force | Out-Null
        $encodedCertificate = Join-Path $env:TEMP "cymrublazor-host-ca-$($trustedHostCa.Thumbprint).crt"
        certutil.exe -encode $hostCaCertificateDer $encodedCertificate | Out-Null
        Get-Content $encodedCertificate | Add-Content $hostCaCertificate
        Remove-Item $encodedCertificate -Force
    }
}

if (-not (Test-Path $artifactDir)) {
    New-Item -ItemType Directory -Path $artifactDir -Force | Out-Null
}

Write-Host "Building Docker image [$ImageTag]..."
docker build -t $ImageTag -f "$repoRoot/docker/playwright/Dockerfile" "$repoRoot"

Write-Host "Running containerized Playwright accessibility tests..."
docker run --rm `
    --init `
    --ipc=host `
    -v "${repoRoot}:/workspace" `
    -w /workspace `
    -e GITHUB_STEP_SUMMARY `
    $(if (Test-Path $hostCaCertificate) { "-v"; "${hostCaCertificate}:/usr/local/share/ca-certificates/cymru-host-root.crt:ro" }) `
    $ImageTag `
    -File ./scripts/Run-AccessibilityTests.Container.ps1

$exitCode = $LASTEXITCODE

if ($exitCode -ne 0) {
    Write-Error "Container test execution failed with exit code $exitCode."
    exit $exitCode
}

Write-Host "Accessibility report generated at: $artifactDir/demo-smoke-report.md"
