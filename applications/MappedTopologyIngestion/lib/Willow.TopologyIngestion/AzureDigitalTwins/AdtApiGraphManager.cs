// -----------------------------------------------------------------------
// <copyright file="AzureDigitalTwinsGraphManager.cs" Company="Willow">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.TopologyIngestion.AzureDigitalTwins
{
    using System;
    using System.Diagnostics.Metrics;
    using System.Text.Json;
    using System.Threading;
    using Azure;
    using Azure.DigitalTwins.Core;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using Willow.AzureDigitalTwins.SDK.Client;
    using Willow.Batch;
    using Willow.Model.Adt;
    using Willow.Model.Requests;
    using Willow.Telemetry;
    using Willow.TopologyIngestion.Extensions;
    using Willow.TopologyIngestion.Interfaces;

    /// <summary>
    /// Output graph manager supporting writing to Azure Digital Twins via AdtApi.
    /// </summary>
    /// <typeparam name="TOptions">Ingestion manager options type.</typeparam>
    public class AdtApiGraphManager<TOptions> : IOutputGraphManager
        where TOptions : IngestionManagerOptions
    {
        private readonly IOptions<TOptions> options;
        private readonly ITwinsClient twinsClient;
        private readonly IRelationshipsClient relationshipsClient;
        private readonly IModelsClient modelsClient;
        private readonly IMeterFactory meterFactory;
        private readonly IMappingClient mappingClient;
        private readonly MetricsAttributesHelper metricsAttributesHelper;
        private readonly Counter<long> relationshipCounter;
        private readonly Counter<long> twinCounter;
        private readonly List<string> skipRelationshipsWithTheseIds = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AdtApiGraphManager{TOptions}"/> class.
        /// </summary>
        /// <param name="logger">Local logger.</param>
        /// <param name="options">Ingestion manager options.</param>
        /// <param name="twinMappingIndexer">Twin mapping index cache.</param>
        /// <param name="twinsClient">Twins client for accessing adt api.</param>
        /// <param name="relationshipsClient">Relationships client for accessing adt api.</param>
        /// <param name="modelsClient">Models client for accessing adt api.</param>
        /// <param name="meterFactory">The meter factory for creating meters.</param>
        /// <param name="mappingClient">An instance of the mapping client for ADT Api.</param>
        /// <param name="metricsAttributesHelper">An instance of the metrics helper.</param>
        public AdtApiGraphManager(ILogger<AdtApiGraphManager<TOptions>> logger,
                                  IOptions<TOptions> options,
                                  ITwinMappingIndexer twinMappingIndexer,
                                  ITwinsClient twinsClient,
                                  IRelationshipsClient relationshipsClient,
                                  IModelsClient modelsClient,
                                  IMeterFactory meterFactory,
                                  IMappingClient mappingClient,
                                  MetricsAttributesHelper metricsAttributesHelper)
        {
            Logger = logger;
            this.options = options;

            var meterOptions = options.Value.MeterOptions;
            var meter = meterFactory.Create(meterOptions.Name, meterOptions.Version, meterOptions.Tags);
            relationshipCounter = meter.CreateCounter<long>("Mti-Relationships", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            twinCounter = meter.CreateCounter<long>("Mti-Twins", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());

            this.twinsClient = twinsClient;
            this.relationshipsClient = relationshipsClient;
            this.modelsClient = modelsClient;
            this.meterFactory = meterFactory;
            this.mappingClient = mappingClient;
            this.metricsAttributesHelper = metricsAttributesHelper;
            TwinMappingIndexer = twinMappingIndexer;
        }

        /// <inheritdoc/>
        public Dictionary<string, string> Errors { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets local logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets twin mapping index cache.
        /// </summary>
        protected ITwinMappingIndexer TwinMappingIndexer { get; }

        /// <summary>
        /// Gets the list of properties in a twin that MTI will patch automatically.
        /// </summary>
        public IEnumerable<string> AutoPatchProperties { get; } = new List<string> { "/alternateClassification", "/externalID", "/externalIds", "/externalIds/mappingKeys", "/mappedConnectorId", "/mappedIds", "/stateText", "/type", "/valueMap" };

        /// <summary>
        /// Get the site id for a building.
        /// </summary>
        /// <param name="buildingId">The Willow Twin Id for a building.</param>
        /// <param name="cancellationToken">Asynchronous task cancellation token.</param>
        /// <returns>The Willow Site Id for a building.</returns>
        public async Task<string> GetSiteIdForBuilding(string buildingId, CancellationToken cancellationToken)
        {
            // Try to get the twin for the passed in buildingId
            var twin = await twinsClient.GetTwinsByIdsAsync(new[] { buildingId }, cancellationToken: cancellationToken);

            if (twin.Content.Count() != 0)
            {
                var buildingTwin = twin.Content.First();
                if (buildingTwin.Twin.Contents.TryGetValue("siteID", out var siteId))
                {
                    var site = siteId.ToString();
                    if (!string.IsNullOrEmpty(site))
                    {
                        return site;
                    }
                }
            }

            // If the twin does not exist, return the passed in buildingId, because in this case, the buildingId is the siteId
            return buildingId;
        }

        /// <summary>
        /// Get the site id for a mapped building id.
        /// </summary>
        /// <param name="buildingId">The Willow Twin Id for a building.</param>
        /// <param name="cancellationToken">Asynchronous task cancellation token.</param>
        /// <returns>The Willow Site Id for a building.</returns>
        public async Task<string> GetSiteIdForMappedBuildingId(string buildingId, CancellationToken cancellationToken)
        {
            var queryFilter = new Model.Requests.QueryFilter()
            {
                Filter = $"externalID = '{buildingId}'",
            };

            // Try to get the twin for the passed in buildingId
            var getTwinsRequest = new GetTwinsInfoRequest()
            {
                QueryFilter = queryFilter,
                SourceType = SourceType.AdtQuery,
            };

            var page = await twinsClient.GetTwinsAsync(getTwinsRequest, cancellationToken: cancellationToken);
            var twins = page.Content.ToList();

            while (page.ContinuationToken != null)
            {
                page = await twinsClient.GetTwinsAsync(getTwinsRequest, continuationToken: page.ContinuationToken, cancellationToken: cancellationToken);
                twins.AddRange(page.Content);
            }

            var buildingTwin = twins.First();

            if (buildingTwin.Twin.Contents.TryGetValue("siteID", out var siteId))
            {
                var site = siteId.ToString();
                if (!string.IsNullOrEmpty(site))
                {
                    return site;
                }
            }

            // If the twin does not exist, return the passed in buildingId, because in this case, the buildingId is the siteId
            return buildingId;
        }

        /// <summary>
        /// Get the Willow id for a mapped id.
        /// </summary>
        /// <param name="mappedId">The Mapped Id for a twin.</param>
        /// <param name="cancellationToken">Asynchronous task cancellation token.</param>
        /// <returns>The Willow Id for a twin.</returns>
        public async Task<BasicDigitalTwin?> GetTwinForMappedId(string mappedId, CancellationToken cancellationToken)
        {
            var queryFilter = new Model.Requests.QueryFilter()
            {
                Filter = $"externalID = '{mappedId}'",
            };

            // Try to get the twin for the passed in buildingId
            var getTwinsRequest = new GetTwinsInfoRequest()
            {
                QueryFilter = queryFilter,
                SourceType = SourceType.AdtQuery,
            };

            Logger.LogInformation("Getting twin for mappedId: {MappedId}", mappedId);

            var page = await twinsClient.GetTwinsAsync(getTwinsRequest, cancellationToken: cancellationToken);

            var twins = page.Content.ToList();

            Logger.LogInformation("Total Twins returned: {TotalTwins}", page.Content.Count());

            while (page.ContinuationToken != null)
            {
                page = await twinsClient.GetTwinsAsync(getTwinsRequest, continuationToken: page.ContinuationToken, cancellationToken: cancellationToken);
                Logger.LogInformation("Total Twins returned: {TotalTwins}", page.Content.Count());
                twins.AddRange(page.Content);
            }

            if (twins.Count != 0)
            {
                Logger.LogInformation("Twin found for mappedId: {MappedId}, TwinId {TwinId}", mappedId, twins.First().Twin.Id);
                return twins.First().Twin;
            }

            Logger.LogInformation("Twin not found for mappedId: {MappedId}", mappedId);
            return null;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetModelsAsync(CancellationToken cancellationToken)
        {
            // Get all of the Models to be used for creating instances of twins
            var modelsResults = await modelsClient.GetModelsAsync(includeModelDefinitions: true, cancellationToken: cancellationToken);
            var modelList = new List<string>();

            foreach (var md in modelsResults)
            {
                // TODO: jobee - Add error handling for models which do not have a DtdlModel (is this even possible?)
                if (md.Model != null)
                {
                    modelList.Add(md.Model.ToString());
                }
            }

            return modelList;
        }

        /// <inheritdoc/>
        public virtual async Task UploadGraphAsync(Dictionary<string, BasicDigitalTwin> twins,
                                                   Dictionary<string, BasicRelationship> relationships,
                                                   string buildingId,
                                                   string connectorId,
                                                   bool autoApprove = false,
                                                   CancellationToken cancellationToken = default)
        {
            skipRelationshipsWithTheseIds.Clear();
            await ImportTwinsAsync(twins, relationships, buildingId, connectorId, autoApprove, cancellationToken: cancellationToken);
            await ImportRelationshipsAsync(relationships, cancellationToken: cancellationToken);
        }

        private static KeyValuePair<string, object?> CreateDimension(string name, string value)
        {
            return new KeyValuePair<string, object?>(name, value);
        }

        /// <summary>
        /// Asynchronously import a set of relationships into the target Azure Digital Twins graph.
        /// </summary>
        /// <param name="relationships">Relationships to upload. Map key is the relationship ID.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>An awaitable task.</returns>
        protected virtual async Task ImportRelationshipsAsync(IDictionary<string, BasicRelationship> relationships, CancellationToken cancellationToken = default)
        {
            // Upload the relationships to ADT
            Logger.LogInformation("Total Relationships to create: {relationshipsCount}", relationships.Count);
            var relationshipCreateSuccesses = 0;
            var relationshipCreateFailures = 0;
            var relationshipCreateSkips = 0;
            var relationshipUpdateSuccesses = 0;
            var relationshipUpdateFailures = 0;
            var relationshipUpdateSkips = 0;

            // We need to shuffle this list to try to reduce the chances of going over the limit for the number of updates per twin per second
            var relationshipList = new List<BasicRelationship>(relationships.Values).Shuffle();

            try
            {
                foreach (var relationship in relationshipList)
                {
                    // Skip relationships that have a source or target that is in the list of twins to be skipped
                    if (skipRelationshipsWithTheseIds.Any(t => t == relationship.SourceId || t == relationship.TargetId))
                    {
                        Logger.LogInformation("Skipping create/update of relationship: Id: {RelationshipId}, SourceId: {SourceId}, TargetId: {TargetId}", relationship.Id, relationship.SourceId, relationship.TargetId);
                        relationshipCreateSkips++;
                        continue;
                    }

                    try
                    {
                        BasicRelationship? existingRelationship = null;
                        var action = Metrics.CreateActionDimension;

                        try
                        {
                            existingRelationship = await relationshipsClient.GetRelationshipAsync(relationship.SourceId, relationship.Id, cancellationToken);

                            if (existingRelationship != null)
                            {
                                action = Metrics.UpdateActionDimension;

                                // Determine if the relationship needs to be updated
                                if (RelationshipsMatch(existingRelationship, relationship))
                                {
                                    relationshipCounter.Add(1, metricsAttributesHelper.GetValues(
                                                            CreateDimension(Metrics.ActionDimensionName, action),
                                                            CreateDimension(Metrics.RelationshipTypeDimensionName, relationship.Name),
                                                            CreateDimension(Metrics.StatusDimensionName, Metrics.SkippedStatusDimension)));
                                    continue;
                                }
                            }
                        }
                        catch (ApiException ex) when (ex.StatusCode == 404)
                        {
                        }

                        try
                        {
                            Logger.LogInformation("Upserting relationship: Id: {RelationshipId}, SourceId: {SourceId}, TargetId: {TargetId}", relationship.Id, relationship.SourceId, relationship.TargetId);

                            await relationshipsClient.UpsertRelationshipAsync(relationship, cancellationToken);

                            relationshipCounter.Add(1, metricsAttributesHelper.GetValues(
                                                    CreateDimension(Metrics.ActionDimensionName, action),
                                                    CreateDimension(Metrics.RelationshipTypeDimensionName, relationship.Name),
                                                    CreateDimension(Metrics.StatusDimensionName, Metrics.SucceededStatusDimension)));

                            relationshipCreateSuccesses++;
                        }
                        catch (ApiException ex)
                        {
                            relationshipCreateFailures++;
                            Errors.TryAdd(relationship.Id, $"Failed to upsert relationship: Id: {relationship.Id}, SourceId: {relationship.SourceId}, TargetId: {relationship.TargetId}, Message: {ex.Message}. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Failed-to-upsert-relationship.");
                            Logger.LogError(ex, "Failed to upsert relationship: Id: {RelationshipId}, SourceId: {SourceId}, TargetId: {TargetId}", relationship.Id, relationship.SourceId, relationship.TargetId);
                            relationshipCounter.Add(1, metricsAttributesHelper.GetValues(
                                                    CreateDimension(Metrics.ActionDimensionName, action),
                                                    CreateDimension(Metrics.RelationshipTypeDimensionName, relationship.Name),
                                                    CreateDimension(Metrics.StatusDimensionName, Metrics.FailedStatusDimension)));
                        }
                    }
                    catch (RequestFailedException ex) when (ex.ErrorCode == "DigitalTwinNotFound")
                    {
                        relationshipCreateFailures++;
                        Errors.TryAdd(relationship.Id, $"Source Twin Not Found. Cannot create or update relationship: {JsonSerializer.Serialize(relationship)}. See: https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Source-Twin-Not-Found.-Cannot-create-or-update-relationship");
                        Logger.LogError(ex, "Source Twin Not Found. Cannot create or update relationship: {relationship}", JsonSerializer.Serialize(relationship));
                    }

                    // Output a status every 1000 relationships
                    if ((relationshipCreateSuccesses + relationshipCreateFailures + relationshipUpdateSuccesses + relationshipUpdateFailures + relationshipUpdateSkips + relationshipCreateSkips) % 1000 == 0)
                    {
                        Logger.LogInformation("{timestamp}: Total Relationships: {twinCount}, Successful relationship creates: {relationshipCreateSuccesses}, Failed Relationship creates: {relationshipCreateFailures}, Skipped Relationship creates: {relationshipCreateSkips} Successful relationship updates: {relationshipUpdateSuccesses}, Failed Relationship updates: {relationshipUpdateFailures}, Skipped Relationship updates: {relationshipUpdateSkips}",
                                              DateTimeOffset.Now,
                                              relationships.Count,
                                              relationshipCreateSuccesses,
                                              relationshipCreateFailures,
                                              relationshipCreateSkips,
                                              relationshipUpdateSuccesses,
                                              relationshipUpdateFailures,
                                              relationshipUpdateSkips);
                    }
                }

                Logger.LogInformation("{timestamp}: Total Relationships: {twinCount}, Successful relationship creates: {relationshipCreateSuccesses}, Failed Relationship creates: {relationshipCreateFailures}, Skipped Relationship creates: {relationshipCreateSkips} Successful relationship updates: {relationshipUpdateSuccesses}, Failed Relationship updates: {relationshipUpdateFailures}, Skipped Relationship updates: {relationshipUpdateSkips}",
                                          DateTimeOffset.Now,
                                          relationships.Count,
                                          relationshipCreateSuccesses,
                                          relationshipCreateFailures,
                                          relationshipCreateSkips,
                                          relationshipUpdateSuccesses,
                                          relationshipUpdateFailures,
                                          relationshipUpdateSkips);
            }
            catch (Exception ex)
            {
                Errors.TryAdd("Relationships", $"Failed to insert a relationship: {ex.Message}. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Failed-to-insert-a-relationship");
                Logger.LogError(ex, "Unable to insert a relationship.");
            }
        }

        /// <summary>
        /// Asynchronously import a set of digital twins into the target Azure Digital Twins graph.
        /// </summary>
        /// <param name="twins">Twins to upload. Map key is the dtId.</param>
        /// <param name="relationships">The list of relationships in the topology.</param>
        /// <param name="buildingId">The building id which was passed in to the request.</param>
        /// <param name="connectorId">The connector id which was passed in to the request.</param>
        /// <param name="autoApprove">A flag indicating whether or not to allow the twin creation to be automatically approved.</param>
        /// <param name="cancellationToken">Cancelation token.</param>
        /// <returns>An awaitable task.</returns>
        protected virtual async Task ImportTwinsAsync(IDictionary<string, BasicDigitalTwin> twins, IDictionary<string, BasicRelationship> relationships, string buildingId, string connectorId, bool autoApprove, CancellationToken cancellationToken = default)
        {
            // Upload the twins to ADT
            Logger.LogInformation("Total Twins to create: {twinsCount}", twins.Count);
            var twinCreateSuccesses = 0;
            var twinCreateFailures = 0;
            var twinUpdateSuccesses = 0;
            var twinUpdateFailures = 0;
            var twinUpdateSkips = 0;
            var mappingsAdded = 0;
            var mappingsPending = 0;
            var mappingsIgnore = 0;

            BasicDigitalTwin? lastTwin = null;

            try
            {
                IEnumerable<MappedEntry> allMappedEntries = await GetMappedEntriesFromWillowAsync(cancellationToken);

                const int batchSize = 50;
                var batches = twins.Chunk(batchSize);

                foreach (var batch in batches)
                {
                    var twinIdsInBatch = batch.Select(b => b.Value.Id);
                    Logger.LogInformation("Getting batch of {Count} twins, TwinIds {twinIds}", batch.Length, JsonSerializer.Serialize(twinIdsInBatch));

                    var existingTwins = await twinsClient.GetTwinsByIdsAsync(twinIdsInBatch, cancellationToken: cancellationToken);

                    Logger.LogInformation("Batch of {Count} twins returned from ADT.", existingTwins.Content.Count());

                    foreach (var twin in batch)
                    {
                        lastTwin = twin.Value;

                        // Is Twin in the list of twins in Willow
                        var existingTwin = existingTwins.Content.FirstOrDefault(t => t.Twin.Id == twin.Value.Id);
                        Logger.LogInformation("TwinId: {TwinId} found in existingTwins: {Discovered}.", twin.Value.Id, existingTwin != null);

                        var mappingId = GetMappingId(twin);

                        // See if the Twin exists in the Mapping Table. The ExternalId should always be the Mapped Id in the mapping table
                        // except in the case where this is a virtual twin for a connector, in which case the MappingId is the twinId
                        MappedEntry? mappedEntry = allMappedEntries.FirstOrDefault(m => m.MappedId == mappingId);

                        if (mappedEntry != null)
                        {
                            // Need to respect the overrides chosen by the users when creating or updating the twin.
                            twin.Value.Metadata.ModelId = mappedEntry.WillowModelId;
                            twin.Value.Contents["name"] = mappedEntry.Name;

                            if (!string.IsNullOrWhiteSpace(mappedEntry.WillowId) && (mappedEntry.WillowId != "-"))
                            {
                                twin.Value.Id = mappedEntry.WillowId;
                            }
                        }

                        // The twin doesn't exist
                        if (existingTwin == null)
                        {
                            // The entry is not found in the mapping table. Add it to the Mapping Table
                            if (mappedEntry == null)
                            {
                                Logger.LogInformation("Creating new mapping entry for twin, Key: {twinId}, Value.Twin.Id: {Value}", twin.Key, twin.Value.Id);
                                mappingsAdded += await CreateMappingEntry(twins, relationships, twin, buildingId, connectorId, cancellationToken);

                                // Add the Id to the list of Ids so we don't try to create relationships to it.
                                skipRelationshipsWithTheseIds.Add(twin.Value.Id);
                                continue;
                            }
                            else
                            {
                                if (!autoApprove)
                                {
                                    // If the status is ignored, we can skip this twin
                                    if (mappedEntry.Status == Status.Ignore)
                                    {
                                        mappingsIgnore += IgnoreMappingEntry(twin);

                                        // Add the Id to the list of Ids so we don't try to create relationships to it.
                                        skipRelationshipsWithTheseIds.Add(twin.Value.Id);
                                        continue;
                                    }

                                    // If the status is pending, we can skip this twin, but we want to update it first to make sure it is current
                                    if (mappedEntry.Status == Status.Pending)
                                    {
                                        mappingsPending += await UpdateMappingEntry(twins, relationships, twin, Status.Pending, buildingId, connectorId, cancellationToken);

                                        // Add the Id to the list of Ids so we don't try to create relationships to it.
                                        skipRelationshipsWithTheseIds.Add(twin.Value.Id);

                                        continue;
                                    }
                                }
                            }

                            try
                            {
                                // If we want to enable twin replace, we need to delete the existing twin and create a new one
                                // Note that this will delete all relationships to the twin as well
                                // We only do this when:
                                // 1. The Mapped Entry is approved
                                // 2. The twinID of an existing twin matches the externalID of the approved twin which means that somehow the Mapped Entry was previously loaded as a twin.
                                // 3. The EnableTwinReplace option is set to true
                                if (options.Value.EnableTwinReplace)
                                {
                                    Logger.LogInformation("Checking to see if existing twin is a duplicate: {TwinId}", mappedEntry.MappedId);

                                    // See if there is an existing twin with the twin Id that matched the external Id of the approved twin
                                    var externalIdTwin = await twinsClient.GetTwinByIdAsync(mappedEntry.MappedId, cancellationToken: cancellationToken);

                                    if (externalIdTwin?.Twin != null)
                                    {
                                        Logger.LogInformation("Deleting existing twin because is it a duplicate: {TwinId}", externalIdTwin.Twin.Id);

                                        // Delete the existing twin
                                        await twinsClient.DeleteTwinsAndRelationshipsAsync(new[] { externalIdTwin.Twin.Id }, true, cancellationToken: cancellationToken);
                                    }
                                }

                                // If the twin is a connector, we need to assign a unique ID to it since it is new
                                if (twin.Value.Metadata.ModelId == Constants.WillowConnectorApplicationModelId)
                                {
                                    twin.Value.Contents["uniqueID"] = Guid.NewGuid();
                                }

                                await twinsClient.UpdateTwinAsync(twin.Value, cancellationToken: cancellationToken);

                                if (!autoApprove)
                                {
                                    await UpdateMappingEntry(twins, relationships, twin, Status.Created, buildingId, connectorId, cancellationToken);
                                }

                                twinCounter.Add(1, metricsAttributesHelper.GetValues(
                                                CreateDimension(Metrics.ActionDimensionName, Metrics.CreateActionDimension),
                                                CreateDimension(Metrics.ModelIdDimensionName, twin.Value.Metadata.ModelId),
                                                CreateDimension(Metrics.StatusDimensionName, Metrics.SucceededStatusDimension)));
                                twinCreateSuccesses++;
                            }
                            catch (Exception ex)
                            {
                                Errors.TryAdd(twin.Value.Id, $"Failed to update twin: {JsonSerializer.Serialize(twin.Value)}. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Failed-to-update-twin");
                                Logger.LogError(ex, "Failed to update twin: {twin}", JsonSerializer.Serialize(twin.Value));
                                twinCreateFailures++;

                                // Add the Id to the list of Ids so we don't try to create relationships to it.
                                skipRelationshipsWithTheseIds.Add(twin.Value.Id);

                                twinCounter.Add(1, metricsAttributesHelper.GetValues(
                                                CreateDimension(Metrics.ActionDimensionName, Metrics.CreateActionDimension),
                                                CreateDimension(Metrics.ModelIdDimensionName, twin.Value.Metadata.ModelId),
                                                CreateDimension(Metrics.StatusDimensionName, Metrics.FailedStatusDimension)));
                            }
                        }
                        else
                        {
                            if (options.Value.EnableUpdates)
                            {
                                try
                                {
                                    if (TwinMergeHelper.TryCreatePatchDocument(existingTwin.Twin, twin.Value, out var patchDocument))
                                    {
                                        var manualOps = new List<Microsoft.AspNetCore.JsonPatch.Operations.Operation>();

                                        foreach (var operation in patchDocument.Operations)
                                        {
                                            if (AutoPatchProperties.Contains(operation.path))
                                            {
                                                var field = operation.path.Split("/").Last();
                                                existingTwin.Twin.Contents[field] = operation.value;
                                            }
                                            else
                                            {
                                                manualOps.Add(operation);
                                                Logger.LogInformation("Manual operation needed: TwinId: {TwinId}: Operation: {Operation}", twin.Value.Id, JsonSerializer.Serialize(operation));
                                            }
                                        }

                                        // If the twin is a connector, we need to assign a unique ID to it if it is missing
                                        if (twin.Value.Metadata.ModelId == Constants.WillowConnectorApplicationModelId && (!twin.Value.Contents.TryGetValue("uniqueID", out var uniqueId) || string.IsNullOrEmpty(uniqueId?.ToString())))
                                        {
                                            twin.Value.Contents["uniqueID"] = Guid.NewGuid();
                                        }

                                        await twinsClient.UpdateTwinAsync(existingTwin.Twin, cancellationToken: cancellationToken);

                                        twinCounter.Add(1, metricsAttributesHelper.GetValues(
                                                        CreateDimension(Metrics.ActionDimensionName, Metrics.UpdateActionDimension),
                                                        CreateDimension(Metrics.ModelIdDimensionName, twin.Value.Metadata.ModelId),
                                                        CreateDimension(Metrics.StatusDimensionName, Metrics.SucceededStatusDimension)));
                                        twinUpdateSuccesses++;

                                        // If the mapping entry is approved, we can update it to created
                                        // This shouldn't happen, but because not everything is an atomic commit, it is possible
                                        // that the twin was previsouly created, but the mapping entry was not updated from approved to created
                                        if (mappedEntry != null && mappedEntry.Status == Status.Approved)
                                        {
                                            await UpdateMappingEntry(twins, relationships, twin, Status.Created, buildingId, connectorId, cancellationToken);
                                        }

                                        if (manualOps.Any())
                                        {
                                            Logger.LogInformation("Manual operations needed for twin: {TwinId}: Operations: {Operations}", twin.Value.Id, JsonSerializer.Serialize(manualOps));

                                            var jsonPatchOperations = new List<JsonPatchOperation>();

                                            foreach (var operation in manualOps)
                                            {
                                                var jsonPatchOperation = new JsonPatchOperation()
                                                {
                                                    Op = operation.OperationType,
                                                    Path = operation.path,
                                                    Value = operation.value,
                                                };

                                                jsonPatchOperations.Add(jsonPatchOperation);
                                            }

                                            await mappingClient.CreateUpdateTwinRequestAsync(jsonPatchOperations, twin.Value.Id, cancellationToken);
                                        }
                                    }
                                    else
                                    {
                                        twinUpdateSkips++;
                                        twinCounter.Add(1, metricsAttributesHelper.GetValues(
                                                        CreateDimension(Metrics.ActionDimensionName, Metrics.UpdateActionDimension),
                                                        CreateDimension(Metrics.ModelIdDimensionName, twin.Value.Metadata.ModelId),
                                                        CreateDimension(Metrics.StatusDimensionName, Metrics.SkippedStatusDimension)));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Errors.TryAdd(twin.Value.Id, $"Failed to insert/update twin: {JsonSerializer.Serialize(twin)}. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Failed-to-insert%2Fupdate-twin");
                                    Logger.LogError(ex, "Failed to insert/update twin: {Twin}", JsonSerializer.Serialize(twin));
                                    twinUpdateFailures++;
                                    twinCounter.Add(1, metricsAttributesHelper.GetValues(
                                                    CreateDimension(Metrics.ActionDimensionName, Metrics.UpdateActionDimension),
                                                    CreateDimension(Metrics.ModelIdDimensionName, twin.Value.Metadata.ModelId),
                                                    CreateDimension(Metrics.StatusDimensionName, Metrics.FailedStatusDimension)));
                                }
                            }
                            else
                            {
                                twinUpdateSkips++;
                            }
                        }

                        // Output a status every 1000 relationships
                        if ((twinCreateSuccesses + twinCreateFailures + twinUpdateSuccesses + twinUpdateFailures + twinUpdateSkips + mappingsAdded + mappingsPending + mappingsIgnore) % 1000 == 0)
                        {
                            Logger.LogInformation("{timestamp}: Total Twins: {twinCount}, Successful twin creates: {twinCreateSuccesses}, Failed twin creates: {twinCreateFailures}, Successful twin updates: {twinUpdateSuccesses}, Failed twin updates: {twinUpdateFailures}, Skipped twin updates: {twinUpdateSkips}, Mappings Added: {mappingsAdded}, Mappings Pending: {mappingsPending}, Mappings Ignored: {mappingsIgnored}",
                                                  DateTimeOffset.Now,
                                                  twins.Count,
                                                  twinCreateSuccesses,
                                                  twinCreateFailures,
                                                  twinUpdateSuccesses,
                                                  twinUpdateFailures,
                                                  twinUpdateSkips,
                                                  mappingsAdded,
                                                  mappingsPending,
                                                  mappingsIgnore);
                        }
                    }
                }

                Logger.LogInformation("{timestamp}: Total Twins: {twinCount}, Successful twin creates: {twinCreateSuccesses}, Failed twin creates: {twinCreateFailures}, Successful twin updates: {twinUpdateSuccesses}, Failed twin updates: {twinUpdateFailures}, Skipped twin updates: {twinUpdateSkips}, Mappings Added: {mappingsAdded}, Mappings Pending: {mappingsPending}, Mappings Ignored: {mappingsIgnored}",
                                          DateTimeOffset.Now,
                                          twins.Count,
                                          twinCreateSuccesses,
                                          twinCreateFailures,
                                          twinUpdateSuccesses,
                                          twinUpdateFailures,
                                          twinUpdateSkips,
                                          mappingsAdded,
                                          mappingsPending,
                                          mappingsIgnore);
            }
            catch (Exception ex)
            {
                Errors.TryAdd("Twins", $"Failed to insert/update a twin: {ex.Message}. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Failed-to-insert%2Fupdate-twin");
                Logger.LogError(ex, "Unable to insert/update a twin. {Twin}", lastTwin != null ? JsonSerializer.Serialize(lastTwin) : "null");
            }
        }

        private static string GetMappingId(KeyValuePair<string, BasicDigitalTwin> twin)
        {
            twin.Value.Contents.TryGetValue("externalID", out var extId);
            return extId?.ToString() ?? twin.Value.Id;
        }

        private async Task<IEnumerable<MappedEntry>> GetMappedEntriesFromWillowAsync(CancellationToken cancellationToken)
        {
            const int pagesize = 100;
            int iteration = 0;
            List<MappedEntry> entries = new List<MappedEntry>();

            var mappedEntryRequest = new MappedEntryRequest()
            {
                PageSize = pagesize,
                Offset = pagesize * iteration,
                FilterSpecifications = new List<FilterSpecificationDto>() { },
            };

            var mappedEntriesResponse = await mappingClient.GetMappedEntriesAsync(mappedEntryRequest, cancellationToken: cancellationToken);
            var mappedEntries = mappedEntriesResponse.Items.ToList();
            Logger.LogInformation("{MappingEntriesCount} Mapping Entries retrieved from Mappings Table.", mappedEntries.Count);

            while (mappedEntries.Count > 0)
            {
                entries.AddRange(mappedEntries);
                iteration++;
                mappedEntryRequest.Offset = iteration * pagesize;

                mappedEntriesResponse = await mappingClient.GetMappedEntriesAsync(mappedEntryRequest, cancellationToken: cancellationToken);
                mappedEntries = mappedEntriesResponse.Items.ToList();
                Logger.LogInformation("{MappingEntriesCount} Mapping Entries retrieved from Mappings Table.", mappedEntries.Count);
            }

            Logger.LogInformation("{MappingEntriesCount} Total Mapping Entries retrieved from Mappings Table.", entries.Count);

            return entries;
        }

        private int IgnoreMappingEntry(KeyValuePair<string, BasicDigitalTwin> twin)
        {
            twinCounter.Add(1, metricsAttributesHelper.GetValues(
                            CreateDimension(Metrics.ActionDimensionName, Metrics.MappingCreateActionDimension),
                            CreateDimension(Metrics.ModelIdDimensionName, twin.Value.Metadata.ModelId),
                            CreateDimension(Metrics.StatusDimensionName, Metrics.MappingIgnoredStatusDimension)));
            return 1;
        }

        private async Task<int> CreateMappingEntry(IDictionary<string, BasicDigitalTwin> twins, IDictionary<string, BasicRelationship> relationships, KeyValuePair<string, BasicDigitalTwin> twin, string buildingId, string connectorId, CancellationToken cancellationToken)
        {
            var relationship = AdtApiGraphManager<TOptions>.GetRelationship(relationships, twin.Value.Id);
            twin.Value.Contents.TryGetValue("description", out var desc);
            var mappingId = GetMappingId(twin);

            string? parentExternalId = string.Empty;
            string? parentWillowId = string.Empty;
            if (relationship != null)
            {
                var rel = relationship.Item1;

                if (rel != null)
                {
                    if (relationship.Item2 == "target")
                    {
                        if (twins.TryGetValue(rel.SourceId, out var sourceTwin))
                        {
                            if (sourceTwin.Contents.TryGetValue("externalID", out object? parentExternalIdObj))
                            {
                                parentExternalId = parentExternalIdObj == null ? string.Empty : parentExternalIdObj.ToString();
                                parentWillowId = relationship != null ? sourceTwin.Id : null;
                            }
                        }
                        else
                        {
                            Logger.LogInformation("Source twin {TwinId} for relationship {Relationship} not found as key in list of twins.", rel.SourceId, relationship.Item1);
                        }
                    }
                    else
                    {
                        if (twins.TryGetValue(rel.TargetId, out var targetTwin))
                        {
                            if (targetTwin.Contents.TryGetValue("externalID", out object? parentExternalIdObj))
                            {
                                parentExternalId = parentExternalIdObj == null ? string.Empty : parentExternalIdObj.ToString();
                                parentWillowId = relationship != null ? targetTwin.Id : null;
                            }
                        }
                        else
                        {
                            Logger.LogInformation("Target twin {TwinId} for relationship {Relationship} not found as key in list of twins.", rel.TargetId, relationship.Item1);
                        }
                    }
                }
            }

            string mappedModelId = GetMappedModelId(twin);
            string unitId = GetUnitId(twin);
            string dataType = GetDataType(twin);

            var createMappedEntry = new CreateMappedEntry()
            {
                Description = desc == null || string.IsNullOrWhiteSpace(desc.ToString()) ? string.Empty : desc.ToString(),
                MappedModelId = mappedModelId,
                Name = twin.Value.Contents["name"].ToString(),
                WillowModelId = twin.Value.Metadata.ModelId,
                Status = Status.Pending,
                MappedId = mappingId,
                ParentMappedId = parentExternalId,
                ParentWillowId = parentWillowId,
                WillowParentRel = relationship?.Item1?.Name,
                ConnectorId = connectorId,
                WillowId = twin.Value.Id,
                BuildingId = buildingId,
                Unit = unitId,
                DataType = dataType,
            };

            Logger.LogInformation("Creating mapping entry: TwinId: {TwinId}, MappedEntry: {MappedEntry}", twin.Value.Id, JsonSerializer.Serialize(createMappedEntry));

            try
            {
                await mappingClient.CreateMappedEntryAsync(createMappedEntry, cancellationToken);
                twinCounter.Add(1, metricsAttributesHelper.GetValues(
                                CreateDimension(Metrics.ActionDimensionName, Metrics.MappingCreateActionDimension),
                                CreateDimension(Metrics.ModelIdDimensionName, twin.Value.Metadata.ModelId),
                                CreateDimension(Metrics.StatusDimensionName, Metrics.SucceededStatusDimension)));
                return 1;
            }
            catch (Exception ex)
            {
                Errors.TryAdd(twin.Value.Id, $"Failed to create mapping entry: {JsonSerializer.Serialize(createMappedEntry)}. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Failed-to-create-mapping-entry");
                Logger.LogError(ex, "Failed to create mapping entry for {MappingEntry}", JsonSerializer.Serialize(createMappedEntry));

                twinCounter.Add(1, metricsAttributesHelper.GetValues(
                                CreateDimension(Metrics.ActionDimensionName, Metrics.MappingCreateActionDimension),
                                CreateDimension(Metrics.ModelIdDimensionName, twin.Value.Metadata.ModelId),
                                CreateDimension(Metrics.StatusDimensionName, Metrics.FailedStatusDimension)));
                return 0;
            }
        }

        private async Task<int> UpdateMappingEntry(IDictionary<string, BasicDigitalTwin> twins,
                                                   IDictionary<string, BasicRelationship> relationships,
                                                   KeyValuePair<string, BasicDigitalTwin> twin,
                                                   Status twinStatus,
                                                   string buildingId,
                                                   string connectorId,
                                                   CancellationToken cancellationToken)
        {
            var relationship = AdtApiGraphManager<TOptions>.GetRelationship(relationships, twin.Value.Id);
            twin.Value.Contents.TryGetValue("description", out var desc);
            var mappingId = GetMappingId(twin);

            string? parentExternalId = string.Empty;
            string? parentWillowId = string.Empty;

            if (relationship != null)
            {
                var rel = relationship.Item1;

                if (rel != null)
                {
                    if (relationship?.Item2 == "target")
                    {
                        if (twins.TryGetValue(rel.SourceId, out var sourceTwin))
                        {
                            if (sourceTwin.Contents.TryGetValue("externalID", out object? parentExternalIdObj))
                            {
                                parentExternalId = parentExternalIdObj == null ? string.Empty : parentExternalIdObj.ToString();
                                parentWillowId = relationship != null ? sourceTwin.Id : null;
                            }
                        }
                        else
                        {
                            Logger.LogWarning("Cannot find source twin {TwinId} for relationship in list of twins.", rel.SourceId);
                        }
                    }
                    else
                    {
                        if (twins.TryGetValue(rel.TargetId, out var targetTwin))
                        {
                            if (targetTwin.Contents.TryGetValue("externalID", out object? parentExternalIdObj))
                            {
                                parentExternalId = parentExternalIdObj == null ? string.Empty : parentExternalIdObj.ToString();
                                parentWillowId = relationship != null ? targetTwin.Id : null;
                            }
                        }
                        else
                        {
                            Logger.LogWarning("Cannot find target twin {TwinId} for relationship in list of twins.", rel.TargetId);
                        }
                    }
                }
            }

            var mappedModelId = GetMappedModelId(twin);
            var unitId = GetUnitId(twin);
            string dataType = GetDataType(twin);

            var updateMappedEntry = new UpdateMappedEntry()
            {
                Description = desc == null || string.IsNullOrWhiteSpace(desc.ToString()) ? string.Empty : desc.ToString(),
                MappedModelId = mappedModelId,
                Name = twin.Value.Contents["name"].ToString(),
                WillowModelId = twin.Value.Metadata.ModelId,
                Status = twinStatus,
                MappedId = mappingId,
                ParentMappedId = parentExternalId,
                ParentWillowId = parentWillowId,
                WillowParentRel = relationship?.Item1?.Name,
                ConnectorId = connectorId,
                WillowId = twin.Value.Id,
                BuildingId = buildingId,
                Unit = unitId,
                DataType = dataType,
            };

            Logger.LogInformation("Updating mapping entry: {MappedEntry}", JsonSerializer.Serialize(updateMappedEntry));

            try
            {
                await mappingClient.UpdateMappedEntryAsync(updateMappedEntry, cancellationToken);
                twinCounter.Add(1, metricsAttributesHelper.GetValues(
                                CreateDimension(Metrics.ActionDimensionName, Metrics.MappingUpdateActionDimension),
                                CreateDimension(Metrics.ModelIdDimensionName, twin.Value.Metadata.ModelId),
                                CreateDimension(Metrics.StatusDimensionName, Metrics.SucceededStatusDimension)));
                return 1;
            }
            catch (Exception ex)
            {
                Errors.TryAdd(twin.Value.Id, $"Failed to update mapping entry: {JsonSerializer.Serialize(updateMappedEntry)}. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Failed-to-update-mapping-entry");
                Logger.LogError(ex, "Unable to create Mapping entry for {MappingEntry}", JsonSerializer.Serialize(updateMappedEntry));
                twinCounter.Add(1, metricsAttributesHelper.GetValues(
                                CreateDimension(Metrics.ActionDimensionName, Metrics.MappingUpdateActionDimension),
                                CreateDimension(Metrics.ModelIdDimensionName, twin.Value.Metadata.ModelId),
                                CreateDimension(Metrics.StatusDimensionName, Metrics.FailedStatusDimension)));

                return 0;
            }
        }

        private static string GetMappedModelId(KeyValuePair<string, BasicDigitalTwin> twin)
        {
            twin.Value.Contents.TryGetValue("alternateClassification", out var alternateClassification);

            string mappedModelId = string.Empty;

            var altClassStr = alternateClassification?.ToString();

            if (altClassStr != null)
            {
                dynamic ac = JObject.Parse(altClassStr);
                var code = ac.brickSchema.code;
                mappedModelId = code.ToString();
            }

            return mappedModelId;
        }

        private static string GetUnitId(KeyValuePair<string, BasicDigitalTwin> twin)
        {
            twin.Value.Contents.TryGetValue("unit", out var unit);

            string unitId = string.Empty;

            var unitStr = unit?.ToString();

            if (unitStr != null)
            {
                dynamic un = JObject.Parse(unitStr);
                var id = un.id;
                unitId = id.ToString();
            }

            return unitId;
        }

        private static string GetDataType(KeyValuePair<string, BasicDigitalTwin> twin)
        {
            twin.Value.Contents.TryGetValue("type", out var type);

            var typeStr = type?.ToString() ?? string.Empty;

            return typeStr;
        }

        private static Tuple<BasicRelationship?, string> GetRelationship(IDictionary<string, BasicRelationship> relationships, string twinId)
        {
            var relationship = relationships.Where(r => r.Value.TargetId == twinId);

            if (relationship != null && relationship.Any())
            {
                return new Tuple<BasicRelationship?, string>(relationship.FirstOrDefault().Value, "target");
            }

            relationship = relationships.Where(r => r.Value.SourceId == twinId);

            if (relationship != null && relationship.Any())
            {
                // We have a preference order for which relationship to display. If there is more that one, use the following order:
                // 1. isCapabilityOf
                // 2. isPartOf
                // 5. Anything else
                if (relationship.FirstOrDefault(r => r.Value.Name == RelationshipTypes.IsCapabilityOf).Value != null)
                {
                    return new Tuple<BasicRelationship?, string>(relationship.FirstOrDefault(r => r.Value.Name == RelationshipTypes.IsCapabilityOf).Value, "source");
                }

                if (relationship.FirstOrDefault(r => r.Value.Name == RelationshipTypes.IsPartOf).Value != null)
                {
                    return new Tuple<BasicRelationship?, string>(relationship.FirstOrDefault(r => r.Value.Name == RelationshipTypes.IsPartOf).Value, "source");
                }

                return new Tuple<BasicRelationship?, string>(relationship.FirstOrDefault().Value, "source");
            }

            return new Tuple<BasicRelationship?, string>(null, string.Empty);
        }

        private static bool RelationshipsMatch(BasicRelationship existingRelationship, BasicRelationship newRelationship)
        {
            return existingRelationship.Name == newRelationship.Name &&
                   existingRelationship.SourceId == newRelationship.SourceId &&
                   existingRelationship.TargetId == newRelationship.TargetId &&
                   RelationshipPropertiesMatch(existingRelationship.Properties, newRelationship.Properties);
        }

        private static bool RelationshipPropertiesMatch(IDictionary<string, object> existingProperties, IDictionary<string, object> newProperties)
        {
            if (existingProperties.Count != newProperties.Count)
            {
                return false;
            }

            foreach (var property in newProperties)
            {
                // If the number of properties is different, then the relationships are different
                if (!existingProperties.TryGetValue(property.Key, out var existingValue))
                {
                    return false;
                }

                // If the value is not a string, then assume the properties are different (this is a simplification)
                if (existingValue is not string)
                {
                    return false;
                }

                // If the values are string and are different, then the relationships are different
                if (existingValue.ToString() != property.Value.ToString())
                {
                    return false;
                }
            }

            // The properties are the same
            return true;
        }
    }
}
