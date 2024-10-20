# Authorization Service

Authorization Service is an external service responsible for evaluating whether a user is permitted to perform a specific action.

Refer to (Confluence Documentation)[https://willow.atlassian.net/wiki/spaces/WCP/pages/2150334469/Twin+Security+Services+Permission+Evaluation+User+Role+Permission+Management+and+Authz] for full details.

## Components

The Authorization service is composed of three main components:

* Client/consumer - this could be anything which wants to perform an authorization check e.g a Web Api but could be an Azure Function etc
* Authorization.PolicyDecisionPoint - this service determines whether a user is permitted to perform an action (deployed as a Twin Platform extension)
* Authorization.TestApi - used for testing PolicyDecisionPoint service

## Setup
Setup is straight forward and requires the following:

* .NET 6
* For Role Based checks a copy of the SQL Azure database DirectoryCoreDB. This can be either a local SQL instance (remember to update the connection string in Authorization.PolicyDecisionPoint appsettings.json which is currently setup for localhost) 

## Setting up Test data
* If you want to setup test schema and data in DirectoryCoreDB connect to empty SQL Azure db and run ~\Authorization.PolicyDecisionPoint\Sql\DirectoryCoreDB\CreateTestDirectoryCoreDBSchema.sql and \Authorization.PolicyDecisionPoint\Sql\DirectoryCoreDB\CreateTestData.sql
* For SQL Azure make sure you grant managed identity permission to connect to db - see ~\Authorization.PolicyDecisionPoint\Sql\DirectoryCoreDB\ConfigureDirectoryCoreDbAccess.sql
* When developing you will probably want to configure debug to start both Authorization.PolicyDecisionPoint and Authorization.TestApi

### Example Requests
If you want to see the solution working you could use Postman to hit the following Authorization.TestApi endpoints:

* Programmatic check: https://localhost:7212/WeatherForecast/site-direct?test=true
* Attribute based: https://localhost:7212/WeatherForecast/1287CA7A-3FD7-43C0-921A-57A40FBD6CD5/site-attribute?test=false

You will need to send a Twin Platform bearer token in the Authorization header (log-into TwinPlatform and copy the Authorization header without the Bearer prefix into Postman).

## Current Functionality
The solution should be considered a v1.0 implementation of external Authorization and contains the following functionality:

* Containerized External Authorization Service that can be run on Twin Platform
* AppInsights Logging and Azure KeyVault Integration
* Support for attribute and programmatic permission checks
* Methods to assist integrating with existing ASP.NET Core projects
* Callers are authenticated via AD Claims (to ensure only specific services may contact the Authorization service)
* Support for Role and Site based checks such as is User X in Role Y (RBAC) using DirectoryCoreDB
* Example of defining a custom policy 
* Ability to assign users to an AD group which will always grant permission requests (for supporting our applications and replacing impersonation)
* Permission check results cached depending on request 

## Projects
This section summarises the function of individual projects.

### Authorization.Common
Various components utilized by Authorization service and clients. Contains PermissionEvaluator that is responsible for validating requests.

### Authorization.PolicyDecisionPoint
ASP.NET Core Web API project that takes an AuthorizeRequest object and returns 

### Authorization.TestApi
This is a boilerplate project used to develop/test Authorization service. 

WeatherForecastController shows example of using Authorization attribute and IAuthorizationService to Authorize client.

Note that the endpoint address of Authorization.PolicyDecisionPoint is configured in appsettings.json PolicyDecisionPoint:Endpoint.

### Authorization.Tests
Tests associated with Authorization.Common and Authorization.PolicyDecisionPoint

## EF Core
`cd Authorization.TwinPlatform.Persistent/`

### Add new Migration
`dotnet ef migrations add [NAME] -p ../Authorization.Migrator -s ../Authorization.Migrator
`
### Migrate Down
```
dotnet ef database update [NAME] -p ../Authorization.Migrator -s ../Authorization.Migrator
dotnet ef migrations remove -p ../Authorization.Migrator -s ../Authorization.Migrator
```