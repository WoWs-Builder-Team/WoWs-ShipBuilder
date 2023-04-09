FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "WoWsShipBuilder.Web/WoWsShipBuilder.Web.csproj"
WORKDIR "/src/WoWsShipBuilder.Web"
RUN dotnet build "WoWsShipBuilder.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WoWsShipBuilder.Web.csproj" -c Release -o /app/publish

FROM base AS final
ENV APPLICATION_USER_ID 1001
ENV APPLICATION_USER appuser
LABEL org.opencontainers.image.source=https://github.com/WoWs-Builder-Team/WoWs-ShipBuilder
LABEL org.opencontainers.image.description="Container iamge for ShipBuilder Web"
LABEL org.opencontainers.image.licenses=MIT

RUN groupadd --gid $APPLICATION_USER_ID $APPLICATION_USER \
    && useradd --uid $APPLICATION_USER_ID --gid $APPLICATION_USER_ID -m $APPLICATION_USER

WORKDIR /app
COPY --from=publish --chown=$APPLICATION_USER /app/publish .

USER $APPLICATION_USER
ENTRYPOINT ["dotnet", "WoWsShipBuilder.Web.dll"]
