# Service Health Aggregator

Reports on health for back-end services that are not a dependency of any publicly available application.

## How to add a health check

1. Ensure that the target app is set up in Envoy DNS. This give the service a generic name across all customer instances.
2. Do not expose the target app externally.
3. Edit the appsettings.json and add a new entry to the HealthChecks section.

    - Name: The name of the service
    - Url: The base URL of the service

Example:
```json
{
  "HealthChecks": [
	{
	  "Name": "MyService",
	  "Url": "http://myservice"
	}
  ]
}
```
