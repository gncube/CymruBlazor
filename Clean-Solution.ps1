# Get the directory where the script is located (or set a specific path)
$TargetDir = Get-Location

Write-Host "Starting cleanup in: $TargetDir" -ForegroundColor Cyan

# Find and remove all 'bin' and 'obj' directories, ignoring .git folders
Get-ChildItem -Path $TargetDir -Recurse -Directory -Depth 5 |
    Where-Object {
        ($_.Name -eq "bin" -or $_.Name -eq "obj") -and
        $_.FullName -notlike "*\.git\*"
    } |
    ForEach-Object {
        Write-Host "Deleting: $($_.FullName)" -ForegroundColor Yellow
        try {
            Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction Stop
        }
        catch {
            Write-Warning "Could not delete $($_.FullName). It might be locked by Visual Studio or another process."
        }
    }

Write-Host "Cleanup complete!" -ForegroundColor Green
