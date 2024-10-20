using Common;
using System.CommandLine;
using static System.Console;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        WriteLine("Starting Receiver...");

        int returnCode = 0;

        Func<string, string, string, string, Task> handler = async (subscriptionOptionValue, certificateOptionValue, passphraseOptionValue, mqttOptionValue) =>
        {
            if (String.IsNullOrEmpty(passphraseOptionValue))
            {
                Write("Certificate passphrase: ");
                passphraseOptionValue = ReadLine()!;
            }

            Title = $"{subscriptionOptionValue} - receiver";
            returnCode = await Receiver.Start(subscriptionOptionValue, certificateOptionValue, passphraseOptionValue, mqttOptionValue);

            if (returnCode != 0) return;

            SetCursorPosition(0, 4);
            WriteLine("Press enter to exit.");
            ReadLine();
        };

        var rootCommand = CommandLine.CreateRootCommand(args, "Receives messages from MQTT", handler);

        await rootCommand.InvokeAsync(args);

        return returnCode;
    }
}
