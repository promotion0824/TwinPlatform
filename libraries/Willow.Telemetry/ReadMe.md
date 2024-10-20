# Willow.Telemetry

## Description

Using this library allows worker services to emit a consistent set of dimensions for both OpenTelemetry metrics
to allow support to group by or filter by the following in Application Insights:

<b>FullCustomerInstanceName:</b> (<EnvironmentShortName>:<RegionShortName>:<StampName><CustomerInstanceName> i.e. the prd:eus2:11:brk-uat)
<b>AppVersion:</b> (i.e. 1.0.0) - Note that this is auto populated from the AssemblyVersion
<b>AppName:</b> (i.e. Willow.MappedTopologyIngestion) - Note that be default, this is populated from Assembly.GetEntryAssembly()?.GetName()?.Name, or can be overridden by setting the WillowContextOptions.AppName property
<b>AppRoleInstanceId:</b> (i.e. dev:eus:01:wil-in1:Willow.MappedTopologyIngestion:mti--hnanrgb-6c74dcdbd4-x2l8r) This value is auto populated from the above values, and is used to uniquely identify the instance of the application. Note that this is the value that is used to group by or filter by in Application Insights.
<b>ReplicaName:</b> (i.e. mti--hnanrgb-6c74dcdbd4-x2l8r) - Note that this is auto populated from the CONTAINER_APP_REPLICA_NAME environment variable, or set to "01" if not found. This is not used as a dimension in the metrics by itself, but is used to populate the AppRoleInstanceId dimension.

## Configuration

These values can be read either from an appsettings.json file in the form of the WillowContextOptions object:

``` json

"WillowContext": {
    "AppName: "MappedTopologyIngestion",
    "EnvironmentConfiguration": {
      "ShortName": "dev"
    },
    "RegionConfiguration": {
      "ShortName": "wus2"
    },
    "StampConfiguration": {
      "Name": "01"
    },
    "CustomerInstanceConfiguration": {
      "CustomerSalesId": "joeb",
      "CustomerInstanceName": "msft-uat",
    }
  }

```

Or from Environment variables:

         "env": {
          "APPLICATIONINSIGHTS__CONNECTIONSTRING": "%WillowTwin_ApplicationInsightsConnectionString%",
          "WillowContext__EnvironmentConfiguration__ShortName": "%WillowTwin_EnvironmentCode%",
          "WillowContext__RegionConfiguration__ShortName": "%RegionShortName%",
          "WillowContext__StampConfiguration__Name": "%StampName%",
          "WillowContext__CustomerInstanceConfiguration__CustomerSalesId": "%CustomerSalesId%",
          "WillowContext__CustomerInstanceConfiguration__CustomerInstanceName": "%WillowTwin_CustomerCode%",
        }

## Set up

Add a nuget reference to Willow.Telemetry.Web

In your program.cs:

Add the following using statement:

``` csharp

using Willow.Telemetry.Web;

```

In your program.cs, add the following call inside your hostBuilder.ConfigureServices method:

``` csharp

services.AddWillowContext(hostContext.Configuration)

```

In your Startup.cs, add the following call inside your Configure method. Note that this only works for apps compiled with the .NET8 framework and above. This ensures the WillowContext is available as metrics dimensions.

``` csharp

app.UseWillowContext(Configuration);

```


## Limitations

Note that Azure Monitor is limited to <b>10</b> custom dimensions per metric. We use 4 for the above settings, which means that at most, we can only add six other custom
dimensions to other metrics. When this limit is exceeded, the Custom Metrics logs table will show the row, but the Azure Application Insights will not.

## OpenTelemetry versus Application Insights

Long term direction from Microsoft is that the Application Insights SDK packages are being deprecated in favor of OpenTelemetry. However, currently both the Application Insights SDK and OpenTelemetry SDK send metrics and logs to the same
back end data store. That will change in the future, and the OpenTelemetry SDK will send data to a different back end data store which will have many more capabilities than the current Application Insights data store. The IAD team at Willow is working with the OpenTelemetry team at Microsoft to ensure that
the new data store will work for our needs. In the meantime, we are using the OpenTelemetry SDK to send data to the Application Insights data store.

