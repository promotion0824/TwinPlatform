using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;

namespace DirectoryCore.Test.MockServices;

public class MockNotificationService : INotificationService
{
    private readonly IDictionary<string, string> _emails = new Dictionary<string, string>();

    public IDictionary<string, string> GetEmails()
    {
        return _emails;
    }

    public Task SendNotificationAsync(Notification notification)
    {
        _emails.Add(notification.TemplateName, notification.Locale);
        return Task.CompletedTask;
    }

    public Task SendNotificationAsync(NotificationMessage notificationMessage)
    {
        return Task.CompletedTask;
    }

    public Task SendScheduledNotificationAsync(Notification notification, DateTime sendOn)
    {
        _emails.Add(notification.TemplateName, notification.Locale);
        return Task.CompletedTask;
    }
}
