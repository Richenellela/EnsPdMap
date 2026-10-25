@echo off
setlocal EnableExtensions
chcp 65001 >nul
set "ADB="
if exist "%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb.exe" set "ADB=%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb.exe"
if exist "%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe" set "ADB=%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe"
if defined ADB goto go
echo [!] adb introuvable. Installez la charge de travail Android de Visual Studio.
pause
exit /b 1
:go
echo Telephone en USB (debogage USB active) :
"%ADB%" devices
"%ADB%" reverse tcp:5080 tcp:5080
echo.
echo Dans l'appli : onglet Hors ligne ^> Serveur ^> http://localhost:5080 ^> Tester.
echo Le telephone joint maintenant l'API du PC par le cable, sans Wi-Fi.
pause
