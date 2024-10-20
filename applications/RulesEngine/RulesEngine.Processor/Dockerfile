FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
EXPOSE 8080
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["applications/RulesEngine/RulesEngine.Processor/RulesEngine.Processor.csproj", "RulesEngine.Processor/"]
COPY ["applications/RulesEngine/RulesEngine.Config/RulesEngine.Config.csproj", "RulesEngine.Config/"]
COPY ["applications/RulesEngine/WillowRules/WillowRules.csproj", "WillowRules/"]
COPY ["applications/RulesEngine/nuget.config", "."]

RUN dotnet restore "RulesEngine.Processor/RulesEngine.Processor.csproj"

# Copy over all the other files
COPY ["applications/RulesEngine/RulesEngine.Processor/", "RulesEngine.Processor/"]
COPY ["applications/RulesEngine/RulesEngine.Config/", "RulesEngine.Config/"]
COPY ["applications/RulesEngine/WillowRules/", "WillowRules/"]

# Props file sits above and affects all projects
COPY ["applications/RulesEngine/Directory.Build.props", "Directory.Build.props"]

RUN dotnet publish "RulesEngine.Processor/RulesEngine.Processor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
USER 1000
ENV ASPNETCORE_URLS http://*:5050;http://*:8080
ENTRYPOINT ["dotnet", "RulesEngine.Processor.dll"]
