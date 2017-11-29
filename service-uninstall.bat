@echo off
REM installutil iedusm.exe
SET THIS_PATH=%~dp0
IF EXIST "%THIS_PATH%\bin\Release\iedusm.exe" "%THIS_PATH%\bin\Release\iedusm.exe" -uninstall
IF NOT EXIST "%THIS_PATH%\bin\Release\iedusm.exe" "%THIS_PATH%\bin\Debug\iedusm.exe" -uninstall
pause