namespace Willow.ServiceBus;

using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

/// <summary>
/// The message consumer.
/// </summary>
public class MessageConsumer : IMessageConsumer
{
    private readonly IServiceBusClientFactory serviceBusFactory;
    private readonly ILogger<MessageConsumer> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageConsumer"/> class.
    /// </summary>
    /// <param name="serviceBusFactory">An instance of the service bus factory that generates the service bus client.</param>
    /// <param name="logger">An instance of an ILogger.</param>
    public MessageConsumer(
        IServiceBusClientFactory serviceBusFactory,
        ILogger<MessageConsumer> logger)
    {
        this.serviceBusFactory = serviceBusFactory;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task StartProcessingMessagesFromQueue(IQueueMessageHandler handler, CancellationToken stoppingToken = default!)
    {
        var processor = serviceBusFactory.GetMessageProcessor(handler);
        if (processor != null)
        {
            await StartMessageProcessing(processor, handler, stoppingToken);
        }
    }

    /// <inheritdoc/>
    public async Task StartProcessingMessagesFromTopic(ITopicMessageHandler handler, CancellationToken stoppingToken = default!)
    {
        var processor = serviceBusFactory.GetMessageProcessor(handler);

        if (processor != null)
        {
            await StartMessageProcessing(processor, handler, stoppingToken);
        }
    }

    private async Task StartMessageProcessing(ServiceBusProcessor processor, IMessageHandler handler, CancellationToken stoppingToken = default!)
    {
        processor.ProcessMessageAsync += async (args) =>
        {
            using (logger.BeginScope("{MessageId}", args.Message.MessageId))
            using (logger.BeginScope("{MessageHandler}", handler.GetType().FullName))
            {
                logger.LogTrace("Message received for processing");

                var result = await handler.ProcessReceivedMessage(args.Message, args.CancellationToken);
                if (result.IsSuccessful)
                {
                    await args.CompleteMessageAsync(args.Message, args.CancellationToken);
                    logger.LogTrace("Message completed after processing");
                    return;
                }

                await args.DeadLetterMessageAsync(args.Message, result.Description, cancellationToken: args.CancellationToken);

                using (logger.BeginScope("{FailureReason}", result.Description))
                {
                    logger.LogError("Failed to process the message. Sent to the dead letter queue.");
                }
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            using (logger.BeginScope("{MessageHandler}", handler.GetType().FullName))
            {
                logger.LogError(args.Exception, "Exception occurred while processing the ServiceBus message.");
            }

            handler.OnError(args.Exception);
            return Task.CompletedTask;
        };

        if (!processor.IsProcessing)
        {
            await processor.StartProcessingAsync(stoppingToken);
        }
    }

    /// <inheritdoc/>
    public async Task StopProcessingMessagesFromQueue(IQueueMessageHandler handler, CancellationToken stoppingToken = default!)
    {
        var processor = serviceBusFactory.GetMessageProcessor(handler);

        if (processor != null)
        {
            await processor.StopProcessingAsync(stoppingToken);
        }
    }

    /// <inheritdoc/>
    public async Task StopProcessingMessagesFromTopic(ITopicMessageHandler handler, CancellationToken stoppingToken = default!)
    {
        var processor = serviceBusFactory.GetMessageProcessor(handler);

        if (processor != null)
        {
            await processor.StopProcessingAsync(stoppingToken);
        }
    }
}
