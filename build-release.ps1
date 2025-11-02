#!/usr/bin/env pwsh
# ============================================================================
# WinShell Release Build Script
# ============================================================================
# This script builds both CLI and GUI versions of WinShell and creates
# distribution packages for GitHub Releases and NuGet.
# ============================================================================

param(
    [string]$Version = "1.0.0",
    [switch]$SkipTests,
    [switch]$CreateNuGetPackage,
    [switch]$CreateZipPackages
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Colors for output
function Write-Step { param($Message) Write-Host "🔹 $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Error-Custom { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Yellow }

# ============================================================================
# Configuration
# ============================================================================
$RootDir = $PSScriptRoot
$OutputDir = Join-Path $RootDir "releases"
$PackagesDir = Join-Path $RootDir "packages"
$SolutionFile = Join-Path $RootDir "WinShell.sln"

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║          WinShell Release Build Script v1.0.0             ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
Write-Host ""

# ============================================================================
# Step 1: Clean Previous Builds
# ============================================================================
Write-Step "Cleaning previous builds..."

if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}
if (Test-Path $PackagesDir) {
    Remove-Item $PackagesDir -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
New-Item -ItemType Directory -Path $PackagesDir -Force | Out-Null

dotnet clean $SolutionFile --configuration Release | Out-Null

Write-Success "Clean completed"

# ============================================================================
# Step 2: Restore Dependencies
# ============================================================================
Write-Step "Restoring NuGet packages..."

dotnet restore $SolutionFile
if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Failed to restore packages"
    exit 1
}

Write-Success "Dependencies restored"

# ============================================================================
# Step 3: Build Solution
# ============================================================================
Write-Step "Building solution in Release mode..."

dotnet build $SolutionFile --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Build failed"
    exit 1
}

Write-Success "Build completed successfully"

# ============================================================================
# Step 4: Run Tests (if available)
# ============================================================================
if (-not $SkipTests) {
    Write-Step "Running tests..."
    
    $testProjects = Get-ChildItem -Path $RootDir -Recurse -Filter "*.Tests.csproj"
    
    if ($testProjects.Count -gt 0) {
        dotnet test $SolutionFile --configuration Release --no-build --verbosity quiet
        if ($LASTEXITCODE -ne 0) {
            Write-Error-Custom "Tests failed"
            exit 1
        }
        Write-Success "All tests passed"
    } else {
        Write-Info "No test projects found, skipping tests"
    }
}

# ============================================================================
# Step 5: Create NuGet Package for CLI
# ============================================================================
if ($CreateNuGetPackage) {
    Write-Step "Creating NuGet package for CLI..."
    
    $cliProject = Join-Path $RootDir "winshell.cli\WinShell.CLI.csproj"
    
    dotnet pack $cliProject `
        --configuration Release `
        --no-build `
        --output $PackagesDir `
        /p:Version=$Version
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "NuGet package creation failed"
        exit 1
    }
    
    $nupkgFile = Get-ChildItem -Path $PackagesDir -Filter "*.nupkg" | Select-Object -First 1
    Write-Success "NuGet package created: $($nupkgFile.Name)"
    Write-Info "To publish to NuGet.org, run:"
    Write-Info "  dotnet nuget push `"$($nupkgFile.FullName)`" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json"
}

# ============================================================================
# Step 6: Publish CLI (Framework-Dependent)
# ============================================================================
Write-Step "Publishing CLI for framework-dependent deployment..."

$cliOutputDir = Join-Path $OutputDir "WinShell-CLI-v$Version-win-x64"

dotnet publish (Join-Path $RootDir "winshell.cli\WinShell.CLI.csproj") `
    --configuration Release `
    --no-build `
    --output $cliOutputDir `
    --runtime win-x64 `
    --no-self-contained

if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "CLI publish failed"
    exit 1
}

Write-Success "CLI published to: $cliOutputDir"

# ============================================================================
# Step 7: Publish GUI (Framework-Dependent)
# ============================================================================
Write-Step "Publishing GUI for framework-dependent deployment..."

$guiOutputDir = Join-Path $OutputDir "WinShell-GUI-v$Version-win-x64"

