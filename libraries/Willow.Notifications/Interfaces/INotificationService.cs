namespace Willow.Notifications.Interfaces;

using Willow.Notifications.Models;

/// <summary>
/// Notification Service.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a message asynchronously to a Service Bus queue.
    /// </summary>
    /// <param name="notification">Notification message.</param>
    /// <remarks>
    /// This method creates a new ServiceBusMessage with the provided notification
    /// and sends it to the queue using the ServiceBusSender.
    /// </remarks>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task SendNotificationAsync(Notification notification);

    /// <summary>
    /// Sends a message asynchronously to a Service Bus queue.
    /// </summary>
    /// <param name="notificationMessage">Notification message.</param>
    /// <remarks>
    /// This method creates a new ServiceBusMessage with the provided notification
    /// and sends it to the queue using the ServiceBusSender.
    /// </remarks>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task SendNotificationAsync(NotificationMessage notificationMessage);

    /// <summary>
    /// Send Scheduled Notification.
    /// </summary>
    /// <param name="notification">Notification message.</param>
    /// <param name="sendOn">Scheduled DateTime.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task SendScheduledNotificationAsync(Notification notification, DateTime sendOn);
}
