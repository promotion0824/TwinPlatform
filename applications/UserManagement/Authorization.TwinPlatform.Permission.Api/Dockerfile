FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY "NuGet.Config" "NuGet.Config"

COPY ["applications/UserManagement/Authorization.TwinPlatform.Permission.Api/Authorization.TwinPlatform.Permission.Api.csproj", "applications/UserManagement/Authorization.TwinPlatform.Permission.Api/"]
COPY ["applications/UserManagement/Authorization.TwinPlatform.Persistence/Authorization.TwinPlatform.Persistence.csproj", "applications/UserManagement/Authorization/Authorization.TwinPlatform.Persistence/"]
COPY ["applications/UserManagement/Authorization.TwinPlatform/Authorization.TwinPlatform.csproj", "applications/UserManagement/Authorization.TwinPlatform/"]
COPY ["applications/UserManagement/Authorization.Common/Authorization.Common.csproj", "applications/UserManagement/Authorization.Common/"]
COPY ["applications/UserManagement/Authorization.Migrator/Authorization.Migrator.csproj", "applications/UserManagement/Authorization.Migrator/"]

RUN dotnet restore "applications/UserManagement/Authorization.TwinPlatform.Permission.Api/Authorization.TwinPlatform.Permission.Api.csproj"
COPY . .
ARG ASSEMBLY_VERSION=1.0.0.0
WORKDIR "/src/applications/UserManagement/Authorization.TwinPlatform.Permission.Api"
RUN dotnet build "Authorization.TwinPlatform.Permission.Api.csproj" -c Release -o /app/build /p:AssemblyVersion=$ASSEMBLY_VERSION /p:Version=$ASSEMBLY_VERSION /p:FileVersion=$ASSEMBLY_VERSION

FROM build AS publish
RUN dotnet publish "Authorization.TwinPlatform.Permission.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:AssemblyVersion=$ASSEMBLY_VERSION /p:Version=$ASSEMBLY_VERSION /p:FileVersion=$ASSEMBLY_VERSION

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN chown -R 1000:1000 /app
USER 1000
ENV ASPNETCORE_URLS http://*:8080
ENTRYPOINT ["dotnet", "Authorization.TwinPlatform.Permission.Api.dll"]