dotnet publish (Join-Path $RootDir "winshell.gui\WinShell.GUI.csproj") `
    --configuration Release `
    --no-build `
    --output $guiOutputDir `
    --runtime win-x64 `
    --no-self-contained

if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "GUI publish failed"
    exit 1
}

Write-Success "GUI published to: $guiOutputDir"

# ============================================================================
# Step 8: Create ZIP Packages
# ============================================================================
if ($CreateZipPackages) {
    Write-Step "Creating ZIP packages..."
    
    # Ensure output directories exist
    if (-not (Test-Path $cliOutputDir)) {
        Write-Error-Custom "CLI output directory not found: $cliOutputDir"
        exit 1
    }
    if (-not (Test-Path $guiOutputDir)) {
        Write-Error-Custom "GUI output directory not found: $guiOutputDir"
        exit 1
    }
    
    # CLI ZIP
    $cliZipPath = Join-Path $OutputDir "WinShell-CLI-v$Version-win-x64.zip"
    if (Test-Path $cliZipPath) {
        Remove-Item $cliZipPath -Force
    }
    Compress-Archive -Path "$cliOutputDir\*" -DestinationPath $cliZipPath -Force
    $cliZipName = Split-Path $cliZipPath -Leaf
    Write-Success "CLI ZIP created: $cliZipName"
    
    # GUI ZIP
    $guiZipPath = Join-Path $OutputDir "WinShell-GUI-v$Version-win-x64.zip"
    if (Test-Path $guiZipPath) {
        Remove-Item $guiZipPath -Force
    }
    Compress-Archive -Path "$guiOutputDir\*" -DestinationPath $guiZipPath -Force
    $guiZipName = Split-Path $guiZipPath -Leaf
    Write-Success "GUI ZIP created: $guiZipName"
    
    # Combined ZIP with both CLI and GUI
    $combinedDir = Join-Path $OutputDir "WinShell-Complete-v$Version-win-x64"
    if (Test-Path $combinedDir) {
        Remove-Item $combinedDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $combinedDir -Force | Out-Null
    New-Item -ItemType Directory -Path "$combinedDir\CLI" -Force | Out-Null
    New-Item -ItemType Directory -Path "$combinedDir\GUI" -Force | Out-Null
    
    Copy-Item "$cliOutputDir\*" "$combinedDir\CLI\" -Recurse -Force
    Copy-Item "$guiOutputDir\*" "$combinedDir\GUI\" -Recurse -Force
    
    # Copy README and LICENSE if they exist
    if (Test-Path (Join-Path $RootDir "README.md")) {
        Copy-Item (Join-Path $RootDir "README.md") $combinedDir -Force
    }
    if (Test-Path (Join-Path $RootDir "LICENSE")) {
        Copy-Item (Join-Path $RootDir "LICENSE") $combinedDir -Force
    }
    
    $combinedZipPath = Join-Path $OutputDir "WinShell-Complete-v$Version-win-x64.zip"
    if (Test-Path $combinedZipPath) {
        Remove-Item $combinedZipPath -Force
    }
    Compress-Archive -Path "$combinedDir\*" -DestinationPath $combinedZipPath -Force
    $combinedZipName = Split-Path $combinedZipPath -Leaf
    Write-Success "Complete ZIP created: $combinedZipName"
}

# ============================================================================
# Step 9: Generate Checksums
# ============================================================================
Write-Step "Generating checksums..."

$checksumFile = Join-Path $OutputDir "checksums.txt"
$zipFiles = Get-ChildItem -Path $OutputDir -Filter "*.zip"

if ($zipFiles.Count -gt 0) {
    foreach ($zip in $zipFiles) {
        $hash = (Get-FileHash -Path $zip.FullName -Algorithm SHA256).Hash
        "$hash  $($zip.Name)" | Out-File -FilePath $checksumFile -Append -Encoding UTF8
    }
    Write-Success "Checksums generated: checksums.txt"
}

# ============================================================================
# Step 10: Summary
# ============================================================================
Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║              Build Completed Successfully!                ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

Write-Host "📦 Build Artifacts:" -ForegroundColor Cyan
Write-Host "   Output Directory: $OutputDir" -ForegroundColor White

if ($CreateNuGetPackage) {
    Write-Host "   NuGet Package:    $PackagesDir" -ForegroundColor White
}

Write-Host ""
Write-Host "📋 Next Steps:" -ForegroundColor Cyan

if ($CreateNuGetPackage) {
    Write-Host "   1. Publish to NuGet.org:" -ForegroundColor Yellow
    $nupkgFile = Get-ChildItem -Path $PackagesDir -Filter "*.nupkg" | Select-Object -First 1
    Write-Host "      dotnet nuget push `"$($nupkgFile.FullName)`" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json" -ForegroundColor Gray
    Write-Host ""
}

if ($CreateZipPackages) {
    Write-Host "   2. Create GitHub Release:" -ForegroundColor Yellow
    Write-Host "      - Go to: https://github.com/23f2003700/WinShell/releases/new" -ForegroundColor Gray
    Write-Host "      - Tag: v$Version" -ForegroundColor Gray
    Write-Host "      - Upload ZIP files from: $OutputDir" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "   3. Test the builds:" -ForegroundColor Yellow
Write-Host "      CLI: $cliOutputDir\WinShell.CLI.exe" -ForegroundColor Gray
Write-Host "      GUI: $guiOutputDir\WinShell.GUI.exe" -ForegroundColor Gray
Write-Host ""

Write-Host "✨ Build script completed at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Green
Write-Host ""
