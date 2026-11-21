@echo off
chcp 65001 >nul
title V-T3 CyberZone - Server LAN

set "PS_EXE=%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe"
if not exist "%PS_EXE%" (
    echo Khong tim thay Windows PowerShell.
    goto :end
)

"%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -File "%~dp0Chay-Server-LAN.ps1"
if errorlevel 1 echo Server khong the khoi dong. Kiem tra MySQL va database roi thu lai.

:end
echo.
echo Nhan phim bat ky de dong cua so.
pause >nul
