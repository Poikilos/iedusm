@echo off
SET THISNAME=iedusm
REM installutil %THISNAME%.exe
SET THIS_PATH=%~dp0
SET SOURCE_PATH=%THIS_PATH%\bin\Release\%THISNAME%.exe
IF EXIST "%THISNAME%.exe" SOURCE_PATH=%THISNAME%.exe
IF NOT EXIST "%PROGRAMFILE%\%THISNAME%" md "%PROGRAMFILE%\%THISNAME%"
IF NOT EXIST "%PROGRAMFILE%\%THISNAME%" echo Failed to create '%PROGRAMFILE%\%THISNAME%' (you must be Administrator)
IF NOT EXIST "%PROGRAMFILE%\%THISNAME%" goto END_ERROR
IF EXIST "%THIS_PATH%\bin\Release\%THISNAME%.exe" "%THIS_PATH%\bin\Release\%THISNAME%.exe" -install
IF NOT EXIST "%THIS_PATH%\bin\Release\%THISNAME%.exe" "%THIS_PATH%\bin\Debug\%THISNAME%.exe" -install
echo Normally (after implemented), this install and the following steps would be done by the iedusm service.
echo Now to go services.msc and do the following:
echo * change Startup type to "Start automatically"
echo * in "Log On" tab:
echo   * change "Log on as" to "Local System account"
echo   * "Allow service to interact with the desktop": yes
echo * OK, right-click the service, Start.
echo * Add an exception to your anti-virus software for %THISNAME%

GOTO END_SILENT
:END_ERROR
echo Installation failed.
:END_SILENT
pause