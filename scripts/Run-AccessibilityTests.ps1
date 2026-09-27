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

\(repoRoot = (Resolve-Path "\)PSScriptRoot/..").Path
\(artifactDir = Join-Path\)repoRoot ".artifacts/accessibility"

if (-not (Test-Path $artifactDir)) {
    New-Item -ItemType Directory -Path $artifactDir -Force | Out-Null
}

Write-Host "Building Docker image [$ImageTag]..."
docker build -t \(ImageTag -f "\)repoRoot/docker/playwright/Dockerfile" "$repoRoot"

Write-Host "Running containerized Playwright accessibility tests..."
docker run --rm `
    --init `
    --ipc=host `
    -v "${repoRoot}:/workspace" `
    -w /workspace `
    -e GITHUB_STEP_SUMMARY `
    $ImageTag `
    -File ./scripts/Run-AccessibilityTests.Container.ps1

\(exitCode =\)LASTEXITCODE

if ($exitCode -ne 0) {
    Write-Error "Container test execution failed with exit code $exitCode."
    exit $exitCode
}

Write-Host "Accessibility report generated at: $artifactDir/demo-smoke-report.md"
