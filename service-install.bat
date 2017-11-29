@echo off
REM installutil iedusm.exe
SET THIS_PATH=%~dp0
REM "%THIS_PATH%iedusm.exe" -uninstall
IF EXIST "%THIS_PATH%\bin\Release\iedusm.exe" "%THIS_PATH%\bin\Release\iedusm.exe" -install
IF NOT EXIST "%THIS_PATH%\bin\Release\iedusm.exe" "%THIS_PATH%\bin\Debug\iedusm.exe" -install
echo Now to go services.msc and do the following:
echo * change Startup type to "Start automatically"
echo * in "Log On" tab:
echo   * change "Log on as" to "Local System account"
echo   * "Allow service to interact with the desktop": yes
echo * OK, right-click the service, Start.
pause