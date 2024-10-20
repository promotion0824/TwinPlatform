namespace Willow.Notifications.Services;

using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;

/// <summary>
/// Notification Service.
/// </summary>
public class NotificationService : INotificationService, IAsyncDisposable
{
    private readonly ServiceBusClient serviceBusClient;
    private readonly ServiceBusSender sender;
    private readonly NotificationsServiceOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="serviceBusClient">serviceBusClient.</param>
    /// <param name="options">configured options.</param>
    public NotificationService(ServiceBusClient serviceBusClient, IOptions<NotificationsServiceOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options.Value);
        this.options = options.Value;
        this.serviceBusClient = serviceBusClient;
        sender = this.serviceBusClient.CreateSender(this.options.QueueOrTopicName);
    }

    /// <summary>
    /// Sends a message asynchronously to a Service Bus queue.
    /// </summary>
    /// <param name="notification">Notification message.</param>
    /// <remarks>
    /// This method creates a new ServiceBusMessage with the provided notification
    /// and sends it to the queue using the ServiceBusSender.
    /// </remarks>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public Task SendNotificationAsync(Notification notification)
    {
        var message = JsonConvert.SerializeObject(notification);
        var serviceBusMessage = new ServiceBusMessage(message);
        return sender.SendMessageAsync(serviceBusMessage);
    }

    /// <summary>
    /// Sends a message asynchronously to a Service Bus queue.
    /// </summary>
    /// <param name="notificationMessage">Notification message.</param>
    /// <remarks>
    /// This method creates a new ServiceBusMessage with the provided notification
    /// and sends it to the queue using the ServiceBusSender.
    /// </remarks>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public Task SendNotificationAsync(NotificationMessage notificationMessage)
    {
        var message = JsonConvert.SerializeObject(notificationMessage);
        var serviceBusMessage = new ServiceBusMessage(message);
        return sender.SendMessageAsync(serviceBusMessage);
    }

    /// <summary>
    /// Send Scheduled Notification.
    /// </summary>
    /// <param name="notification">Notification message.</param>
    /// <param name="sendOn">Scheduled DateTime.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public Task SendScheduledNotificationAsync(Notification notification, DateTime sendOn)
    {
        var message = JsonConvert.SerializeObject(notification);
        var serviceBusMessage = new ServiceBusMessage(message);

        // Ensure scheduled DateTime is UTC
        var dateTimeOffset = new DateTimeOffset(sendOn.Year, sendOn.Month, sendOn.Day, sendOn.Hour, sendOn.Minute, sendOn.Second, TimeSpan.FromSeconds(0));
        return sender.ScheduleMessageAsync(serviceBusMessage, dateTimeOffset);
    }

    /// <summary>
    /// Clean up resources.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await sender.CloseAsync();
    }
}
