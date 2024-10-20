using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Willow.TelemetryGenerator.Options;

namespace Willow.TelemetryGenerator.Commands;

internal static class Telemetry
{
    public static Command CreateCommand(string[] args)
    {
        Option<string> connectorIdOption = new(["--connectorid", "-c"], "Connector Id");
        Option<string> externalIdOption = new(["--externalid", "-e"], "External Id");
        Option<string> dtIdOption = new(["--dtid", "-d"], "Twin Id");

        var telemetryCommand = new Command("telemetry", "Generate Telemetry");

        telemetryCommand.AddOption(connectorIdOption);
        telemetryCommand.AddOption(externalIdOption);
        telemetryCommand.AddOption(dtIdOption);


        Func<string, string, string, Task> handler = async (string connectorId, string externalId, string dtId) =>
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddHostedService<TelemetryGenerator>();

            builder.Services.AddEventHubSender(c => builder.Configuration.Bind("EventHub", c));

            builder.Services.Configure<TelemetryOptions>(ts =>
            {
                ts.ConnectorId = connectorId;
                ts.ExternalId = externalId;
                ts.DtId = dtId;
            });

            var host = builder.Build();
            await host.RunAsync();
        };

        telemetryCommand.SetHandler(handler, connectorIdOption, externalIdOption, dtIdOption);

        return telemetryCommand;
    }
}
