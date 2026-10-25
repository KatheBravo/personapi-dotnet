@echo off
chcp 65001 >nul
title personapi-dotnet - Laboratorio 1
cd /d "%~dp0"

echo ==========================================================
echo  personapi-dotnet - Laboratorio 1 (Grupo 3)
echo  Levanta SQL Server 2022 + base persona_db + la aplicacion
echo ==========================================================
echo.

REM --- 1. Asegurar que Docker Desktop este encendido ---------------
docker info >nul 2>&1
if %errorlevel%==0 goto docker_ok

echo Iniciando Docker Desktop...
if exist "%LOCALAPPDATA%\Programs\DockerDesktop\Docker Desktop.exe" start "" "%LOCALAPPDATA%\Programs\DockerDesktop\Docker Desktop.exe"
if exist "%ProgramFiles%\Docker\Docker\Docker Desktop.exe" start "" "%ProgramFiles%\Docker\Docker\Docker Desktop.exe"

set /a intentos=0
:esperar_docker
timeout /t 5 /nobreak >nul
docker info >nul 2>&1
if %errorlevel%==0 goto docker_ok
set /a intentos+=1
echo   esperando a Docker... (%intentos%)
if %intentos% lss 60 goto esperar_docker
echo.
echo [ERROR] Docker no respondio. Abra Docker Desktop manualmente y vuelva a ejecutar este archivo.
pause
exit /b 1

:docker_ok
echo Docker esta listo.
echo.

REM --- 2. Construir y levantar los contenedores ---------------------
echo Construyendo y levantando los contenedores (la primera vez tarda varios minutos)...
docker compose up -d --build
if not %errorlevel%==0 (
  echo.
  echo [ERROR] Fallo docker compose. Revise el mensaje de arriba.
  pause
  exit /b 1
)

REM --- 3. Esperar a que la aplicacion responda ----------------------
echo.
echo Esperando a que la aplicacion responda en http://localhost:8080 ...
set /a intentos=0
:esperar_app
timeout /t 3 /nobreak >nul
powershell -NoProfile -Command "try { $r = Invoke-WebRequest -UseBasicParsing -TimeoutSec 3 http://localhost:8080/swagger/v1/swagger.json; exit 0 } catch { exit 1 }"
if %errorlevel%==0 goto app_ok
set /a intentos+=1
if %intentos% lss 100 goto esperar_app
echo [AVISO] La aplicacion aun no responde. Estado de los contenedores:
docker compose ps
docker compose logs --tail 40 db-init app
pause
exit /b 1

:app_ok
echo.
echo ==========================================================
echo  TODO LISTO
echo   Aplicacion web : http://localhost:8080
echo   Swagger        : http://localhost:8080/swagger
echo   SQL Server     : localhost,1433  usuario sa  clave Lab1-Persona2026
echo ==========================================================
start "" "http://localhost:8080/swagger"
start "" "http://localhost:8080"
echo.
docker compose ps
echo.
echo Puede cerrar esta ventana; los contenedores siguen corriendo.
echo Para detenerlos ejecute DETENER.bat
pause
