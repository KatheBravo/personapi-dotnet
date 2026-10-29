# ---------------------------------------------------------------
# Etapa 1: compilación con el SDK de .NET 10
# ---------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo el .csproj para aprovechar la caché de capas en el restore:
# si no cambian los paquetes, Docker reutiliza esta capa y la compilación es más rápida.
COPY personapi-dotnet.csproj ./
RUN dotnet restore personapi-dotnet.csproj

# Luego el resto del código y la publicación en modo Release
COPY . .
RUN dotnet publish personapi-dotnet.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------------------------------------------------------------
# Etapa 2: imagen final solo con el runtime de ASP.NET Core 10
# (no lleva el SDK, por eso pesa mucho menos)
# ---------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Desde .NET 8 las imágenes escuchan en el puerto 8080 y traen un usuario sin privilegios ("app").
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
COPY --from=build /app/publish .

# Buena práctica de seguridad: la aplicación no corre como root dentro del contenedor.
USER $APP_UID

ENTRYPOINT ["dotnet", "personapi-dotnet.dll"]
