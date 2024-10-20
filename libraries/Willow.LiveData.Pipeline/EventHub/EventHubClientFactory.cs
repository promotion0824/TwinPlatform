namespace Willow.LiveData.Pipeline.EventHub;

using Azure.Core;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Willow.LiveData.Pipeline.Configuration;

/// <summary>
/// Creates instances of <see cref="EventHubProducerClient"/> and <see cref="EventProcessorClient"/>.
/// </summary>
internal class EventHubClientFactory(IOptions<EventHubConfig> eventHubConfigOptions, TokenCredential tokenCredential)
{
    private readonly EventHubConfig eventHubConfig = eventHubConfigOptions.Value ?? throw new ArgumentNullException(nameof(eventHubConfigOptions));
    private EventHubProducerClient? producerClient;

    public EventProcessorClient CreateEventProcessorClient()
    {
        eventHubConfig.ThrowIfSourceNull();

        var containerUri = new Uri(eventHubConfig.Source!.StorageAccountUri, eventHubConfig.Source.StorageContainerName);
        var containerClient = new BlobContainerClient(containerUri, tokenCredential);
        containerClient.CreateIfNotExists();

        var options = new EventProcessorClientOptions
        {
            PartitionOwnershipExpirationInterval = TimeSpan.FromMinutes(2),
        };

        return new EventProcessorClient(
            containerClient,
            eventHubConfig.Source.ConsumerGroup,
            eventHubConfig.Source.FullyQualifiedNamespace,
            eventHubConfig.Source.Name,
            tokenCredential,
            options);
    }

    public EventHubProducerClient CreateEventProducerClient()
    {
        if (eventHubConfig.Destination == null)
        {
            throw new InvalidOperationException("Cannot create an Event Hub producer client, no config has been provided");
        }

        if (producerClient is null || producerClient.IsClosed)
        {
            producerClient = new EventHubProducerClient(
                eventHubConfig.Destination.FullyQualifiedNamespace,
                eventHubConfig.Destination.Name,
                new DefaultAzureCredential());
        }

        return producerClient;
    }
}
