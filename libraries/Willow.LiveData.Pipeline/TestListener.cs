namespace Willow.LiveData.Pipeline;

using Microsoft.Extensions.Hosting;

/// <summary>
/// A test listener that "receives" fake telemetry every 5 seconds.
/// </summary>
internal class TestListener(ITelemetryProcessor telemetryProcessor)
    : BackgroundService
{
    private readonly ITelemetryProcessor telemetryProcessor = telemetryProcessor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Random valueGenerator = new(Guid.NewGuid().GetHashCode());

        while (true)
        {
            Telemetry telemetry = new()
            {
                SourceTimestamp = DateTime.Now.AddMinutes(-15).AddMilliseconds(-valueGenerator.Next(3000000)),
                EnqueuedTimestamp = DateTime.Now,
                ScalarValue = Math.Round(valueGenerator.NextDouble() * 10.0, 2),
                ExternalId = "WAL5678",
                ConnectorId = "a995d72c-b4ce-4b2c-9de3-9474400062e1",
            };

            await telemetryProcessor.ProcessAsync(telemetry, stoppingToken);

            Thread.Sleep(5000);
        }
    }
}

internal class TestListener<TTelemetry> : BackgroundService
    where TTelemetry : Telemetry, new()
{
    private readonly ITelemetryProcessor<TTelemetry> telemetryProcessor;

    internal TestListener(ITelemetryProcessor<TTelemetry> telemetryProcessor)
    {
        this.telemetryProcessor = telemetryProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Random valueGenerator = new(Guid.NewGuid().GetHashCode());

        while (true)
        {
            TTelemetry telemetry = new()
            {
                SourceTimestamp = DateTime.Now.AddMinutes(-15).AddMilliseconds(-valueGenerator.Next(3000000)),
                EnqueuedTimestamp = DateTime.Now,
                ScalarValue = Math.Round(valueGenerator.NextDouble() * 10.0, 2),
                ExternalId = "WAL5678",
                ConnectorId = "a995d72c-b4ce-4b2c-9de3-9474400062e1",
            };

            await telemetryProcessor.ProcessAsync(telemetry, stoppingToken);

            Thread.Sleep(5000);
        }
    }
}
