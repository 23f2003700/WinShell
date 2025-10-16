@echo off
setlocal enabledelayedexpansion

cd /d "%~dp0"

echo Checking for existing WinShell CLI processes...
tasklist /FI "IMAGENAME eq WinShell.CLI.exe" 2>NUL | find /I /N "WinShell.CLI.exe">NUL
if %ERRORLEVEL%==0 (
    echo WinShell CLI is already running. Closing existing instances...
    taskkill /F /IM "WinShell.CLI.exe" /T >NUL 2>&1
    timeout /t 2 >NUL
)

echo Launching WinShell CLI (Self-Contained - No .NET Required)...
start "" "%~dp0cli\WinShell.CLI.exe"

if !ERRORLEVEL! neq 0 (
    echo Failed to launch WinShell CLI.
    pause
)