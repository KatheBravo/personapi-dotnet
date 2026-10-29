@echo off
REM Detiene los contenedores. Los datos quedan guardados en el volumen de Docker.
REM Para borrar tambien la base de datos use: docker compose down -v
cd /d "%~dp0"
echo Deteniendo los contenedores de personapi-dotnet (los datos se conservan)...
docker compose down
pause
