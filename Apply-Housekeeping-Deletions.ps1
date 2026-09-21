<#
  Removes the files/folders identified as redundant in the pre-v1.4.0 housekeeping pass.
  Run from the repository root, AFTER extracting housekeeping-changes.zip over the repo.
  Preview first:  .\Apply-Housekeeping-Deletions.ps1 -WhatIf
  Safe to re-run (missing items are skipped).
  Works in PowerShell Constrained Language Mode (no .NET method calls).
#>
[CmdletBinding(SupportsShouldProcess)]
param()

$files = @(
  'MIGRATION_CLEANUP.md',                                   # completed .github agent-migration note
  'tests/CymruBlazor.Tests/Components/ButtonTests.cs',      # superseded by Components/Button/CyButtonTests.cs
  'src/CymruBlazor/build/BundleCss.props',                  # orphaned; cymrublazor.css @imports are the single list
  'src/CymruBlazor/wwwroot/background.png',                 # unreferenced Razor Class Library template asset
  'src/CymruBlazor.Demo/Shared/DemoWorkbench.razor'         # superseded by inlined workbench markup on 29 pages
)
$emptyDirs = @('demo-publish', 'demo-smoke', 'test-results') # local output folders (git-ignored)

foreach ($f in $files) {
  if (Test-Path -LiteralPath $f) {
    # -WhatIf on this script is inherited by Remove-Item automatically.
    Remove-Item -LiteralPath $f -Force
    Write-Output "processed $f"
  }
  else { Write-Output "skip (not found) $f" }
}

foreach ($d in $emptyDirs) {
  if (Test-Path -LiteralPath $d) {
    $hasContent = Get-ChildItem -LiteralPath $d -Force | Select-Object -First 1
    if ($hasContent) { Write-Output "skip (not empty) $d" }
    else { Remove-Item -LiteralPath $d -Force; Write-Output "processed $d/" }
  }
}
