namespace MappedTopologyIngestionApi.Controllers
{
    using System.Text.Json;
    using System.Threading;
    using Asp.Versioning;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Willow.AzureDigitalTwins.SDK.Client;
    using Willow.MappedTopologyIngestionApi;
    using Willow.Model.Async;
    using Willow.ServiceBus;
    using Willow.TopologyIngestion.Interfaces;

    /// <summary>
    /// Allows calling of various method to synchronize data between Mapped and Azure Digital Twins.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SyncController"/> class.
    /// </remarks>
    /// <param name="logger">An ILoggerInstance.</param>
    /// <param name="options">An IOptions instance to allow access to config settings.</param>
    /// <param name="graphIngestionProcessor">A GraphIngestion Processor to pull the topology from Mapped and insert to ADT.</param>
    /// <param name="messageSender">A Service Bus Client Message Sender.</param>
    /// <param name="jobsClient">jobs client.</param>
    [ApiVersion(1)]
    [ApiVersion(2)]
    [ApiController]
    [Route("[controller]")]
    public class SyncController(ILogger<SyncController> logger, IOptions<MtiOptions> options, IGraphIngestionProcessor graphIngestionProcessor, IMessageSender messageSender, IJobsClient jobsClient) : ControllerBase
    {
        private readonly ILogger<SyncController> logger = logger;
        private readonly IOptions<MtiOptions> options = options;
        private readonly IGraphIngestionProcessor graphIngestionProcessor = graphIngestionProcessor;
        private readonly IMessageSender messageSender = messageSender;
        private readonly IJobsClient jobsClient = jobsClient;

