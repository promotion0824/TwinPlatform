
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
EXPOSE 5001
WORKDIR /app

# Copy the published .NET project over - do dotnet publish in build pipeline
COPY ["tools/WillowWSUP/WsupApi/bin/Release/net8.0/publish", "."]

RUN chown -R 1000:1000 /app
USER 1000
ENV ASPNETCORE_URLS http://*:5001
ENTRYPOINT ["dotnet", "WsupApi.dll"]
