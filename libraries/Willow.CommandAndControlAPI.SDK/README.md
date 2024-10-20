# Command And Control SDK

## Getting Started

Add the following to your host builder:

```csharp
var options = new CommandAndControlClientOption
{
    BaseAddress = "http://localhost:5000",
};

services.AddCommandAndControlAPIHttpClient(options);
```

## Usage

```csharp
public class MyService(ICommandAndControlClient client)
{
    public async Task DoSomething(CancellationToken token = default)
    {
        var commands = new PostRequestedCommandsDto
        {
            Commands =
            [
                new RequestedCommandDto
                {
                    CommandName = "Do Something",
                    // Other properties
                }
            ],
        };

        await client.PostRequestedCommands(commands, token);
    }
}
```
