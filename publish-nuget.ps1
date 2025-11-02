#!/usr/bin/env pwsh
# ============================================================================
# WinShell NuGet Publishing Script
# ============================================================================
# This script publishes WinShell CLI to NuGet.org
# Usage: .\publish-nuget.ps1 -ApiKey YOUR_API_KEY [-Version 1.0.0]
# ============================================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ApiKey,
    
    [string]$Version = "1.0.0",
    
    [string]$Source = "https://api.nuget.org/v3/index.json",
    
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

# Colors
function Write-Step { param($Message) Write-Host "🔹 $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Error-Custom { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Warning-Custom { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║          WinShell NuGet Publishing Script                 ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
Write-Host ""

$RootDir = $PSScriptRoot
$PackagesDir = Join-Path $RootDir "packages"
$ProjectFile = Join-Path $RootDir "winshell.cli\WinShell.CLI.csproj"

# ============================================================================
# Step 1: Validate API Key
# ============================================================================
if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    Write-Error-Custom "API Key is required!"
    Write-Host "Get your API key from: https://www.nuget.org/account/apikeys" -ForegroundColor Yellow
    exit 1
}

if ($ApiKey -eq "YOUR_API_KEY") {
    Write-Error-Custom "Please replace 'YOUR_API_KEY' with your actual NuGet API key"
    Write-Host "Get your API key from: https://www.nuget.org/account/apikeys" -ForegroundColor Yellow
    exit 1
}

Write-Success "API key validated"

# ============================================================================
# Step 2: Clean and Restore
# ============================================================================
Write-Step "Cleaning previous builds..."

if (Test-Path $PackagesDir) {
    Remove-Item $PackagesDir -Recurse -Force
}
New-Item -ItemType Directory -Path $PackagesDir -Force | Out-Null

dotnet clean $ProjectFile --configuration Release | Out-Null
Write-Success "Clean completed"

Write-Step "Restoring dependencies..."
dotnet restore $ProjectFile
if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Restore failed"
    exit 1
}
Write-Success "Dependencies restored"

# ============================================================================
# Step 3: Build
# ============================================================================
Write-Step "Building project..."

dotnet build $ProjectFile --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Build failed"
    exit 1
}
Write-Success "Build completed"

# ============================================================================
# Step 4: Create NuGet Package
# ============================================================================
Write-Step "Creating NuGet package (version $Version)..."

dotnet pack $ProjectFile `
    --configuration Release `
    --no-build `
    --output $PackagesDir `
    /p:Version=$Version

if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Pack failed"
    exit 1
}

$nupkgFile = Get-ChildItem -Path $PackagesDir -Filter "*.nupkg" | Select-Object -First 1

if (-not $nupkgFile) {
    Write-Error-Custom "Package file not found!"
    exit 1
}

Write-Success "Package created: $($nupkgFile.Name)"

# ============================================================================
# Step 5: Validate Package
# ============================================================================
Write-Step "Validating package..."

$packageSize = [math]::Round($nupkgFile.Length / 1MB, 2)
Write-Host "   Package Size: $packageSize MB" -ForegroundColor White

if ($packageSize -gt 500) {
    Write-Warning-Custom "Package is larger than 500 MB! Consider reducing size."
}

# ============================================================================
# Step 6: Publish to NuGet (or Dry Run)
# ============================================================================
if ($DryRun) {
    Write-Warning-Custom "DRY RUN MODE - Package NOT published"
    Write-Host ""
    Write-Host "Package ready for publishing:" -ForegroundColor Cyan
    Write-Host "   File: $($nupkgFile.FullName)" -ForegroundColor White
    Write-Host "   Size: $packageSize MB" -ForegroundColor White
    Write-Host ""
    Write-Host "To publish for real, run without -DryRun flag:" -ForegroundColor Yellow
    Write-Host "   .\publish-nuget.ps1 -ApiKey YOUR_API_KEY -Version $Version" -ForegroundColor Gray
} else {
    Write-Step "Publishing to NuGet.org..."
    Write-Warning-Custom "This will make the package publicly available!"
    
    Write-Host ""
    Write-Host "Publishing: $($nupkgFile.Name)" -ForegroundColor Yellow
    Write-Host "To: $Source" -ForegroundColor Yellow
    Write-Host ""
    
    $confirmation = Read-Host "Are you sure you want to publish? (yes/no)"
    
    if ($confirmation -eq "yes") {
        dotnet nuget push $nupkgFile.FullName `
            --api-key $ApiKey `
            --source $Source `
            --skip-duplicate
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Success "Package published successfully!"
            Write-Host ""
            Write-Host "📦 Package Information:" -ForegroundColor Cyan
            Write-Host "   Package ID: WinShell.CLI" -ForegroundColor White
            Write-Host "   Version: $Version" -ForegroundColor White
            Write-Host "   NuGet URL: https://www.nuget.org/packages/WinShell.CLI/$Version" -ForegroundColor White
            Write-Host ""
            Write-Host "🎉 Users can now install with:" -ForegroundColor Green
            Write-Host "   dotnet tool install --global WinShell.CLI --version $Version" -ForegroundColor Gray
            Write-Host ""
            Write-Host "⏰ Note: It may take a few minutes for the package to appear in search results." -ForegroundColor Yellow
        } else {
            Write-Error-Custom "Publishing failed! Check the error message above."
            exit 1
        }
    } else {
        Write-Warning-Custom "Publishing cancelled by user"
        exit 0
    }
}

Write-Host ""
Write-Host "✨ Script completed at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Green
Write-Host ""
