FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
# Install Node.js
RUN apt-get update
RUN apt-get install -y ca-certificates curl gnupg
RUN mkdir -p /etc/apt/keyrings
RUN curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg
RUN echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_20.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list
RUN apt-get update && apt-get install -y nodejs
RUN node -v

COPY "NuGet.Config" "NuGet.Config"

COPY ["applications/UserManagement/Authorization.TwinPlatform.Web/Authorization.TwinPlatform.Web.csproj", "applications/UserManagement/Authorization.TwinPlatform.Web/"]
COPY ["applications/UserManagement/Authorization.TwinPlatform.Persistence/Authorization.TwinPlatform.Persistence.csproj", "applications/UserManagement/Authorization.TwinPlatform.Persistence/"]
COPY ["applications/UserManagement/Authorization.TwinPlatform/Authorization.TwinPlatform.csproj", "applications/UserManagement/Authorization.TwinPlatform/"]
COPY ["applications/UserManagement/Authorization.Common/Authorization.Common.csproj", "applications/UserManagement/Authorization.Common/"]
COPY ["applications/UserManagement/Authorization.TwinPlatform.Common/Authorization.TwinPlatform.Common.csproj", "applications/UserManagement/Authorization.TwinPlatform.Common/"]

RUN dotnet restore "applications/UserManagement/Authorization.TwinPlatform.Web/Authorization.TwinPlatform.Web.csproj"
COPY . .
WORKDIR "/src/applications/UserManagement/Authorization.TwinPlatform.Web"
ARG ASSEMBLY_VERSION=1.0.0.0
RUN dotnet build "Authorization.TwinPlatform.Web.csproj" -c Release -o /app/build /p:AssemblyVersion=$ASSEMBLY_VERSION /p:Version=$ASSEMBLY_VERSION /p:FileVersion=$ASSEMBLY_VERSION

FROM build AS publish
RUN dotnet publish "Authorization.TwinPlatform.Web.csproj" -c Release -o /app/publish /p:AssemblyVersion=$ASSEMBLY_VERSION /p:Version=$ASSEMBLY_VERSION /p:FileVersion=$ASSEMBLY_VERSION

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN chown -R 1000:1000 /app
USER 1000
ENV ASPNETCORE_URLS http://*:8080
ENTRYPOINT ["dotnet", "Authorization.TwinPlatform.Web.dll"]
