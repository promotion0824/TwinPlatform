FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "applications/ConnectorManagement/EdgeMetricsProcessor/src/Willow.EdgeMetricsProcessor/Willow.EdgeMetricsProcessor.csproj"

ARG VERSION
WORKDIR "applications/ConnectorManagement/EdgeMetricsProcessor/src/Willow.EdgeMetricsProcessor"
RUN dotnet build --no-restore "Willow.EdgeMetricsProcessor.csproj" -c Release -o /app/build

FROM build as publish
RUN dotnet publish "Willow.EdgeMetricsProcessor.csproj" -c Release -o /app/publish

FROM base as final
WORKDIR /app

RUN chown -R 1000:1000 /app
USER 1000
ENV ASPNETCORE_URLS http://*:8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Willow.EdgeMetricsProcessor.dll"]
