using System.CommandLine;
using System.CommandLine.Binding;
using Willow.SubscriptionUpload;

Command command = new RootCommand
{
    new Option<string>(["--file", "-f"], "The name of the file containing the subscriptions"),
    new Option<string>(["--subscription", "-s"], "The ID of the subscription"),
    new Option<string?>(["--conn", "-c"], "The storage table connection string"),
    new Option<Uri?>(["--url", "-u"], "The storage table URL"),
};

Func<string, string, string?, Uri?, Task> handler = SubscriptionUpload.Start;

command.SetHandler(handler, (command.Options[0] as IValueDescriptor<string>)!, (command.Options[1] as IValueDescriptor<string>)!, (command.Options[2] as IValueDescriptor<string?>)!, (command.Options[3] as IValueDescriptor<Uri?>)!);

await command.InvokeAsync(args);
