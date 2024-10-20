using System;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;

using Willow.Common;
using Azure.Core;
using Azure.Identity;

namespace Willow.ServiceBus
{
    /// <summary>
    /// Send messages to ServiceBus message queues or topics
    /// </summary>
    public class ServiceBus : IMessageQueue, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

        public ServiceBus(string connectionString, string queueOrTopicName)
        {
            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(queueOrTopicName);       
        }

        public ServiceBus(string namespaceName, TokenCredential credential, string queueOrTopicName)
        {
            _client = new ServiceBusClient($"{namespaceName}.servicebus.windows.net", credential);
            _sender = _client.CreateSender(queueOrTopicName);       
        }

        public async Task Send(string message, DateTime? sendOn = null)
        {
            var msg = new ServiceBusMessage(message);

            if(sendOn.HasValue)
            {
                // Ensure scheduled datetime is utc
                await _sender.ScheduleMessageAsync(msg, new DateTimeOffset(sendOn.Value.Year, sendOn.Value.Month, sendOn.Value.Day, sendOn.Value.Hour, sendOn.Value.Minute, sendOn.Value.Second, TimeSpan.FromSeconds(0)));
            }
            else
            {
                // Send now
                await _sender.SendMessageAsync(msg);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}