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
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WoWsShipBuilder.Web.dll"]
