using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;

namespace InsightCore.Test.Infrastructure;

public static class TestContainer
{
    public static Mock<INotificationService> NotificationService = new();
}

