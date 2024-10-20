using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkflowCore.Infrastructure.Configuration;

namespace WorkflowCore.Services.Background
{
    public abstract class BaseMessageProcessorHostedService : BackgroundService
    {
        protected readonly ServiceBusClient _client;
        protected ServiceBusProcessor _processor;
        protected readonly ILogger _logger;
        private readonly string _subscriptionName;
        private readonly string _connectionString;
        private readonly string _topicName;

        protected BaseMessageProcessorHostedService(string subscriptionName, ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            var appSettings = configuration.Get<AppSettings>();
            _subscriptionName = subscriptionName;
            _topicName = appSettings?.TicketEventsTopicName;
            _connectionString = appSettings?.ServiceBusConnectionString;
            try
            {

                _client = new ServiceBusClient(_connectionString);
                _processor = _client.CreateProcessor(_topicName, subscriptionName, new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false
                });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error occurred while initializing the subscription processor for {SubscriptionName}", _subscriptionName);
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            _logger.LogInformation("Starting ticket subscriptions processor ");
            try
            {
                _logger.LogInformation("Starting subscription processor for {SubscriptionName}", _subscriptionName);
                _processor.ProcessMessageAsync += ProcessMessageAsync;
                _processor.ProcessErrorAsync += ExceptionReceivedHandler;

                await _processor.StartProcessingAsync(stoppingToken);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error occurred while starting the subscription processor for {SubscriptionName}", _subscriptionName);
            }


        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stopping subscription processor for {SubscriptionName}", _subscriptionName);
            // when the background service is stopped, clean up resources
            await _processor.DisposeAsync();
            await _client.DisposeAsync();

            await base.StopAsync(stoppingToken);
        }

        private Task ExceptionReceivedHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Error occurred while processing message");
            return Task.CompletedTask;
        }
        // each derived class will implement this method to process the message
        protected abstract Task ProcessMessageAsync(ProcessMessageEventArgs args);

    }
}
