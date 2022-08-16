#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0-jammy AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-jammy AS build
WORKDIR /src
COPY ["pnp-memmon-broker/pnp-memmon-broker.csproj", "pnp-memmon-broker/"]
RUN dotnet restore "pnp-memmon-broker/pnp-memmon-broker.csproj"
COPY . .
WORKDIR "/src/pnp-memmon-broker"
RUN dotnet build "pnp-memmon-broker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "pnp-memmon-broker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "pnp-memmon-broker.dll"]