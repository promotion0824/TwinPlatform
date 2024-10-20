using System.CommandLine;

RootCommand rootCommand = new("Message Generator");

Option<int> frequency = new(["--frequency", "-f"], () => 5, "Frequency of messages in seconds");

rootCommand.AddGlobalOption(frequency);

rootCommand.AddCommand(Willow.TelemetryGenerator.Commands.Telemetry.CreateCommand(args));
rootCommand.AddCommand(Willow.TelemetryGenerator.Commands.Snowflake.CreateCommand(args));

await rootCommand.InvokeAsync(args);
