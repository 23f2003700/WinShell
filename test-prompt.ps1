# Test script for WinShell CLI prompt functionality

Write-Host "Testing WinShell CLI Prompt Features" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan

Write-Host "`n1. Default WS Prompt:" -ForegroundColor Yellow
Write-Host "   Shows as: WS [folder]>" -ForegroundColor Green

Write-Host "`n2. Available prompt commands:" -ForegroundColor Yellow
Write-Host "   prompt              - Show current prompt template and help" -ForegroundColor Green
Write-Host "   prompt WS`$G        - Default WinShell branded prompt" -ForegroundColor Green  
Write-Host "   prompt `$P`$G        - Full path prompt (like PowerShell)" -ForegroundColor Green
Write-Host "   prompt `$U@`$M`$G     - User@Machine prompt (like Linux)" -ForegroundColor Green
Write-Host "   prompt [`$T] `$G     - Time-based prompt" -ForegroundColor Green

Write-Host "`n3. Prompt Variables:" -ForegroundColor Yellow
Write-Host "   `$P = Current directory path" -ForegroundColor Green
Write-Host "   `$G = Greater than symbol (>)" -ForegroundColor Green  
Write-Host "   `$D = Current date" -ForegroundColor Green
Write-Host "   `$T = Current time" -ForegroundColor Green
Write-Host "   `$U = Username" -ForegroundColor Green
Write-Host "   `$M = Machine name" -ForegroundColor Green
Write-Host "   WS = WinShell brand (shows directory name only)" -ForegroundColor Green

Write-Host "`n4. Example Usage in WinShell CLI:" -ForegroundColor Yellow
Write-Host "   WS [winshell2]> prompt" -ForegroundColor Cyan
Write-Host "   WS [winshell2]> prompt `$U@`$M`$G" -ForegroundColor Cyan  
Write-Host "   WS [winshell2]> cd .." -ForegroundColor Cyan
Write-Host "   WS [Internet]> prompt [`$T] `$G" -ForegroundColor Cyan
Write-Host "   [14:30:25] > " -ForegroundColor Cyan

Write-Host "`nThe CLI is now running in a separate window with:" -ForegroundColor Magenta
Write-Host "• Professional WINSHELL ASCII banner" -ForegroundColor White
Write-Host "• WS [folder]> branded prompt (instead of full path)" -ForegroundColor White  
Write-Host "• Customizable prompt templates" -ForegroundColor White
Write-Host "• All original shell functionality" -ForegroundColor White