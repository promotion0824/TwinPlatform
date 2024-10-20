FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "applications/IngestionPipeline/CognianTelemetryAdapter/CognianTelemetryAdapter/CognianTelemetryAdapter.csproj"

ARG VERSION
WORKDIR "/src/applications/IngestionPipeline/CognianTelemetryAdapter/CognianTelemetryAdapter"
RUN dotnet build --no-restore "CognianTelemetryAdapter.csproj" -c Release -o /app/build

FROM build as publish
RUN dotnet publish "CognianTelemetryAdapter.csproj" -c Release -o /app/publish /p:Version=${VERSION}

FROM base as final
WORKDIR /app

RUN chown -R 1000:1000 /app
USER 1000
ENV ASPNETCORE_URLS http://*:8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Willow.CognianTelemetryAdapter.dll"]
