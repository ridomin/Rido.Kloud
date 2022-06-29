#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["pnp-memmon-hive/pnp-memmon-hive.csproj", "pnp-memmon-hive/"]
RUN dotnet restore "pnp-memmon-hive/pnp-memmon-hive.csproj"
COPY . .
WORKDIR "/src/pnp-memmon-hive"
RUN dotnet build "pnp-memmon-hive.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "pnp-memmon-hive.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "pnp-memmon-hive.dll"]