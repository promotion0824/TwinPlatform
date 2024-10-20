FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["applications/UserManagement/Authorization.Migrator/Authorization.Migrator.csproj", "applications/UserManagement/Authorization.Migrator/"]
COPY ["applications/UserManagement/Authorization.TwinPlatform.Persistence/Authorization.TwinPlatform.Persistence.csproj", "applications/UserManagement/Authorization.TwinPlatform.Persistence/"]

RUN dotnet restore "applications/UserManagement/Authorization.Migrator/Authorization.Migrator.csproj"
COPY . .
WORKDIR "/src/applications/UserManagement/Authorization.Migrator"

ARG Version="0.0.1"

RUN dotnet build "Authorization.Migrator.csproj" -c Release -o /app/build /p:Version=$Version

FROM build AS publish
RUN dotnet publish "Authorization.Migrator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER 1000
ENTRYPOINT ["dotnet", "Authorization.Migrator.dll"]
