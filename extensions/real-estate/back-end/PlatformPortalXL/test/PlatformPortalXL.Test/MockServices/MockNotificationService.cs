using System;
using System.Threading.Tasks;
using Willow.Notifications.Models;
using Willow.Notifications.Interfaces;

namespace PlatformPortalXL.Test.MockServices;
public class MockNotificationService : INotificationService
{
    public Task SendNotificationAsync(Notification notification)
    {
        return Task.CompletedTask;
    }

    public Task SendNotificationAsync(NotificationMessage notificationMessage)
    {
        return Task.CompletedTask;
    }

    public Task SendScheduledNotificationAsync(Notification notification, DateTime sendOn)
    {
        return Task.CompletedTask;
    }

}
