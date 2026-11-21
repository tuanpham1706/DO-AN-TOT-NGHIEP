@echo off
chcp 65001 >nul
title VT3 CyberZone - Desktop App
cd /d "%~dp0"

echo ========================================================
echo        KHOI DONG VT3 CYBERZONE DESKTOP
echo ========================================================
echo.

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    if exist "%~dp0..\.dotnet\dotnet.exe" (
        set "DOTNET_CMD=%~dp0..\.dotnet\dotnet.exe"
    ) else if exist "%~dp0.dotnet\dotnet.exe" (
        set "DOTNET_CMD=%~dp0.dotnet\dotnet.exe"
    ) else (
        echo [LOI] May chua cai dat .NET 10 Desktop!
        echo Vui long tai va cai dat .NET 10 tai:
        echo https://dotnet.microsoft.com/download/dotnet/10.0
        echo.
        pause
        exit /b 1
    )
) else (
    set "DOTNET_CMD=dotnet"
)

echo Dang mo ung dung Desktop...
cd "%~dp0src\VT3CyberZone.Desktop"
%DOTNET_CMD% run
