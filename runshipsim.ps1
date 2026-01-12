# PowerShell script to run SPHDEM_Simulation from any directory
param(
    [string]$ProjectDir = "$(Get-Location)"
)

$simPath = Join-Path $ProjectDir "SPHDEM_Simulation"
if (!(Test-Path $simPath)) {
    Write-Error "SPHDEM_Simulation directory not found in $ProjectDir"
    exit 1
}

Write-Host "Running SPHDEM_Simulation..."
dotnet run --project $simPath
