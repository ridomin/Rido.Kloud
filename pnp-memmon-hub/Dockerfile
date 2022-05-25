#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["pnp-memmon-hub/pnp-memmon-hub.csproj", "pnp-memmon-hub/"]
RUN dotnet restore "pnp-memmon-hub/pnp-memmon-hub.csproj"
COPY . .
WORKDIR "/src/pnp-memmon-hub"
RUN dotnet build "pnp-memmon-hub.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "pnp-memmon-hub.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "pnp-memmon-hub.dll"]