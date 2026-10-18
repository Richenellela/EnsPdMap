@echo off
setlocal EnableExtensions
chcp 65001 >nul
cd /d "%~dp0"
title EnsPdMap - installation et lancement

set "PROJ=EnsPdMap\EnsPdMap.csproj"
set "TFM=net9.0-android"

:menu
cls
echo ==================================================
echo   EnsPdMap - front-end .NET MAUI (Android)
echo ==================================================
echo.
echo   1. Verifier / installer les prerequis
echo   2. Restaurer et compiler le projet
echo   3. Compiler et lancer sur appareil ou emulateur
echo   4. Tout faire (1 + 2 + 3)
echo   5. Nettoyer (supprimer bin et obj)
echo   6. Lancer en mode RELEASE (beaucoup plus fluide, sans debogage)
echo   7. Quitter
echo.
set "CHOIX="
set /p CHOIX=Votre choix (1-7) : 
if "%CHOIX%"=="1" goto do1
if "%CHOIX%"=="2" goto do2
if "%CHOIX%"=="3" goto do3
if "%CHOIX%"=="4" goto do4
if "%CHOIX%"=="5" goto do5
if "%CHOIX%"=="6" goto do6
if "%CHOIX%"=="7" goto fin
goto menu

:do1
call :prereq
pause
goto menu

:do2
call :build
pause
goto menu

:do3
call :run
pause
goto menu

:do4
call :prereq
if errorlevel 1 (
  pause
  goto menu
)
call :build
if errorlevel 1 (
  pause
  goto menu
)
call :run
pause
goto menu

:do5
if exist "EnsPdMap\bin" rmdir /s /q "EnsPdMap\bin"
if exist "EnsPdMap\obj" rmdir /s /q "EnsPdMap\obj"
echo Dossiers bin et obj supprimes.
pause
goto menu

:do6
echo.
echo Compilation Release et lancement (premiere fois : plusieurs minutes)...
dotnet build "%PROJ%" -f %TFM% -c Release -t:Run
if errorlevel 1 echo [!] Echec. Verifiez qu'un appareil ou un emulateur est demarre.
pause
goto menu

:: ------------------------------------------------------------
:prereq
echo.
echo [1/4] Recherche du SDK .NET...
where dotnet >nul 2>&1
if errorlevel 1 goto install_sdk
dotnet --list-sdks | findstr /b "9." >nul
if errorlevel 1 goto install_sdk
echo     SDK .NET 9 present.
goto after_sdk

:install_sdk
echo     SDK .NET 9 introuvable. Installation via winget...
winget install --id Microsoft.DotNet.SDK.9 -e --accept-source-agreements --accept-package-agreements
echo.
echo     Fermez cette fenetre puis relancez ce fichier (le PATH doit etre recharge).
exit /b 1

:after_sdk
echo.
echo [2/4] Installation de la charge de travail MAUI Android (peut prendre plusieurs minutes)...
net session >nul 2>&1
if errorlevel 1 echo     Conseil : si une erreur de droits apparait, relancez ce fichier en tant qu'administrateur.
dotnet workload install maui-android
if errorlevel 1 (
  echo     [!] Echec de l'installation de la charge de travail maui-android.
  exit /b 1
)

echo.
echo [3/4] Recherche du SDK Android et de adb...
set "ADB="
if exist "%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb.exe" set "ADB=%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb.exe"
if exist "%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe" set "ADB=%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe"
if defined ADB goto adb_ok
echo     [!] SDK Android introuvable.
echo         Ouvrez Visual Studio Installer, modifiez Visual Studio 2022 et cochez la charge de travail
echo         "Developpement d'applications mobiles avec .NET" (elle installe JDK, SDK Android et emulateur).
exit /b 1

:adb_ok
echo     adb trouve : %ADB%

echo.
echo [4/4] Appareils Android detectes :
"%ADB%" devices
echo.
echo     Si la liste est vide : demarrez un emulateur (Visual Studio ^> Outils ^> Android ^> Android Device Manager)
echo     ou branchez un telephone avec le debogage USB active.
echo.
echo Prerequis OK.
exit /b 0

:: ------------------------------------------------------------
:build
echo.
echo Restauration et compilation...
dotnet build "%PROJ%" -f %TFM% -c Debug
if errorlevel 1 (
  echo.
  echo [!] La compilation a echoue. Copiez les lignes "error" ci-dessus et envoyez-les moi.
  exit /b 1
)
echo.
echo Compilation reussie.
exit /b 0

:: ------------------------------------------------------------
:run
echo.
echo Compilation, installation et lancement sur l'appareil / l'emulateur connecte...
dotnet build "%PROJ%" -f %TFM% -c Debug -t:Run
if errorlevel 1 (
  echo.
  echo [!] Echec. Verifiez qu'un appareil ou un emulateur est demarre ^(option 1 pour la liste^).
  exit /b 1
)
exit /b 0

:fin
endlocal
exit /b 0
