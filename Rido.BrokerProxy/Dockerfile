#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Rido.BrokerProxy/Rido.BrokerProxy.csproj", "Rido.BrokerProxy/"]
RUN dotnet restore "Rido.BrokerProxy/Rido.BrokerProxy.csproj"
COPY . .
WORKDIR "/src/Rido.BrokerProxy"
RUN dotnet build "Rido.BrokerProxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rido.BrokerProxy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rido.BrokerProxy.dll"]
