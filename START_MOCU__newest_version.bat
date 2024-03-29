@echo off
setlocal

set EXE_PATH=.\MOCU-UnityCore\Build\MOCU.exe

:: will show error message and wait for 2sec if there is no internet, then start old version of app
echo Checking internet connection...
curl -s --head https://github.com | findstr "200 OK"
if %errorlevel% neq 0 (
    echo You are not connected to the Internet (GitHub), so it is not the newest version of the program that will start (potentially)
    timeout /t 2 /nobreak > nul
    goto :startApp
)

:: will show error message and wait for 2sec if there is no GIT on this PC, then start old version of app
echo Checking git...
where git >nul 2>nul
if %errorlevel% neq 0 (
    echo Git is not installed, so it is not the newest version of the program that will start (potentially)
    timeout /t 2 /nobreak > nul
    goto :startApp
)

:: will show error message and wait for 2sec if got an error while fetching grom git, then start old version of app
echo Fetching latest changes for the repository...
git pull origin main
if %errorlevel% neq 0 (
    echo Error: Failed to fetch latest changes, so it is not the newest version of the program that will start (potentially)
    timeout /t 2 /nobreak > nul
    goto :startApp
)

:startApp
:: will show error message and wait for 2sec if couldn't start the app, then close this .bat file
start "" "%EXE_PATH%"
if %errorlevel% neq 0 (
    echo Error: Failed to start the app
    timeout /t 2 /nobreak > nul
    exit /b %errorlevel%
)

endlocal