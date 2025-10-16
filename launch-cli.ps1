# Launch WinShell CLI (Self-Contained - No .NET Required)
param([switch]$Force)

Set-Location $PSScriptRoot

# Check for existing WinShell CLI processes
$existingProcesses = Get-Process -Name "WinShell.CLI" -ErrorAction SilentlyContinue

if ($existingProcesses -and !$Force) {
    Write-Host "WinShell CLI is already running. Do you want to:" -ForegroundColor Yellow
    Write-Host "1. Close existing instances and start new one" -ForegroundColor Cyan
    Write-Host "2. Cancel" -ForegroundColor Cyan
    $choice = Read-Host "Enter your choice (1 or 2)"
    
    if ($choice -eq "1") {
        Write-Host "Closing existing WinShell CLI instances..." -ForegroundColor Yellow
        $existingProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    } else {
        Write-Host "Launch cancelled." -ForegroundColor Red
        exit
    }
} elseif ($existingProcesses -and $Force) {
    Write-Host "Force mode: Closing existing WinShell CLI instances..." -ForegroundColor Yellow
    $existingProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
}

# Launch the CLI directly (no build needed - self-contained)
Write-Host "Launching WinShell CLI..." -ForegroundColor Green
& "$PSScriptRoot\cli\WinShell.CLI.exe"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to launch WinShell CLI." -ForegroundColor Red
    Read-Host "Press Enter to continue..."
}