@echo off
title WinShell Installation Test
echo.
echo =============================================
echo  WinShell Installation Test
echo =============================================
echo.

echo Testing WinShell CLI installation...
echo.

REM Check if WinShell CLI is in PATH
where WinShell.CLI.exe >nul 2>&1
if %errorlevel%==0 (
    echo ✅ WinShell CLI found in PATH
    echo    Location: 
    where WinShell.CLI.exe
    echo.
) else (
    echo ❌ WinShell CLI not found in PATH
    echo.
)

REM Check installation directory
set "INSTALL_DIR=%ProgramFiles%\WinShell"
if exist "%INSTALL_DIR%\WinShell.CLI.exe" (
    echo ✅ WinShell CLI found in Program Files
    echo    Path: %INSTALL_DIR%\WinShell.CLI.exe
    echo.
) else (
    echo ❌ WinShell CLI not found in Program Files
    echo.
)

if exist "%INSTALL_DIR%\WinShell.GUI.exe" (
    echo ✅ WinShell GUI found in Program Files  
    echo    Path: %INSTALL_DIR%\WinShell.GUI.exe
    echo.
) else (
    echo ❌ WinShell GUI not found in Program Files
    echo.
)

REM Check Start Menu shortcuts
set "START_MENU=%ProgramData%\Microsoft\Windows\Start Menu\Programs\WinShell"
if exist "%START_MENU%" (
    echo ✅ Start Menu shortcuts created
    echo    Location: %START_MENU%
    dir "%START_MENU%\*.lnk" /b 2>nul
    echo.
) else (
    echo ❌ Start Menu shortcuts not found
    echo.
)

REM Check Desktop shortcuts (optional)
set "DESKTOP=%PUBLIC%\Desktop"
if exist "%DESKTOP%\WinShell GUI.lnk" (
    echo ✅ Desktop GUI shortcut found
) else (
    echo ℹ️  Desktop GUI shortcut not found (may not have been selected)
)

if exist "%DESKTOP%\WinShell CLI.lnk" (
    echo ✅ Desktop CLI shortcut found
) else (
    echo ℹ️  Desktop CLI shortcut not found (may not have been selected)
)

echo.
echo =============================================
echo  Test Launch (GUI)
echo =============================================
echo.

choice /m "Would you like to test launch WinShell GUI"
if errorlevel 2 goto testcli
if errorlevel 1 (
    echo Launching WinShell GUI...
    start "" "%INSTALL_DIR%\WinShell.GUI.exe"
    timeout /t 3 >nul
)

:testcli
echo.
echo =============================================
echo  Test Launch (CLI)
echo =============================================
echo.

choice /m "Would you like to test launch WinShell CLI"
if errorlevel 2 goto finish
if errorlevel 1 (
    echo Launching WinShell CLI...
    start "" "%INSTALL_DIR%\WinShell.CLI.exe"
    timeout /t 3 >nul
)

:finish
echo.
echo =============================================
echo  Installation Test Complete!
echo =============================================
echo.
echo If any items show ❌, the installation may need to be re-run
echo with Administrator privileges.
echo.
pause