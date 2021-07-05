#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["emby-exporter/emby-exporter.csproj", "emby-exporter/"]
RUN dotnet restore "emby-exporter/emby-exporter.csproj"
COPY . .
WORKDIR "/src/emby-exporter"
RUN dotnet build "emby-exporter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "emby-exporter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "emby-exporter.dll"]