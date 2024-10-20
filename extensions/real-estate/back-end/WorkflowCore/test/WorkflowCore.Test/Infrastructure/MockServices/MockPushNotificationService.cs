using System;
using System.Threading.Tasks;
using Willow.Common;
using WorkflowCore.Services;

namespace Willow.Tests.Infrastructure.MockServices
{
    public class MockPushNotificationServer : IPushNotificationServer
    {
        public Task SendNotification(Guid correlationId, Guid customerId, Guid userId, string templateName, string defaultLanguage, object data)
        {
            return Task.CompletedTask;
        }
    }
}