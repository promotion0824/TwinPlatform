namespace ConnectorCore.Services;

using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using ConnectorCore.Dtos;
using ConnectorCore.Entities;
using ConnectorCore.Infrastructure.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal class EventNotificationService : IEventNotificationService
{
    private readonly IConfiguration configuration;
    private readonly ILogger<EventNotificationService> logger;
    private const string ConnectionStringSuffix = "UnifiedEventHubConnectionString";
    private const string UnifiedEventHubFullyQualifiedNamespace = "UnifiedEventHubFullyQualifiedNamespace";
    private const string EventHubName = "connector-state-to-adx";
    private const string SingleTenantEventHubName = "evh-connector-state-to-adx";

    public EventNotificationService(IConfiguration configuration, ILogger<EventNotificationService> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task EventHubNotifyAsync(string connectionString, ConnectorEntity connectorEntity)
    {
        await using var producerClient = new EventHubProducerClient(connectionString, EventHubName);
        await SendAsync(producerClient, connectorEntity);
    }

    public async Task EventHubNotifyAsync(Guid clientId, ConnectorEntity connectorEntity)
    {
        var configurationKey = $"{clientId.ToString("D").ToUpperInvariant()}:{UnifiedEventHubFullyQualifiedNamespace}";
        var fullyQualifiedNamespace = configuration.GetValue<string>(configurationKey);
        var eventHubName = EventHubName;

        if (string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
        {
            fullyQualifiedNamespace = configuration.GetValue<string>(UnifiedEventHubFullyQualifiedNamespace);
            if (string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
            {
                throw new NotFoundException($"Customer {clientId} does not exist or its {UnifiedEventHubFullyQualifiedNamespace} reference has not been configured yet.");
            }

            // Use the single tenant event hub name if the fully qualified namespace is not null or empty
            // This can be removed and cleaned up when no longer needing to support multi-tenant and single tenant together
            eventHubName = SingleTenantEventHubName;
        }

        await using var producerClient = new EventHubProducerClient(fullyQualifiedNamespace, eventHubName, new DefaultAzureCredential());
        await SendAsync(producerClient, connectorEntity);
    }

    private async Task SendAsync(EventHubProducerClient producerClient, ConnectorEntity connectorEntity)
    {
        var interval = GetInterval(connectorEntity);
        try
        {
            var record = new ConnectorStateDto
            {
                ConnectorId = connectorEntity.Id,
                ConnectionType = connectorEntity.ConnectionType,
                Timestamp = DateTime.UtcNow,
                Enabled = connectorEntity.IsEnabled,
                Archived = connectorEntity.IsArchived,
                Interval = interval,
            };

            var recordString = JsonConvert.SerializeObject(record);
            var eventData = new EventData(Encoding.UTF8.GetBytes(recordString));

            using var eventBatch = await producerClient.CreateBatchAsync();
            eventBatch.TryAdd(eventData);

            await producerClient.SendAsync(eventBatch);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new UnauthorizedAccessException($"Error sending connector state change event for ConnectorId: {connectorEntity.Id}. Exception: {exception.Message}");
        }
    }

    private int GetInterval(ConnectorEntity connectorEntity)
    {
        var interval = 0;
        try
        {
            if (!string.IsNullOrEmpty(connectorEntity.Configuration))
            {
                dynamic dynamicConfiguration = JObject.Parse(connectorEntity.Configuration);
                if (dynamicConfiguration.Interval != null)
                {
                    interval = dynamicConfiguration.Interval;
                }
            }
        }
        catch (JsonReaderException ex)
        {
            logger.LogError(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }

        return interval;
    }

    public async Task<string> GetConnectionString(Guid clientId)
    {
        var configurationKey = $"{clientId.ToString("D").ToUpperInvariant()}:{ConnectionStringSuffix}";
        var connectionString = configuration.GetValue<string>(configurationKey);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogWarning("Customer {ClientId} does not exist or its ConnectorState EventHub reference has not been configured yet", clientId);
        }

        return await Task.FromResult(connectionString);
    }
}
