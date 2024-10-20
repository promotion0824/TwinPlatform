# Willow.Hosting

Handles all the standard boilerplate code that every app should use.


It provides:

- Standard startup and shutdown logic including an application-level exception handler
- Open Telemetry / Willow Context
- Audit logger configuration
- Health checks
- Liveness and readiness checks
- Header forwarding

Not included:

- Authentication and authorization
- Swagger / OpenAPI

## Getting started

### Web

In your program.cs file, call `WebApplicationStart.Run` to start the application.

```csharp
WebApplicationStart.Run(args, "MyAppName", Configure, ConfigureApp, AddHealthChecks);

static void Configure(WebApplicationBuilder builder)
{
	// Add application-specific DI here
}

static void ConfigureApp(WebApplication app)
{
	// Add application-specific middleware here
}

static void AddHealthChecks(IHealthChecksBuilder healthChecks)
{
	// Add application-specific health checks here
}
```


### Worker

In your program.cs file, call `WorkerStart.Run` to start the application.

```csharp
WorkerStart.Run(args, "MyAppName", Configure, AddHealthChecks);

static void Configure(IWebHostEnvironment environment, IConfiguration configuration, IServiceCollection services)
{
	// Add application-specific DI here
}

static void AddHealthChecks(IHealthChecksBuilder healthChecks)
{
	// Add application-specific health checks here
}
```
