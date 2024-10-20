namespace Willow.ServiceBus;

using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Willow.ServiceBus.Options;

/// <summary>
/// The service bus client factory.
/// </summary>
public class ServiceBusClientFactory : IServiceBusClientFactory
{
    private readonly IMemoryCache memoryCache;
    private readonly ServiceBusOptions serviceBusOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBusClientFactory"/> class.
    /// </summary>
    /// <param name="memoryCache">A memory cache.</param>
    /// <param name="serviceBusOptions">The service bus options to configure the connection with.</param>
    public ServiceBusClientFactory(IMemoryCache memoryCache, IOptions<ServiceBusOptions> serviceBusOptions)
    {
        this.memoryCache = memoryCache;
        this.serviceBusOptions = serviceBusOptions.Value;
    }

    /// <inheritdoc/>
    public ServiceBusSender? GetMessageSender(string serviceBusInstance, string queueOrTopicName)
    {
        var serviceBusNamespace = serviceBusOptions.Namespaces[serviceBusInstance];
        var serviceBusClient = GetServiceBusClient(serviceBusNamespace);

        if (serviceBusClient == null)
        {
            return null;
        }

        var cacheKey = $"#Namespace{serviceBusClient.FullyQualifiedNamespace}#Sender#{serviceBusInstance}#{queueOrTopicName}";
        return memoryCache.GetOrCreate(cacheKey, _ => serviceBusClient.CreateSender(queueOrTopicName));
    }

    /// <inheritdoc/>
    public ServiceBusSender? GetMessageSender(string serviceBusInstance, string serviceBusNamespace, string queueOrTopicName)
    {
        var serviceBusClient = GetServiceBusClient(serviceBusNamespace);

        if (serviceBusClient == null)
        {
            return null;
        }

        var cacheKey = $"#Namespace{serviceBusClient.FullyQualifiedNamespace}#Sender#{serviceBusInstance}#{queueOrTopicName}";
        return memoryCache.GetOrCreate(cacheKey, _ => serviceBusClient.CreateSender(queueOrTopicName));
    }

    /// <inheritdoc/>
    public ServiceBusProcessor? GetMessageProcessor(IQueueMessageHandler handler)
    {
        var serviceBusNamespace = serviceBusOptions.Namespaces[handler.ServiceBusInstance];
        var serviceBusClient = GetServiceBusClient(serviceBusNamespace);

        if (serviceBusClient == null)
        {
            return null;
        }

        var cacheKey = $"#Namespace{serviceBusClient.FullyQualifiedNamespace}#Processor#{handler.ServiceBusInstance}#{handler.QueueName}";
        return memoryCache.GetOrCreate(cacheKey, _ => serviceBusClient.CreateProcessor(
            handler.QueueName, handler.ServiceBusProcessorOptions));
    }

    /// <inheritdoc/>
    public ServiceBusProcessor? GetMessageProcessor(ITopicMessageHandler handler)
    {
        var serviceBusNamespace = serviceBusOptions.Namespaces[handler.ServiceBusInstance];
        var serviceBusClient = GetServiceBusClient(serviceBusNamespace);

        if (serviceBusClient == null)
        {
            return null;
        }

        var cacheKey = $"#Namespace{serviceBusClient.FullyQualifiedNamespace}#Processor#{handler.ServiceBusInstance}#{handler.TopicName}#{handler.SubscriptionName}";
        return memoryCache.GetOrCreate(cacheKey, _ => serviceBusClient.CreateProcessor(
            handler.TopicName, handler.SubscriptionName, handler.ServiceBusProcessorOptions));
    }

    private ServiceBusClient? GetServiceBusClient(string serviceBusNamespace)
    {
        return memoryCache.GetOrCreate(serviceBusNamespace, _ =>
            serviceBusOptions.ConnectionString is not null
                ? new ServiceBusClient(serviceBusOptions.ConnectionString)
                : new ServiceBusClient(serviceBusNamespace, new DefaultAzureCredential()));
    }
}
