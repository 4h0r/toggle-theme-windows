@echo off
setlocal

REM First, try to find MSBuild in PATH (for GitHub Actions or when added to PATH)
where msbuild.exe >nul 2>&1
if %ERRORLEVEL% EQU 0 (
  set "MSBUILD_PATH=msbuild.exe"
  goto :build
)

REM If not in PATH, try the standard Visual Studio 2022 location
set "MSBUILD_PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"

if not exist "%MSBUILD_PATH%" (
  set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
  if exist "%VSWHERE%" (
    for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set "MSBUILD_PATH=%%i"
  )
)

if not exist "%MSBUILD_PATH%" (
  if "%MSBUILD_PATH%"=="msbuild.exe" goto :build
  echo MSBuild not found. Ensure Visual Studio 2022 Build Tools are installed.
  exit /b 1
)

:build

"%MSBUILD_PATH%" "ToggleTheme.sln" /p:Configuration=Release /m
set "ERR=%ERRORLEVEL%"
if not "%ERR%"=="0" (
  echo Build failed with exit code %ERR%.
  exit /b %ERR%
)

echo Build succeeded. Output: ToggleTheme\bin\Release\ToggleTheme.exe
endlocal
