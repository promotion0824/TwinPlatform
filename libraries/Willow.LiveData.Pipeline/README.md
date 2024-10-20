# Willow.LiveData.Pipeline

Simplifies reading and writing telemetry to Event Hub.

## Listening for telemetry

Create a processor class that implements `ITelemetryProcessor`.

```csharp
public class MyTelemetryProcessor : ITelemetryProcessor<MyTelemetryType>
{
    /// <summary>
    /// Processes the given telemetry.
    /// </summary>
    /// <param name="telemetry">The telemetry to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An awaitable task.</returns>
    public Task ProcessAsync(TTelemetry telemetry, CancellationToken cancellationToken = default)
    {
        // Do something with the telemetry
    }

	/// <summary>
    /// Processes the given telemetry.
    /// </summary>
    /// <param name="batch">The batch of telemetry to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An awaitable task.</returns>
    public Task ProcessAsync(IEnumerable<TTelemetry> batch, CancellationToken cancellationToken = default)
	{
		// Do something with the telemetry
	}
}
```

## Sending telemetry

Inject an instance of `ISender` into your class and call `SendAsync` to send telemetry, either individually or in a batch.
For sending telemetry of a custom type MyTelemetry, you can inject an instance of the `ISender<MyTelemetry>` into your class.

```csharp
public class MyTelemetrySender
{
	private readonly ISender _sender;

	public MyTelemetrySender(ISender sender)
	{
		_sender = sender;
	}

	public async Task Process(MyTelemetryType telemetry)
	{
		await _sender.SendAsync(telemetry);
	}

	public async Task Process(IEnumerable<MyTelemetryType> batch)
	{
		await _sender.SendAsync(batch);
	}
}
```

## Dependency Injection

Choose the one extension method to add the listener.

```csharp

public void ConfigureServices(IServiceCollection services)
{
	// Add an Event Hub sender for default Telemetry type
	services.AddEventHubSender(options => context.Configuration.Bind("EventHub", options));

    // Add an Event Hub sender for custom Telemetry type
	services.AddEventHubSender<MyTelemetryType>(options => context.Configuration.Bind("EventHub", options));

	// Add an Event Hub listener with a processor that accepts a custom telemetry type
	services.AddEventHubListener<MyTelemetryType, MyTelemetryProcessor>(options => context.Configuration.Bind("EventHub", options));

	// Add a batch Event Hub listener with a processor that accepts a custom telemetry type
	services.AddBatchEventHubListener<MyTelemetryProcessor, MyTelemetryProcessor>(options => context.Configuration.Bind("EventHub", options));

	// Add an Event Hub listener with a processor that accepts the base telemetry type
	services.AddEventHubListener<MyTelemetryProcessor>(options => context.Configuration.Bind("EventHub", options));

	// Add a batch Event Hub listener with a processor that accepts the base telemetry type
	services.AddBatchEventHubListener<MyTelemetryProcessor>(options => context.Configuration.Bind("EventHub", options));
}
```

## Configuration

For the full options available in ``EventProcessOptions`` see the [Microsoft Documentation](https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.eventhubs.primitives.eventprocessoroptions?view=azure-dotnet).

Set CompressionEnabled to true to indicate if the source data in the eventhub is compressed.

```json
{
  "EventHub": {
    "Source": {
        "FullyQualifiedNamespace": "my-event-hub.servicebus.windows.net",
        "Name": "source-event-hub",
        "ConsumerGroup": "$Default",
        "StorageAccountUri": "https://...",
        "StorageAccountName": "my-storage-account",
        "MaxBatchSize": 100,
        "CompressionEnabled": false,
        "EventProcessorOptions": {
            "PrefetchCount": 300,
        }
    },
    "Destination": {
        "FullyQualifiedNamespace": "my-event-hub.servicebus.windows.net",
        "Name": "dest-event-hub",
    }
  }
}
```

## Test Listener

To avoid connecting to an Event Hub instance, you can use the Test Listener, that will "receive" a telemetry message every 5 seconds.

```csharp
public void ConfigureServices(IServiceCollection services)
{
	// Add the test listener
	services.AddTestListener<MyTelemetryProcessor>();
}
```
