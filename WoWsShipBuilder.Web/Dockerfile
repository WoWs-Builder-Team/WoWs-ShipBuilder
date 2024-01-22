FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

# Copy all project files
COPY . ./

# Restore only projects needed for ShipBuilder Web
RUN dotnet restore "WoWsShipBuilder.Web/WoWsShipBuilder.Web.csproj"

# Publish ShipBuilder Web
RUN dotnet publish "WoWsShipBuilder.Web/WoWsShipBuilder.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS deploy-env
WORKDIR /app

LABEL org.opencontainers.image.source="https://github.com/WoWs-Builder-Team/WoWs-ShipBuilder"
LABEL org.opencontainers.image.description="Container image for ShipBuilder Web"
LABEL org.opencontainers.image.licenses=MIT
LABEL org.opencontainers.image.authors="WoWs Builder Team"

COPY --from=build-env --chown=app /app/publish .

USER app
ENTRYPOINT ["dotnet", "WoWsShipBuilder.Web.dll"]