## Where can I find my data?

The quick answers are Grafana or Application Insights.

### In Application Insights:
- Your should see your metrics in the "CustomMetrics" table.
- Your trace logs in the "Traces" table.
- Your exceptions in the "Exceptions" table.
- Your request logs in the Requests table.
- You can also use the "Metrics Explorer" to view your metrics in Application Insights, or use Grafana.

### In Log Analytics:
- You should see your metrics in the "AppMetrics" table.
- Your trace logs in the "AppTraces" table.
- Your exceptions in the "AppExceptions" table.
- Your request logs in the AppRequests table.

Note that in Application Insights Log Tables, the Cloud_RoleName and Cloud_RoleInstance columns map to the AppRoleName and AppRoleInstance columns in LogAnalytics.

## Advanced Configuration

### Automated Http Service Metrics

If you are in an ASPNETCore application, all incoming HTTP calls can also have metrics recorded by setting the following configuration in your appsettings.json

``` json
  "OpenTelemetry": {
    "AddAspNetCoreInstrumentation": true
  }
```

Look for the Metric Namespace "Azure.ApplicationInsights" and the metric "http.server.request.duration"

### Automated Http Client Metrics

If you are in an ASPNETCore, all outgoing HTTP calls can also have metrics recorded by setting the following configuration in your appsettings.json

``` json
  "OpenTelemetry": {
    "AddHttpClientInstrumentation": true
    }
```

Look for the Metric Namespace "Azure.ApplicationInsights" and the metric "http.client.request.duration"

### Console Logging

You can turn console logging on or off by setting the following configuration in your appsettings.json. Note that Console logging is off by default, and notoriously verbose. It should only be used for debugging purposes and turned off in production.

``` json
  "OpenTelemetry": {
    "ConsoleLogging": true
  }
```

### Sampling

By default, OpenTelemetry will sample 100% of the logs. This can be changed by setting the following configuration in your appsettings.json. This uses a sampling ratio of 10, which means that 10% of the logs will be sent to Application Insights.
The system uses a TraceIdRatioBasedSampler, which means that all logs for a given trace will be sent to Application Insights, or none of them will be sent.

``` json
  "OpenTelemetry": {
    "SamplingRatio": 10
  }
```

### Filtering

Log filtering is done in 2 ways:
    1. Inside the Willow.Telemetry.Web package, if AspNetCoreInstrumentation is enabled, the package will filter out all <b>AppRequest Logs</b> for "/healthz", "/readyz", and "/livez" endpoints.
    2. You can also filter out <b>Trace logs</b> by setting the following configuration in your appsettings.json. This example filters out all Information logs which originate from the "Microsoft" or "Azure" namespace.

``` json
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Azure": "Warning",
          "Microsoft": "Warning",
          "Willow": "Information"
        }
     },
  ```

<b>Note:</b> Currently it is not possible to filter metrics in the OpenTelemetry SDK. This is a known issue and is being worked on by the OpenTelemetry team.

## Audit Logging

Logging user actions is important for security and compliance.  The Willow.Telemetry library provides a way to log user actions in a consistent way.

Use the `IAuditLogger` interface to log user actions. The following code snippet shows how to log a user action:

``` csharp
public class MyAction(IAuditLogger auditLogger, ISomeContext context, IAuthCheck authCheck)
{
	public async Task Execute()
	{
        if (!authCheck.IsAuthorized(context))
		{
            // Log the failed action
            auditLogger.LogWarning(context.UserId, "Unauthorised attempt to call MyAction");
            return;
		})

        // Do something

		// Log the user action
		auditLogger.LogInformation(context.UserId, "MyAction completed");

		return;
	}
}
```
