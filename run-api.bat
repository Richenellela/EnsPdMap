@echo off
setlocal EnableExtensions
chcp 65001 >nul
cd /d "%~dp0"
title EnsPdMap API

set "API=EnsPdMap.Api\EnsPdMap.Api.csproj"

where dotnet >nul 2>&1
if errorlevel 1 (
  echo [!] SDK .NET introuvable. Lancez d'abord lancer-EnsPdMap.bat ^(option 1^).
  pause
  exit /b 1
)

echo.
echo ==================================================
echo   EnsPdMap API - http://localhost:5080
echo   Depuis l'emulateur Android : http://10.0.2.2:5080
echo   Test rapide : http://localhost:5080/api/health
echo ==================================================
echo.
echo Restauration et demarrage (Ctrl+C pour arreter)...
echo.
dotnet run --project "%API%"
pause
endlocal
