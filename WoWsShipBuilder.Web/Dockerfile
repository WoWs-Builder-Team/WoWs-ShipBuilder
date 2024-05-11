FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-env
ARG TARGETARCH
WORKDIR /src

# Copy all project files
COPY . ./

# Restore only projects needed for ShipBuilder Web
RUN dotnet restore "WoWsShipBuilder.Web/WoWsShipBuilder.Web.csproj" -a $TARGETARCH

# Publish ShipBuilder Web
RUN dotnet publish "WoWsShipBuilder.Web/WoWsShipBuilder.Web.csproj" -a $TARGETARCH --no-restore -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS deploy-env
EXPOSE 8080

# Enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
ENV \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8
RUN apk add --no-cache \
    icu-data-full \
    icu-libs

LABEL org.opencontainers.image.source="https://github.com/WoWs-Builder-Team/WoWs-ShipBuilder"
LABEL org.opencontainers.image.description="Container image for ShipBuilder Web"
LABEL org.opencontainers.image.licenses=MIT
LABEL org.opencontainers.image.authors="WoWs Builder Team"

WORKDIR /app
COPY --from=build-env /app .
USER $APP_UID
ENTRYPOINT ["./WoWsShipBuilder.Web"]
