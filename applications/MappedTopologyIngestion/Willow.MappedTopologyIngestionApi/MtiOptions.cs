namespace Willow.MappedTopologyIngestionApi
{
    using Azure.Messaging.ServiceBus;

    /// <summary>
    /// Configurations for the Service Bus Queue.
    /// </summary>
    public class MtiOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not to enable direct sync calls and skip the queue.
        /// </summary>
        public bool EnableDirectSyncCalls { get; set; } = false;

        /// <summary>
        /// Gets or sets the Service Bus Name.
        /// </summary>
        public string ServiceBusName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Service Bus Queue Name.
        /// </summary>
        public string ServiceBusQueueName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the Service Bus Processor Options.
        /// </summary>
        public ServiceBusProcessorOptions? ServiceBusProcessorOptions { get; init; }
    }
}
