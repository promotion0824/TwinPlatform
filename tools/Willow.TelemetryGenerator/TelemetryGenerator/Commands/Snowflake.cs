using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Willow.TelemetryGenerator.Options;

namespace Willow.TelemetryGenerator.Commands;

internal static class Snowflake
{
    public static Command CreateCommand(string[] args)
    {
        Option<Uri> uriOption = new(["--uri", "-u"], "Event Grid URI");
        Option<string?> keyOption = new(["--key", "-k"], "Event Grid Key")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

        Option<string?> errorTypeOption = new(["--err", "-e"], () => "task", "Error Type - Task or SnowPipe");

        var snowflakeErrors = new Command("snowflake", "Generate Snowflake pipeline errors");

        snowflakeErrors.AddOption(uriOption);
        snowflakeErrors.AddOption(keyOption);
        snowflakeErrors.AddOption(errorTypeOption);

        Func<Uri, string?, string?, Task> handler = async (Uri uri, string? key, string? errorType) =>
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddHostedService<SnowflakeErrorGenerator>();

            builder.Services.Configure<EventGridOptions>(c =>
            {
                c.TopicEndpoint = uri;
                c.Key = key;
                c.ErrorType = errorType;
            });

            var host = builder.Build();
            await host.RunAsync();
        };

        snowflakeErrors.SetHandler(handler, uriOption, keyOption, errorTypeOption);

        return snowflakeErrors;
    }
}
