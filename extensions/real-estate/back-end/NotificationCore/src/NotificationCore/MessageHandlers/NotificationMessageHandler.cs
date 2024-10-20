using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NotificationCore.Infrastructure.Configuration;
using NotificationCore.Models;
using NotificationCore.Services;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Willow.ServiceBus;

namespace NotificationCore.MessageHandlers;

public class NotificationMessageHandler : ITopicMessageHandler
{
    private readonly NotificationTopic _topicOptions;
    private readonly ILogger<NotificationMessageHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    public NotificationMessageHandler(IOptions<NotificationTopic> topicOptions, ILogger<NotificationMessageHandler> logger, IServiceProvider serviceProvider)
    {
        _topicOptions = topicOptions.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    public string TopicName => _topicOptions.TopicName;

    public string SubscriptionName => _topicOptions.SubscriptionName;

    public string ServiceBusInstance => _topicOptions.ServiceBusName;
    public void OnError(Exception ex)
    {
        _logger.LogInformation($"{ex.Message}");
    }
    public async Task<MessageProcessingResult> ProcessReceivedMessage(ServiceBusReceivedMessage receivedMessage, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return MessageProcessingResult.Failed("Message processing cancelled");
        }
        try
        {
            var msgBody = Encoding.UTF8.GetString(receivedMessage.Body.ToArray());
            var eventNotificationMessage = JsonConvert.DeserializeObject<EventMessageNotification>(msgBody);
            using (var scope = _serviceProvider.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var notificationMessage = EventMessageNotification.MapTo(eventNotificationMessage);
                await notificationService.CreateNotificationAsync(notificationMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            return MessageProcessingResult.Failed("Message processing failed");
        }


        // Process the message
        return MessageProcessingResult.Success();
    }
}

