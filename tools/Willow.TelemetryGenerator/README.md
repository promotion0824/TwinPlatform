# Message Generator

Generates messages for different test scenarios.

## Telemetry

Generates telemetry data in the Willow format and posts to an Event Hub.

### Usage

Add the event hub name and fully qualified namespace to the appsettings.json file.

```json
{
  "EventHubName": "eventhubname",
  "EventHubNamespace": "namespace.servicebus.windows.net"
}
```

Execute the following command to generate telemetry data.

```console
telgen telemetry --connectorid <Connector Id> --externalId <External Id> --dtid <Twin Id> --frequency <Frequency in seconds>
```

## Snowflake

Generates Snowflake pipeline errors and posts to Azure Event Grid.

### Usage

Execute the following command to generate Snowflake pipeline errors.

```console
telgen snowflake --uri <Event Grid Topic URI> --frequency <Frequency in seconds>
```
