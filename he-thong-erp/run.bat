@echo off
chcp 65001 > nul
title TDSouth - He Thong ERP (Port 8082)
cd /d "%~dp0"
echo ========================================================
echo   KHOI CHAY HE THONG ERP DOANH NGHIEP (TDSOUTH)
echo ========================================================
echo Compiling C# source code (neu chua co hoac ma nguon thay doi)...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /r:System.Web.dll /r:System.Data.dll /out:ErpWeb.exe Program.cs Controllers\ErpController.cs Services\ErpService.cs Models\ErpModels.cs
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Bien dich that bai! Vui long kiem tra lai ma nguon.
    pause
    exit /b %ERRORLEVEL%
)
echo.
echo Khoi dong may chu web tren port 8082...
ErpWeb.exe
pause