        /// <summary>
        /// Synchronizes the organization data from Mapped to Azure Digital Twins.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous task.</returns>
        [MapToApiVersion(1)]
        [HttpPost("SyncOrganization")]
        public async Task SyncOrganization(string userId, CancellationToken cancellationToken)
        {
            JobsEntry jobsEntry = new JobsEntry
            {
                UserId = userId,
                JobType = "MTI",
                JobSubtype = MtiAsyncJobType.SyncOrganization.ToString(),
            };
            var job = await jobsClient.CreateOrUpdateJobEntryAsync(jobsEntry, cancellationToken);

            if (options.Value.EnableDirectSyncCalls)
            {
                job.Status = AsyncJobStatus.Processing;
                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);

                logger.LogInformation("Syncing Organization.");
                bool autoApprove = false;
                await graphIngestionProcessor.SyncOrganizationAsync(autoApprove, cancellationToken);

                job.Status = AsyncJobStatus.Done;
                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);
            }
            else
            {
                logger.LogInformation("Sending message to start organization sync.");
                await messageSender.Send(options.Value.ServiceBusName, options.Value.ServiceBusQueueName, job, null, Guid.NewGuid().ToString(), cancellationToken);
            }
        }

        /// <summary>
        /// Synchronizes the connector data from Mapped to Azure Digital Twins for a building.
        /// </summary>
        /// <param name="buildingId">The Mapped building identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous task.</returns>
        [MapToApiVersion(1)]
        [HttpPost("SyncConnectors")]
        public async Task SyncConnectors(string buildingId, string userId, CancellationToken cancellationToken)
        {
            JobsEntry jobsEntry = new JobsEntry
            {
                UserId = userId,
                JobType = "MTI",
                JobSubtype = MtiAsyncJobType.SyncConnectors.ToString(),
                JobsEntryDetail = new JobsEntryDetail()
                {
                    InputsJson = JsonSerializer.Serialize(new { buildingId }),
                },
            };
            var job = await jobsClient.CreateOrUpdateJobEntryAsync(jobsEntry, cancellationToken);

            if (options.Value.EnableDirectSyncCalls)
            {
                job.Status = AsyncJobStatus.Processing;
                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);

                logger.LogInformation("Syncing Connectors for buildingId: {BuildingId}", buildingId);
                bool autoApprove = false;
                await graphIngestionProcessor.SyncConnectorsAsync(buildingId, autoApprove, cancellationToken);

                job.Status = AsyncJobStatus.Done;
                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);
            }
            else
            {
                logger.LogInformation("Sending message to start connector sync for buildingId: {BuildingId}", buildingId);

                await messageSender.Send(options.Value.ServiceBusName, options.Value.ServiceBusQueueName, job, null, Guid.NewGuid().ToString(), cancellationToken);
            }
        }

        /// <summary>
        /// Synchronizes the spatial data from Mapped to Azure Digital Twins for a building.
        /// </summary>
        /// <param name="buildingId">The Mapped building identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous task.</returns>
        [MapToApiVersion(1)]
        [HttpPost("SyncSpatial")]
        public async Task SyncSpatial(string buildingId, string userId, CancellationToken cancellationToken)
        {
            JobsEntry jobsEntry = new JobsEntry
            {
                UserId = userId,
                JobType = "MTI",
                JobSubtype = MtiAsyncJobType.SyncSpatial.ToString(),
                JobsEntryDetail = new JobsEntryDetail()
                {
                    InputsJson = JsonSerializer.Serialize(new { buildingId }),
                },
            };
            var job = await jobsClient.CreateOrUpdateJobEntryAsync(jobsEntry, cancellationToken);

            if (options.Value.EnableDirectSyncCalls)
            {
                job.Status = AsyncJobStatus.Processing;
                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);

                logger.LogInformation("Syncing Spatial data for buildingId: {BuildingId}", buildingId);
                bool autoApprove = false;
                await graphIngestionProcessor.SyncSpatialAsync(buildingId, autoApprove, cancellationToken);

                job.Status = AsyncJobStatus.Done;
                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);
            }
            else
            {
                logger.LogInformation("Sending message to start spatial sync  for buildingId: {BuildingId}", buildingId);

                await messageSender.Send(options.Value.ServiceBusName, options.Value.ServiceBusQueueName, job, null, Guid.NewGuid().ToString(), cancellationToken);
            }
        }

        /// <summary>
        /// Synchronizes the asset data from Mapped to Azure Digital Twins for a building and connector.
        /// </summary>
        /// <param name="buildingId">The Mapped building identifier.</param>
        /// <param name="connectorId">The Mapped connector identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous task.</returns>
        [MapToApiVersion(1)]
        [HttpPost("SyncAssets")]
        public async Task SyncAssets(string buildingId, string connectorId, string userId, CancellationToken cancellationToken)
        {
            JobsEntry jobsEntry = new JobsEntry
            {
                UserId = userId,
                JobType = "MTI",
                JobSubtype = MtiAsyncJobType.SyncAssets.ToString(),
                JobsEntryDetail = new JobsEntryDetail()
                {
                    InputsJson = JsonSerializer.Serialize(new { buildingId, connectorId }),
                },
            };
            var job = await jobsClient.CreateOrUpdateJobEntryAsync(jobsEntry, cancellationToken);

            if (options.Value.EnableDirectSyncCalls)
            {
                job.Status = AsyncJobStatus.Processing;
                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);

                logger.LogInformation("Syncing Asset data for buildingId: {BuildingId} and connectorId: {ConnectorId}", buildingId, connectorId);
                bool autoApprove = false;
                await graphIngestionProcessor.SyncThingsAsync(buildingId, connectorId, autoApprove, cancellationToken);

                job.Status = AsyncJobStatus.Done;
                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);
            }
            else
            {
                logger.LogInformation("Sending message to start asset sync for building: {BuildingId} and connectorId: {ConnectorId}", buildingId, connectorId);

                await messageSender.Send(options.Value.ServiceBusName, options.Value.ServiceBusQueueName, job, null, Guid.NewGuid().ToString(), cancellationToken);
            }
        }

        /// <summary>
        /// Synchronizes the asset data from Mapped to Azure Digital Twins for a building and connector.
        /// </summary>
        /// <param name="buildingId">The Mapped building identifier.</param>
        /// <param name="connectorId">The Mapped connector identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous task.</returns>
        [MapToApiVersion(1)]
        [HttpPost("SyncCapabilities")]
        public async Task SyncCapabilties(string buildingId, string connectorId, string userId, CancellationToken cancellationToken)
        {
            JobsEntry jobsEntry = new JobsEntry
            {
                UserId = userId,
                JobType = "MTI",
                JobSubtype = MtiAsyncJobType.SyncCapabilities.ToString(),
                JobsEntryDetail = new JobsEntryDetail()
                {
                    InputsJson = JsonSerializer.Serialize(new { buildingId, connectorId }),
                },
            };
            var job = await jobsClient.CreateOrUpdateJobEntryAsync(jobsEntry, cancellationToken);

            if (options.Value.EnableDirectSyncCalls)
            {
                job.Status = AsyncJobStatus.Processing;
                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);

                logger.LogInformation("Syncing Capabilties data for buildingId: {BuildingId} and connectorId: {ConnectorId}", buildingId, connectorId);
                bool autoApprove = false;
                await graphIngestionProcessor.SyncPointsAsync(buildingId, connectorId, autoApprove, cancellationToken);

                job.Status = AsyncJobStatus.Done;
                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);
            }
            else
            {
                logger.LogInformation("Sending message to start capability sync for building: {BuildingId} and connectorId: {ConnectorId}", buildingId, connectorId);

                await messageSender.Send(options.Value.ServiceBusName, options.Value.ServiceBusQueueName, job, null, Guid.NewGuid().ToString(), cancellationToken);
            }
        }

        /// <summary>
        /// Synchronizes the organization data from Mapped to Azure Digital Twins.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="autoApprove">Flag to auto approve.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous task.</returns>
        [MapToApiVersion(2)]
        [HttpPost("SyncOrganization")]
        public async Task SyncOrganizationV2(string userId, bool autoApprove, CancellationToken cancellationToken)
        {
            JobsEntry jobsEntry = new JobsEntry
            {
                UserId = userId,
                JobType = "MTI",
                JobSubtype = MtiAsyncJobType.SyncOrganization.ToString(),
                JobsEntryDetail = new JobsEntryDetail()
                {
                    InputsJson = JsonSerializer.Serialize(new { autoApprove }),
                },
            };

            var job = await jobsClient.CreateOrUpdateJobEntryAsync(jobsEntry, cancellationToken);

            if (options.Value.EnableDirectSyncCalls)
            {
                job.Status = AsyncJobStatus.Processing;

                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);

                logger.LogInformation("Syncing Organization.");

                await graphIngestionProcessor.SyncOrganizationAsync(autoApprove, cancellationToken);

                job.Status = AsyncJobStatus.Done;

                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);
            }
            else
            {
                logger.LogInformation("Sending message to start organization sync.");
                await messageSender.Send(options.Value.ServiceBusName, options.Value.ServiceBusQueueName, job, null, Guid.NewGuid().ToString(), cancellationToken);
            }
        }

        /// <summary>
        /// Synchronizes the spatial data from Mapped to Azure Digital Twins for a building.
        /// </summary>
        /// <param name="buildingIds">Array of Mapped building identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="autoApprove">Flag to auto approve.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous task.</returns>
        [MapToApiVersion(2)]
        [HttpPost("SyncSpatial")]
        public async Task SyncSpatialV2(string[] buildingIds, string userId, bool autoApprove, CancellationToken cancellationToken)
        {
            JobsEntry jobsEntry = new JobsEntry
            {
                UserId = userId,
                JobType = "MTI",
                JobSubtype = MtiAsyncJobType.SyncSpatial.ToString(),
                JobsEntryDetail = new JobsEntryDetail()
                {
                    InputsJson = JsonSerializer.Serialize(new { buildingIds, autoApprove }),
                },
            };

            var job = await jobsClient.CreateOrUpdateJobEntryAsync(jobsEntry, cancellationToken);

            if (options.Value.EnableDirectSyncCalls)
            {
                job.Status = AsyncJobStatus.Processing;

                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);

                foreach (var buildingId in buildingIds)
                {
                    logger.LogInformation("Syncing Spatial data for buildingId: {BuildingId}", buildingId);
                    await graphIngestionProcessor.SyncSpatialAsync(buildingId, autoApprove, cancellationToken);
                }

                job.Status = AsyncJobStatus.Done;

                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);
            }
            else
            {
                logger.LogInformation("Sending message to start spatial sync for buildingIds: {BuildingIds}", buildingIds);

                await messageSender.Send(options.Value.ServiceBusName, options.Value.ServiceBusQueueName, job, null, Guid.NewGuid().ToString(), cancellationToken);
            }
        }

        /// <summary>
        /// Synchronizes the asset data from Mapped to Azure Digital Twins for a building and connector.
        /// </summary>
        /// <param name="buildingIds">Array of Mapped building identifier.</param>
        /// <param name="connectorId">The Mapped connector identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="autoApprove">Flag to auto approve.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous task.</returns>
        [MapToApiVersion(2)]
        [HttpPost("SyncAssets")]
        public async Task SyncAssetsV2(string[] buildingIds, string connectorId, string userId, bool autoApprove, CancellationToken cancellationToken)
        {
            JobsEntry jobsEntry = new JobsEntry
            {
                UserId = userId,
                JobType = "MTI",
                JobSubtype = MtiAsyncJobType.SyncAssets.ToString(),
                JobsEntryDetail = new JobsEntryDetail()
                {
                    InputsJson = JsonSerializer.Serialize(new { buildingIds, connectorId, autoApprove }),
                },
            };

            var job = await jobsClient.CreateOrUpdateJobEntryAsync(jobsEntry, cancellationToken);

            if (options.Value.EnableDirectSyncCalls)
            {
                job.Status = AsyncJobStatus.Processing;

                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);

                foreach (var buildingId in buildingIds)
                {
                    logger.LogInformation("Syncing Asset data for buildingId: {BuildingId} and connectorId: {ConnectorId}", buildingId, connectorId);
                    await graphIngestionProcessor.SyncThingsAsync(buildingId, connectorId, autoApprove, cancellationToken);
                }

                job.Status = AsyncJobStatus.Done;

                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);
            }
            else
            {
                logger.LogInformation("Sending message to start asset sync for building: {BuildingIds} and connectorId: {ConnectorId}", buildingIds, connectorId);

                await messageSender.Send(options.Value.ServiceBusName, options.Value.ServiceBusQueueName, job, null, Guid.NewGuid().ToString(), cancellationToken);
            }
        }

        /// <summary>
        /// Synchronizes the asset data from Mapped to Azure Digital Twins for a building and connector.
        /// </summary>
        /// <param name="buildingIds">Array of Mapped building identifier.</param>
        /// <param name="connectorId">The Mapped connector identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="autoApprove">Flag to auto approve.</param>
        /// <param name="matchStdPntList">Flag to filter to match Standard Points List.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous task.</returns>
        [MapToApiVersion(2)]
        [HttpPost("SyncCapabilities")]
        public async Task SyncCapabiltiesV2(string[] buildingIds, string connectorId, string userId, bool autoApprove, bool matchStdPntList, CancellationToken cancellationToken)
        {
            JobsEntry jobsEntry = new JobsEntry
            {
                UserId = userId,
                JobType = "MTI",
                JobSubtype = MtiAsyncJobType.SyncCapabilities.ToString(),
                JobsEntryDetail = new JobsEntryDetail()
                {
                    InputsJson = JsonSerializer.Serialize(new { buildingIds, connectorId, autoApprove, matchStdPntList }),
                },
            };

            var job = await jobsClient.CreateOrUpdateJobEntryAsync(jobsEntry, cancellationToken);

            if (options.Value.EnableDirectSyncCalls)
            {
                job.Status = AsyncJobStatus.Processing;

                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);

                foreach (var buildingId in buildingIds)
                {
                    logger.LogInformation("Syncing Capabilties data for buildingId: {BuildingId} and connectorId: {ConnectorId}", buildingId, connectorId);
                    await graphIngestionProcessor.SyncPointsAsync(buildingId, connectorId, autoApprove, cancellationToken);
                }

                job.Status = AsyncJobStatus.Done;

                await jobsClient.CreateOrUpdateJobEntryAsync(job, cancellationToken);
            }
            else
            {
                logger.LogInformation("Sending message to start capability sync for building: {BuildingIds} and connectorId: {ConnectorId}", buildingIds, connectorId);

                await messageSender.Send(options.Value.ServiceBusName, options.Value.ServiceBusQueueName, job, null, Guid.NewGuid().ToString(), cancellationToken);
            }
        }

        /// <summary>
        /// Get the meta data for the connector.
        /// </summary>
        /// <param name="connectorId">The Mapped connector identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous task.</returns>
        [HttpGet("GetConnectorMetadata/{connectorId}")]
        public async Task<IActionResult> GetConnectorMetadata(string connectorId, CancellationToken cancellationToken)
        {
            logger.LogInformation("Getting connector metadata data for connectorId: {ConnectorId}", connectorId);

            var response = await graphIngestionProcessor.GetConnectorMetadataAsync(connectorId, cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Health check for the sync controller.
        /// </summary>
        /// <returns>An asynchronous task.</returns>
        [HttpGet("HealthCheck")]
        public Task HealthCheck()
        {
            return Task.CompletedTask;
        }
    }
}
