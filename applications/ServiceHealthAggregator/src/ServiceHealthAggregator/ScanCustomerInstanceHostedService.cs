namespace Willow.ServiceHealthAggregator
{
    using System.Net.Http.Headers;
    using System.Text.Json;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using Willow.AzureDigitalTwins.SDK.Client;
    using Willow.Model.Adt;
    using Willow.Model.Requests;
    using Willow.Security.KeyVault;
    using Willow.Support.SDK;

    /// <summary>
    /// Scans the customer instance for data needed by other systems for monitoring and alerting.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ScanCustomerInstanceHostedService"/> class.
    /// </remarks>
    /// <param name="logger">Instance of an ILogger.</param>
    /// <param name="options">Configuration options for the scan.</param>
    /// <param name="twinsClient">Client for accessing the digital twins via the Twins Api.</param>
    /// <param name="httpClientFactory">HttpClientFactory for generating http clients.</param>
    /// <param name="secretManager">An instance of the secret manager used for getting secrets from keyvault.</param>
    public class ScanCustomerInstanceHostedService(ILogger<ScanCustomerInstanceHostedService> logger,
                                                   IOptions<InstanceOptions> options,
                                                   ITwinsClient twinsClient,
                                                   IHttpClientFactory httpClientFactory,
                                                   ISecretManager secretManager)
        : IHostedService, IDisposable
    {
        private readonly ILogger<ScanCustomerInstanceHostedService> logger = logger;
        private readonly IOptions<InstanceOptions> options = options;
        private readonly ITwinsClient twinsClient = twinsClient;
        private readonly IHttpClientFactory httpClientFactory = httpClientFactory;
        private readonly ISecretManager secretManager = secretManager;
        private Timer? timer = null;
        private CancellationToken stoppingToken;
        private int pageSize = 100;
        private const int ActiveConnectorStatus = 2;
        private const int CommissioningBuildingConnectorStatus = 1;

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;
            if (options.Value.EnableInstanceScan)
            {
                logger.LogInformation("Scan Customer Instance Hosted Service running.");

                timer = new Timer(ExecuteTaskAsync, null, TimeSpan.Zero, TimeSpan.FromMinutes(options.Value.RefreshInterval));
            }

            return Task.CompletedTask;
        }

        private async void ExecuteTaskAsync(object? state)
        {
            try
            {
                logger.LogInformation("Scan Customer Instance Hosted Service Run Starting.");

                // Add the Buildings Twins to WSUP
                var buildings = await GetBuildings(stoppingToken);

                await AddBuildingsToWsup(buildings);

                // Connect to Twins API and Get a list of all Connector Twins
                var connectors = await GetConnectors(stoppingToken);

                // Add the Connector Twins to WSUP
                await AddConnectorsToWsup(connectors);

                // Add the Building-Connector relationships to WSUP
                await AddBuildingConnectorRelationshipsToWsup(buildings, connectors);

                logger.LogInformation("Scan Customer Instance Hosted Service Run Completed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error running Scan Customer Instance Hosted Service");
            }
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Scan Customer Instance Hosted Service is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Disposes of the timer.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            timer?.Dispose();
        }

        private async Task<IEnumerable<TwinWithRelationships>> GetBuildings(CancellationToken cancellationToken)
        {
            var buildings = new List<TwinWithRelationships>();

            try
            {
                // Connect to Twins API and Get a list of all Buildings
                var buildingsRequest = new GetTwinsInfoRequest
                {
                    ModelId = ["dtmi:com:willowinc:Building;1"],
                    SourceType = SourceType.AdtQuery,
                    IncludeRelationships = true,
                };

                var page = await twinsClient.GetTwinsAsync(buildingsRequest, pageSize, cancellationToken: cancellationToken);
                buildings.AddRange(page.Content);

                while (page.ContinuationToken != null)
                {
                    page = await twinsClient.GetTwinsAsync(buildingsRequest, continuationToken: page.ContinuationToken, cancellationToken: cancellationToken);
                    buildings.AddRange(page.Content);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting buildings from ADT");
                throw;
            }

            return buildings;
        }

        private async Task<IEnumerable<TwinWithRelationships>> GetConnectors(CancellationToken cancellationToken)
        {
            var connectors = new List<TwinWithRelationships>();

            try
            {
                var connectorsRequest = new GetTwinsInfoRequest
                {
                    ModelId = ["dtmi:com:willowinc:ConnectorApplication;1"],
                    SourceType = SourceType.AdtQuery,
                    IncludeRelationships = true,
                };

                var page = await twinsClient.GetTwinsAsync(connectorsRequest, pageSize, cancellationToken: cancellationToken);
                connectors.AddRange(page.Content);

                while (page.ContinuationToken != null)
                {
                    page = await twinsClient.GetTwinsAsync(connectorsRequest, continuationToken: page.ContinuationToken, cancellationToken: cancellationToken);
                    connectors.AddRange(page.Content);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting connectors from ADT");
                throw;
            }

            return connectors;
        }

        private async Task AddBuildingsToWsup(IEnumerable<TwinWithRelationships> basicDigitalTwinWithRelationships)
        {
            try
            {
                var httpClient = await GetHttpClient();

                logger.LogInformation("Adding Buildings to WSUP");

                var buildingsClient = new BuildingsClient(httpClient);

                var existingBuildings = await buildingsClient.GetAllByCustomerInstanceIdAsync(options.Value.CustomerInstanceId);

                logger.LogInformation("Buildings in ADT: {AdtBuildingCount}. Existing buildings in WSUP: {BuildingCount}.", basicDigitalTwinWithRelationships.Count(), existingBuildings.Count);

                foreach (var building in basicDigitalTwinWithRelationships)
                {
                    // For now, we are only adding buildings that do not already exist in WSUP
                    if (existingBuildings.Any(b => b.Id == building.Twin.Id))
                    {
                        continue;
                    }

                    var newBuilding = new Building()
                    {
                        Id = building.Twin.Id,
                        Name = building.Twin.Contents["name"].ToString(),
                        CustomerInstanceId = options.Value.CustomerInstanceId,
                    };

                    await buildingsClient.CreateAsync(newBuilding);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding buildings to WSUP");
                throw;
            }
        }

        private async Task AddConnectorsToWsup(IEnumerable<TwinWithRelationships> basicDigitalTwinWithRelationships)
        {
            try
            {
                var httpClient = await GetHttpClient();

                var connectorTypesClient = new ConnectorTypesClient(httpClient);
                var connectorsClient = new ConnectorsClient(httpClient);

                logger.LogInformation("Adding {ConnectorCount} Connectors to WSUP", basicDigitalTwinWithRelationships.Count());

                var existingConnectors = await connectorsClient.GetAllByCustomerInstanceAsync(options.Value.CustomerInstanceId);

                logger.LogInformation("Existing connectors: {ConnectorCount}", existingConnectors.Count);

                foreach (var connector in basicDigitalTwinWithRelationships)
                {
                    // For now, we are only adding connectors that do not already exist in WSUP
                    if (existingConnectors.Any(b => b.Id == connector.Twin.Id))
                    {
                        continue;
                    }

                    if (connector.Twin.Contents.TryGetValue("connectorType", out var connectorTypeRaw))
                    {
                        var connectorTypeElement = (JsonElement)connectorTypeRaw;
                        var connectorTypeName = connectorTypeElement.GetProperty("name").GetString();
                        var connectorTypeId = connectorTypeElement.GetProperty("id").GetString();

                        var existingConnectorTypes = await connectorTypesClient.GetAllAsync();
                        logger.LogInformation("Existing connectorTypes: {ConnectorTypeCount}. Existing connectors: {ConnectorCount}", existingConnectorTypes.Count, existingConnectors.Count);

                        logger.LogInformation("ConnectorType for ConnectorId '{ConnectorId}' is {ConnectorType}", connector.Twin.Id, connectorTypeName);

                        // Determine if the connector type already exists in WSUP
                        if (!existingConnectorTypes.Any(ct => ct.Id == connectorTypeId))
                        {
                            connectorTypeElement.TryGetProperty("version", out var versionProperty);
                            var version = (versionProperty.ValueKind == JsonValueKind.String) ? versionProperty.GetString() : string.Empty;

                            connectorTypeElement.TryGetProperty("direction", out var directionProperty);
                            var direction = (versionProperty.ValueKind == JsonValueKind.String) ? directionProperty.GetString() : string.Empty;

                            var newConnectorType = new ConnectorType()
                            {
                                Id = connectorTypeId,
                                Name = connectorTypeName,
                                Version = version,
                                Direction = direction,
                            };

                            await connectorTypesClient.CreateAsync(newConnectorType);
                        }

                        // Create the connector in WSUP for the current customer instance
                        var newConnector = new Connector()
                        {
                            Id = connector.Twin.Id,
                            Name = connector.Twin.Contents["name"].ToString(),
                            CustomerInstanceId = options.Value.CustomerInstanceId,
                            ConnectorTypeId = connectorTypeId,
                            ConnectorStatusId = ActiveConnectorStatus,
                        };

                        await connectorsClient.CreateAsync(newConnector);
                    }
                    else
                    {
                        logger.LogWarning("Connector '{ConnectorId}' does not have a connectorType", connector.Twin.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding connectors to WSUP");
                throw;
            }
        }

        private async Task AddBuildingConnectorRelationshipsToWsup(IEnumerable<TwinWithRelationships> buildings, IEnumerable<TwinWithRelationships> connectors)
        {
            try
            {
                logger.LogInformation("Adding Building-Connectors to WSUP");

                var httpClient = await GetHttpClient();

                var buildingConnectorsClient = new BuildingConnectorsClient(httpClient);

                var existingBuildingConnectors = await buildingConnectorsClient.GetAll2Async(options.Value.CustomerInstanceId);

                foreach (var building in buildings)
                {
                    var buildingId = building.Twin.Id;

                    var outgoingBuildingConnectors = building.OutgoingRelationships.Where(r => r.Name == "servedBy" && connectors.Any(c => c.Twin.Id == r.TargetId));

                    foreach (var buildingConnector in outgoingBuildingConnectors)
                    {
                        var connectorId = buildingConnector.TargetId;

                        // For now, we are only adding building-connector relationships that do not already exist in WSUP
                        if (existingBuildingConnectors.Any(bc => bc.BuildingId == buildingId && bc.ConnectorId == connectorId))
                        {
                            continue;
                        }

                        var newBuildingConnector = new BuildingConnector()
                        {
                            BuildingId = buildingId,
                            ConnectorId = connectorId,
                            BuildingConnectorStatusId = CommissioningBuildingConnectorStatus,
                            CustomerInstanceId = options.Value.CustomerInstanceId,
                        };

                        try
                        {
                            await buildingConnectorsClient.CreateAsync(newBuildingConnector);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error adding building connectors to WSUP. Building Id: {BuildingId}, ConnectorId: {ConnectorId}", buildingId, connectorId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding building connectors to WSUP");
                throw;
            }
        }

        private async Task<HttpClient> GetHttpClient()
        {
            var endpoint = options.Value.WsupApiEndpoint ?? throw new ArgumentNullException("WsupApi Endpoint is not configured in appsettings.json");
            var httpClient = httpClientFactory.CreateClient("Willow.ServiceHealthAggregator");
            httpClient.BaseAddress = new Uri(endpoint);

            var token = await GetAccessTokenClientId();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return httpClient;
        }

        private async Task<string> GetAccessTokenClientId()
        {
            var authority = options.Value.WsupApiAuthority ?? throw new ArgumentNullException("Authority is not configured in appsettings.json");
            var wsupApiScope = options.Value.WsupApiScope ?? throw new ArgumentNullException("WsupApiScope is not configured in appsettings.json");
            var clientSecret = await secretManager.GetSecretAsync("WsupApi--ClientSecret");

            if (string.IsNullOrEmpty(clientSecret?.Value))
            {
                throw new ArgumentNullException("Client Secret is not configured in Key Vault");
            }

            IConfidentialClientApplication msalClient = ConfidentialClientApplicationBuilder.Create(options.Value.WsupApiClientId)
                   .WithClientSecret(clientSecret.Value)
                   .WithAuthority(new Uri(authority))
                   .Build();

            var msalAuthenticationResult = await msalClient.AcquireTokenForClient(new string[] { wsupApiScope }).ExecuteAsync();
            var token = msalAuthenticationResult.AccessToken;

            return token;
        }
    }
}
