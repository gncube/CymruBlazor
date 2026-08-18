param(
  [string]$RootPath = ".",
  [string]$OutZip,
  [string[]]$Targets = @(
    ".github",
    "docs",
    "plan",
    "samples",
    "src",
    "tests",
    ".editorconfig",
    ".gitignore",
    "CHANGELOG.md",
    "Directory.Build.props",
    "Directory.Build.targets",
    "Directory.Packages.props",
    "LICENSE",
    "*.sln",
    "*.slnx",
    "README.md",
    "MIGRATION_CLEANUP.md",
    "Pack-Solution.ps1"
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

# Resolve any wildcards in the Target parameter, expanding directories into individual files
$resolvedTargets = @()
foreach ($t in $Targets) {
  if ($t -like "*\*" -or $t -like "*/*" -or $t -like "*.*" -or $t -like "*`*") {
    # It might be a wildcard pattern
    $items = Get-ChildItem -Path $Root -Filter $t -ErrorAction SilentlyContinue
    foreach ($item in $items) {
      if ($item.PSIsContainer) {
        # Expand directories to individual files
        $resolvedTargets += Get-ChildItem -Path $item.FullName -Recurse -File | ForEach-Object { $_.FullName }
      } else {
        $resolvedTargets += $item.FullName
      }
    }
  } elseif (Test-Path $t) {
    $resolved = (Resolve-Path -LiteralPath $t).ProviderPath
    $item = Get-Item -LiteralPath $resolved
    if ($item.PSIsContainer) {
      # Expand directories to individual files
      $resolvedTargets += Get-ChildItem -Path $resolved -Recurse -File | ForEach-Object { $_.FullName }
    } else {
      $resolvedTargets += $resolved
    }
  }
}
$resolvedTargets = $resolvedTargets | Select-Object -Unique

# 2. Filter out bin/ and obj/ files BEFORE deleting directories
$resolvedTargets = $resolvedTargets | Where-Object {
  $path = $_
  -not ($path -match '\\bin\\' -or $path -match '\\obj\\' -or $path -match '/bin/' -or $path -match '/obj/')
}

# 3. Clean bin and obj folders
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

# 4. Resolve exclusions
$excludeResolved = @()
foreach ($e in $Excludes) {
  # Handle **/filename patterns by extracting the filename and searching recursively
  if ($e -like "*/*" -or $e -like "*\*") {
    # Extract the filename part after the last / or \
    $filename = $e -replace '^.*[/\\]', ''
    $excludeResolved += Get-ChildItem -Path $Root -Filter $filename -Recurse -File -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }
  } else {
    # Direct filename or simple path
    $excludeResolved += Get-ChildItem -Path $Root -Filter $e -Recurse -File -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }
  }
}

# Also exclude *.lscache files
$lscacheFiles = Get-ChildItem -Path $Root -Filter *.lscache -File -Recurse -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }
$excludeResolved += $lscacheFiles
$excludeResolved = $excludeResolved | Select-Object -Unique

# 5. Build include list of existing items, excluding sensitive files
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

# 6. Compress with folder structure preservation
if (Test-Path $OutZip) { Remove-Item $OutZip -Force }
Write-Host "Creating zip: $OutZip"

# Create a temporary staging directory using the solution name
$stagingDirName = if ($solutionFile) {
  $solutionFile.BaseName + "-staging"
} else {
  ".pack-staging"
}
$stagingDir = Join-Path $Root $stagingDirName
if (Test-Path $stagingDir) { Remove-Item $stagingDir -Recurse -Force }
New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null

# Copy files to staging area, preserving structure
foreach ($file in $existing) {
  # Compute relative path
  $relativePath = $file -replace [regex]::Escape($Root + '\'), ''
  $targetPath = Join-Path $stagingDir $relativePath
  $targetDir = Split-Path $targetPath -Parent

  # Create directory structure if needed
  if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
  }

  # Copy file
  Copy-Item -LiteralPath $file -Destination $targetPath -Force
}

# Compress the staging directory
Compress-Archive -Path $stagingDir -DestinationPath $OutZip -Force

# Clean up staging directory
Remove-Item $stagingDir -Recurse -Force

Write-Host "Created $OutZip at $Root"
Pop-Location

