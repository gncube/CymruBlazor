param(
  [string]$RootPath = ".",
  [string]$OutZip,
  [string[]]$Targets = @(
    ".github",
    ".template.config",
    ".vscode",
    "infra",
    "pack",
    "plan",
    "requests",
    "src",
    "tests",
    ".editconfig",
    ".editorconfig",
    ".gitignore",
    ".template-version",    
    "Directory.Build.props",
    "Directory.Build.targets",
    "Directory.Packages.props",
    "*.yml",
    "*.md",
    "*.ps1",
    "*.json",
    "*.sln",
    "*.slnx"
  ),
  [string[]]$Excludes = @("**/local.settings.json", "**/appsettings.Development.json"),
  [switch]$DryRun
)

$Root = (Resolve-Path -Path $RootPath).ProviderPath
Push-Location $Root

# 1. Determine dynamic OutZip name if not specified
if ($null -eq $OutZip -or $OutZip -eq "") {
  # Find any .sln or .slnx files in the root
  $solutionFile = Get-ChildItem -Path $Root -Filter *.slnx -File | Select-Object -First 1
  if ($null -eq $solutionFile) {
    $solutionFile = Get-ChildItem -Path $Root -Filter *.sln -File | Select-Object -First 1
  }

  if ($null -ne $solutionFile) {
    $OutZip = $solutionFile.Name -replace '\.slnx?$', '.zip'
  } else {
    $OutZip = "Archive.zip"
  }
}

# Resolve any wildcards in the Target parameter
$resolvedTargets = @()
foreach ($t in $Targets) {
  if ($t -like "*\*" -or $t -like "*/*" -or $t -like "*.*" -or $t -like "*`*") {
    # It might be a wildcard pattern
    $resolvedTargets += Get-ChildItem -Path $Root -Filter $t -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }
  } elseif (Test-Path $t) {
    $resolvedTargets += (Resolve-Path -LiteralPath $t).ProviderPath
  }
}
$resolvedTargets = $resolvedTargets | Select-Object -Unique

# 2. Clean bin and obj folders
Write-Host "Scanning for 'bin' and 'obj' directories under $Root..."
$dirs = Get-ChildItem -Path $Root -Directory -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -in @('bin','obj') }

if ($dirs.Count -eq 0) {
  Write-Host "No 'bin' or 'obj' directories found."
} elseif ($DryRun) {
  $dirs | ForEach-Object { Write-Host "Would remove: $($_.FullName)" }
} else {
  $dirs | ForEach-Object {
    Write-Host "Removing: $($_.FullName)"
    Remove-Item -LiteralPath $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
  }
}

# 3. Resolve exclusions
$excludeResolved = @()
foreach ($e in $Excludes) {
  # Accept wildcards or direct paths
  $excludeResolved += Get-ChildItem -Path $Root -Filter $e -Recurse -File -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }
}

# Also exclude *.lscache files
$lscacheFiles = Get-ChildItem -Path $Root -Filter *.lscache -File -Recurse -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }
$excludeResolved += $lscacheFiles
$excludeResolved = $excludeResolved | Select-Object -Unique

# 4. Build include list of existing items, excluding sensitive files
$existing = @()
foreach ($t in $resolvedTargets) {
  if ($excludeResolved -contains $t) {
    Write-Host "Excluding sensitive/cached file: $t"
  } else {
    $existing += $t
  }
}
$existing = $existing | Select-Object -Unique

if ($existing.Count -eq 0) {
  Write-Host "Nothing to include in the zip. Exiting."
  Pop-Location
  exit 1
}

if ($DryRun) {
  Write-Host "Would create zip ($OutZip) containing:"
  $existing | ForEach-Object { Write-Host "  $_" }
  Pop-Location
  exit 0
}

# 5. Compress
if (Test-Path $OutZip) { Remove-Item $OutZip -Force }
Write-Host "Creating zip: $OutZip"
Compress-Archive -Path $existing -DestinationPath $OutZip -Force
Write-Host "Created $OutZip at $Root"
Pop-Location

