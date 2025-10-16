# Quick Launch WinShell CLI (Direct Executable)
param([switch]$Force)

Set-Location $PSScriptRoot

# Check for existing processes and handle them
$existingProcesses = Get-Process -Name "WinShell.CLI" -ErrorAction SilentlyContinue
if ($existingProcesses) {
    if ($Force) {
        Write-Host "Force mode: Closing existing instances..." -ForegroundColor Yellow
        $existingProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 1
    } else {
        Write-Host "WinShell CLI is already running. Use -Force to close existing instances." -ForegroundColor Yellow
        exit
    }
}

# Check if executable exists
$exePath = ".\winshell.cli\bin\Debug\net6.0-windows\WinShell.CLI.exe"
if (!(Test-Path $exePath)) {
    Write-Host "Executable not found. Building project..." -ForegroundColor Yellow
    & dotnet build winshell.cli\WinShell.CLI.csproj --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed." -ForegroundColor Red
        exit 1
    }
}

# Launch directly
Write-Host "Launching WinShell CLI (Direct)..." -ForegroundColor Green
& $exePath