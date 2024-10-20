using System.CommandLine;
using Common;
using static System.Console;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        WriteLine("Starting Sender...");

        int returnCode = 0;

        Func<string, string, string, string, string, string, string, Task> handler = async (subscriptionOptionValue, certificateOptionValue, passphraseOptionValue, mqttOptionValue, destinationSubscriptionValue, connectorIdValue, externalIdValue) =>
        {
            if (String.IsNullOrEmpty(passphraseOptionValue))
            {
                Write("Certificate passphrase: ");
                passphraseOptionValue = ReadLine()!;
            }

            Title = $"{subscriptionOptionValue} - receiver";
            returnCode = await Sender.Start(subscriptionOptionValue, certificateOptionValue, passphraseOptionValue, mqttOptionValue, destinationSubscriptionValue, connectorIdValue, externalIdValue);

            if (returnCode != 0) return;

            SetCursorPosition(0, 4);
            WriteLine("Press enter to exit.");
            ReadLine();
        };

        var rootCommand = Common.CommandLine.CreateRootCommand(args, "Sends messages to MQTT");

        var destinationSubscriptionOption = new Option<string>(["--destination", "-d", "--dest"], "The destination subscription ID")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true,
        };

        var connectorIdOption = new Option<string>(["--connectorid", "-ci", "--connid"], "The Connector ID")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true,
        };

        var externalIdOption = new Option<string>(["--externalid", "-ei", "--extid"], "The External ID")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true,
        };

        rootCommand.AddOption(destinationSubscriptionOption);
        rootCommand.AddOption(connectorIdOption);
        rootCommand.AddOption(externalIdOption);


        rootCommand.SetHandler(handler, (rootCommand.Options[0] as Option<string>)!, (rootCommand.Options[1] as Option<string>)!, (rootCommand.Options[2] as Option<string>)!, (rootCommand.Options[3] as Option<string>)!, (rootCommand.Options[4] as Option<string>)!, (rootCommand.Options[5] as Option<string>)!, (rootCommand.Options[6] as Option<string>)!);

        await rootCommand.InvokeAsync(args);

        return returnCode;
    }


}
