using System;
using System.Threading.Tasks;
using Willow.Common;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;

namespace WorkflowCore.Services
{
    public interface IPushNotificationServer
    {
        Task SendNotification(Guid correlationId, Guid customerId, Guid userId, string templateName, string defaultLanguage, object data = null);
    }

    public class PushNotificationService : IPushNotificationServer
    {
        private readonly INotificationService _notificationService;

        public PushNotificationService(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public Task SendNotification(Guid correlationId, Guid customerId, Guid userId, string templateName, string defaultLanguage, object data)
        {
            return _notificationService.SendNotificationAsync(new Notification
            {
                CorrelationId = correlationId,
                CommunicationType = CommunicationType.PushNotification,
                CustomerId = customerId,
                Data = data?.ToDictionary(),
                Tags = null,
                TemplateName = templateName,
                UserId = userId,
                Locale = defaultLanguage
            });
           
        }
    }
}
