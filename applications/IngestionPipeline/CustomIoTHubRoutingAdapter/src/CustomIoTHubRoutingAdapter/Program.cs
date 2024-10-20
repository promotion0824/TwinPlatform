using Microsoft.AspNetCore.Hosting;
using Willow.CustomIoTHubRoutingAdapter;
using Willow.CustomIoTHubRoutingAdapter.Models;
using Willow.CustomIoTHubRoutingAdapter.Options;
using Willow.Hosting.Worker;

return WorkerStart.Run(args, "CustomIoTHubRoutingAdapter", ConfigureServices);

void ConfigureServices(IWebHostEnvironment env, IConfiguration configuration, IServiceCollection services)
{
    services.AddIotHubListener<UnifiedTelemetryMessage, TelemetryProcessor>(config => configuration.Bind("EventHub", config));
    services.AddEventHubSender(config => configuration.Bind("EventHub", config));

    services.Configure<ConnectorIdOption>(options =>
    {
        var connectorIds = configuration[ConnectorIdOption.Section];
        var connectorIdList = connectorIds?.Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).ToList();
        if (connectorIdList is null || connectorIdList.Count == 0)
        {
            Console.WriteLine("List of connectorIds to route through must be specified in 'ConnectorIdList'. Exiting");
            throw new ArgumentNullException("ConnectorIdList");
        }

        options.ConnectorIdList = connectorIdList;
    });
}
