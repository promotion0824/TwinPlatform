ARG NUGET_ACCESSTOKEN
ARG NUGET_FEED
FROM node:16 AS build-node
WORKDIR /ClientApp

COPY ["applications/RulesEngine/RulesEngine.Web/ClientApp/package.json", "./"]
COPY ["applications/RulesEngine/RulesEngine.Web/ClientApp/package-lock.json", "./"]

RUN npm install

COPY ["applications/RulesEngine/RulesEngine.Web/ClientApp/", "./"]

# Do the build
RUN npm run build

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
EXPOSE 8080
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["applications/RulesEngine/nuget.config", "."]
COPY ["applications/RulesEngine/RulesEngine.Web/RulesEngine.Web.csproj", "RulesEngine.Web/"]
COPY ["applications/RulesEngine/RulesEngine.Config/RulesEngine.Config.csproj", "RulesEngine.Config/"]
COPY ["applications/RulesEngine/WillowRules/WillowRules.csproj", "WillowRules/"]

RUN dotnet restore "RulesEngine.Web/RulesEngine.Web.csproj"

# Copy over everything else
# Copy over all the other files
COPY ["applications/RulesEngine/RulesEngine.Web/", "RulesEngine.Web/"]
COPY ["applications/RulesEngine/RulesEngine.Config/", "RulesEngine.Config/"]
COPY ["applications/RulesEngine/WillowRules/", "WillowRules/"]
# Props file sits above and affects all projects
COPY ["applications/RulesEngine/Directory.Build.props", "Directory.Build.props"]

RUN dotnet publish "RulesEngine.Web/RulesEngine.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build-node /ClientApp/build ClientApp/build
USER 1000
ENV ASPNETCORE_URLS http://*:5050;http://*:8080
ENTRYPOINT ["dotnet", "RulesEngine.Web.dll"]
