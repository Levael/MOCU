:: todo - download release only, not the codebase

@echo off
setlocal


echo Checking internet connection...
::curl -s --head https://github.com | findstr "200 OK"
if %errorlevel% neq 0 (
    echo You are not connected to the Internet (GitHub), so it is not the newest version of the program that will start (potentially)
    goto :endpoint
)

echo Checking git...
::where git >nul 2>nul
if %errorlevel% neq 0 (
    echo Git is not installed, so it is not the newest version of the program that will start (potentially)
    goto :endpoint
)

echo Fetching latest changes for the repository...
::git pull origin main
if %errorlevel% neq 0 (
    echo Error: Failed to fetch latest changes, so it is not the newest version of the program that will start (potentially)
    goto :endpoint
)

:endpoint
echo Error: Failed to update the app (press any KEY to close)
::pause > nul


endlocal