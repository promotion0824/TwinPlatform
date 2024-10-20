namespace Willow.ServiceBus.HostedServices;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// A background service that listens for messages from the Service Bus.
/// </summary>
public class MessageListenerBackgroundService : BackgroundService
{
    private readonly IMessageConsumer messageConsumer;
    private readonly IEnumerable<ITopicMessageHandler> topicMessageHandlers;
    private readonly IEnumerable<IQueueMessageHandler> queueMessageHandlers;
    private readonly ILogger<MessageListenerBackgroundService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageListenerBackgroundService"/> class.
    /// </summary>
    /// <param name="messageConsumer">An instance of an IMessageConsumer.</param>
    /// <param name="topicMessageHandlers">A collection of topic message handlers.</param>
    /// <param name="queueMessageHandlers">A collection of queue message handlers.</param>
    /// <param name="logger">An ILogger instance.</param>
    public MessageListenerBackgroundService(
        IMessageConsumer messageConsumer,
        IEnumerable<ITopicMessageHandler> topicMessageHandlers,
        IEnumerable<IQueueMessageHandler> queueMessageHandlers,
        ILogger<MessageListenerBackgroundService> logger)
    {
        this.messageConsumer = messageConsumer;
        this.topicMessageHandlers = topicMessageHandlers;
        this.queueMessageHandlers = queueMessageHandlers;
        this.logger = logger;
    }

    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="stoppingToken">A asynchronous task cancellation token.</param>
    /// <returns>An asynchronous task.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogTrace("Failed to register ServiceBus Topic message handler");

        foreach (var handler in topicMessageHandlers)
        {
            using (logger.BeginScope("{MessageHandler}", handler.GetType().Name))
            using (logger.BeginScope("{Topic}", handler.TopicName))
            using (logger.BeginScope("{Subscription}", handler.SubscriptionName))
            {
                try
                {
                    await messageConsumer.StartProcessingMessagesFromTopic(handler, stoppingToken);
                    logger.LogTrace("Started listening for messages");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to register ServiceBus Topic message handler");
                    handler.OnError(ex);
                }
            }
        }

        foreach (var handler in queueMessageHandlers)
        {
            using (logger.BeginScope("{MessageHandler}", handler.GetType().Name))
            using (logger.BeginScope("{Queue}", handler.QueueName))
            {
                try
                {
                    await messageConsumer.StartProcessingMessagesFromQueue(handler, stoppingToken);
                    logger.LogTrace("Started listening for messages");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to register ServiceBus Queue message handler");
                    handler.OnError(ex);
                }
            }
        }
    }

    /// <summary>
    /// Stops the background service.
    /// </summary>
    /// <param name="cancellationToken">A asynchronous task cancellation token.</param>
    /// <returns>An asynchronous task.</returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogTrace("Stopping service bus message listeners");

        foreach (var handler in queueMessageHandlers)
        {
            logger.LogTrace("Stopping {QueueName}", handler.QueueName);
            await messageConsumer.StopProcessingMessagesFromQueue(handler, cancellationToken);
        }

        foreach (var handler in topicMessageHandlers)
        {
            logger.LogTrace("Stopping {TopicName} - {SubscriptionName}", handler.TopicName, handler.SubscriptionName);
            await messageConsumer.StopProcessingMessagesFromTopic(handler, cancellationToken);
        }
    }
}
