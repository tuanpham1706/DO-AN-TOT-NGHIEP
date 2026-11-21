@echo off
chcp 65001 >nul
title VT3 CyberZone - Backend Server
cd /d "%~dp0"

echo ========================================================
echo        KHOI DONG SERVER VT3 CYBERZONE
echo ========================================================
echo.

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    if exist "%~dp0..\.dotnet\dotnet.exe" (
        set "DOTNET_CMD=%~dp0..\.dotnet\dotnet.exe"
    ) else if exist "%~dp0.dotnet\dotnet.exe" (
        set "DOTNET_CMD=%~dp0.dotnet\dotnet.exe"
    ) else (
        echo [LOI] May chua cai dat .NET 10 SDK!
        echo Vui long tai va cai dat .NET 10 tai:
        echo https://dotnet.microsoft.com/download/dotnet/10.0
        echo.
        pause
        exit /b 1
    )
) else (
    set "DOTNET_CMD=dotnet"
)

echo Dang khoi dong Server tai http://0.0.0.0:5121 ...
echo (Luu y: Can dam bao MySQL 8 dang chay tren may)
echo.
cd "%~dp0src\VT3CyberZone.Server"
%DOTNET_CMD% run --launch-profile http
pause
