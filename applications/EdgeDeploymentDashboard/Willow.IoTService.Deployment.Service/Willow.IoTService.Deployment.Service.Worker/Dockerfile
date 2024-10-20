FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "applications/EdgeDeploymentDashboard/Willow.IoTService.Deployment.Service/Willow.IoTService.Deployment.Service.Worker/Willow.IoTService.Deployment.Service.Worker.csproj"

ARG VERSION
WORKDIR "/src/applications/EdgeDeploymentDashboard/Willow.IoTService.Deployment.Service/Willow.IoTService.Deployment.Service.Worker"
RUN dotnet build --no-restore "Willow.IoTService.Deployment.Service.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Willow.IoTService.Deployment.Service.Worker.csproj" -c Release -o /app/publish 

FROM base AS final
WORKDIR /app
RUN chown 1000:1000 /app
USER 1000
ENV ASPNETCORE_URLS http://*:8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Willow.IoTService.Deployment.Service.Worker.dll"]
