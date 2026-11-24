@echo off
setlocal
powershell.exe -NoProfile -File "%~dp0scripts\iniciar.ps1" %*
set "resultado=%errorlevel%"
if not "%resultado%"=="0" pause
exit /b %resultado%
