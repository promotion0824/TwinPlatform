namespace Willow.ServiceBus;

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

/// <summary>
/// The message handler interface.
/// </summary>
public interface IMessageHandler
{
    /// <summary>
    /// Gets the service bus instance.
    /// </summary>
    string ServiceBusInstance { get; }

    /// <summary>
    /// Gets the service bus processor options.
    /// </summary>
    ServiceBusProcessorOptions? ServiceBusProcessorOptions => null;

    /// <summary>
    /// Process a received message.
    /// </summary>
    /// <param name="receivedMessage">A message from the service bus.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<MessageProcessingResult> ProcessReceivedMessage(
        ServiceBusReceivedMessage receivedMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process an exception.
    /// </summary>
    /// <param name="ex">The exception to process.</param>
    void OnError(Exception ex);
}

/// <summary>
/// A message handler for a queue.
/// </summary>
public interface IQueueMessageHandler : IMessageHandler
{
    /// <summary>
    /// Gets the queue name.
    /// </summary>
    string QueueName { get; }
}

/// <summary>
/// A message handler for a topic.
/// </summary>
public interface ITopicMessageHandler : IMessageHandler
{
    /// <summary>
    /// Gets the topic name.
    /// </summary>
    string TopicName { get; }

    /// <summary>
    /// Gets the subscription name.
    /// </summary>
    string SubscriptionName { get; }
}
