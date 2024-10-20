# Command and Control WebAPI Application

## Architecture
The Command and Control application consists of multiple components. This solution provides the backend WebAPI application support.

## Dependencies

* Rules Engine
* InsightCore
* Azure Service Bus
* Azure Application Insights
* Azure SQL Database
* Azure ADB2C
* Willow.Telemetry.Web nuget package (for logging)
* Docker (for testing)

## Dependency configuration

Azure dependencies are configured using appsettings.json. Environment variables can be overridden while running locally using user secrets (secrets.json).
The following environment variables are required for integrating OpenTelemetry observability package:

```` json
"ApplicationInsights": {
    "ConnectionString": "",
    "LogLevel": {
        "Default": "Information"
    }
},
"WillowContext": {
    "EnvironmentConfiguration": {
        "ShortName": ""
    },
    "RegionConfiguration": {
        "ShortName": ""
    },
    "StampConfiguration": {
        "Name": ""
    },
    "CustomerInstanceConfiguration": {
        "CustomerSalesId": "",
        "CustomerInstanceName": ""
    }
}
````
The WillowContext environment variables are available and substituted in Pulumi for Single tenant deployments. For multi-tenant
deployments, these may need to be filled in manually. `CustomerSalesId` is the id from SalesForce and can be obtained from the configuration
file in Single Tenant IAD project.

The WebApi application uses Serilog for logging. The following environment variables are required for configuring Serilog:

```` json
"Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.ApplicationInsights"
    ],
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "ApplicationInsights",
        "Args": {
          "telemetryConverter": "CommandAndControl.WebApi.Infrastructure.CloudRoleTelemetryConverter, CommandAndControl.WebApi"
        }
      }
    ],
    "Enrich": [
      "FromLogContext"
    ]
  }
````

### Azure Service Bus
TBD

### Azure SQL Database
TBD

## Structure
_This is a work in progress and is subject to change._

*Command and Control WebAPI* is the backend WebAPI application that provides the API endpoints for the Command and Control UI.

*Command and Control UI* is the internal (Willow only) UI for the Command and Control application.

[Wiki: Command and Control Initial Architecture](https://willow.atlassian.net/wiki/spaces/IOT/pages/2489155620/Command+Control+Initial+Architecture)

## Authentication to Azure Resources
The goal is to use managed identities wherever possible and avoid having to specify connection strings.

## Authorization
TBD


## Unit test
This project uses testcontainers to run simulated db unit test so your machine will need to have docker to run it.

If you encounter the following error
```
System.ArgumentException : Docker is either not running or misconfigured. Please ensure that Docker is running and that the endpoint is properly configured. You can customize your configuration using either the environment variables or the ~/.testcontainers.properties file. For more information, visit:
https://dotnet.testcontainers.org/custom_configuration/ (Parameter 'DockerEndpointAuthConfig')
```
Make sure that you have created a `~/.testcontainers.properties` file on your home directory and add the following line.
```
docker.host=tcp://docker:2375
```
and save the file
