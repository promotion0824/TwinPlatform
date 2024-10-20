using System.CommandLine;

namespace Common;
public static class CommandLine
{
    public static RootCommand CreateRootCommand(string[] args, string description)
    {
        var certificateOption = new Option<string>(["--cert", "-c"], "The client certificate file")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true,
        };
        var passphraseOption = new Option<string>(["--passphrase", "-p"], "The certificate passphrase")
        {
            Arity = ArgumentArity.ExactlyOne,
        };
        var subscriptionOption = new Option<string>(["--subscription", "-s"], "The MQTT subscription ID")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true,
        };
        var mqttOption = new Option<string>(["--mqtt", "-m"], "The MQTT URL")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true,
        };

        certificateOption.AddValidator(result =>
        {
            if (!File.Exists(result.Tokens[0].Value))
            {
                result.ErrorMessage = "Certificate file does not exist";
            }
        });

        RootCommand rootCommand = new(description);

        rootCommand.AddOption(subscriptionOption);
        rootCommand.AddOption(certificateOption);
        rootCommand.AddOption(passphraseOption);
        rootCommand.AddOption(mqttOption);

        return rootCommand;
    }

    public static RootCommand CreateRootCommand(string[] args, string description, Func<string, string, string, string, Task> handler)
    {
        var certificateOption = new Option<string>(["--cert", "-c"], "The client certificate file")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true,
        };
        var passphraseOption = new Option<string>(["--passphrase", "-p"], "The certificate passphrase")
        {
            Arity = ArgumentArity.ExactlyOne,
        };
        var subscriptionOption = new Option<string>(["--subscription", "-s"], "The MQTT subscription ID")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true,
        };
        var mqttOption = new Option<string>(["--mqtt", "-m"], "The MQTT URL")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true,
        };

        certificateOption.AddValidator(result =>
        {
            if (!File.Exists(result.Tokens[0].Value))
            {
                result.ErrorMessage = "Certificate file does not exist";
            }
        });

        RootCommand rootCommand = new(description);

        rootCommand.AddOption(certificateOption);
        rootCommand.AddOption(passphraseOption);
        rootCommand.AddOption(subscriptionOption);
        rootCommand.AddOption(mqttOption);

        rootCommand.SetHandler(handler, subscriptionOption, certificateOption, passphraseOption, mqttOption);

        return rootCommand;
    }
}
