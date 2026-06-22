@echo off
setlocal EnableDelayedExpansion
chcp 65001 >nul

REM ============================================================
REM EbookDiff 一键打包脚本
REM   1) dotnet publish 生成自包含单文件 exe
REM   2) ISCC 编译 Inno Setup 安装包
REM
REM 前置条件：
REM   - 安装 .NET 8 SDK：https://dotnet.microsoft.com/download/dotnet/8.0
REM   - 安装 Inno Setup 6：https://jrsoftware.org/isinfo.php
REM ============================================================

set "ROOT=%~dp0"
set "PROJECT=%ROOT%EbookDiff\EbookDiff.csproj"
set "PUBLISH_DIR=%ROOT%EbookDiff\bin\Release\net8.0-windows\win-x64\publish"
set "ISS=%ROOT%installer.iss"

REM ---------- 检查 dotnet ----------
where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] 未找到 dotnet 命令。请先安装 .NET 8 SDK：
    echo         https://dotnet.microsoft.com/download/dotnet/8.0
    exit /b 1
)

echo.
echo === [1/3] 还原 NuGet 依赖 ===
pushd "%ROOT%EbookDiff"
dotnet restore EbookDiff.csproj
if errorlevel 1 ( popd & echo [ERROR] restore 失败 & exit /b 1 )

echo.
echo === [2/3] 发布单文件 exe（自包含 .NET 8 运行时） ===
dotnet publish EbookDiff.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true
if errorlevel 1 ( popd & echo [ERROR] publish 失败 & exit /b 1 )
popd

if not exist "%PUBLISH_DIR%\EbookDiff.exe" (
    echo [ERROR] 未找到发布产物：%PUBLISH_DIR%\EbookDiff.exe
    exit /b 1
)

echo.
echo 发布产物：
dir /b "%PUBLISH_DIR%\EbookDiff.exe"
for %%F in ("%PUBLISH_DIR%\EbookDiff.exe") do echo 大小：%%~zF 字节

REM ---------- 查找 ISCC ----------
set "ISCC="
for %%P in (
    "%ProgramFiles(x86)%\Inno Setup 7\ISCC.exe"
    "%ProgramFiles%\Inno Setup 7\ISCC.exe"
    "D:\Program File\Inno Setup 7\ISCC.exe"
    "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
    "%ProgramFiles%\Inno Setup 6\ISCC.exe"
) do (
    if exist %%P set "ISCC=%%~P"
)

if "%ISCC%"=="" (
    echo.
    echo [SKIP] 未找到 Inno Setup 6 (ISCC.exe)。
    echo        发布 exe 已生成。若需打包安装程序，请安装 Inno Setup：
    echo        https://jrsoftware.org/isinfo.php
    echo        装好后重新运行本脚本。
    exit /b 0
)

echo.
echo === [3/3] 用 Inno Setup 打包安装程序 ===
echo 使用 ISCC：%ISCC%
"%ISCC%" "%ISS%"
if errorlevel 1 ( echo [ERROR] ISCC 编译失败 & exit /b 1 )

echo.
echo === 完成 ===
echo 安装包输出在：%ROOT%dist\
dir /b "%ROOT%dist\*.exe" 2>nul

endlocal
exit /b 0
