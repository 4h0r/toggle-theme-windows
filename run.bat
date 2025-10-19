@echo off
setlocal

set EXE=ToggleTheme\bin\Release\ToggleTheme.exe
if not exist "%EXE%" (
  echo Build output not found at %EXE%.
  echo Run build.bat first.
  exit /b 1
)

"%EXE%"
endlocal
