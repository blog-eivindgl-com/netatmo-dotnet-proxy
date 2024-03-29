#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY NetatmoProxy.sln ./
COPY ./NetatmoProxy/NetatmoProxy.csproj ./NetatmoProxy/
COPY ./NetatmoProxy.Core/NetatmoProxy.Core.csproj ./NetatmoProxy.Core/
COPY ./NetatmoProxy.Services/NetatmoProxy.Services.csproj ./NetatmoProxy.Services/
COPY ./NetatmoProxy.Services.Tests/NetatmoProxy.Services.Tests.csproj ./NetatmoProxy.Services.Tests/
RUN dotnet restore "NetatmoProxy.sln"
COPY . .
WORKDIR /src
RUN dotnet build "NetatmoProxy.sln" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetatmoProxy/NetatmoProxy.csproj" -c Release -o /app/publish

FROM base AS final
RUN apt update && apt install tzdata -y
ENV TZ="Europe/Oslo"
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetatmoProxy.dll"]