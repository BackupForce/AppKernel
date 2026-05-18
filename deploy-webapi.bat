@echo off
setlocal

set IMAGE_NAME=webapi
set IMAGE_TAG=latest
set TAR_PATH=C:\Users\neora\source\repos\images\webapi.tar

set PEM_PATH=C:\Users\neora\.ssh\neo-tw.pem
set REMOTE_USER=ubuntu
set REMOTE_HOST=43.213.147.197
set REMOTE_IMAGE_PATH=/opt/docker-images/webapi.tar
set REMOTE_APP_PATH=/opt/apps

echo.
echo =============================
echo 1. Build WebApi Image
echo =============================

docker build -f src/Web.Api/Dockerfile -t %IMAGE_NAME%:%IMAGE_TAG% .
if %errorlevel% neq 0 goto error

echo.
echo =============================
echo 2. Save Image
echo =============================

docker save -o "%TAR_PATH%" %IMAGE_NAME%:%IMAGE_TAG%
if %errorlevel% neq 0 goto error

echo.
echo =============================
echo 3. Upload
echo =============================

scp -i "%PEM_PATH%" "%TAR_PATH%" %REMOTE_USER%@%REMOTE_HOST%:%REMOTE_IMAGE_PATH%
if %errorlevel% neq 0 goto error

echo.
echo =============================
echo 4. Remote Deploy
echo =============================

ssh -i "%PEM_PATH%" %REMOTE_USER%@%REMOTE_HOST% "docker load -i %REMOTE_IMAGE_PATH% && cd %REMOTE_APP_PATH% && docker compose up -d --no-deps --force-recreate web-api"

if %errorlevel% neq 0 goto error

echo.
echo =============================
echo ✅ Deployment SUCCESS
echo =============================
goto end

:error
echo.
echo =============================
echo ❌ Deployment FAILED
echo =============================
echo ErrorLevel: %errorlevel%

:end
echo.
echo =============================
echo Press any key to close...
echo =============================
pause >nul

endlocal