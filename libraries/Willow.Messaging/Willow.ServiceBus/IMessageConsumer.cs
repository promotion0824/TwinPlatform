namespace Willow.ServiceBus;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The message consumer interface.
/// </summary>
public interface IMessageConsumer
{
    /// <summary>
    /// Start the message processor for the specified queue.
    /// </summary>
    /// <param name="handler">The queue message handler.</param>
    /// <param name="stoppingToken">A cancellation token to stop the processing.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task StartProcessingMessagesFromQueue(IQueueMessageHandler handler, CancellationToken stoppingToken = default!);

    /// <summary>
    /// Start the message processor for the specified topic.
    /// </summary>
    /// <param name="handler">The topic message handler.</param>
    /// <param name="stoppingToken">A cancellation token to stop the processing.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task StartProcessingMessagesFromTopic(ITopicMessageHandler handler, CancellationToken stoppingToken = default!);

    /// <summary>
    /// Stop the message processor for the specified queue.
    /// </summary>
    /// <param name="handler">The queue message handler.</param>
    /// <param name="stoppingToken">A cancellation token to stop the processing.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task StopProcessingMessagesFromQueue(IQueueMessageHandler handler, CancellationToken stoppingToken = default!);

    /// <summary>
    /// Stop the message processor for the specified topic.
    /// </summary>
    /// <param name="handler">The topic message handler.</param>
    /// <param name="stoppingToken">A cancellation token to stop the processing.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task StopProcessingMessagesFromTopic(ITopicMessageHandler handler, CancellationToken stoppingToken = default!);
}
