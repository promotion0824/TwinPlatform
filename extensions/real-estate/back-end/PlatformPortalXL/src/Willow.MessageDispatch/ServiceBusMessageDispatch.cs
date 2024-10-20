using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Azure.Messaging.ServiceBus;

namespace Willow.MessageDispatch
{

    public interface IHostedMessageDispatch : IHostedService, IMessageDispatch
    { 
    }

    /// <summary>
    /// Notify ServiceBus message received events
    /// </summary>
    public class ServiceBusMessageDispatch : IHostedMessageDispatch, IAsyncDisposable
    {
		private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;

        public ServiceBusMessageDispatch(string connectionString, string queueOrTopicName)
        {
			if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(queueOrTopicName))
			{
				_client = new ServiceBusClient(connectionString);
				_processor = _client.CreateProcessor(queueOrTopicName);
			}
		}

        public ServiceBusMessageDispatch(string connectionString, string queueOrTopicName, IMessageDispatchHandler handler)
            : this(connectionString, queueOrTopicName)
        {
            RaiseMessageDispatchEvent += handler.OnMessageDispatch;
        }

        public event EventHandler<MessageDispatchEventArgs> RaiseMessageDispatchEvent;

        protected virtual async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string message = args.Message.Body.ToString();

            RaiseMessageDispatchEvent?.Invoke(this, new MessageDispatchEventArgs(message));

            await args.CompleteMessageAsync(args.Message);
        }

        protected virtual Task ErrorHandler(ProcessErrorEventArgs args)
        {
            if (args.CancellationToken.IsCancellationRequested)
            {
                return StopAsync(args.CancellationToken);
            }

            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
			if (_processor != null)
			{
				_processor.ProcessMessageAsync += MessageHandler;
				_processor.ProcessErrorAsync += ErrorHandler;
				return _processor.StartProcessingAsync(cancellationToken);
			}

			return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _processor?.StopProcessingAsync(cancellationToken) ?? Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
			if (_processor != null)
			{
				await _processor.DisposeAsync();
			}

			if (_client != null)
			{
				await _client.DisposeAsync();
			}
        }
    }
}
