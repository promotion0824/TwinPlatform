FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated6.0 AS base
WORKDIR /home/site/wwwroot


ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY /publish .
