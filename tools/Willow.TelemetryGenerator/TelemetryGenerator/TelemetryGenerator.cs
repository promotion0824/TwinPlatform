using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Willow.LiveData.Pipeline;
using Willow.TelemetryGenerator.Options;
using static System.Console;
using static System.ConsoleColor;

namespace Willow.TelemetryGenerator;
internal class TelemetryGenerator : IHostedService
{
    private readonly System.Timers.Timer _timer = new(TimeSpan.FromSeconds(5));
    private readonly ISender _sender;
    private readonly TelemetryOptions _testSettings;

    public TelemetryGenerator(ISender sender, IOptions<TelemetryOptions> testSettingsOptions)
    {
        _sender = sender;
        _testSettings = testSettingsOptions.Value;

        _timer.Elapsed += Send;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Stop();
        return Task.CompletedTask;
    }

    public void Send(object? _o, EventArgs _e)
    {
        ForegroundColor = Yellow;
        WriteLine("Sending telemetry...");
        ForegroundColor = Blue;
        LiveData.Pipeline.Telemetry telemetry = new()
        {
            ConnectorId = _testSettings.ConnectorId,
            ExternalId = _testSettings.ExternalId,
            DtId = _testSettings.DtId,
            EnqueuedTimestamp = DateTime.UtcNow,
            SourceTimestamp = DateTime.UtcNow.AddMinutes(-15),
            ScalarValue = Math.Round(Random.Shared.NextDouble() * 10.0, 2),
        };
        WriteLine(JsonConvert.SerializeObject(telemetry, Formatting.Indented));
        ForegroundColor = Gray;
        _sender.SendAsync(telemetry, default).Wait();
    }
}

