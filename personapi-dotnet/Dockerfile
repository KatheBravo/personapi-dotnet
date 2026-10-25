# ---------------------------------------------------------------
# Etapa 1: compilación con el SDK de .NET 7
# ---------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Primero solo el .csproj para aprovechar la caché de capas en el restore
COPY personapi-dotnet.csproj ./
RUN dotnet restore personapi-dotnet.csproj

# Luego el resto del código y la publicación en modo Release
COPY . .
RUN dotnet publish personapi-dotnet.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------------------------------------------------------------
# Etapa 2: imagen final solo con el runtime de ASP.NET Core 7
# ---------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "personapi-dotnet.dll"]
