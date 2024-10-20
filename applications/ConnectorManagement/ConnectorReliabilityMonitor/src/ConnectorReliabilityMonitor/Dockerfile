FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY ["applications/ConnectorManagement/ConnectorReliabilityMonitor/src/ConnectorReliabilityMonitor/bin/Release/net8.0/publish", "."]

RUN chown -R 1000:1000 /app
USER 1000
ENTRYPOINT ["dotnet", "Willow.ConnectorReliabilityMonitor.dll"]
