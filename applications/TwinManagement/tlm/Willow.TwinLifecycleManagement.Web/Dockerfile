FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Install Node.js
RUN apt-get update
RUN apt-get install -y ca-certificates curl gnupg
RUN mkdir -p /etc/apt/keyrings
RUN curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg
RUN echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_20.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list
RUN apt-get update && apt-get install nodejs -y
WORKDIR /src

COPY "NuGet.Config" "NuGet.Config"

COPY "applications/TwinManagement/tlm/Willow.TwinLifecycleManagement.Web/Willow.TwinLifecycleManagement.Web.csproj" "applications/TwinManagement/tlm/Willow.TwinLifecycleManagement.Web/"
COPY "applications/TwinManagement/lib/Willow.AzureDigitalTwins.SDK/Willow.AzureDigitalTwins.SDK.csproj" "applications/TwinManagement/lib/Willow.AzureDigitalTwins.SDK/"
COPY "applications/TwinManagement/adt-api/Willow.Model/Willow.Model.csproj" "applications/TwinManagement/adt-api/Willow.Model/"
COPY "applications/TwinManagement/adt-api/Willow.DataQuality.Model/Willow.DataQuality.Model.csproj" "applications/TwinManagement/adt-api/Willow.DataQuality.Model/"
COPY "applications/TwinManagement/lib/Willow.Exceptions/Willow.Exceptions.csproj" "applications/TwinManagement/lib/Willow.Exceptions/"

RUN dotnet restore "applications/TwinManagement/tlm/Willow.TwinLifecycleManagement.Web/Willow.TwinLifecycleManagement.Web.csproj"
RUN dotnet restore "applications/TwinManagement/lib/Willow.AzureDigitalTwins.SDK/Willow.AzureDigitalTwins.SDK.csproj"
RUN dotnet restore "applications/TwinManagement/adt-api/Willow.Model/Willow.Model.csproj"
RUN dotnet restore "applications/TwinManagement/adt-api/Willow.DataQuality.Model/Willow.DataQuality.Model.csproj"
RUN dotnet restore "applications/TwinManagement/lib/Willow.Exceptions/Willow.Exceptions.csproj"

COPY "applications/TwinManagement/tlm/Willow.TwinLifecycleManagement.Web" "applications/TwinManagement/tlm/Willow.TwinLifecycleManagement.Web"
COPY "applications/TwinManagement/lib/Willow.AzureDigitalTwins.SDK" "applications/TwinManagement/lib/Willow.AzureDigitalTwins.SDK"
COPY "applications/TwinManagement/adt-api/Willow.Model" "applications/TwinManagement/adt-api/Willow.Model"
COPY "applications/TwinManagement/adt-api/Willow.DataQuality.Model" "applications/TwinManagement/adt-api/Willow.DataQuality.Model"
COPY "applications/TwinManagement/lib/Willow.Exceptions" "applications/TwinManagement/lib/Willow.Exceptions"

WORKDIR "/src/applications/TwinManagement/tlm/Willow.TwinLifecycleManagement.Web"

ARG ASSEMBLY_VERSION=1.0.0.0
RUN dotnet build "Willow.TwinLifecycleManagement.Web.csproj" -c Release /p:AssemblyVersion=$ASSEMBLY_VERSION /p:Version=$ASSEMBLY_VERSION /p:FileVersion=$ASSEMBLY_VERSION --no-restore

RUN dotnet publish "Willow.TwinLifecycleManagement.Web.csproj" -c Release -o /publish --no-build --no-restore /p:AssemblyVersion=$ASSEMBLY_VERSION /p:Version=$ASSEMBLY_VERSION /p:FileVersion=$ASSEMBLY_VERSION

FROM base AS final
WORKDIR /app
COPY --from=build /publish .


RUN chown -R 1000:1000 /app
USER 1000
ENV ASPNETCORE_URLS http://*:8080
ENTRYPOINT [ "dotnet", "Willow.TwinLifecycleManagement.Web.dll" ]
