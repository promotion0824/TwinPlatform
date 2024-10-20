namespace Willow.MappedTopologyIngestionApi
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Messaging.ServiceBus;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using Willow.AzureDigitalTwins.SDK.Client;
    using Willow.MappedTopologyIngestionApi.HealthChecks;
    using Willow.Model.Async;
    using Willow.ServiceBus;
    using Willow.TopologyIngestion.Interfaces;

    /// <summary>
    /// The SyncMessageHandler class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SyncMessageHandler"/> class.
    /// </remarks>
    /// <param name="options">The configuration of the Mti Service.</param>
    /// <param name="healthCheckServiceBus">The health check for the service bus connection.</param>
    /// <param name="logger">An ILogger Instance.</param>
    /// <param name="jobsClient">The jobs client for the sync jobs.</param>
    /// <param name="serviceProvider">The dependency injection provider.</param>
    public class SyncMessageHandler(IOptions<MtiOptions> options,
                              HealthCheckServiceBus healthCheckServiceBus,
                              ILogger<SyncMessageHandler> logger,
                              IJobsClient jobsClient,
                              IServiceProvider serviceProvider) : IQueueMessageHandler
    {
        private readonly IOptions<MtiOptions> options = options;
        private readonly HealthCheckServiceBus healthCheckServiceBus = healthCheckServiceBus;
        private readonly ILogger<SyncMessageHandler> logger = logger;
        private readonly IJobsClient jobsClient = jobsClient;

        /// <summary>
        /// Gets the Service Bus Queue Name.
        /// </summary>
        public string QueueName => options.Value.ServiceBusQueueName;

        /// <summary>
        /// Gets the Service Bus Instance.
        /// </summary>
        public string ServiceBusInstance => options.Value.ServiceBusName;

        /// <summary>
        /// Gets the Service Bus Processor Options.
        /// </summary>
        public ServiceBusProcessorOptions? ServiceBusProcessorOptions => options.Value.ServiceBusProcessorOptions;

        /// <summary>
        /// Handles the error case for reading messages from the queue.
        /// </summary>
        /// <param name="ex">The exception encountered.</param>
        public void OnError(Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve message from service bus.");
            var healthCheck = new HealthCheckResult(HealthStatus.Unhealthy, "Failed to retrieve message from service bus.", ex);
            healthCheckServiceBus.Current = healthCheck;
        }

        /// <summary>
        /// Process a received message from the service bus.
        /// </summary>
        /// <param name="receivedMessage">The recevied message. Expected to be of type SyncMessage.</param>
        /// <param name="cancellationToken">An asynchronous operation cancellation token.</param>
        /// <returns>A Message Processing result.</returns>
        public async Task<MessageProcessingResult> ProcessReceivedMessage(ServiceBusReceivedMessage receivedMessage, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return MessageProcessingResult.Failed("Message processing cancelled");
            }

            logger.LogInformation("Processing message {MessageId}", receivedMessage.MessageId);

            JobsEntry? syncMessage = null;

            // Need to resolve this here, because we want a new instance for each message, not just when the handler is created.
            var graphIngestionProcessor = serviceProvider.GetRequiredService<IGraphIngestionProcessor>();

            try
            {
                syncMessage = JsonSerializer.Deserialize<JobsEntry>(receivedMessage.Body);

                if (syncMessage == null)
                {
                    logger.LogError("Failed to deserialize message {MessageId}", receivedMessage.MessageId);
                    return MessageProcessingResult.Failed("Failed to deserialize message");
                }

                syncMessage.Status = AsyncJobStatus.Processing;
                await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);

                // Access inputs Json to get parameters.
                var inputsJson = JsonSerializer.Deserialize<JsonElement>(syncMessage?.JobsEntryDetail?.InputsJson ?? "{}");
                string? buildingId = null;
                string[] buildingIds = [];
                string? connectorId = null;
                bool autoApprove = false;
                bool matchStdPntList = false;

                if (inputsJson.TryGetProperty("buildingId", out var buildingIdElement) && buildingIdElement.ValueKind == JsonValueKind.String)
                {
                    buildingId = buildingIdElement.GetString();
                }

                if (inputsJson.TryGetProperty("buildingIds", out var buildingIdsElement) && buildingIdsElement.ValueKind == JsonValueKind.Array)
                {
                    buildingIds = JsonSerializer.Deserialize<string[]>(buildingIdsElement.GetRawText())!;
                }

                // Handle case for version 1 of sync endpoints where the parameter buildingid is just a string.
                if (buildingId != null)
                {
                    buildingIds = [buildingId];
                }

                if (inputsJson.TryGetProperty("connectorId", out var connectorIdElement) && connectorIdElement.ValueKind == JsonValueKind.String)
                {
                    connectorId = connectorIdElement.GetString();
                }

                if (inputsJson.TryGetProperty("autoApprove", out var autoApproveElement) && (autoApproveElement.ValueKind == JsonValueKind.True || autoApproveElement.ValueKind == JsonValueKind.False))
                {
                    autoApprove = autoApproveElement.GetBoolean();
                }

                if (inputsJson.TryGetProperty("matchStdPntList", out var matchStdPntListElement) && (matchStdPntListElement.ValueKind == JsonValueKind.True || matchStdPntListElement.ValueKind == JsonValueKind.False))
                {
                    matchStdPntList = matchStdPntListElement.GetBoolean();
                }

                switch (syncMessage?.JobSubtype)
                {
                    case nameof(MtiAsyncJobType.SyncOrganization):
                        {
                            logger.LogInformation("Received message to sync organization.");

                            syncMessage.ProgressStatusMessage += "\nSyncing organization.";
                            await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);

                            await graphIngestionProcessor.SyncOrganizationAsync(autoApprove, cancellationToken);

                            syncMessage.ProgressStatusMessage += "\nSyncing organization wrapping up.";
                            await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);

                            break;
                        }

                    case nameof(MtiAsyncJobType.SyncCapabilities):
                        {
                            if (buildingIds.IsNullOrEmpty() || connectorId == null)
                            {
                                throw new InvalidOperationException("BuildingIds and ConnectorId are required for SyncCapabilities. ");
                            }

                            logger.LogInformation("Received message to sync points.");

                            syncMessage.ProgressStatusMessage += "\nSyncing capabilities starting...";
                            await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);

                            foreach (var buildingId1 in buildingIds!)
                            {
                                syncMessage.ProgressStatusMessage += $"\nSyncing capabilities for building: {buildingId1}.";
                                await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);

                                await graphIngestionProcessor.SyncPointsAsync(buildingId1, connectorId, autoApprove, cancellationToken);

                                syncMessage.ProgressStatusMessage += $"\nSyncing capabilities for building: {buildingId1} completed.";
                                await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);
                            }

                            syncMessage.ProgressStatusMessage += $"\nSyncing capabilities wrapping up.";
                            await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);

                            break;
                        }

                    case nameof(MtiAsyncJobType.SyncSpatial):
                        {
                            if (buildingIds.IsNullOrEmpty())
                            {
                                throw new InvalidOperationException("BuildingIds is required for SyncSpatial. ");
                            }

                            logger.LogInformation("Received message to sync spatial.");

                            syncMessage.ProgressStatusMessage += $"\nSyncing spatial started.";
                            await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);

                            foreach (var buildingId1 in buildingIds!)
                            {
                                syncMessage.ProgressStatusMessage += $"\nSyncing spatial for building: {buildingId1}.";
                                await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);

                                await graphIngestionProcessor.SyncSpatialAsync(buildingId1, autoApprove, cancellationToken);

                                syncMessage.ProgressStatusMessage += $"\nSyncing spatial for building: {buildingId1} completed.";
                                await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);
                            }

                            syncMessage.ProgressStatusMessage += $"\nSyncing spatial wrapping up.";
                            await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);

                            break;
                        }

                    case nameof(MtiAsyncJobType.SyncAssets):
                        {
                            if (buildingIds.IsNullOrEmpty() || connectorId == null)
                            {
                                throw new InvalidOperationException("BuildingIds and ConnectorId are required for SyncAssets. ");
                            }

                            logger.LogInformation("Received message to sync things.");

                            syncMessage.ProgressStatusMessage += $"\nSyncing assets started.";
                            await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);

                            foreach (var buildingId1 in buildingIds!)
                            {
                                syncMessage.ProgressStatusMessage += $"\nSyncing assets for building: {buildingId1}.";
                                await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);

                                await graphIngestionProcessor.SyncThingsAsync(buildingId1, connectorId, autoApprove, cancellationToken);

                                syncMessage.ProgressStatusMessage += $"\nSyncing assets for building: {buildingId1} completed.";
                                await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);
                            }

                            syncMessage.ProgressStatusMessage += $"\nSyncing assets wrapping up.";
                            await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);

                            break;
                        }

                    default:
                        logger.LogError("Unknown message type {MessageType} for message {MessageId}", syncMessage?.JobType, receivedMessage.MessageId);
                        return MessageProcessingResult.Failed("Unknown message type");
                }

                if (graphIngestionProcessor.Errors.Count > 0)
                {
                    syncMessage.Status = AsyncJobStatus.Error;

                    if (syncMessage.JobsEntryDetail == null)
                    {
                        syncMessage.JobsEntryDetail = new JobsEntryDetail
                        {
                            ErrorsJson = JsonSerializer.Serialize(graphIngestionProcessor.Errors),
                        };
                    }
                    else
                    {
                        syncMessage.JobsEntryDetail.ErrorsJson = JsonSerializer.Serialize(graphIngestionProcessor.Errors);
                    }
                }
                else
                {
                    syncMessage.Status = AsyncJobStatus.Done;
                }

                await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);

                var healthCheck = new HealthCheckResult(HealthStatus.Healthy, "Message processed successfully");
                healthCheckServiceBus.Current = healthCheck;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process message {MessageId}", receivedMessage.MessageId);

                if (syncMessage is not null)
                {
                    syncMessage.Status = AsyncJobStatus.Error;
                    syncMessage.ProgressStatusMessage += ex.Message;
                    await jobsClient.CreateOrUpdateJobEntryAsync(syncMessage, cancellationToken);
                }

                var healthCheck = new HealthCheckResult(HealthStatus.Unhealthy, "Message failed to process successfully");
                healthCheckServiceBus.Current = healthCheck;
                return MessageProcessingResult.Failed("Failed to process message");
            }

            return MessageProcessingResult.Success();
        }
    }
}
