using System;
using System.Threading.Tasks;
using System.Threading;
using Azure.Messaging.ServiceBus;
using Willow.MessageDispatch;

namespace PlatformPortalXL.Test.MockServices
{
    /// <summary>
    /// Notify ServiceBus message received events
    /// </summary>
    public class MockMessageDispatch : IHostedMessageDispatch //, IAsyncDisposable
    {

        public MockMessageDispatch() //(string connectionString, string queueOrTopicName, IMessageDispatchHandler handler)
        {
            //RaiseMessageDispatchEvent += handler.OnMessageDispatch;
        }

        public event EventHandler<MessageDispatchEventArgs> RaiseMessageDispatchEvent;

        protected virtual Task MessageHandler(ProcessMessageEventArgs args)
        {
            //string message = args.Message.Body.ToString();

            //RaiseMessageDispatchEvent?.Invoke(this, new MessageDispatchEventArgs(message));

            //await args.CompleteMessageAsync(args.Message);

            return Task.CompletedTask;
        }

        protected virtual Task ErrorHandler(ProcessErrorEventArgs args)
        {
            // the error source tells me at what point in the processing an error occurred
            //Console.WriteLine(args.ErrorSource);
            //// the fully qualified namespace is available
            //Console.WriteLine(args.FullyQualifiedNamespace);
            //// as well as the entity path
            //Console.WriteLine(args.EntityPath);
            //Console.WriteLine(args.Exception.ToString());

            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            //_processor.ProcessMessageAsync += MessageHandler;
            //_processor.ProcessErrorAsync += ErrorHandler;
            //return _processor.StartProcessingAsync(stoppingToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            //return _processor.StopProcessingAsync(stoppingToken);

            return Task.CompletedTask;
        }

        //public ValueTask DisposeAsync()
        //{
        //    //await _processor.DisposeAsync();
        //    //await _client.DisposeAsync();
        //}
    }
}
