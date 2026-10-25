@echo off
cd /d "%~dp0"
echo Deteniendo los contenedores de personapi-dotnet (los datos se conservan)...
docker compose down
pause
