#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

EXPOSE 8080
EXPOSE 8081

WORKDIR /src

COPY . .

RUN dotnet restore "applications/MappedTopologyIngestion/Willow.MappedTopologyIngestionApi/Willow.MappedTopologyIngestionApi.csproj"
WORKDIR "/src/applications/MappedTopologyIngestion/Willow.MappedTopologyIngestionApi"
RUN dotnet build --no-restore "Willow.MappedTopologyIngestionApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Willow.MappedTopologyIngestionApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

RUN chown -R 1000:1000 /app
USER 1000
ENV ASPNETCORE_URLS http://*:8080
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "Willow.MappedTopologyIngestionApi.dll" ]
