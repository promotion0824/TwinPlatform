FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY ["publish", "."]

RUN chown -R 1000:1000 /app
USER 1000
ENTRYPOINT ["dotnet", "Willow.PublicApi.dll"]
