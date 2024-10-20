using System;
using System.Threading.Tasks;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;

namespace MobileXL.Test.Infrastructure.MockServices;

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
