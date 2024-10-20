using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MobileXL.Options;
using Willow.ServiceBus;

namespace MobileXL.Services
{
    public interface IPushNotificationServer
    {
        Task AddOrUpdateInstallation(Guid userId, string handle, string platform);
        Task DeleteInstallation(Guid userId, string handle);
    }

    public class PushNotificationService : IPushNotificationServer
    {
        private readonly IMessageSender _installationQueue;
        private readonly PushInstallationOptions _pushInstallationOptions;
        public PushNotificationService(IMessageSender installationQueue, IOptions<PushInstallationOptions> queueOptions)
        {
            _installationQueue = installationQueue;
            _pushInstallationOptions = queueOptions.Value;
        }

        public Task AddOrUpdateInstallation(Guid userId, string handle, string platform)
        {
            return _installationQueue.Send(_pushInstallationOptions.ServiceBusName, _pushInstallationOptions.QueueName, new { UserId = userId, Handle = handle, Platform = platform, Action = "addorupdate" });
        }

        public Task DeleteInstallation(Guid userId, string handle)
        {
            return _installationQueue.Send(_pushInstallationOptions.ServiceBusName, _pushInstallationOptions.QueueName, new { UserId = userId, Handle = handle, Action = "delete" });
        }
    }
}
