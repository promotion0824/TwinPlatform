namespace Willow.ServiceBus;

using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

/// <summary>
/// The message sender.
/// </summary>
public class MessageSender : IMessageSender
{
    private readonly IServiceBusClientFactory serviceBusClientFactory;
    private readonly ILogger<MessageSender> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageSender"/> class.
    /// </summary>
    /// <param name="serviceBusClientFactory">The service bus client factory to return an instance of the service bus client.</param>
    /// <param name="logger">An ILogger instance.</param>
    public MessageSender(
        IServiceBusClientFactory serviceBusClientFactory,
        ILogger<MessageSender> logger)
    {
        this.serviceBusClientFactory = serviceBusClientFactory;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> Send<T>(
        string serviceBusInstance,
        string queueOrTopicName,
        T messageObject,
        IDictionary<string, object>? messageProperties = null,
        string messageId = "",
        CancellationToken cancellationToken = default)
    {
        var sender = serviceBusClientFactory.GetMessageSender(serviceBusInstance, queueOrTopicName);

        if (sender != null)
        {
            return await Send(sender, queueOrTopicName, messageObject, messageProperties, messageId, cancellationToken);
        }

        return string.Empty;
    }

    /// <inheritdoc/>
    public async Task<string> Send<T>(
        string serviceBusInstance,
        string serviceBusNamespace,
        string queueOrTopicName,
        T messageObject,
        IDictionary<string, object>? messageProperties = null,
        string messageId = "",
        CancellationToken cancellationToken = default)
    {
        var sender = serviceBusClientFactory.GetMessageSender(serviceBusInstance, serviceBusNamespace, queueOrTopicName);

        if (sender is null)
        {
            return string.Empty;
        }

        return await Send(sender, queueOrTopicName, messageObject, messageProperties, messageId, cancellationToken);
    }

    private async Task<string> Send<T>(
        ServiceBusSender sender,
        string queueOrTopicName,
        T messageObject,
        IDictionary<string, object>? messageProperties = null,
        string messageId = "",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(messageId))
        {
            messageId = Guid.NewGuid().ToString();
        }

        var messageBody = JsonSerializer.Serialize(messageObject);

        using (logger.BeginScope("{MessageId}", messageId))
        using (logger.BeginScope("{QueueOrTopic}", queueOrTopicName))
        using (logger.BeginScope("{ServiceBusNamespace}", sender.FullyQualifiedNamespace))
        using (logger.BeginScope("{MessageBody}", messageBody))
        {
            logger.LogTrace("Sending {MessageType} message", typeof(T).Name);
            var message = new ServiceBusMessage(messageBody)
            {
                ContentType = MediaTypeNames.Application.Json,
                MessageId = messageId,
            };

            if (messageProperties is not null)
            {
                foreach (var (key, value) in messageProperties)
                {
                    message.ApplicationProperties.TryAdd(key, value);
                }
            }

            await sender.SendMessageAsync(message, cancellationToken);
            logger.LogTrace("{MessageType} message sent", message.GetType().Name);

            return message.MessageId;
        }
    }
}
