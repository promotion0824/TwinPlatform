Willow.HealthChecks
====

This package provides shared classes and methods for implementing IHealthCheck.

Please look at how these are used in rules engine.

To create a HealthCheck for a service, inherit from `HealthCheckBase<T>` like so, adding any extra status messages you wish to include on one of the allowed states (healthy, degraded or Unhealthy):

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WillowRules.HealthChecks;

namespace RulesEngine.Web.Services;

/// <summary>
/// A health check for the authorization serice
/// </summary>
public class HealthCheckAuthorizationService : HealthCheckBase<IAuthorizationService>
{
    /// <summary>
    /// Not configured
    /// </summary>
    public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Degraded("Authorization Service not configured");

    /// <summary>
    /// Failing requests
    /// </summary>
    public static readonly HealthCheckResult FailingRequests = HealthCheckResult.Degraded("Authorization Service failing requests");
}
```

Now register each of your healthcheck services with DI on Startup like so:

```csharp
    services.AddSingleton<HealthCheckAuthorizationService>();
    services.AddSingleton<HealthCheckPublicAPI>();
    services.AddSingleton<HealthCheckServiceBus>();
    services.AddSingleton<HealthCheckADX>();
```


Now add them as healthchecks using the Microsoft provided method:

```csharp
    var cancellationTokenSource = new CancellationTokenSource();

    services.AddHealthChecks()
      .AddCheck<HealthCheckFederated>("Rules Engine Processor", tags: ["healthz"])
      .AddCheck<HealthCheckPublicAPI>("Public API", tags: ["healthz"])
      .AddCheck<HealthCheckServiceBus>("Service Bus", tags: ["healthz"])
      .AddCheck<HealthCheckSearch>("Search", tags: ["healthz"])
      .AddCheck<HealthCheckADX>("ADX", tags: ["healthz"])
      .AddCheck<HealthCheckAuthorizationService>("Authorization Service", tags: ["healthz"]);
      .AddCheck("livez", () => HealthCheckResult.Healthy("System is live."), tags: ["livez"])
      .AddCheck("readyz", () =>
      {
        cancellationTokenSource.Token.ThrowIfCancellationRequested();
        return HealthCheckResult.Healthy("System is ready.")
      }, tags: ["readyz"]);
```

And finally add the healthcheck publisher:

```csharp
    services.AddSingleton<IHealthCheckPublisher>(s => new HealthCheckPublisher>("Your app name", s.GetRequiredService<ILogger<HealthCheckPublisher>>());
```

This last component allows you to get the status of the healthchecks in any action method (and not just the healthz endpoint). Rules Engine for example uses this when displaying its home page to indicate the status of its dependencies.

Now, register the healthz endpoints and give it a HealthCheckResponse Object

```csharp
    app.Lifetime.ApplicationStopping.Register(cancellationTokenSource.Cancel);
    app.Lifetime.ApplicationStopped.Register(cancellationTokenSource.Cancel);

    var healthCheckResponse = new HealthCheckResponse()
    {
        HealthCheckDescription = "<appname> Health",
        HealthCheckName = "<app name>",
    };

    app.UseWillowHealthChecks(healthCheckResponse);
```

There are 3 method you can override in the HealthCheckResponse object if needed:

- WriteHealthZResponse
- WriteLiveZResponse
- WriteReadyZResponse


Most of this code is documented in Microsoft's documentation, the main enhancement is the tree approach to collecting child statuses and rolling them up into an overall status and the addition of the assembly version by default to all system health statuses.

Take a look at the rules engine code for more examples, including a 'federated' healthcheck which calls out to a child system to check its health.

Authentication / Authorization
----
Endpoints should implement Authentication / Authorization as is done in the Marketplace code base. Non-authorized requests should get an empty payload and the 200/500 status. Authorized requests should get the full payload.

Additional Properties
----
Additional properties can be added to the application-level payload as needed for other observability requirements. These are out of scope of this readme, see general observability guidelines.

Federated Healthcheck
----
Also included is a class `HealthCheckFederated` which can be used to call down to a subsystem, to gather its status and to bundle it up into the health status for this application.

For example, Rules Engine Web (which is exposed to the internet) calls down into Rules Engine Processor (which is not exposed to the internet) to collect its status and returns it as part of the overall health status.


```csharp
    services.AddSingleton((c) =>
        new HealthCheckFederated("http://subsystem",
            c.GetRequiredService<IHttpClientFactory>(), Environment.IsDevelopment()));
```

Simplified Setup
----

The above setup can be simplified by using the `AddWillowHealthChecks` and `AddLivezReadyz` extension methods and the `UseWillowHealthChecks` extension that accepts the `CancellationTokenSource`.

```csharp
    const string appName = "MyApp";

    var cancellationTokenSource = new CancellationTokenSource();

    services.AddWillowHealthChecks(appName)
            .AddLivez()
            .AddReadyz(cancellationTokenSource);

    // Build the WebAppliation instance.

    app.UseWillowHealthChecks(appName, "My app description", cancellationTokenSource);
```

Helper methods for some common service health checks are also available.

```csharp
    services.AddWillowHealthChecks(appName).AddAuthz()
                                           .AddPublicApi();
```
