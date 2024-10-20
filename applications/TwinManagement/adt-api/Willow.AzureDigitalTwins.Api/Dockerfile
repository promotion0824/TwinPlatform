FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR src
COPY "NuGet.Config" "NuGet.Config"

COPY "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Api/Willow.AzureDigitalTwins.Api.csproj" "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Api/"
COPY "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Api.Persistence/Willow.AzureDigitalTwins.Api.Persistence.csproj" "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Api.Persistence/"
COPY "applications/TwinManagement/adt-api/Willow.Model/Willow.Model.csproj" "applications/TwinManagement/adt-api/Willow.Model/"
COPY "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Services/Willow.AzureDigitalTwins.Services.csproj" "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Services/"
COPY "applications/TwinManagement/adt-api/Willow.AzureDataExplorer/Willow.AzureDataExplorer.csproj" "applications/TwinManagement/adt-api/Willow.AzureDataExplorer/"
COPY "applications/TwinManagement/adt-api/Willow.Storage/Willow.Storage.csproj" "applications/TwinManagement/adt-api/Willow.Storage/"
COPY "applications/TwinManagement/adt-api/Willow.DataQuality.Execution/Willow.DataQuality.Execution.csproj" "applications/TwinManagement/adt-api/Willow.DataQuality.Execution/"
COPY "applications/TwinManagement/adt-api/Willow.DataQuality.Model/Willow.DataQuality.Model.csproj" "applications/TwinManagement/adt-api/Willow.DataQuality.Model/"
COPY "applications/TwinManagement/lib/Willow.Exceptions/Willow.Exceptions.csproj" "applications/TwinManagement/lib/Willow.Exceptions/"

RUN dotnet restore "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Api/Willow.AzureDigitalTwins.Api.csproj"
RUN dotnet restore "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Api.Persistence/Willow.AzureDigitalTwins.Api.Persistence.csproj"
RUN dotnet restore "applications/TwinManagement/adt-api/Willow.Model/Willow.Model.csproj"
RUN dotnet restore "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Services/Willow.AzureDigitalTwins.Services.csproj"
RUN dotnet restore "applications/TwinManagement/adt-api/Willow.AzureDataExplorer/Willow.AzureDataExplorer.csproj"
RUN dotnet restore "applications/TwinManagement/adt-api/Willow.Storage/Willow.Storage.csproj"
RUN dotnet restore "applications/TwinManagement/adt-api/Willow.DataQuality.Execution/Willow.DataQuality.Execution.csproj"
RUN dotnet restore "applications/TwinManagement/adt-api/Willow.DataQuality.Model/Willow.DataQuality.Model.csproj"
RUN dotnet restore "applications/TwinManagement/lib/Willow.Exceptions/Willow.Exceptions.csproj"

COPY "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Api" "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Api"
COPY "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Api.Persistence" "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Api.Persistence"
COPY "applications/TwinManagement/adt-api/Willow.Model" "applications/TwinManagement/adt-api/Willow.Model"
COPY "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Services" "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Services"
COPY "applications/TwinManagement/adt-api/Willow.AzureDataExplorer" "applications/TwinManagement/adt-api/Willow.AzureDataExplorer"
COPY "applications/TwinManagement/adt-api/Willow.Storage" "applications/TwinManagement/adt-api/Willow.Storage"
COPY "applications/TwinManagement/adt-api/Willow.DataQuality.Execution" "applications/TwinManagement/adt-api/Willow.DataQuality.Execution"
COPY "applications/TwinManagement/adt-api/Willow.DataQuality.Model" "applications/TwinManagement/adt-api/Willow.DataQuality.Model"
COPY "applications/TwinManagement/lib/Willow.Exceptions" "applications/TwinManagement/lib/Willow.Exceptions"

ARG ASSEMBLY_VERSION=1.0.0.0
RUN dotnet build "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Api/Willow.AzureDigitalTwins.Api.csproj" -c Release --no-restore /p:AssemblyVersion=$ASSEMBLY_VERSION /p:Version=$ASSEMBLY_VERSION /p:FileVersion=$ASSEMBLY_VERSION
RUN dotnet publish "applications/TwinManagement/adt-api/Willow.AzureDigitalTwins.Api/Willow.AzureDigitalTwins.Api.csproj" -c Release -o /publish --no-build --no-restore /p:AssemblyVersion=$ASSEMBLY_VERSION /p:Version=$ASSEMBLY_VERSION /p:FileVersion=$ASSEMBLY_VERSION

FROM base AS final
WORKDIR /app
COPY --from=build /publish .

USER 1000
ENV ASPNETCORE_URLS http://*:8080
ENTRYPOINT ["dotnet", "Willow.AzureDigitalTwins.Api.dll"]
