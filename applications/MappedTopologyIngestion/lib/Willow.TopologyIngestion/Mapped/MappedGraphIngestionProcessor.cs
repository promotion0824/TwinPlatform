//-----------------------------------------------------------------------
// <copyright file="MappedGraphIngestionProcessor.cs" Company="Willow">
//   Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Willow.TopologyIngestion.Mapped
{
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using Azure.DigitalTwins.Core;
    using DTDLParser;
    using DTDLParser.Models;
    using global::Mapped.Ontologies.Mappings.OntologyMapper;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Willow.Telemetry;
    using Willow.TopologyIngestion;
    using Willow.TopologyIngestion.Entities;
    using Willow.TopologyIngestion.Interfaces;

    /// <summary>
    /// Loads a building graph from a Mapped input source to the target.
    /// The logic here is specific to the way Mapped stores its topology and if a new
    /// Input Graph Provider is added, this logic will likely have to be customized.
    /// </summary>
    /// <typeparam name="TOptions">Anything that inherits from the base class of IngestionManagerOptions.</typeparam>
    public class MappedGraphIngestionProcessor<TOptions> : IGraphIngestionProcessor
    where TOptions : IngestionManagerOptions
    {
        private readonly Counter<long> exactTypeNotFoundCounter;
        private readonly Counter<long> accountProcessedCounter;
        private readonly Counter<long> siteProcessedCounter;
        private readonly Counter<long> siteNotFoundCounter;
        private readonly Counter<long> twinsCounter;
        private readonly Counter<long> buildingsCounter;
        private readonly Counter<long> relationshipsCounter;
        private readonly Counter<long> thingDtmiNotFoundCounter;
        private readonly Counter<long> relationshipNotFoundInModelCounter;
        private readonly Counter<long> duplicateMappingPropertyFoundCounter;
        private readonly Counter<long> inputInterfaceNotFoundCounter;
        private readonly Counter<long> invalidTargetDtmisCounter;
        private readonly Counter<long> invalidOutputDtmiCounter;
        private readonly Counter<long> targetDtmiNotFoundCounter;
        private readonly Counter<long> outputMappingForInputDtmiNotFoundCounter;
        private readonly Counter<long> mappingForInputDtmiNotFoundCounter;
        private readonly MetricsAttributesHelper metricsAttributesHelper;

        private const string WillowConnectorTypeId = "willow-source";
        private const string BacNetConnectorExactType = "BACnetObjectId";
        private const string IdentityTwinPrefix = "IDN";

        private readonly IGraphNamingManager graphNamingManager;

        /// <summary>
        /// Default mapped connector ID, used when no connector ID is specified in the input graph.
        /// </summary>
        protected const string DefaultMappedConnectorId = "00000000-35C5-4415-A4B3-7B798D0568E8";

        /// <summary>
        /// Initializes a new instance of the <see cref="MappedGraphIngestionProcessor{TOptions}"/> class.
        /// </summary>
        /// <param name="logger">An instance of an <see cref="ILogger">ILogger</see> used to log status as needed.</param>
        /// <param name="inputGraphManager">An instance of an <see cref="IInputGraphManager">IInputGraphManager</see> used to load a graph from the input source.</param>
        /// <param name="ontologyMappingManager">An instance of an <see cref="IOntologyMappingManager">IOntologyMappingManager</see> used to map the input ontology to the output ontology.</param>
        /// <param name="outputGraphManager">An instance of an <see cref="IOutputGraphManager">IOutputGraphManager</see> used to save a graph to the output target.</param>
        /// <param name="graphNamingManager">An instance of an <see cref="IGraphNamingManager">IGraphNamingManager</see> used to build the names of items in the graph.</param>
        /// <param name="options">Ingestion Manager Options.</param>
        /// <param name="meterFactory">The meter factory for creating meters.</param>
        /// <param name="metricsAttributesHelper">An instance of the metrics helper.</param>
        public MappedGraphIngestionProcessor(ILogger<MappedGraphIngestionProcessor<TOptions>> logger,
            IInputGraphManager inputGraphManager,
            IOntologyMappingManager ontologyMappingManager,
            IOutputGraphManager outputGraphManager,
            IGraphNamingManager graphNamingManager,
            IOptions<TOptions> options,
            IMeterFactory meterFactory,
            MetricsAttributesHelper metricsAttributesHelper)
        {
            Logger = logger;
            var meterOptions = options.Value.MeterOptions;
            var meter = meterFactory.Create(meterOptions.Name, meterOptions.Version, meterOptions.Tags);
            exactTypeNotFoundCounter = meter.CreateCounter<long>("Mti-ExactTypeNotFound", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            accountProcessedCounter = meter.CreateCounter<long>("Mti-AccountProcessed", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            siteProcessedCounter = meter.CreateCounter<long>("Mti-SiteProcessed", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            siteNotFoundCounter = meter.CreateCounter<long>("Mti-SiteNotFound", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            twinsCounter = meter.CreateCounter<long>("Mti-Twins", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            relationshipsCounter = meter.CreateCounter<long>("Mti-Relationships", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            buildingsCounter = meter.CreateCounter<long>("Mti-Buildings", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            thingDtmiNotFoundCounter = meter.CreateCounter<long>("Mti-ThingDtmiNotFound", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            relationshipNotFoundInModelCounter = meter.CreateCounter<long>("Mti-RelationshipNotFoundInModel", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            duplicateMappingPropertyFoundCounter = meter.CreateCounter<long>("Mti-DuplicateMappingPropertyFound", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            inputInterfaceNotFoundCounter = meter.CreateCounter<long>("Mti-InputInterfaceNotFound", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            invalidTargetDtmisCounter = meter.CreateCounter<long>("Mti-InvalidTargetDtmis", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            invalidOutputDtmiCounter = meter.CreateCounter<long>("Mti-InvalidOutputDtmi", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            targetDtmiNotFoundCounter = meter.CreateCounter<long>("Mti-TargetDtmiNotFound", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            outputMappingForInputDtmiNotFoundCounter = meter.CreateCounter<long>("Mti-OutputMappingForInputDtmiNotFound", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            mappingForInputDtmiNotFoundCounter = meter.CreateCounter<long>("Mti-MappingForInputDtmiNotFound", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());

            Options = options;
            this.metricsAttributesHelper = metricsAttributesHelper;
            InputGraphManager = inputGraphManager;
            OntologyMappingManager = ontologyMappingManager;
            TargetModelParser = new ModelParser(new ParsingOptions() { AllowUndefinedExtensions = WhenToAllow.Always });
            OutputGraphManager = outputGraphManager;
            this.graphNamingManager = graphNamingManager;
        }

        private static Dtmi GridRegion => new Dtmi("dtmi:com:willowinc:GridRegion;1");

        /// <inheritdoc/>
        public IDictionary<string, string> Errors { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets the entity types to sync.
        /// </summary>
        protected EntityTypes EntityTypes { get; } = new EntityTypes
        {
            IncludeOrganization = false,
            IncludeSites = false,
            IncludeBuildings = false,
            IncludeConnectors = false,
            IncludeSpaces = false,
            IncludeLevels = false,
            IncludePoints = false,
            IncludeThings = false,
        };

        /// <summary>
        /// Gets Configuration Options.
        /// </summary>
        protected IOptions<IngestionManagerOptions> Options { get; }

        /// <summary>
        /// Gets ingestion processor local logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets ontology mapping manager.
        /// </summary>
        protected IOntologyMappingManager OntologyMappingManager { get; }

        /// <summary>
        /// Gets input graph manager.
        /// </summary>
        protected IInputGraphManager InputGraphManager { get; }

        /// <summary>
        /// Gets a JSON object with only an empty <c>$metadata</c> field, used to scaffold an empty DTDL Component in target twins.
        /// </summary>
        protected static JsonElement EmptyComponentElement { get => JsonDocument.Parse("{ \"$metadata\": {} }").RootElement; }

        /// <summary>
        ///  Gets target model parser, used to read target ontology into memory.
        /// </summary>
        protected ModelParser TargetModelParser { get; }

        /// <summary>
        /// Gets output graph manager.
        /// </summary>
        protected IOutputGraphManager OutputGraphManager { get; }

        /// <summary>
        /// Gets or sets the site id for the current building.
        /// </summary>
        protected string SiteId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the WillowConnectorId for the current building.
        /// </summary>
        protected string WillowConnectorId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the BacNetConnectorId for the current building.
        /// </summary>
        protected string BacNetConnectorId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ConnectorMappings for the current building.
        /// </summary>
        protected Dictionary<string, Guid> ConnectorMappings { get; set; } = [];

        /// <summary>
        /// Gets in-memory representation of target ontology model (such that it can be
        /// queried for mapping validations).
        /// <br /><br />
        /// Because this value is determined in an async call, it cannot be called in the constructor,
        /// so we use the null-forgiving operator (null!) to tell the compiler that this is set later
        /// (in the Init method).
        /// </summary>
        protected IReadOnlyDictionary<Dtmi, DTEntityInfo> TargetObjectModel { get; private set; } = null!;

        /// <summary>
        /// Gets a dictionary of all the input interfaces that are not found in the target model indexed by model id.
        /// </summary>
        protected IDictionary<string, DTEntityInfo> TargetObjectModels { get; private set; } = new Dictionary<string, DTEntityInfo>();

        private async Task SyncAccounts(IDictionary<string, BasicDigitalTwin> twins,
                                        IDictionary<string, BasicRelationship> relationships,
                                        CancellationToken cancellationToken)
        {
            if (EntityTypes.IncludeAccounts)
            {
                // Generate the outermost query to run against the input graph. Starts by getting the list of sites
                var query = InputGraphManager.GetAccountsQuery();

                // Get the input twin graph
                var inputAccounts = await InputGraphManager.GetTwinGraphAsync(query, cancellationToken).ConfigureAwait(false);

                // Loop through all of the accounts and process
                if (inputAccounts != null)
                {
                    foreach (var topElement in inputAccounts.RootElement.EnumerateObject())
                    {
                        foreach (var dataElement in topElement.Value.EnumerateObject())
                        {
                            foreach (var accountElement in dataElement.Value.EnumerateArray())
                            {
                                // Get the Id of the individual item in the graph
                                if (!GetTwinId(accountElement, out var accountMapping))
                                {
                                    return;
                                }

                                // Look up the Model Id from the Incoming element
                                if (accountElement.TryGetProperty("exactType", out var accountExactType))
                                {
                                    var accountDtmi = AddTwin(twins, accountElement, accountMapping.TwinId, accountExactType.ToString(), false);

                                    // Determine if the node has descendants, and if so, iterate through them to add child twins
                                    var elements = accountElement.EnumerateObject();

                                    foreach (var innerElement in elements)
                                    {
                                        switch (innerElement.Value.ValueKind)
                                        {
                                            case JsonValueKind.Array:

                                                if (innerElement.Name == "hasProvider")
                                                {
                                                    foreach (var item in innerElement.Value.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.Object))
                                                    {
                                                        // Get the Id of the individual item in the graph
                                                        if (!GetTwinId(item, out var providerTwinMapping))
                                                        {
                                                            return;
                                                        }

                                                        // Look up the Model Id from the Incoming element
                                                        if (item.TryGetProperty("exactType", out var providerExactType))
                                                        {
                                                            AddTwin(twins, item, providerTwinMapping.TwinId, providerExactType.ToString(), false);
                                                            var relationshipProperties = new Dictionary<string, object>();

                                                            AddRelationship(relationships, accountMapping.TwinId, accountDtmi, RelationshipType.IsProvidedBy, providerTwinMapping.TwinId, providerExactType.ToString(), relationshipProperties);
                                                        }
                                                    }
                                                }

                                                if (innerElement.Name == "hasBill")
                                                {
                                                    foreach (var item in innerElement.Value.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.Object))
                                                    {
                                                        // Get the Id of the individual item in the graph
                                                        if (!GetTwinId(item, out var billTwinMapping))
                                                        {
                                                            return;
                                                        }

                                                        // Look up the Model Id from the Incoming element
                                                        if (item.TryGetProperty("exactType", out var billExactType))
                                                        {
                                                            AddTwin(twins, item, billTwinMapping.TwinId, billExactType.ToString(), false);
                                                            var relationshipProperties = new Dictionary<string, object>();

                                                            AddRelationship(relationships, accountMapping.TwinId, accountDtmi, RelationshipType.HasUtilityBill, billTwinMapping.TwinId, billExactType.ToString(), relationshipProperties);
                                                        }
                                                    }
                                                }

                                                break;
                                        }
                                    }

                                    accountProcessedCounter.Add(1, metricsAttributesHelper.GetValues(new KeyValuePair<string, object?>(Metrics.StatusDimensionName, true)));
                                }
                            }
                        }
                    }
                }
                else
                {
                    Errors.TryAdd("Processing Error", "No Mapped sites found for this organization. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI");
                    Logger.LogInformation("No sites found. Please check the configuration.");
                    siteNotFoundCounter.Add(1, metricsAttributesHelper.GetValues());
                }
            }
        }

        private async Task SyncOrganization(IDictionary<string, BasicDigitalTwin> twins,
                                            IDictionary<string, BasicRelationship> relationships,
                                            CancellationToken cancellationToken)
        {
            if (EntityTypes.IncludeOrganization)
            {
                // Generate the outermost query to run against the input graph. Starts by getting the list of sites
                var query = InputGraphManager.GetOrganizationQuery();

                // Get the input twin graph
                var inputSites = await InputGraphManager.GetTwinGraphAsync(query, cancellationToken).ConfigureAwait(false);

                // Loop through all of the sites and process
                if (inputSites != null)
                {
                    foreach (var topElement in inputSites.RootElement.EnumerateObject())
                    {
                        foreach (var dataElement in topElement.Value.EnumerateObject())
                        {
                            foreach (var siteElement in dataElement.Value.EnumerateArray())
                            {
                                var isSuccessful = await SyncSiteAsync(twins, relationships, siteElement, cancellationToken);
                                siteProcessedCounter.Add(1, metricsAttributesHelper.GetValues(
                                                         new KeyValuePair<string, object?>(Metrics.SiteDimensionName, siteElement.GetProperty("name").ToString()),
                                                         new KeyValuePair<string, object?>(Metrics.StatusDimensionName, isSuccessful)));
                            }
                        }
                    }
                }
                else
                {
                    Errors.TryAdd("Processing Error", "No Mapped sites found for this organization. Please check the MTI configuration. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#No-Mapped-sites-found-for-this-organization");
                    Logger.LogInformation("No sites found. Please check the configuration.");
                    siteNotFoundCounter.Add(1, metricsAttributesHelper.GetValues());
                }
            }
        }

        private async Task<bool> SyncSiteAsync(IDictionary<string, BasicDigitalTwin> twins,
                                               IDictionary<string, BasicRelationship> relationships,
                                               JsonElement siteElement,
                                               CancellationToken cancellationToken)
        {
            if (EntityTypes.IncludeSites)
            {
                Logger.LogInformation("Creating Site...");

                WillowConnectorId = await GetWillowConnector(cancellationToken);

                await GetPlacesAsync(twins, relationships, siteElement, null, null, cancellationToken);

                if (!siteElement.TryGetProperty("id", out var idProp))
                {
                    Errors.TryAdd("Processing Error", "No SiteId found for this site. Please check the Mapped Site twins. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#No-SiteId-found-for-this-site");
                    Logger.LogWarning("No SiteId found...");
                    return false;
                }

                var siteDtId = idProp.ToString();

                if (EntityTypes.IncludeBuildings)
                {
                    var filteredQuery = InputGraphManager.GetBuildingsForSiteQuery(siteDtId);

                    // Get the buildings for the site
                    var inputBuildings = await InputGraphManager.GetTwinGraphAsync(filteredQuery, cancellationToken).ConfigureAwait(false);

                    // Loop through all of the buildings and process
                    if (inputBuildings != null)
                    {
                        foreach (var topElement in inputBuildings.RootElement.EnumerateObject())
                        {
                            foreach (var dataElement in topElement.Value.EnumerateObject())
                            {
                                foreach (var sitesElement in dataElement.Value.EnumerateArray())
                                {
                                    foreach (var buildingsElement in sitesElement.EnumerateObject().Where(e => e.Name == "buildings"))
                                    {
                                        ConnectorMappings.Clear();

                                        foreach (var buildingElement in buildingsElement.Value.EnumerateArray().Where(e => e.ValueKind != JsonValueKind.Null && buildingsElement.Value.ValueKind != JsonValueKind.Undefined))
                                        {
                                            GetTwinId(buildingElement, out var buildingTwinMapping);

                                            SiteId = await OutputGraphManager.GetSiteIdForBuilding(buildingTwinMapping.TwinId, cancellationToken);

                                            await GetPlacesAsync(twins, relationships, buildingElement, sitesElement, RelationshipType.HasPart, cancellationToken);

                                            if (EntityTypes.IncludeConnectors)
                                            {
                                                await GetBuildingConnectorsAsync(twins, relationships, buildingElement, cancellationToken);
                                            }

                                            if (EntityTypes.IncludeThings)
                                            {
                                                await GetBuildingThingsAsync(twins, relationships, buildingElement, cancellationToken);

                                                twinsCounter.Add(twins.Count, metricsAttributesHelper.GetValues(
                                                                 new KeyValuePair<string, object?>(Metrics.BuildingDimensionName, buildingElement.GetProperty("name").ToString())));
                                            }

                                            relationshipsCounter.Add(relationships.Count, metricsAttributesHelper.GetValues(
                                                new KeyValuePair<string, object?>(Metrics.BuildingDimensionName, buildingElement.GetProperty("name").ToString())));

                                            buildingsCounter.Add(relationships.Count, metricsAttributesHelper.GetValues(
                                                                 new KeyValuePair<string, object?>(Metrics.BuildingDimensionName, buildingElement.GetProperty("name").ToString())));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Logger.LogInformation("No buildings found for site: '{siteDtId}'.", siteDtId);
                    }
                }

                Logger.LogInformation("Completed updating site.");
            }

            return true;
        }

        private async Task<string> GetWillowConnector(CancellationToken cancellationToken)
        {
            var connectorQuery = InputGraphManager.GetConnectorsQuery();
            var inputConnectors = await InputGraphManager.GetTwinGraphAsync(connectorQuery, cancellationToken).ConfigureAwait(false);

            var willowConnectorId = string.Empty;

            // Loop through all of the buildings and process.
            if (inputConnectors != null)
            {
                foreach (var topElement in inputConnectors.RootElement.EnumerateObject())
                {
                    foreach (var dataElement in topElement.Value.EnumerateObject())
                    {
                        foreach (var connector in dataElement.Value.EnumerateArray())
                        {
                            if (connector.TryGetProperty("connectorTypeId", out var connectorTypeId))
                            {
                                if (connectorTypeId.ToString() == WillowConnectorTypeId)
                                {
                                    if (connector.TryGetProperty("id", out var willowConnectorIdElement))
                                    {
                                        return willowConnectorIdElement.ToString();
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return willowConnectorId;
        }

        private async Task SyncOrganizationConnectors(IDictionary<string, BasicDigitalTwin> twins,
                                                      IDictionary<string, BasicRelationship> relationships,
                                                      CancellationToken cancellationToken)
        {
            var query = InputGraphManager.GetConnectorsQuery();

            // Get the Connectors for the organization.
            var inputConnectors = await InputGraphManager.GetTwinGraphAsync(query, cancellationToken).ConfigureAwait(false);

            // Loop through all of the buildings and process.
            if (inputConnectors != null)
            {
                foreach (var topElement in inputConnectors.RootElement.EnumerateObject())
                {
                    foreach (var dataElement in topElement.Value.EnumerateObject())
                    {
                        var connectors = new Dictionary<string, TwinMapping>();

                        foreach (var connectorElement in dataElement.Value.EnumerateArray())
                        {
                            GetConnector(twins, relationships, connectorElement);
                        }
                    }
                }
            }
        }

        private async Task SyncBuildingConnectors(IDictionary<string, BasicDigitalTwin> twins,
                                                  IDictionary<string, BasicRelationship> relationships,
                                                  string buildingId,
                                                  CancellationToken cancellationToken)
        {
            var building = await OutputGraphManager.GetTwinForMappedId(buildingId, cancellationToken);

            if (building == null)
            {
                Errors.TryAdd(buildingId, "Willow Building not found for Mapped Building Id. Please check the Mapped Building twins. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Willow-Building-not-found-for-Mapped-Building-Id");
                Logger.LogWarning("Sync Building Connectors: Willow Building not found for Mapped Building Id: '{buildingId}'.", buildingId);
                return;
            }

            // Need to add the building to the list of twins for relationship processing
            if (twins.TryAdd(building.Id, building))
            {
                Logger.LogInformation("Added building to twins list: '{buildingId}'.", buildingId);
            }
            else
            {
                Logger.LogWarning("Building already exists in twins list: '{buildingId}'.", buildingId);
            }

            var query = InputGraphManager.GetBuildingConnectorsQuery(buildingId);

            // Get the Connectors for the building.
            var inputBuildings = await InputGraphManager.GetTwinGraphAsync(query, cancellationToken).ConfigureAwait(false);

            // Loop through all of the buildings and process.
            if (inputBuildings != null)
            {
                foreach (var topElement in inputBuildings.RootElement.EnumerateObject())
                {
                    foreach (var dataElement in topElement.Value.EnumerateObject())
                    {
                        foreach (var buildingsElement in dataElement.Value.EnumerateArray())
                        {
                            var connectors = new Dictionary<string, TwinMapping>();

                            foreach (var connectorsElement in buildingsElement.EnumerateObject())
                            {
                                foreach (var connectorElement in connectorsElement.Value.EnumerateArray())
                                {
                                    GetConnector(twins, relationships, connectorElement, building);
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task SyncBuildingThings(IDictionary<string, BasicDigitalTwin> twins,
                                              IDictionary<string, BasicRelationship> relationships,
                                              string buildingId,
                                              string connectorId,
                                              CancellationToken cancellationToken)
        {
            var building = await OutputGraphManager.GetTwinForMappedId(buildingId, cancellationToken);

            if (building == null)
            {
                Errors.TryAdd(buildingId, "Willow Building not found for Mapped Building Id. Please check the Mapped Building twins. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Willow-Building-not-found-for-Mapped-Building-Id");
                Logger.LogWarning("Sync Building Things: Willow Building not found for Mapped Building Id: '{buildingId}'.", buildingId);
                return;
            }

            // Need to add the building to the list of twins for relationship processing
            if (twins.TryAdd(building.Id, building))
            {
                Logger.LogInformation("Added building to twins list: '{buildingId}'.", buildingId);
            }
            else
            {
                Logger.LogWarning("Building already exists in twins list: '{buildingId}'.", buildingId);
            }

            WillowConnectorId = await GetWillowConnector(cancellationToken);

            var query = InputGraphManager.GetBuildingThingsQuery(buildingId, connectorId);

            // Get the Things for the building.
            var inputBuildings = await InputGraphManager.GetTwinGraphAsync(query, cancellationToken).ConfigureAwait(false);

            // Loop through all of the buildings and process.
            if (inputBuildings != null)
            {
                foreach (var topElement in inputBuildings.RootElement.EnumerateObject())
                {
                    foreach (var dataElement in topElement.Value.EnumerateObject())
                    {
                        foreach (var buildingsElement in dataElement.Value.EnumerateArray())
                        {
                            var things = new Dictionary<string, TwinMapping>();

                            foreach (var thingsElement in buildingsElement.EnumerateObject().Where(e => e.Name == "things"))
                            {
                                foreach (var thingElement in thingsElement.Value.EnumerateArray())
                                {
                                    var thing = await GetThingAsync(twins, relationships, thingElement, cancellationToken).ConfigureAwait(false);

                                    if (thing != null)
                                    {
                                        things.TryAdd(thing.Item1, thing.Item2);
                                    }
                                }
                            }

                            if (things.Count > 0)
                            {
                                await GetPointsAsync(twins, relationships, things, cancellationToken).ConfigureAwait(false);
                                things.Clear();
                            }
                        }
                    }
                }
            }
        }

        private async Task GetBuildingConnectorsAsync(IDictionary<string, BasicDigitalTwin> twins,
                                                      IDictionary<string, BasicRelationship> relationships,
                                                      JsonElement targetElement,
                                                      CancellationToken cancellationToken)
        {
            if (!targetElement.TryGetProperty("id", out var basicDtIdProp))
            {
                Logger.LogWarning("Building id is missing... skipping.");
                return;
            }

            var basicDtId = basicDtIdProp.ToString();

            await SyncBuildingConnectors(twins, relationships, basicDtId, cancellationToken);
        }

        private async Task GetBuildingThingsAsync(IDictionary<string, BasicDigitalTwin> twins,
                                          IDictionary<string, BasicRelationship> relationships,
                                          JsonElement targetElement,
                                          CancellationToken cancellationToken)
        {
            if (!targetElement.TryGetProperty("id", out var basicDtIdProp))
            {
                Logger.LogWarning("Building id is missing... skipping.");
                return;
            }

            var basicDtId = basicDtIdProp.ToString();

            await SyncBuildingThings(twins, relationships, basicDtId, string.Empty, cancellationToken);
        }

        /// <summary>
        /// Connectors in Mapped are not twins. There is no model id, and there are no identities, so we must build a fake twin, and create a Willow Connector Id by creating a hash of the Mapped Id.
        /// </summary>
        /// <param name="twins">The existing list of twins.</param>
        /// <param name="relationships">The existing list of relationships.</param>
        /// <param name="connectorElement">The connector elemetn from the Mapped graph query.</param>
        /// <param name="building">The Willow Building Twin.</param>
        private void GetConnector(IDictionary<string, BasicDigitalTwin> twins, IDictionary<string, BasicRelationship> relationships, JsonElement connectorElement, BasicDigitalTwin? building = null)
        {
            // Get the Id of the individual item in the graph
            if (!GetTwinId(connectorElement, out var connectorMapping))
            {
                return;
            }

            // Create a basic twin
            var basicTwin = new BasicDigitalTwin
            {
                Id = connectorMapping.TwinId,

                // model Id of digital twin
                Metadata = { ModelId = Constants.WillowConnectorApplicationModelId },
            };

            // Populate the content of the twin
            var contentDictionary = new Dictionary<string, object>();

            if (connectorElement.TryGetProperty("name", out var connectorName))
            {
                contentDictionary.TryAdd("name", string.IsNullOrWhiteSpace(connectorName.ToString()) ? "None" : connectorName.ToString());
            }

            contentDictionary.TryAdd("siteID", SiteId);

            if (connectorElement.TryGetProperty("connectorType", out var connectorTypeElement))
            {
                connectorTypeElement.TryGetProperty("direction", out var connectorTypeDirection);
                connectorTypeElement.TryGetProperty("name", out var connectorTypeName);
                connectorTypeElement.TryGetProperty("version", out var connectorTypeVersion);
                connectorTypeElement.TryGetProperty("id", out var connectorTypeId);

                var connectorType = new
                {
                    id = connectorTypeId,
                    name = connectorTypeName,
                    version = connectorTypeVersion,
                    direction = connectorTypeDirection,
                };

                contentDictionary.TryAdd("connectorType", connectorType);
            }

            basicTwin.Contents = contentDictionary;

            twins.TryAdd(basicTwin.Id, basicTwin);

            if (building != null)
            {
                // Create a relationship to the building
                var relationshipProperties = new Dictionary<string, object>();
                var relationshipType = RelationshipType.ServedBy;
                var buildingDtmi = new Dtmi(building.Metadata.ModelId);
                AddRelationship(relationships, building.Id, buildingDtmi, relationshipType, basicTwin.Id, Constants.WillowConnectorApplicationModelId, relationshipProperties);
            }
        }

        private async Task<Tuple<string, TwinMapping>?> GetThingAsync(IDictionary<string, BasicDigitalTwin> twins, IDictionary<string, BasicRelationship> relationships, JsonElement thingElement, CancellationToken cancellationToken)
        {
            // Get the Id of the individual item in the graph
            if (!GetTwinId(thingElement, out var thingMapping))
            {
                return null;
            }

            thingElement.TryGetProperty("mappingKey", out var mappingKeyProperty);

            string thingMappingKey = mappingKeyProperty.ValueKind != JsonValueKind.Undefined && mappingKeyProperty.ValueKind != JsonValueKind.Null ? mappingKeyProperty.ToString() : string.Empty;

            // Look up the Model Id from the Incoming element
            if (thingElement.TryGetProperty("exactType", out var thingExactType))
            {
                thingMapping.Dtmi = AddTwin(twins, thingElement, thingMapping.TwinId, thingExactType.ToString());

                // Add the relationship to the location
                var locationElement = thingElement.EnumerateObject().FirstOrDefault(t => t.Name == RelationshipType.HasLocation);

                if (locationElement.Value.ValueKind != JsonValueKind.Null && locationElement.Value.ValueKind != JsonValueKind.Undefined)
                {
                    if (GetTwinId(locationElement.Value, out var locationMapping))
                    {
                        var relationshipProperties = new Dictionary<string, object>();

                        if (locationMapping != null)
                        {
                            if (locationElement.Value.TryGetProperty("exactType", out var locationExactType))
                            {
                                Dtmi? locationDtmi = GetInputInterfaceDtmi(locationExactType.ToString());
                                AddRelationship(relationships, locationMapping.TwinId, locationDtmi, RelationshipType.IsLocationOf, thingMapping.TwinId, thingExactType.ToString(), relationshipProperties);
                            }
                        }
                    }
                }

                if (thingMapping.Dtmi != null)
                {
                    // Add the IsFedBy relationships
                    await AddThingRelationships(twins, relationships, thingElement, thingMapping.TwinId, thingMapping.Dtmi, RelationshipType.IsFedBy, cancellationToken);

                    // Add the Serves relationships
                    await AddThingRelationships(twins, relationships, thingElement, thingMapping.TwinId, thingMapping.Dtmi, RelationshipType.Serves, cancellationToken);

                    return new Tuple<string, TwinMapping>(thingMapping.TwinId, thingMapping);
                }
            }

            return null;
        }

        private async Task AddThingRelationships(IDictionary<string, BasicDigitalTwin> twins, IDictionary<string, BasicRelationship> relationships, JsonElement thingElement, string thingDtId, Dtmi thingDtmi, string relationshipType, CancellationToken cancellationToken)
        {
            var relations = thingElement.EnumerateObject().FirstOrDefault(t => t.Name == relationshipType);

            if (relations.Value.ValueKind != JsonValueKind.Null && relations.Value.ValueKind != JsonValueKind.Undefined)
            {
                foreach (var relationsElement in relations.Value.EnumerateArray())
                {
                    // If a thing has a "serves" relationship, it's to a space (or zone). We need to add the space or zone as a twin to the graph
                    if (relationshipType == RelationshipType.Serves)
                    {
                        await GetPlacesAsync(twins, relationships, relationsElement, thingElement, RelationshipType.Serves, cancellationToken);
                    }

                    var relationshipProperties = new Dictionary<string, object>();

                    if (GetTwinId(relationsElement, out var relationsMapping))
                    {
                        if (relationsMapping != null)
                        {
                            var relationsProperties = relationsElement.EnumerateObject().FirstOrDefault(t => t.Name == "properties");

                            if (relationsProperties.Value.ValueKind != JsonValueKind.Null && relationsProperties.Value.ValueKind != JsonValueKind.Undefined)
                            {
                                foreach (var relationProperty in relationsProperties.Value.EnumerateObject())
                                {
                                    relationshipProperties.TryAdd(relationProperty.Name, relationProperty.Value.ToString());
                                }
                            }

                            if (relationsElement.TryGetProperty("exactType", out var relationExactType))
                            {
                                Dtmi? relationsDtmi = GetInputInterfaceDtmi(relationExactType.ToString());
                                AddRelationship(relationships, thingDtId, thingDtmi, relationshipType, relationsMapping.TwinId, relationExactType.ToString(), relationshipProperties);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the Willow Id value for the twin. If the twin has an identity with a source of Willow, then use that value. Otherwise used the Mapped Id value.
        /// </summary>
        /// <param name="twinElement">The Json that contains the twin.</param>
        /// <param name="twinMapping">The various twin mappings.</param>
        /// <returns>True if id is found, false if not.</returns>
        private bool GetTwinId(JsonElement twinElement, out TwinMapping twinMapping)
        {
            twinMapping = new TwinMapping();

            if (!twinElement.TryGetProperty("id", out JsonElement idProp))
            {
                return false;
            }

            // The Id property on the twin coming from Mapped will always be the Mapped Id
            twinMapping.MappedId = idProp.ToString();

            var identityProperty = twinElement.EnumerateObject().FirstOrDefault(t => t.Name == "identities");

            if (identityProperty.Value.ValueKind == JsonValueKind.Array)
            {
                var scopes = new Dictionary<string, DateTime>();

                // Get the Org Id
                foreach (var identity in identityProperty.Value.EnumerateArray())
                {
                    if (identity.TryGetProperty("scope", out var scope))
                    {
                        if (scope.ToString() == "ORG")
                        {
                            // It's possible to have more than one org Id, so we need to keep track of them all, and then use the most recent one
                            if (identity.TryGetProperty("value", out var orgValue))
                            {
                                if (orgValue.ValueKind == JsonValueKind.String)
                                {
                                    if (orgValue.ToString().StartsWith("urn:willowinc:twin:id:"))
                                    {
                                        // If there is a valid value for the Willow Id, then set that
                                        var id = orgValue.ToString().Replace("urn:willowinc:twin:id:", string.Empty);

                                        if (identity.TryGetProperty("dateCreated", out var createdDateValue))
                                        {
                                            if (createdDateValue.ValueKind == JsonValueKind.String)
                                            {
                                                if (DateTime.TryParse(createdDateValue.ToString(), out var resultCreatedDateValue))
                                                {
                                                    scopes.TryAdd(id, resultCreatedDateValue);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Get the most recent org Id
                if (scopes.Count > 0)
                {
                    var maxValue = scopes.OrderByDescending(x => x.Value).FirstOrDefault();
                    twinMapping.WillowId = maxValue.Key.ToString();
                    return true;
                }

                // If we weren't able to find an Org Id, then try the Willow id
                foreach (var identity in identityProperty.Value.EnumerateArray())
                {
                    if (identity.TryGetProperty("scopeId", out var sourceIdValue))
                    {
                        if (sourceIdValue.ToString() == WillowConnectorId)
                        {
                            if (identity.TryGetProperty("value", out var identityValue))
                            {
                                if (identityValue.ValueKind == JsonValueKind.String)
                                {
                                    // Todo - Waiting on mapped to remove site id which is always a guid
                                    if (!Guid.TryParse(identityValue.ToString(), out var resultIdValue))
                                    {
                                        // If there is a valid value for the Willow Id, then set that
                                        var id = identityValue.ToString();

                                        // It's possible to have more than one Willow Id, so we need to keep track of them all, and then use the most recent one
                                        if (identity.TryGetProperty("dateCreated", out var createdDateValue))
                                        {
                                            if (createdDateValue.ValueKind == JsonValueKind.String)
                                            {
                                                if (DateTime.TryParse(createdDateValue.ToString(), out var resultCreatedDateValue))
                                                {
                                                    scopes.TryAdd(id, resultCreatedDateValue);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Get the most recent scope id
                if (scopes.Count > 0)
                {
                    var maxValue = scopes.OrderByDescending(x => x.Value).FirstOrDefault();
                    twinMapping.WillowId = maxValue.Key.ToString();
                    return true;
                }
            }

            return true;
        }

        private async Task GetPointsAsync(IDictionary<string, BasicDigitalTwin> twins, IDictionary<string, BasicRelationship> relationships, IDictionary<string, TwinMapping> thingsToGetPointsFor, CancellationToken cancellationToken)
        {
            if (EntityTypes.IncludePoints)
            {
                // Loop through all of the things and process (in batches of 1000 to avoid timeouts)
                for (var i = 0; i < thingsToGetPointsFor.Count; i += Options.Value.ThingQueryBatchSize)
                {
                    var subThings = thingsToGetPointsFor.OrderBy(p => p.Key)
                                          .Skip(i)
                                          .Take(Options.Value.ThingQueryBatchSize)
                                          .ToDictionary(p => p.Key, p => p.Value.MappedId);

                    var pointsQuery = InputGraphManager.GetPointsForThingsQuery(subThings.Values.ToList());

                    // Get the points for the things
                    var inputThings = await InputGraphManager.GetTwinGraphAsync(pointsQuery, cancellationToken).ConfigureAwait(false);

                    // Loop through all of the buildings and process
                    if (inputThings != null)
                    {
                        foreach (var topThingElement in inputThings.RootElement.EnumerateObject())
                        {
                            foreach (var dataThingElement in topThingElement.Value.EnumerateObject())
                            {
                                if (dataThingElement.Value.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var thingElement in dataThingElement.Value.EnumerateArray())
                                    {
                                        GetTwinId(thingElement, out var thingMapping);

                                        if (!twins.TryGetValue(thingMapping.TwinId, out var thingTwin))
                                        {
                                            // In a real-world situation, this should never happen. In Unit tests, however, it might since the unit tests are
                                            // currently build with only a fragment of the full graph
                                            Errors.TryAdd(thingMapping.TwinId, "Thing twin not found in twins collection. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Thing-twin-not-found-in-twins-collection");
                                            thingDtmiNotFoundCounter.Add(1, metricsAttributesHelper.GetValues(
                                                                         new KeyValuePair<string, object?>(Metrics.TwinDimensionName, thingMapping.TwinId)));
                                            continue;
                                        }

                                        // Get the Dtmi for the source thing
                                        thingMapping.Dtmi = new Dtmi(thingTwin.Metadata.ModelId);

                                        foreach (var pointsElement in thingElement.EnumerateObject())
                                        {
                                            if (pointsElement.Value.ValueKind == JsonValueKind.Array)
                                            {
                                                foreach (var pointElement in pointsElement.Value.EnumerateArray())
                                                {
                                                    // Get the Id of the individual item in the graph
                                                    if (!GetTwinId(pointElement, out var pointMapping))
                                                    {
                                                        continue;
                                                    }

                                                    // Check if the point is marked as unused
                                                    if (pointElement.TryGetProperty("unused", out var unusedProp) && unusedProp.ValueKind != JsonValueKind.Null)
                                                    {
                                                        if (unusedProp.ValueKind == JsonValueKind.True)
                                                        {
                                                            // Skip this point as it is unused
                                                            continue;
                                                        }
                                                    }

                                                    // Look up the Model Id from the Incoming element
                                                    if (pointElement.TryGetProperty("exactType", out var pointExactType))
                                                    {
                                                        var pointDtmi = AddTwin(twins, pointElement, pointMapping.TwinId, pointExactType.ToString(), true);

                                                        var relationshipProperties = new Dictionary<string, object>();
                                                        AddRelationship(relationships, thingMapping.TwinId, thingMapping.Dtmi, RelationshipType.HasPoint, pointMapping.TwinId, pointExactType.ToString(), relationshipProperties);

                                                        // Determine if the node has descendants, and if so, iterate through them to add child twins
                                                        var elements = pointElement.EnumerateObject();

                                                        foreach (var innerElement in elements)
                                                        {
                                                            switch (innerElement.Value.ValueKind)
                                                            {
                                                                case JsonValueKind.Object:

                                                                    if (innerElement.Name == "isBilledTo")
                                                                    {
                                                                        // Get the Id of the individual item in the graph
                                                                        if (!GetTwinId(innerElement.Value, out var billedToTwinMapping))
                                                                        {
                                                                            return;
                                                                        }

                                                                        // Look up the Model Id from the Incoming element
                                                                        if (innerElement.Value.TryGetProperty("exactType", out var billedToExactType))
                                                                        {
                                                                            AddTwin(twins, innerElement.Value, billedToTwinMapping.TwinId, billedToExactType.ToString(), true);
                                                                            var billedTorelationshipProperties = new Dictionary<string, object>();

                                                                            AddRelationship(relationships, pointMapping.TwinId, pointDtmi, RelationshipType.IsBilledTo, billedToTwinMapping.TwinId, billedToExactType.ToString(), relationshipProperties);
                                                                        }
                                                                    }

                                                                    break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task SyncBuildingSpaces(IDictionary<string, BasicDigitalTwin> twins,
                                              IDictionary<string, BasicRelationship> relationships,
                                              string buildingId,
                                              CancellationToken cancellationToken)
        {
            var building = await OutputGraphManager.GetTwinForMappedId(buildingId, cancellationToken);

            if (building == null)
            {
                Errors.TryAdd(buildingId, "Willow Building not found for Mapped Building Id. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Willow-Building-not-found-for-Mapped-Building-Id");
                Logger.LogWarning("Sync Building Spaces: Willow Building not found for Mapped Building Id: '{buildingId}'.", buildingId);
                return;
            }

            // Need to add the building to the list of twins for relationship processing
            if (twins.TryAdd(building.Id, building))
            {
                Logger.LogInformation("Added building to twins list: '{buildingId}'.", buildingId);
            }
            else
            {
                Logger.LogWarning("Building already exists in twins list: '{buildingId}'.", buildingId);
            }

            WillowConnectorId = await GetWillowConnector(cancellationToken);

            var query = InputGraphManager.GetBuildingQuery(buildingId);

            // Get the floors for the building.
            var inputBuildings = await InputGraphManager.GetTwinGraphAsync(query, cancellationToken).ConfigureAwait(false);

            // Loop through all of the buildings and process
            if (inputBuildings != null)
            {
                foreach (var topElement in inputBuildings.RootElement.EnumerateObject())
                {
                    foreach (var dataElement in topElement.Value.EnumerateObject().Where(e => e.Name == "buildings"))
                    {
                        foreach (var buildingElement in dataElement.Value.EnumerateArray())
                        {
                            ConnectorMappings.Clear();
                            await GetPlacesAsync(twins, relationships, buildingElement, null, null, cancellationToken);
                        }
                    }
                }
            }
        }

        private async Task GetPlacesAsync(IDictionary<string, BasicDigitalTwin> twins,
                                          IDictionary<string, BasicRelationship> relationships,
                                          JsonElement targetElement,
                                          JsonElement? sourceElement,
                                          string? relationshipType,
                                          CancellationToken cancellationToken)
        {
            // Get the Id of the individual item in the graph
            // If the element has no id, then it is not a twin, so return
            if (!GetTwinId(targetElement, out var targetTwinMapping))
            {
                return;
            }

            // Look up the Model Id from the Incoming element
            if (targetElement.TryGetProperty("exactType", out var targetExactType))
            {
                var targetDtmi = AddTwin(twins, targetElement, targetTwinMapping.TwinId, targetExactType.ToString());

                // Determine if the node has descendants, and if so, iterate through them to add child twins
                var elements = targetElement.EnumerateObject();

                foreach (var innerElement in elements)
                {
                    switch (innerElement.Value.ValueKind)
                    {
                        case JsonValueKind.Array:

                            if (innerElement.Name == "points" || innerElement.Name == "hasPoints" || innerElement.Name == "hasPoint")
                            {
                                foreach (var item in innerElement.Value.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.Object))
                                {
                                    // Get the Id of the individual item in the graph
                                    if (!GetTwinId(item, out var pointTwinMapping))
                                    {
                                        return;
                                    }

                                    // Look up the Model Id from the Incoming element
                                    if (item.TryGetProperty("exactType", out var pointExactType))
                                    {
                                        AddTwin(twins, item, pointTwinMapping.TwinId, pointExactType.ToString(), true);
                                        var relationshipProperties = new Dictionary<string, object>();

                                        AddRelationship(relationships, targetTwinMapping.TwinId, targetDtmi, RelationshipType.HasPoint, pointTwinMapping.TwinId, pointExactType.ToString(), relationshipProperties);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var item in innerElement.Value.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.Object))
                                {
                                    await GetPlacesAsync(twins, relationships, item, targetElement, innerElement.Name, cancellationToken);
                                }
                            }

                            break;

                        case JsonValueKind.Object:
                            await GetPlacesAsync(twins, relationships, innerElement.Value, targetElement, innerElement.Name, cancellationToken);
                            break;
                    }
                }

                if (sourceElement != null && !string.IsNullOrWhiteSpace(relationshipType) && targetElement.ValueKind != JsonValueKind.Array)
                {
                    if (GetTwinId(sourceElement.Value, out var sourceMapping))
                    {
                        var sourceExactType = sourceElement.Value.GetProperty("exactType").ToString();
                        var sourceDtmi = GetInputInterfaceDtmi(sourceExactType);

                        var relationshipProperties = new Dictionary<string, object>();

                        if (relationshipType == "zones")
                        {
                            relationshipType = RelationshipType.HasPart;
                        }

                        if (relationshipType == "isPartOf" && targetDtmi == GridRegion)
                        {
                            relationshipType = RelationshipType.LocatedInGridRegion;
                        }

                        if (relationshipType == "isAdjacentTo")
                        {
                            relationshipType = RelationshipType.IsAdjacentTo;
                        }

                        AddRelationship(relationships, sourceMapping.TwinId, sourceDtmi, relationshipType, targetTwinMapping.TwinId, targetExactType.ToString(), relationshipProperties);
                    }
                }

                if (EntityTypes.IncludeLevels && string.Equals(targetExactType.ToString(), "floor", StringComparison.OrdinalIgnoreCase))
                {
                    await SyncLevels(twins, relationships, targetElement, targetTwinMapping, targetExactType, cancellationToken);
                }
            }
            else
            {
                // Identity Twins have a different structure, so we can ignore them here.
                if (!targetTwinMapping.TwinId.StartsWith(IdentityTwinPrefix))
                {
                    Errors.TryAdd(targetTwinMapping.TwinId, "ExactType not found for TwinId. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#ExactType-not-found-for-TwinId");
                    Logger.LogWarning("ExactType not found for TwinId: '{targetId}' : Target Element '{JSON}'", targetTwinMapping.TwinId, JsonObject.Create(targetElement)?.ToJsonString());
                    exactTypeNotFoundCounter.Add(1, metricsAttributesHelper.GetValues(new KeyValuePair<string, object?>(Metrics.IdDimensionName, targetTwinMapping)));
                }
            }
        }

        private async Task SyncLevels(IDictionary<string, BasicDigitalTwin> twins, IDictionary<string, BasicRelationship> relationships, JsonElement targetElement, TwinMapping targetTwinMapping, JsonElement targetExactType, CancellationToken cancellationToken)
        {
            // Get sub spaces of floor
            var filteredQuery = InputGraphManager.GetFloorQuery(targetTwinMapping.MappedId);
            var inputFloor = await InputGraphManager.GetTwinGraphAsync(filteredQuery, cancellationToken);

            if (inputFloor != null)
            {
                // Loop through all the spaces returned from input query
                // TODO: determine if there is a more efficient way to walk the graph
                var rootElement = inputFloor.RootElement;

                foreach (var topElement in rootElement.EnumerateObject())
                {
                    foreach (var dataElement in topElement.Value.EnumerateObject())
                    {
                        foreach (var floorElement in dataElement.Value.EnumerateArray())
                        {
                            foreach (var innerElement in floorElement.EnumerateObject())
                            {
                                switch (innerElement.Value.ValueKind)
                                {
                                    case JsonValueKind.Array:

                                        foreach (var item in innerElement.Value.EnumerateArray())
                                        {
                                            await GetPlacesAsync(twins, relationships, item, targetElement, innerElement.Name, cancellationToken);
                                        }

                                        break;

                                    case JsonValueKind.Object:
                                        await GetPlacesAsync(twins, relationships, innerElement.Value, targetElement, innerElement.Name, cancellationToken);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method is called for each twin in the graph to add properties to the twin based on inputs.
        /// </summary>
        /// <param name="inputDtmi">The DTMI of the input twin.</param>
        /// <returns>A dictionary of strings and objects to add to the contents of the twin.</returns>
        protected virtual IDictionary<string, object> GetTargetSpecificContents(Dtmi inputDtmi)
        {
            return new Dictionary<string, object>();
        }

        /// <inheritdoc/>
        public async Task SyncOrganizationAsync(bool autoApprove, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting organization sync process");

            await Init(cancellationToken);

            EntityTypes.IncludeAccounts = true;
            EntityTypes.IncludeOrganization = true;
            EntityTypes.IncludeSites = true;
            EntityTypes.IncludeBuildings = true;
            EntityTypes.IncludeConnectors = true;
            EntityTypes.IncludeSpaces = false;
            EntityTypes.IncludeLevels = false;
            EntityTypes.IncludePoints = false;
            EntityTypes.IncludeThings = false;

            // Create a list of all of the twins that need to be created
            var twins = new Dictionary<string, BasicDigitalTwin>();

            // Create a list of all of the relationships that need to be created
            var relationships = new Dictionary<string, BasicRelationship>();

            await SyncOrganization(twins, relationships, cancellationToken);

            await SyncOrganizationConnectors(twins, relationships, cancellationToken);

            await SyncAccounts(twins, relationships, cancellationToken);

            RemoveRedundantRelationships(twins, relationships);

            await OutputGraphManager.UploadGraphAsync(twins, relationships, string.Empty, string.Empty, autoApprove, cancellationToken);

            foreach (var error in OutputGraphManager.Errors)
            {
                Errors.TryAdd(error.Key, error.Value);
            }

            Logger.LogInformation("Completed organization sync process");
        }

        /// <inheritdoc/>
        public async Task SyncConnectorsAsync(string buildingId, bool autoApprove, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting connector sync process");

            await Init(CancellationToken.None);

            EntityTypes.IncludeOrganization = false;
            EntityTypes.IncludeSites = false;
            EntityTypes.IncludeBuildings = true;
            EntityTypes.IncludeConnectors = true;
            EntityTypes.IncludeSpaces = false;
            EntityTypes.IncludeLevels = false;
            EntityTypes.IncludePoints = false;
            EntityTypes.IncludeThings = false;

            // Create a list of all of the twins that need to be created
            var twins = new Dictionary<string, BasicDigitalTwin>();

            // Create a list of all of the relationships that need to be created
            var relationships = new Dictionary<string, BasicRelationship>();

            SiteId = await OutputGraphManager.GetSiteIdForMappedBuildingId(buildingId, cancellationToken);

            await SyncBuildingConnectors(twins, relationships, buildingId, cancellationToken);

            RemoveRedundantRelationships(twins, relationships);

            await OutputGraphManager.UploadGraphAsync(twins, relationships, buildingId, string.Empty, autoApprove, cancellationToken);

            foreach (var error in OutputGraphManager.Errors)
            {
                Errors.TryAdd(error.Key, error.Value);
            }

            Logger.LogInformation("Completed connector sync process");
        }

        /// <inheritdoc/>
        public async Task SyncSpatialAsync(string buildingId, bool autoApprove, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting spatial sync process");

            await Init(CancellationToken.None);

            EntityTypes.IncludeOrganization = false;
            EntityTypes.IncludeSites = false;
            EntityTypes.IncludeBuildings = true;
            EntityTypes.IncludeConnectors = false;
            EntityTypes.IncludeSpaces = true;
            EntityTypes.IncludeLevels = true;
            EntityTypes.IncludePoints = false;
            EntityTypes.IncludeThings = false;

            // Create a list of all of the twins that need to be created
            var twins = new Dictionary<string, BasicDigitalTwin>();

            // Create a list of all of the relationships that need to be created
            var relationships = new Dictionary<string, BasicRelationship>();

            SiteId = await OutputGraphManager.GetSiteIdForMappedBuildingId(buildingId, cancellationToken);

            await SyncBuildingSpaces(twins, relationships, buildingId, cancellationToken);

            RemoveRedundantRelationships(twins, relationships);

            await OutputGraphManager.UploadGraphAsync(twins, relationships, buildingId, string.Empty, autoApprove, cancellationToken);

            foreach (var error in OutputGraphManager.Errors)
            {
                Errors.TryAdd(error.Key, error.Value);
            }

            Logger.LogInformation("Completed spatial sync process");
        }

        /// <inheritdoc/>
        public async Task SyncThingsAsync(string buildingId, string connectorId, bool autoApprove, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting assets sync process");

            await Init(CancellationToken.None);

            EntityTypes.IncludeOrganization = false;
            EntityTypes.IncludeSites = false;
            EntityTypes.IncludeBuildings = true;
            EntityTypes.IncludeConnectors = false;
            EntityTypes.IncludeSpaces = false;
            EntityTypes.IncludeLevels = false;
            EntityTypes.IncludePoints = false;
            EntityTypes.IncludeThings = true;

            // Create a list of all of the twins that need to be created
            var twins = new Dictionary<string, BasicDigitalTwin>();

            // Create a list of all of the relationships that need to be created
            var relationships = new Dictionary<string, BasicRelationship>();

            SiteId = await OutputGraphManager.GetSiteIdForMappedBuildingId(buildingId, cancellationToken);

            await SyncBuildingThings(twins, relationships, buildingId, connectorId, cancellationToken);

            RemoveRedundantRelationships(twins, relationships);

            await OutputGraphManager.UploadGraphAsync(twins, relationships, buildingId, connectorId, autoApprove, cancellationToken);

            foreach (var error in OutputGraphManager.Errors)
            {
                Errors.TryAdd(error.Key, error.Value);
            }

            Logger.LogInformation("Completed assets sync process");
        }

        /// <inheritdoc/>
        public async Task SyncPointsAsync(string buildingId, string connectorId, bool autoApprove, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting capabilities sync process");

            await Init(CancellationToken.None);

            EntityTypes.IncludeOrganization = true;
            EntityTypes.IncludeSites = true;
            EntityTypes.IncludeBuildings = true;
            EntityTypes.IncludeConnectors = false;
            EntityTypes.IncludeSpaces = false;
            EntityTypes.IncludeLevels = false;
            EntityTypes.IncludePoints = true;
            EntityTypes.IncludeThings = true;

            // Create a list of all of the twins that need to be created
            var twins = new Dictionary<string, BasicDigitalTwin>();

            // Create a list of all of the relationships that need to be created
            var relationships = new Dictionary<string, BasicRelationship>();

            SiteId = await OutputGraphManager.GetSiteIdForMappedBuildingId(buildingId, cancellationToken);

            await SyncBuildingThings(twins, relationships, buildingId, connectorId, cancellationToken);

            RemoveRedundantRelationships(twins, relationships);

            await OutputGraphManager.UploadGraphAsync(twins, relationships, buildingId, connectorId, autoApprove, cancellationToken);

            foreach (var error in OutputGraphManager.Errors)
            {
                Errors.TryAdd(error.Key, error.Value);
            }

            Logger.LogInformation("Completed capabilties sync process");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TicketMetadataDto>> GetConnectorMetadataAsync(string connectorId, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting connector metadata get.");

            var connectorQuery = InputGraphManager.GetConnectorMetadataQuery(connectorId);

            var inputConnectors = await InputGraphManager.GetTwinGraphAsync(connectorQuery, cancellationToken).ConfigureAwait(false);

            var connectorDtos = new List<TicketMetadataDto>();

            if (inputConnectors != null)
            {
                foreach (var topElement in inputConnectors.RootElement.EnumerateObject())
                {
                    foreach (var dataElement in topElement.Value.EnumerateObject())
                    {
                        foreach (var connector in dataElement.Value.EnumerateArray())
                        {
                            var ticketMetadataDto = new TicketMetadataDto();

                            if (connector.TryGetProperty("config", out var configElement))
                            {
                                if (configElement.TryGetProperty("api", out var apiElement))
                                {
                                    if (apiElement.TryGetProperty("nuvoloRequestTypes", out var requestTypesElement))
                                    {
                                        foreach (var requestTypeElement in requestTypesElement.EnumerateArray())
                                        {
                                            Guid sysId = Guid.Empty;
                                            string requestType = string.Empty;

                                            if (requestTypeElement.TryGetProperty("sys_id", out var idElement))
                                            {
                                                sysId = new Guid(idElement.ToString());
                                            }

                                            if (requestTypeElement.TryGetProperty("requestType", out var requestTElement))
                                            {
                                                requestType = requestTElement.ToString();
                                            }

                                            var request = new RequestType(sysId, requestType);
                                            ticketMetadataDto.RequestTypes.Add(request);
                                        }
                                    }

                                    if (apiElement.TryGetProperty("nuvoloServicesNeeded", out var servicesNeededElement))
                                    {
                                        foreach (var serviceNeededElement in servicesNeededElement.EnumerateArray())
                                        {
                                            Guid sysId = Guid.Empty;
                                            Guid requestTypeId = Guid.Empty;
                                            string name = string.Empty;

                                            if (serviceNeededElement.TryGetProperty("u_service_needed_sys_id", out var idElement))
                                            {
                                                sysId = new Guid(idElement.ToString());
                                            }

                                            if (serviceNeededElement.TryGetProperty("u_request_type", out var requestTElement))
                                            {
                                                requestTypeId = new Guid(requestTElement.ToString());
                                            }

                                            if (serviceNeededElement.TryGetProperty("u_service_needed", out var nameElement))
                                            {
                                                name = nameElement.ToString();
                                            }

                                            var serviceNeeded = new ServiceNeeded(sysId, requestTypeId, name);
                                            ticketMetadataDto.ServiceNeededList.Add(serviceNeeded);
                                        }
                                    }

                                    if (apiElement.TryGetProperty("nuvoloSpacesServicesNeeded", out var spacesServicesNeededElement))
                                    {
                                        foreach (var spacesServiceNeededElement in spacesServicesNeededElement.EnumerateArray())
                                        {
                                            Guid buildingSysId = Guid.Empty;
                                            Guid spaceSysId = Guid.Empty;
                                            List<Guid> spaceServicesNeeded = [];

                                            if (spacesServiceNeededElement.TryGetProperty("space_sys_id", out var spaceSysIdElement))
                                            {
                                                spaceSysId = new Guid(spaceSysIdElement.ToString());
                                            }

                                            if (spacesServiceNeededElement.TryGetProperty("building_sys_id", out var buildingSysIdElement))
                                            {
                                                buildingSysId = new Guid(buildingSysIdElement.ToString());
                                            }

                                            if (spacesServiceNeededElement.TryGetProperty("u_service_needed_sys_ids", out var servicesNeededSysIdsElement))
                                            {
                                                foreach (var serviceNeededSysIdElement in servicesNeededSysIdsElement.EnumerateArray())
                                                {
                                                    spaceServicesNeeded.Add(new Guid(serviceNeededSysIdElement.ToString()));
                                                }
                                            }

                                            var spaceServiceNeeded = new SpaceServiceNeeded(buildingSysId, spaceSysId, spaceServicesNeeded);
                                            ticketMetadataDto.SpaceServiceNeededList.Add(spaceServiceNeeded);
                                        }
                                    }

                                    if (apiElement.TryGetProperty("nuvoloJobTypes", out var jobTypesElement))
                                    {
                                        foreach (var jobTypeElement in jobTypesElement.EnumerateArray())
                                        {
                                            Guid sysId = Guid.Empty;
                                            string jobTypeName = string.Empty;

                                            if (jobTypeElement.TryGetProperty("sys_id", out var idElement))
                                            {
                                                sysId = new Guid(idElement.ToString());
                                            }

                                            if (jobTypeElement.TryGetProperty("jobType", out var jobTElement))
                                            {
                                                jobTypeName = jobTElement.ToString();
                                            }

                                            var jobType = new JobType(sysId, jobTypeName);
                                            ticketMetadataDto.JobTypes.Add(jobType);
                                        }
                                    }
                                }
                            }

                            connectorDtos.Add(ticketMetadataDto);
                        }
                    }
                }
            }

            return connectorDtos;
        }

        /// <summary>
        /// Returns a DTMI from the input graph ontology, corresponding to a string representation, if one exists.
        /// </summary>
        /// <param name="interfaceType">Sought interface name.</param>
        /// <returns><c>DTMI</c> representation of said interface, if it exists; else null.</returns>
        protected Dtmi? GetInputInterfaceDtmi(string interfaceType)
        {
            Dtmi? dtmi = null;

            if (InputGraphManager.TryGetDtmi(interfaceType.ToString(), out var dtmiVal))
            {
                dtmi = new Dtmi(dtmiVal);
            }
            else
            {
                if (interfaceType == Constants.WillowConnectorApplicationModelId)
                {
                    dtmi = new Dtmi(Constants.WillowConnectorApplicationModelId);
                }
                else
                {
                    Errors.TryAdd(interfaceType, $"Mapping for interfaceType '{interfaceType}' not found in DTDL. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Mapping-for-interfaceType-not-found-in-DTDL");
                    Logger.LogWarning("Mapping for interfaceType '{interfaceType}' not found in DTDL", interfaceType);
                    inputInterfaceNotFoundCounter.Add(1, metricsAttributesHelper.GetValues(new KeyValuePair<string, object?>(Metrics.InterfaceTypeDimensionName, interfaceType)));
                }
            }

            return dtmi;
        }

        /// <summary>
        /// Get an output Relationship name and direction, after ontology mapping, corresponding to an input Relationship name.
        /// If no mapping is found, simply returns the input Relationship.
        /// </summary>
        /// <param name="inputRelationshipType">The sought input Relationship.</param>
        /// <returns>A <c>string,bool</c> tuple, where the <c>string</c> indicates output Relationship name, and the <c>bool</c> indicates whether or not the relationship direction is reversed after mapping compared to the input direction.</returns>
        protected Tuple<string, bool> GetOutputRelationshipType(string inputRelationshipType)
        {
            // If there is a remapping, use that. If not, assume the input and output mapping are the same
            if (OntologyMappingManager.TryGetRelationshipRemap(inputRelationshipType, out var outputRelationship) && outputRelationship != null)
            {
                return new Tuple<string, bool>(outputRelationship.OutputRelationship, outputRelationship.ReverseRelationshipDirection);
            }

            return new Tuple<string, bool>(inputRelationshipType, false);
        }

        /// <summary>
        /// Try to get the output DTMI, after ontology mapping, corresponding to an input DTMI.
        /// </summary>
        /// <param name="inputDtmi">The sought input DTMI.</param>
        /// <param name="outputDtmi">The corresponding output DTMI.</param>
        /// <returns><c>true</c> if a mapping could be found, in which case <paramref name="outputDtmi"/> will hold a result, else <c>false</c>, in which case <paramref name="outputDtmi"/> will be null.</returns>
        protected bool TryGetOutputInterfaceDtmi(Dtmi inputDtmi, out Dtmi? outputDtmi)
        {
            // Try to get the input DTMI from the output DTDL
            if (TargetObjectModel.TryGetValue(inputDtmi, out var dTEntityInfo))
            {
                outputDtmi = dTEntityInfo.Id;
                return true;
            }
            else
            {
                outputDtmi = null;
                DtmiRemap? dtmiRemap = null;
                try
                {
                    if (OntologyMappingManager.TryGetInterfaceRemapDtmi(inputDtmi, out dtmiRemap) && dtmiRemap != null)
                    {
                        outputDtmi = new Dtmi(dtmiRemap.OutputDtmi);
                        return true;
                    }
                }
                catch (ParsingException ex)
                {
                    if (dtmiRemap != null)
                    {
                        Errors.TryAdd(dtmiRemap.OutputDtmi, $"DTMI '{dtmiRemap.OutputDtmi}' cannot be parsed. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#DTMI-cannot-be-parsed");
                        Logger.LogWarning(ex, "Output DTMI cannot be parsed: {invalidTarget}.", dtmiRemap.OutputDtmi);
                        invalidOutputDtmiCounter.Add(1, metricsAttributesHelper.GetValues(new KeyValuePair<string, object?>(Metrics.OutputDtmiTypeDimensionName, dtmiRemap.OutputDtmi)));
                    }
                    else
                    {
                        Errors.TryAdd(inputDtmi.ToString(), $"Output DTMI is null for this DTMI '{inputDtmi}'. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Output-DTMI-is-null-for-this-DTMI");
                        Logger.LogWarning(ex, "Output DTMI is null for inputDtmi: {invalidTarget}.", inputDtmi);
                    }

                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a new digital twin to the input twins collection, named and typed per the
        /// input parameters, by parsing an input source element.
        /// <br/><br/>
        /// Note that this method will return a DTMI if ontology mapping can be carried out,
        /// irrespective of whether a new twin was successfully created or not.
        /// </summary>
        /// <param name="twins">Collection to which the new twin is added.</param>
        /// <param name="sourceElement">Element to be parsed in the input graph.</param>
        /// <param name="targetDtId">dtId for the new twin to add.</param>
        /// <param name="sourceTwinInterface">The interface of the source twin.</param>
        /// <param name="isPoint">Is this a point object.</param>
        /// <returns>Target DTMI corresponding with <paramref name="sourceTwinInterface"/> after
        /// ontology mapping (or <c>null</c> if no such mapping could be found.</returns>
        protected Dtmi? AddTwin(IDictionary<string, BasicDigitalTwin> twins,
                                JsonElement sourceElement,
                                string targetDtId,
                                string sourceTwinInterface,
                                bool isPoint = false)
        {
            Dtmi? inputDtmi = GetInputInterfaceDtmi(sourceTwinInterface);

            if (inputDtmi != null)
            {
                if (TryGetOutputInterfaceDtmi(inputDtmi, out var outputDtmi) && outputDtmi != null)
                {
                    // Create a basic twin
                    var basicTwin = new BasicDigitalTwin
                    {
                        Id = targetDtId,

                        // model Id of digital twin
                        Metadata = { ModelId = outputDtmi.ToString() },
                    };

                    // Populate the content of the twin
                    var contentDictionary = new Dictionary<string, object>();

                    // Check to see if there are any custom properties that need to be added to the twin based on the input DTMI.
                    var targetSpecificContents = GetTargetSpecificContents(inputDtmi);

                    if (targetSpecificContents != null)
                    {
                        foreach (var content in targetSpecificContents)
                        {
                            contentDictionary.Add(content.Key, JsonSerializer.SerializeToDocument(content.Value).RootElement);
                        }
                    }

                    // Get the model needed
                    if (TargetObjectModel.TryGetValue(outputDtmi, out var model))
                    {
                        // Get a list of the properties of the model
                        foreach (var targetContentEntity in ((DTInterfaceInfo)model).Contents.Values.Where(v => v.EntityKind == DTEntityKind.Property || v.EntityKind == DTEntityKind.Component))
                        {
                            switch (targetContentEntity.EntityKind)
                            {
                                case DTEntityKind.Property:
                                    {
                                        AddTwinProperty(sourceElement, targetDtId, sourceTwinInterface, contentDictionary, targetContentEntity, outputDtmi.ToString());
                                        break;
                                    }

                                case DTEntityKind.Component:
                                    {
                                        AddComponent(sourceElement, contentDictionary, targetContentEntity);
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        Errors.TryAdd(targetDtId, $"Target DTMI: '{targetDtId}' with InterfaceType: '{sourceTwinInterface}' not found in target model parser. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Target-DTMI%3A-'%7BtargetDtId%7D'-with-InterfaceType%3A-'%7BsourceTwinInterface%7D'-not-found-in-target-model-parser");
                        Logger.LogWarning("Target DTMI: '{OutputDtmi}' with InterfaceType: '{InterfaceType}' not found in target model parser.", targetDtId, sourceTwinInterface);
                        targetDtmiNotFoundCounter.Add(1, metricsAttributesHelper.GetValues(new KeyValuePair<string, object?>(Metrics.ModelIdDimensionName, sourceTwinInterface.ToString())));
                    }

                    // Twins are required to have a name
                    if (!contentDictionary.TryGetValue("name", out var name))
                    {
                        contentDictionary.TryAdd("name", "None");
                    }

                    // All Twins need to have the Site Id set
                    if (!contentDictionary.TryGetValue("siteID", out var siteId))
                    {
                        contentDictionary.TryAdd("siteID", SiteId);
                    }

                    // Stuff all the identities into the mappedIdentities property
                    if (sourceElement.TryGetProperty("identities", out var identitiesProperty) && identitiesProperty.ValueKind != JsonValueKind.Null)
                    {
                        var identities = new List<MappedId>();

                        foreach (var identity in identitiesProperty.EnumerateArray())
                        {
                            var mappedId = new MappedId();

                            if (identity.TryGetProperty("__typename", out var exactTypeProperty))
                            {
                                mappedId.exactType = exactTypeProperty.ToString();
                            }

                            if (identity.TryGetProperty("scope", out var scopeProperty))
                            {
                                mappedId.scope = scopeProperty.ToString();
                            }

                            if (identity.TryGetProperty("scopeId", out var scopeIdProperty))
                            {
                                mappedId.scopeId = scopeIdProperty.ToString();
                            }

                            if (identity.TryGetProperty("value", out var valueProperty))
                            {
                                mappedId.value = valueProperty.ToString();
                            }

                            if (identity.TryGetProperty("dateCreated", out var dateCreatedProperty))
                            {
                                if (DateTime.TryParse(dateCreatedProperty.ToString(), out var dateCreated))
                                {
                                    mappedId.dateCreated = dateCreated.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
                                }
                            }

                            // It's possible that the identity is not a mapped identity. In that case, we don't want to add it to the list
                            if (!string.IsNullOrEmpty(mappedId.exactType))
                            {
                                identities.Add(mappedId);
                            }
                        }

                        contentDictionary.Add("mappedIds", identities);

                        try
                        {
                            // Get the most recent BACnet identity if there is one
                            if (identities.Where(i => i.exactType == BacNetConnectorExactType).OrderByDescending(i => i.dateCreated).FirstOrDefault() is MappedId bacNetIdentity)
                            {
                                // Mapped BACnet identity schema: <DeviceID>/object=<ObjectTypeID>:<InstanceNumber>
                                var deviceID = int.Parse(bacNetIdentity.value.Split('/')[0]);
                                var objectTypeId = int.Parse(bacNetIdentity.value.Split('/')[1].Split('=')[1].Split(':')[0]);
                                var instanceNumber = int.Parse(bacNetIdentity.value.Split('/')[1].Split('=')[1].Split(':')[1]);

                                var objectType = (BACnetCapabilityObjectType)objectTypeId;

                                var bacnet = new BACnetCapability
                                {
                                    deviceID = deviceID,
                                    objectID = instanceNumber,
                                    objectType = objectType.ToString(),
                                };

                                var newCommunication = new CapabilityCommunication
                                {
                                    BACnet = bacnet,
                                };

                                if (contentDictionary.TryGetValue("communication", out var communication))
                                {
                                    contentDictionary["communication"] = newCommunication;
                                }
                                else
                                {
                                    contentDictionary.Add("communication", newCommunication);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Error parsing BACnet identity for twin: {TwinId}", targetDtId);
                        }
                    }

                    // Points need a value set in the type field
                    if (isPoint)
                    {
                        // Not all Points are Capabilities. Check to see if this one is
                        if (IsPointACapability(outputDtmi))
                        {
                            // Set the default type to analog
                            if (!contentDictionary.TryGetValue("type", out var twinType))
                            {
                                contentDictionary.Add("type", "analog");
                            }
                            else
                            {
                                contentDictionary["type"] = "analog";
                            }

                            // Set the default type to analog
                            if (!contentDictionary.TryGetValue("trendInterval", out var trendInterval))
                            {
                                contentDictionary.Add("trendInterval", 900);
                            }

                            // See if the point has a datatype field (it should... but just in case)
                            if (sourceElement.TryGetProperty("datatype", out var datatypeProperty))
                            {
                                switch (datatypeProperty.ToString().ToLower())
                                {
                                    case "double":
                                        contentDictionary["type"] = "analog";
                                        break;
                                    case "int":
                                        contentDictionary["type"] = "multiState";
                                        break;
                                }
                            }
                        }

                        // This is different from the Mapped Connector Id. Kept only for legacy purposes
                        contentDictionary.TryAdd("connectorID", DefaultMappedConnectorId);

                        // Get the MappingKey for the Point so we can get the mappedConnectorId
                        if (sourceElement.TryGetProperty("mappingKey", out var mappingKeyProperty))
                        {
                            string? mappingKey = mappingKeyProperty.ToString();

                            if (mappingKey != null)
                            {
                                string connectorStr = GetConnectorId(mappingKey);
                                contentDictionary.Add("mappedConnectorId", connectorStr);
                            }
                        }
                    }

                    basicTwin.Contents = contentDictionary;

                    twins.TryAdd(basicTwin.Id, basicTwin);
                }
                else
                {
                    Errors.TryAdd(targetDtId, $"Output mapping for input Dtmi: '{inputDtmi}' with InterfaceType: '{sourceTwinInterface}' to output Dtmi not found. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Output-mapping-for-input-Dtmi%3A-'%7BinputDtmi%7D'-with-InterfaceType%3A-'%7BsourceTwinInterface%7D'-to-output-Dtmi-not-found");
                    Logger.LogWarning("Output mapping for input Dtmi: '{InputDtmi}' with InterfaceType: '{InterfaceType}' to output Dtmi not found.", targetDtId, sourceTwinInterface);
                    outputMappingForInputDtmiNotFoundCounter.Add(1, metricsAttributesHelper.GetValues(new KeyValuePair<string, object?>(Metrics.ModelIdDimensionName, inputDtmi.ToString())));
                }

                return outputDtmi;
            }
            else
            {
                Errors.TryAdd(targetDtId, $"Mapping for input interface: '{targetDtId}' with InterfaceType: '{sourceTwinInterface}' not found. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Mapping-for-input-interface%3A-'%7BtargetDtId%7D'-with-InterfaceType%3A-'%7BsourceTwinInterface%7D'-not-found");
                Logger.LogWarning("Mapping for input interface: '{InputDtmi}' with InterfaceType: '{InterfaceType}' not found.", targetDtId, sourceTwinInterface);
                mappingForInputDtmiNotFoundCounter.Add(1, metricsAttributesHelper.GetValues(new KeyValuePair<string, object?>(Metrics.ModelIdDimensionName, sourceTwinInterface.ToString())));

                return null;
            }
        }

        private bool IsPointACapability(Dtmi dtmi)
        {
            const string capabilityInterface = "dtmi:com:willowinc:Capability;1";

            var dtInterfaces = TargetObjectModel.Where(p => p.Key == dtmi && p.Value.EntityKind == DTEntityKind.Interface);
            var dtInterfaceInfos = new List<DTInterfaceInfo>();

            foreach (var dtInterface in dtInterfaces)
            {
                if (dtInterface.Value is DTInterfaceInfo info)
                {
                    dtInterfaceInfos.Add(info);
                }
            }

            while (dtInterfaceInfos.Any())
            {
                var parentDtos = new List<DTInterfaceInfo>();

                foreach (var dtInterfaceInfo in dtInterfaceInfos)
                {
                    if (dtInterfaceInfo != null)
                    {
                        var extends = dtInterfaceInfo.Extends.ToList();

                        if (extends.Any(m => m.Id.ToString() == capabilityInterface))
                        {
                            return true;
                        }
                        else if (extends.Any())
                        {
                            foreach (var dtInfo in extends)
                            {
                                parentDtos.Add(dtInfo);
                            }
                        }
                    }
                }

                dtInterfaceInfos = parentDtos;
            }

            return false;
        }

        /// <summary>
        /// Extracts the connector id from the mapping key.
        /// </summary>
        /// <param name="mappingKey">The input mapping key in the format: "msrc://CONCskq1UEa1z9JuHHpkQaQuS@MAPPED_UG/BLDG5o26DguWKu5T9nRvSYn5Em/SW_Stair_VAV_Box_Reheat_Coil#Heating_Coil_Electricity_Rate".</param>
        /// <returns>The connector id: CONCskq1UEa1z9JuHHpkQaQuS.</returns>
        private static string GetConnectorId(string mappingKey)
        {
            // Extract the connector id from the Mapping Key
            return mappingKey.Split('/')[2].Split('@')[0];
        }

        /// <summary>
        /// Parses a source JSON element and populates a provided Property declaration in the input content
        /// directory based on the structure of that source element.
        /// </summary>
        /// <param name="sourceElement">Element parsed from source graph.</param>
        /// <param name="basicDtId">dtID of target digital twin that the contents dictionary belongs to (used for logging).</param>
        /// <param name="interfaceType">Interface of the source digital twin (used for logging).</param>
        /// <param name="contentDictionary">The content dictionary to which the generated Property will be addded.</param>
        /// <param name="property">Property declaration on the target twin's Interface.</param>
        /// <param name="outputDtmi">Interface of the target digital twin (that the contents directory belongs to).</param>
        protected void AddTwinProperty(JsonElement sourceElement, string basicDtId, string interfaceType, Dictionary<string, object> contentDictionary, DTContentInfo property, string outputDtmi)
        {
            // Do the transform first to see if there is a special mapping for this property
            var matchResult = OntologyMappingManager.TryGetObjectTransformation(outputDtmi, property.Name, out var objectTransformation);

            if (matchResult == ObjectTransformationMatch.PropertyAndTypeMatch)
            {
                Logger.LogDebug("Found object transformation for property: '{propertyName}' on interface: '{interfaceType}' for target DTMI: '{outputDtmi}'.", property.Name, interfaceType, outputDtmi);
                PerformObjectTransformation(sourceElement, basicDtId, interfaceType, contentDictionary, objectTransformation);
            }
            else if (matchResult == ObjectTransformationMatch.PropertyMatchOnly)
            {
                Logger.LogDebug("No object transformation found for property: '{propertyName}' on interface: '{interfaceType}' for target DTMI: '{outputDtmi}'. Checking ancestors.", property.Name, interfaceType, outputDtmi);

                var oDtmi = new Dtmi(outputDtmi);
                var hashSet = new HashSet<string>();
                var queue = new Queue<Dtmi>();

                GetParentModels(queue, hashSet, oDtmi);

                var dequeueSuccess = queue.TryDequeue(out var parent);

                // Walk the ancestor tree to find the first parent that has a mapping
                while (dequeueSuccess && parent != null)
                {
                    Logger.LogDebug("No object transformation found for property: '{propertyName}' on interface: '{interfaceType}' for target DTMI: '{outputDtmi}'. Checking parent: '{parentDtmi}'.", property.Name, interfaceType, outputDtmi, parent);
                    if (OntologyMappingManager.TryGetObjectTransformation(parent.ToString(), property.Name, out objectTransformation) == ObjectTransformationMatch.PropertyAndTypeMatch)
                    {
                        Logger.LogInformation("Found object transformation for property: '{propertyName}' on interface: '{interfaceType}' for target DTMI: '{outputDtmi}'.", property.Name, interfaceType, outputDtmi);
                        PerformObjectTransformation(sourceElement, basicDtId, interfaceType, contentDictionary, objectTransformation);

                        // Stop at the first match
                        break;
                    }
                    else
                    {
                        GetParentModels(queue, hashSet, parent);
                    }

                    dequeueSuccess = queue.TryDequeue(out parent);
                }
            }

            // Find the property on the input type that matches the propertyName of this property
            if (sourceElement.TryGetProperty(property.Name, out var propertyValue))
            {
                if (propertyValue.ValueKind != JsonValueKind.Null)
                {
                    // If the property already exists, we don't want to overwrite it
                    contentDictionary.TryAdd(property.Name, propertyValue);
                }
                else
                {
                    // Check to see if there are fields we should use to fill the output property with if the input property is null
                    if (OntologyMappingManager.TryGetFillProperty(outputDtmi, property.Name, out var fillProperty) && fillProperty != null)
                    {
                        // Loop through the list
                        foreach (var inputProperty in fillProperty.InputPropertyNames)
                        {
                            // See if the input element has a value for that property
                            if (sourceElement.TryGetProperty(inputProperty, out var inputValue))
                            {
                                // Take the first one that is not null
                                if (inputValue.ValueKind != JsonValueKind.Null)
                                {
                                    // If the property already exists, we don't want to overwrite it
                                    contentDictionary.TryAdd(property.Name, inputValue);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // See if there are any projections we need to make for the properties
            if (OntologyMappingManager.TryGetPropertyProjection(outputDtmi, property.Name, out var propertyProjection))
            {
                if (propertyProjection != null)
                {
                    foreach (var inputProperty in propertyProjection.InputPropertyNames)
                    {
                        // Get the value of the input property
                        if (sourceElement.TryGetProperty(inputProperty, out var inputValue))
                        {
                            // If the output target is a collection, add the value to the target collection
                            if (propertyProjection.IsOutputPropertyCollection)
                            {
                                if (!contentDictionary.TryGetValue(propertyProjection.OutputPropertyName, out var outputProperty))
                                {
                                    var newProperty = new Dictionary<string, string>() { { inputProperty, inputValue.ToString() } };
                                    contentDictionary.Add(propertyProjection.OutputPropertyName, newProperty);
                                }
                                else
                                {
                                    if (outputProperty is Dictionary<string, string> coll)
                                    {
                                        if (!coll.TryAdd(inputProperty, inputValue.ToString()))
                                        {
                                            Errors.TryAdd(basicDtId, $"Duplicate target property in collection: '{propertyProjection.OutputPropertyName}' with InterfaceType: '{interfaceType}' for DTMI: '{basicDtId}'. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Duplicate-target-property-in-collection");
                                            Logger.LogWarning("Duplicate target property in collection: '{OutputPropertyName}' with InterfaceType: '{InterfaceType}' for DTMI: '{DtId}'.", propertyProjection.OutputPropertyName, interfaceType, basicDtId);
                                            duplicateMappingPropertyFoundCounter.Add(1, metricsAttributesHelper.GetValues(new KeyValuePair<string, object?>("PropertyName", propertyProjection.OutputPropertyName)));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // If the output target is not a collection, add the value to the target
                                if (!contentDictionary.TryAdd(propertyProjection.OutputPropertyName, inputValue.ToString()))
                                {
                                    Errors.TryAdd(basicDtId, "Duplicate target property: '{propertyProjection.OutputPropertyName}' with InterfaceType: '{interfaceType}' for DTMI: '{basicDtId}'. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Duplicate-target-property");
                                    Logger.LogWarning("Duplicate target property: '{OutputPropertyName}' with InterfaceType: '{InterfaceType}' for DTMI: '{DtId}'.", propertyProjection.OutputPropertyName, interfaceType, basicDtId);
                                    duplicateMappingPropertyFoundCounter.Add(1, metricsAttributesHelper.GetValues(new KeyValuePair<string, object?>("PropertyName", propertyProjection.OutputPropertyName)));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void PerformObjectTransformation(JsonElement sourceElement, string basicDtId, string interfaceType, Dictionary<string, object> contentDictionary, ObjectTransformation? objectTransformation)
        {
            if (objectTransformation != null)
            {
                Logger.LogDebug("Performing object transformation for property: '{outputPropertyName}' with InterfaceType: '{interfaceType}' for DTMI: '{dtId}'.", objectTransformation.OutputPropertyName, interfaceType, basicDtId);

                // Get the value of the input property
                if (sourceElement.TryGetProperty(objectTransformation.InputProperty, out var inputProperty))
                {
                    if (inputProperty.ValueKind == JsonValueKind.Object)
                    {
                        // Get the value of the input property
                        if (inputProperty.TryGetProperty(objectTransformation.InputPropertyName, out var inputPropertyValue))
                        {
                            if (!contentDictionary.TryAdd(objectTransformation.OutputPropertyName, inputPropertyValue.ToString()))
                            {
                                Errors.TryAdd(basicDtId, "Duplicate target property: '{objectTransformation.OutputPropertyName}' with InterfaceType: '{interfaceType}' for DTMI: '{basicDtId}'. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Duplicate-target-property");
                                Logger.LogWarning("Duplicate target property: '{outputPropertyName}' with InterfaceType: '{interfaceType}' for DTMI: '{dtId}'.", objectTransformation.OutputPropertyName, interfaceType, basicDtId);
                                duplicateMappingPropertyFoundCounter.Add(1, metricsAttributesHelper.GetValues(new KeyValuePair<string, object?>("PropertyName", objectTransformation.OutputPropertyName)));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parses a source JSON element and populates a provided component declaration in the input content
        /// directory based on the structure of that source element.
        /// </summary>
        /// <param name="sourceElement">Element parsed from source graph.</param>
        /// <param name="contentDictionary">The content dictionary to which the generated component will be added.</param>
        /// <param name="component">Component declaration on the target twin's Interface.</param>
        protected void AddComponent(JsonElement sourceElement, Dictionary<string, object> contentDictionary, DTContentInfo component)
        {
            // Find the property on the input type that matches the propertyName of this component
            if (sourceElement.TryGetProperty(component.Name, out var propertyValue) && propertyValue.ValueKind != JsonValueKind.Null)
            {
                contentDictionary.Add(component.Name, propertyValue);
            }
            else
            {
                // If there is a component field on the Target Model, and there is not input value, create an element with empty $metadata as components are not optional
                contentDictionary.Add(component.Name, EmptyComponentElement);
            }
        }

        /// <summary>
        /// Adds a new relationship to the provided relationships collection, based on the provided  parameters
        /// and employing ontology mapping to translate DTMI of the relationship source/target twins' Interfaces,
        /// relationship name/direction, etc.
        /// </summary>
        /// <param name="relationships">Collection to which the new relationship is added.</param>
        /// <param name="sourceDtId">dtId of the input relationship source.</param>
        /// <param name="inputSourceDtmi">DTMI of the input relationship source's Interface.</param>
        /// <param name="inputRelationshipType">Input relationship name.</param>
        /// <param name="targetDtId">dtId of the input relationship target.</param>
        /// <param name="targetInterfaceType">DTMI of the input relationship target's Interface.</param>
        /// <param name="relationshipProperties">A dictionary of proporties for the relationship.</param>
        protected void AddRelationship(IDictionary<string, BasicRelationship> relationships,
                                      string sourceDtId,
                                      Dtmi? inputSourceDtmi,
                                      string? inputRelationshipType,
                                      string targetDtId,
                                      string targetInterfaceType,
                                      IDictionary<string, object> relationshipProperties)
        {
            // Get the Dtmi for the input Target entity
            Dtmi? targetInputDtmi = GetInputInterfaceDtmi(targetInterfaceType);

            Dtmi? outputSourceDtmi = null;

            if (inputSourceDtmi != null)
            {
                if (!TryGetOutputInterfaceDtmi(inputSourceDtmi, out outputSourceDtmi) || outputSourceDtmi == null)
                {
                    Logger.LogInformation("No output interface found for input interface: '{inputInterfaceType}'.", inputSourceDtmi);
                }
            }
            else
            {
                Logger.LogInformation("Input source interface is null for sourceDtId: '{SourceDtId}'.", sourceDtId);
            }

            if (targetInputDtmi != null)
            {
                // Now try to get the matching outputDtmi for the Target entity
                if (TryGetOutputInterfaceDtmi(targetInputDtmi, out var targetOutputDtmi) && targetOutputDtmi != null)
                {
                    if (!string.IsNullOrEmpty(inputRelationshipType))
                    {
                        // Get Output relationship
                        var outputRelationship = GetOutputRelationshipType(inputRelationshipType);

                        if (outputSourceDtmi != null && TargetObjectModel.TryGetValue(outputSourceDtmi, out var model))
                        {
                            var relationship = ((DTInterfaceInfo)model).Contents.FirstOrDefault(p => p.Value.EntityKind == DTEntityKind.Relationship && p.Value.Name == outputRelationship.Item1);
                            var relationshipId = outputRelationship.Item2 ? graphNamingManager.GetRelationshipName(targetDtId, sourceDtId, outputRelationship.Item1, relationshipProperties) :
                                                                            graphNamingManager.GetRelationshipName(sourceDtId, targetDtId, outputRelationship.Item1, relationshipProperties);

                            // Create a basic relationship
                            var basicRelationship = new BasicRelationship
                            {
                                SourceId = outputRelationship.Item2 ? targetDtId : sourceDtId,
                                TargetId = outputRelationship.Item2 ? sourceDtId : targetDtId,
                                Id = relationshipId,
                                Name = outputRelationship.Item1.ToString(),
                            };

                            if (relationshipProperties != null)
                            {
                                foreach (var relationshipProperty in relationshipProperties)
                                {
                                    basicRelationship.Properties.Add(new KeyValuePair<string, object>(relationshipProperty.Key, relationshipProperty.Value));
                                }
                            }

                            relationships.TryAdd(basicRelationship.Id, basicRelationship);
                        }
                        else
                        {
                            var errorMessage = $"Output relationship '{outputRelationship.Item1}' not found in Target Model. Source Element Id: '{sourceDtId}', SourceDtmi: '{outputSourceDtmi}', TargetId: '{targetDtId}, TargetDtmi: '{targetOutputDtmi}'. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Output-relationship-'%7BoutputRelationship.Item1%7D'-not-found-in-Target-Model";
                            Errors.TryAdd(sourceDtId, errorMessage);
                            Logger.LogWarning(errorMessage);

                            relationshipNotFoundInModelCounter?.Add(1, new KeyValuePair<string, object?>(Metrics.RelationshipTypeDimensionName, outputRelationship.Item1 ?? "NotFound"));
                        }
                    }
                    else
                    {
                        var errorMessage = $"Input relationship type is null. Source Element Id: '{sourceDtId}', TargetId: '{targetDtId}, TargetDtmi: '{targetOutputDtmi}'. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Input-relationship-type-is-null";
                        Errors.TryAdd(sourceDtId, errorMessage);
                        Logger.LogWarning(errorMessage);

                        relationshipNotFoundInModelCounter?.Add(1, new KeyValuePair<string, object?>(Metrics.RelationshipTypeDimensionName, inputRelationshipType ?? "NotFound"));
                    }
                }
            }
        }

        private void GetParentModels(Queue<Dtmi> queue, HashSet<string> hashSet, Dtmi model)
        {
            if (model == null)
            {
                return;
            }

            var dtInterfaceInfo = TargetObjectModel.FirstOrDefault(p => p.Key == model && p.Value.EntityKind == DTEntityKind.Interface);

            if (dtInterfaceInfo.Value != null)
            {
                var p = dtInterfaceInfo.Value as DTInterfaceInfo;

                if (p != null)
                {
                    var extends = p.Extends.ToList();
                    if (extends.Count > 0)
                    {
                        foreach (var dtInterface in extends)
                        {
                            if (!hashSet.Contains(dtInterface.Id.ToString()))
                            {
                                hashSet.Add(dtInterface.Id.ToString());
                                queue.Enqueue(dtInterface.Id);
                            }
                        }
                    }
                }
            }

            return;
        }

        private async Task Init(CancellationToken cancellationToken)
        {
            var targetModelList = await OutputGraphManager.GetModelsAsync(cancellationToken);

            try
            {
                // Load the target model into the Model Parser, to make it possible to write queries against the model
                TargetObjectModel = TargetModelParser.Parse(targetModelList);

                foreach (var model in TargetObjectModel)
                {
                    TargetObjectModels.TryAdd(model.Key.AbsoluteUri, model.Value);
                }

                // Validate the target map. Don't need to stop processing if there is an error, but results will show up in the logs
                if (!OntologyMappingManager.ValidateTargetOntologyMapping(TargetObjectModel, out var invalidTargets) && invalidTargets != null)
                {
                    invalidTargetDtmisCounter.Add(invalidTargets.Count, metricsAttributesHelper.GetValues());

                    foreach (var invalidTarget in invalidTargets)
                    {
                        Logger.LogWarning("Invalid Target DTMI found: {invalidTarget}", invalidTarget);
                    }
                }
            }
            catch (ParsingException ex)
            {
                Errors.TryAdd("Processing error", "Error parsing target model. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Error-parsing-target-model");
                Logger.LogError(ex, "Error parsing models: {errors}", ex.Errors);
            }
        }

        /// <summary>
        /// Handles special cases where we don't want to create certain relationships.
        /// </summary>
        /// <param name="twins">The list of twins to be created.</param>
        /// <param name="relationships">The list of relationships to be created.</param>
        private void RemoveRedundantRelationships(Dictionary<string, BasicDigitalTwin> twins, Dictionary<string, BasicRelationship> relationships)
        {
            const string buildingModelId = "dtmi:com:willowinc:Building;1";
            const string levelModelId = "dtmi:com:willowinc:Level;1";
            const string zoneModelId = "dtmi:com:willowinc:Zone;1";

            var relationshipsToRemove = new List<string>();

            // Twins can't be located in more than one place
            foreach (var twin in twins)
            {
                var relationshipsForTwin = relationships.Where(r => r.Value.SourceId == twin.Key && r.Value.Name == RelationshipType.IsLocatedIn);

                if (relationshipsForTwin.Count() < 2)
                {
                    continue;
                }

                var targetTwins = relationshipsForTwin.Select(r => r.Value.TargetId).ToList();

                var relTwins = new List<BasicDigitalTwin>();

                foreach (var relTwin in targetTwins)
                {
                    var t = twins[relTwin];
                    relTwins.Add(t);
                }

                var buildings = relTwins.Where(t => IsChildOf(TargetObjectModels, buildingModelId, t.Metadata.ModelId));
                var floors = relTwins.Where(t => IsChildOf(TargetObjectModels, levelModelId, t.Metadata.ModelId));

                // If there are space twins, remove the relationships to building and floor
                if (relTwins.Count > buildings.Count() + floors.Count())
                {
                    relationshipsToRemove.AddRange(buildings.Select(f => f.Id));
                    relationshipsToRemove.AddRange(floors.Select(f => f.Id));
                }
                else
                {
                    // If there are more building twins than space twins, remove the relationships to the building
                    if (relTwins.Count > buildings.Count())
                    {
                        relationshipsToRemove.AddRange(buildings.Select(f => f.Id));
                    }
                }
            }

            if (relationshipsToRemove.Count > 0)
            {
                foreach (var relation in relationshipsToRemove)
                {
                    relationships.Remove(relation);
                }
            }

            relationshipsToRemove.Clear();

            // Twins can't be part of more than one place
            foreach (var twin in twins)
            {
                var relationshipsForTwin = relationships.Where(r => r.Value.SourceId == twin.Key && r.Value.Name == RelationshipType.IsPartOf);

                if (relationshipsForTwin.Count() < 2)
                {
                    continue;
                }

                var targetTwins = relationshipsForTwin.Select(r => r.Value.TargetId).ToList();

                var relTwins = new List<BasicDigitalTwin>();

                foreach (var relTwin in targetTwins)
                {
                    var t = twins[relTwin];
                    relTwins.Add(t);
                }

                var buildings = relTwins.Where(t => IsChildOf(TargetObjectModels, buildingModelId, t.Metadata.ModelId));
                var floors = relTwins.Where(t => IsChildOf(TargetObjectModels, levelModelId, t.Metadata.ModelId));

                // If there are space twins, remove the relationships to building and floor
                if (relTwins.Count > buildings.Count() + floors.Count())
                {
                    relationshipsToRemove.AddRange(buildings.Select(f => f.Id));
                    relationshipsToRemove.AddRange(floors.Select(f => f.Id));
                }
                else
                {
                    // If there are more building twins than space twins, remove the relationships to the building
                    if (relTwins.Count > buildings.Count())
                    {
                        relationshipsToRemove.AddRange(buildings.Select(f => f.Id));
                    }
                }
            }

            if (relationshipsToRemove.Count > 0)
            {
                foreach (var relation in relationshipsToRemove)
                {
                    relationships.Remove(relation);
                }
            }

            relationshipsToRemove.Clear();

            // We don't want a relationship between level and zone if the zone is a part of a room
            foreach (var twin in twins)
            {
                // If the twin is not a zone, don't worry about it
                if (!IsChildOf(TargetObjectModels, zoneModelId, twin.Value.Metadata.ModelId))
                {
                    continue;
                }

                // Get the relationships to the zone where the source twin is the zone, and it's an isPartOf relationship (Levels)
                var sourceRelationshipsForTwin = relationships.Where(r => r.Value.SourceId == twin.Key && r.Value.Name == RelationshipType.IsPartOf);

                // Get the relationships to the zone where the target twin is the zone, and it's an isPartOf relationship (Spaces)
                var targetRelationshipsForTwin = relationships.Where(r => r.Value.TargetId == twin.Key && r.Value.Name == RelationshipType.IsPartOf);

                // If either has no entities, we don't need to remove any
                if (!sourceRelationshipsForTwin.Any() || !targetRelationshipsForTwin.Any())
                {
                    continue;
                }

                // Since we now have a zone which is set up as PartOf a level, and it hasParts of space, remove the level relationship,
                relationshipsToRemove.AddRange(sourceRelationshipsForTwin.Select(r => r.Key));
            }

            if (relationshipsToRemove.Count > 0)
            {
                foreach (var relation in relationshipsToRemove)
                {
                    relationships.Remove(relation);
                }
            }
        }

        private bool IsChildOf(IDictionary<string, DTEntityInfo> interfaceInfos, string parent, string child)
        {
            if (parent == child)
            {
                return true;
            }

            if (!interfaceInfos.TryGetValue(child, out var dtEntityInfo))
            {
                Errors.TryAdd(child, $"Model {child} is not defined. See https://willow.atlassian.net/wiki/spaces/PE/pages/2547908609/Mapped+Topology+Ingestion+MTI#Model-is-not-defined");
                Logger.LogWarning("Model {ch} is not defined", child.ToString());
                return false;
            }

            var extends = ((DTInterfaceInfo)dtEntityInfo).Extends;

            if ((extends?.Count ?? 0) < 1)
            {
                return false;
            }

            if (extends != null)
            {
                return extends.Any(i => IsChildOf(interfaceInfos, parent, i.Id.AbsoluteUri));
            }

            return false;
        }
    }
}
