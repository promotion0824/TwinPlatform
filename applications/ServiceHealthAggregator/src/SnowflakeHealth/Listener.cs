namespace Willow.ServiceHealthAggregator.Snowflake;

using System.Diagnostics.Metrics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.ServiceHealthAggregator.Snowflake.Options;

internal class Listener(ServiceBusClient client, Meter meter, IMessageForwarder messageForwarder, IOptions<SnowflakeOptions> options, ILogger<Listener> logger) : BackgroundService
{
    private readonly Counter<long> messageCounter = meter.CreateCounter<long>("SnowflakePipelineError", description: "Number of Snowflake pipeline errors");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ServiceBusReceiver receiver = client.CreateReceiver(options.Value.ServiceBus.QueueName);

        while (!stoppingToken.IsCancellationRequested)
        {
            var message = await receiver.ReceiveMessageAsync(cancellationToken: stoppingToken);

            if (message == null)
            {
                continue;
            }

            await ReceiveMessageAsync(message, stoppingToken);
            await receiver.CompleteMessageAsync(message, stoppingToken);
        }

        await receiver.DisposeAsync();
    }

    private async Task ReceiveMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        if (message?.Body == null)
        {
            logger.LogWarning("Received message with no body");
            return;
        }

        string data = message.Body.ToString();

        // Assume this is a serialized Event Grid event
        try
        {
            EventGridEvent? eventGridEvent = JsonSerializer.Deserialize<EventGridEvent>(data);

            if (eventGridEvent != null)
            {
                data = eventGridEvent.Data.ToString();
            }
        }
        catch (JsonException ex)
        {
            logger.LogWarning("Unable to parse message body as EventGridEvent: {Exception}", ex);
            return;
        }

        Notification notification = new()
        {
            Id = message.MessageId,
            Data = data,
            EnqueuedTime = message.EnqueuedTime,
            Subject = message.Subject,
        };

        messageCounter.Add(1, new KeyValuePair<string, object?>("TaskOrPipeName", notification.TaskOrPipeName));

        await messageForwarder.ForwardAsync(notification, cancellationToken);
    }
}
