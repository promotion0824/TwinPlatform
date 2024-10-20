FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Install Node.js
RUN apt-get update
RUN apt-get install -y ca-certificates curl gnupg
RUN mkdir -p /etc/apt/keyrings
RUN curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg
RUN echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_16.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list
RUN apt-get update && apt-get install nodejs npm -y
RUN node -v
RUN npm -v

COPY . .
RUN dotnet restore "/src/applications/EdgeDeploymentDashboard/Willow.IoTService.Deployment.Dashboard/Willow.IoTService.Deployment.Dashboard/Willow.IoTService.Deployment.Dashboard.csproj"
ARG NPM_AUTH_TOKEN
RUN echo "//npm.pkg.github.com/:_authToken=$NPM_AUTH_TOKEN" >> /src/applications/EdgeDeploymentDashboard/Willow.IoTService.Deployment.Dashboard/deployment-dashboard-webui/.npmrc

ARG VERSION
WORKDIR "/src/applications/EdgeDeploymentDashboard/Willow.IoTService.Deployment.Dashboard/Willow.IoTService.Deployment.Dashboard"
RUN dotnet build --no-restore "Willow.IoTService.Deployment.Dashboard.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Willow.IoTService.Deployment.Dashboard.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN chown 1000:1000 /app
USER 1000
EXPOSE 8080
ENV ASPNETCORE_URLS http://*:8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Willow.IoTService.Deployment.Dashboard.dll"]
