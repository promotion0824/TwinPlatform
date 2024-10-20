namespace Willow.ServiceBus;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The message sender interface.
/// </summary>
public interface IMessageSender
{
    /// <summary>
    /// Send a message to the specified queue or topic.
    /// </summary>
    /// <typeparam name="T">The type of the object to be sent.</typeparam>
    /// <param name="serviceBusInstance">The name of the service bus instance.</param>
    /// <param name="queueOrTopicName">The service bus queue or topic name.</param>
    /// <param name="messageObject">The message object to send.</param>
    /// <param name="messageProperties">The properties of the message to add to the send request.</param>
    /// <param name="messageId">The message id for tracking purposes.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<string> Send<T>(
        string serviceBusInstance,
        string queueOrTopicName,
        T messageObject,
        IDictionary<string, object>? messageProperties = null,
        string messageId = "",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a message to the specified queue or topic.
    /// </summary>
    /// <typeparam name="T">The type of the object to be sent.</typeparam>
    /// <param name="serviceBusInstance">The name of the service bus instance.</param>
    /// <param name="serviceBusNamespace">The name of the service bus namespace.</param>
    /// <param name="queueOrTopicName">The service bus queue or topic name.</param>
    /// <param name="messageObject">The message object to send.</param>
    /// <param name="messageProperties">The properties of the message to add to the send request.</param>
    /// <param name="messageId">The message id for tracking purposes.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<string> Send<T>(
        string serviceBusInstance,
        string serviceBusNamespace,
        string queueOrTopicName,
        T messageObject,
        IDictionary<string, object>? messageProperties = null,
        string messageId = "",
        CancellationToken cancellationToken = default);
}
