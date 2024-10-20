namespace Willow.CommandAndControl.Application.BackgroundJobs;

using System.Text.Json;
using Azure.Messaging.ServiceBus;

internal class CommandStatusProcessor(IServiceProvider serviceProvider,
    ILogger<CommandStatusProcessor> logger,
    ServiceBusClient serviceBusClient,
    IOptions<ServiceBusOptions> sbOptions) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processor = serviceBusClient.CreateProcessor(sbOptions.Value.ListenStatusTopic, sbOptions.Value.ListenStatusSubscription, new ServiceBusProcessorOptions());
        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ProcessErrorAsync;
        return processor.StartProcessingAsync(stoppingToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();
        if (string.IsNullOrWhiteSpace(body))
        {
            return;
        }

        CommandExecutionResultDto result = JsonSerializer.Deserialize<CommandExecutionResultDto>(body)!;

        if (result != null)
        {
            await using var asyncScope = serviceProvider.CreateAsyncScope();
            var willowConnectorCommandSender = asyncScope.ServiceProvider.GetRequiredService<IWillowConnectorCommandSender>();
            await willowConnectorCommandSender.PostCommandExecutionResultAsync(result);
        }

        await args.CompleteMessageAsync(args.Message);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Message handler encountered an exception {message}.", args.Exception.Message);
        return Task.CompletedTask;
    }
}
