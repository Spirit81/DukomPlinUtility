@echo off
setlocal
cd /d "%~dp0DukomPlinUtility"
echo Building DUKOM PLIN Utility Professional...
dotnet publish DukomPlinUtility.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -o ..\publish
if errorlevel 1 (
  echo.
  echo BUILD FAILED.
  pause
  exit /b 1
)
echo.
echo DONE. EXE is in publish\DukomPlinUtility.exe
pause
