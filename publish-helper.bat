@echo off

pwsh -NoLogo -NoProfile -File "%~dp0scripts\Publish-Helper.ps1" %*

exit /b %errorlevel%
