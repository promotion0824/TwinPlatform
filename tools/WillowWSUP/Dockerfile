
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
EXPOSE 5000
WORKDIR /app

# Copy the npm build front end files - do npm run build before docker build
COPY ["ClientApp/dist/", "ClientApp"]

# Copy the published .NET project over - do dotnet publish in build pipeline
COPY ["AdminApp/bin/Release/net8.0/publish", "."]

USER 1000
ENV ASPNETCORE_URLS http://*:5000
ENTRYPOINT ["dotnet", "AdminApp.dll"]
