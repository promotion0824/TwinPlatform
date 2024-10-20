namespace Willow.ServiceBus;

using Azure.Messaging.ServiceBus;

/// <summary>
/// The service bus client factory.
/// </summary>
public interface IServiceBusClientFactory
{
    /// <summary>
    /// Get a message sender for the specified queue or topic.
    /// </summary>
    /// <param name="serviceBusInstance">The service bus to connect to.</param>
    /// <param name="queueOrTopicName">The queue or topic name to connect to.</param>
    /// <returns>A service bus sender instance.</returns>
    ServiceBusSender? GetMessageSender(string serviceBusInstance, string queueOrTopicName);

    /// <summary>
    /// Get a message sender for the specified queue or topic.
    /// </summary>
    /// <param name="serviceBusInstance">The service bus to connect to.</param>
    /// <param name="serviceBusNamespace">The service bus namespace.</param>
    /// <param name="queueOrTopicName">The queue or topic name to connect to.</param>
    /// <returns>A service bus sender instance.</returns>
    ServiceBusSender? GetMessageSender(string serviceBusInstance, string serviceBusNamespace, string queueOrTopicName);

    /// <summary>
    /// Get a message processor for the specified queue.
    /// </summary>
    /// <param name="handler">An instance of a queue message handler.</param>
    /// <returns>A service bus processor instance.</returns>
    ServiceBusProcessor? GetMessageProcessor(IQueueMessageHandler handler);

    /// <summary>
    /// Get a message processor for the specified queue.
    /// </summary>
    /// <param name="handler">An instance of a topic message handler.</param>
    /// <returns>A service bus processor instance.</returns>
    ServiceBusProcessor? GetMessageProcessor(ITopicMessageHandler handler);
}
