using System;
using System.Threading.Tasks;

namespace Willow.Common
{
    /// <summary>
    /// Interface for sending messages
    /// </summary>
    public interface IMessageQueue
    {
        /// <summary>
        /// Send a message
        /// </summary>
        /// <param name="message">Message to send. Could be plain text or a json object</param>
        /// <param name="sendOn">Optional dateTime when to send</param>
        Task Send(string message, DateTime? sendOn = null);
    }

    /// <summary>
    /// Interface for creating IMessageQueue(s)
    /// </summary>
    public interface IMessageQueueFactory
    {
        /// <summary>
        /// Create a message queue
        /// </summary>
        /// <param name="queueName">Name of message queue to create</param>
        IMessageQueue CreateQueue(string queueName);
    }
}
