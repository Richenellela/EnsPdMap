@echo off
chcp 65001 >nul
net session >nul 2>&1
if errorlevel 1 (
  echo [!] Lancez ce fichier en tant qu'administrateur ^(clic droit ^> Executer en tant qu'administrateur^).
  pause
  exit /b 1
)
netsh advfirewall firewall delete rule name="EnsPdMap API" >nul 2>&1
netsh advfirewall firewall add rule name="EnsPdMap API" dir=in action=allow protocol=TCP localport=5080
echo.
echo Port 5080 autorise : un telephone du meme Wi-Fi peut joindre l'API.
pause
