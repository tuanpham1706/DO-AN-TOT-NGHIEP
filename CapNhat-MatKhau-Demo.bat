@echo off
chcp 65001 >nul
title V-T3 CyberZone - Cap nhat tai khoan demo

set "MYSQL=C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe"
if not exist "%MYSQL%" (
    echo Khong tim thay MySQL. Hay kiem tra lai cai dat MySQL.
    goto :end
)

echo Nhap mat khau MySQL root khi duoc hoi.
echo Lenh nay chi cap nhat mat khau cua 5 tai khoan demo.
"%MYSQL%" --user=root --password vt3_cyberzone_db < "%~dp0CapNhat-MatKhau-Demo.sql"
if errorlevel 1 (
    echo Cap nhat that bai. Kiem tra mat khau root va database.
) else (
    echo Da cap nhat tai khoan demo. Mat khau dang nhap ung dung: 123456
)

:end
echo.
pause
