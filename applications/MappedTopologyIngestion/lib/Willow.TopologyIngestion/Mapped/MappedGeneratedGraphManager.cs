//-----------------------------------------------------------------------
// <copyright file="MappedGeneratedGraphManager.cs" Company="Willow">
//   Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Willow.TopologyIngestion.Mapped
{
    using System.Diagnostics.Metrics;
    using System.Net.Http.Json;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using mpd;
    using Polly;
    using Willow.Security.KeyVault;
    using Willow.Telemetry;
    using Willow.TopologyIngestion.Interfaces;

    /// <summary>
    /// Load a topology graph from a Mapped instance via the Mapped API.
    /// </summary>
    public class MappedGeneratedGraphManager : IInputGraphManager
    {
        private readonly ILogger logger;
        private readonly ISecretManager secretManager;
        private readonly MetricsAttributesHelper metricsAttributesHelper;
        private readonly MappedIngestionManagerOptions options;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly JsonDocument model;
        private readonly Counter<long> queryFailedCounter;
        private readonly Counter<long> secretReloadFailedCounter;
        private const string MappedSecretName = "MappedToken";

        /// <summary>
        /// Initializes a new instance of the <see cref="MappedGeneratedGraphManager"/> class.
        /// </summary>
        /// <param name="logger">An instance of an <see cref="ILogger">ILogger</see> used to log status as needed.</param>
        /// <param name="httpClientFactory">An instance of <see cref="IHttpClientFactory">IHttpClientFactory</see> used to create an HttpClient.</param>
        /// <param name="options">An instance of IOptions of <see cref="MappedIngestionManagerOptions">MappedIngestionManagerOptions</see> used to pass paramters to the Graph Manager.</param>
        /// <param name="meterFactory">The meter factory for creating meters.</param>
        /// <param name="secretManager">The secret manager instance for getting secrets from KeyVault and managing failover.</param>
        /// <param name="metricsAttributesHelper">An instance of the metrics helper.</param>
        public MappedGeneratedGraphManager(ILogger<MappedGeneratedGraphManager> logger,
                                           IHttpClientFactory httpClientFactory,
                                           IOptions<MappedIngestionManagerOptions> options,
                                           IMeterFactory meterFactory,
                                           ISecretManager secretManager,
                                           MetricsAttributesHelper metricsAttributesHelper)
        {
            this.logger = logger;
            this.secretManager = secretManager;
            this.metricsAttributesHelper = metricsAttributesHelper;
            this.options = options.Value;

            model = LoadObjectModelJson();

            var meterOptions = options.Value.MeterOptions;
            var meter = meterFactory.Create(meterOptions.Name, meterOptions.Version, meterOptions.Tags);
            queryFailedCounter = meter.CreateCounter<long>("Mti-ExactTypeNotFound", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            secretReloadFailedCounter = meter.CreateCounter<long>("Mti-SecretReloadFailed", null, null, options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
            this.httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Generic method for getting a JsonDocument from Mapped Graph API for a passed in Graph Query.
        /// </summary>
        /// <param name="query">A formatted graph query.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A JSON Document containing the results of the query against the Mapped API.</returns>
        public virtual async Task<JsonDocument?> GetTwinGraphAsync(string query, CancellationToken cancellationToken)
        {
            logger.LogInformation("Getting topology from mapped. {query}", query);

            var queryObject = new
            {
                query = "query " + query,
            };

            var httpRequestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, options.MappedRootUrl)
            {
                Headers =
                {
                    {
                        "Accept", "application/json"
                    },
                },
                Content = JsonContent.Create(queryObject),
            };

            try
            {
                using var httpClient = httpClientFactory.CreateClient("Willow.TopologyIngestion");

                var mappedToken = await secretManager.GetSecretAsync(MappedSecretName).ConfigureAwait(false);

                if (mappedToken == null)
                {
                    throw new SecretNotFoundException(MappedSecretName);
                }

                httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", mappedToken.Value);

                var policy = Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.Unauthorized || r.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    .WaitAndRetryAsync(6,
                                       retryAttempt => TimeSpan.FromSeconds(Math.Pow(5, retryAttempt)),
                                       async (result, timespan, retryNo, context) =>
                                       {
                                           logger.LogWarning("Query Failed. StatusCode: {status}. Query: {mappedQuery}", result.Result.StatusCode, query);
                                           await secretManager.IncrementFailureAsync(MappedSecretName);

                                           var mappedToken = await secretManager.GetSecretAsync(MappedSecretName).ConfigureAwait(false);

                                           if (mappedToken == null)
                                           {
                                               throw new SecretNotFoundException(MappedSecretName);
                                           }

                                           httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", mappedToken.Value);
                                       });

                var httpResponseMessage = await policy.ExecuteAsync(async () =>
                {
                    return await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
                });

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    await secretManager.ResetFailureAsync(MappedSecretName);
                    return JsonDocument.Parse(response);
                }
                else
                {
                    logger.LogWarning("Query Failed. StatusCode: {status}. Query: {mappedQuery}", httpResponseMessage.StatusCode, query);
                    queryFailedCounter.Add(1, metricsAttributesHelper.GetValues());
                }
            }
            catch (SecretReloadException ex)
            {
                logger.LogError(ex, "Maximum retries exceeded while loading secrets.");
                secretReloadFailedCounter.Add(1, metricsAttributesHelper.GetValues());
            }

            return null;
        }

        /// <summary>
        /// Creates a query that returns all the sites associated to an organization.
        /// </summary>
        /// <returns>A formatted graph query.</returns>
        public virtual string GetOrganizationQuery()
        {
            var queryBuilder = new OrgQueryBuilder()
            .WithSites(new SiteQueryBuilder()
                .WithAllScalarFields()
                .WithIdentities(new SiteIdentityUnionQueryBuilder()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())));

            var query = queryBuilder.Build();

            return query;
        }

        /// <summary>
        /// Creates a query that returns all the Accounts associated to an organization.
        /// </summary>
        /// <returns>A formatted graph query.</returns>
        public virtual string GetAccountsQuery()
        {
            var queryBuilder = new QueryQueryBuilder()
                .WithAccounts(new AccountQueryBuilder()
                    .WithAllScalarFields()
                    .WithHasBill(new PointQueryBuilder()
                            .WithAllScalarFields()
                            .WithIdentities(new PointIdentityUnionQueryBuilder()
                                .WithAllScalarFields()
                                .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated()))
                            .WithUnit(new UnitQueryBuilder()
                                .WithAllScalarFields()))
                    .WithHasProvider(new OrganizationQueryBuilder()
                        .WithAllScalarFields()
                        .WithIdentities(new OrganizationIdentityUnionQueryBuilder()
                            .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                .WithAllScalarFields().WithDateCreated())
                            .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                .WithAllScalarFields().WithDateCreated())
                            .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                .WithAllScalarFields().WithDateCreated())
                            .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                .WithAllScalarFields().WithDateCreated())
                            .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                .WithAllScalarFields().WithDateCreated())
                            .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                .WithAllScalarFields().WithDateCreated())
                            .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                .WithAllScalarFields().WithDateCreated())))
                    .WithIdentities(new AccountIdentityUnionQueryBuilder()
                                        .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())));

            var query = queryBuilder.Build();

            return query;
        }

        /// <summary>
        /// Creates a query that returns all the buildings associated to a site.
        /// </summary>
        /// <param name="siteId">Site to search.</param>
        /// <returns>A formatted graph query.</returns>
        public virtual string GetBuildingsForSiteQuery(string siteId)
        {
            var siteIdParameter = new GraphQlQueryParameter<SiteFilter>("siteId", new SiteFilter() { Id = new IdFilterExpressionInput() { Eq = siteId } });

            var queryBuilder = new QueryQueryBuilder()
                .WithSites(new SiteQueryBuilder()
                    .WithAllScalarFields()
                    .WithBuildings(
                        new BuildingQueryBuilder()
                            .WithAllScalarFields()
                            .WithIsAdjacentTo(new PlaceQueryBuilder()
                                .WithAllScalarFields()
                                .WithSpaceFragment(new SpaceQueryBuilder()
                                    .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                                        .WithAllScalarFields()
                                                .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated()))))
                            .WithIsPartOf(new PlaceQueryBuilder()
                                .WithAllScalarFields()
                                .WithHasPoint(new PointQueryBuilder()
                                    .WithAllScalarFields()
                                        .WithIdentities(new PointIdentityUnionQueryBuilder()
                                            .WithAllScalarFields()
                                            .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated()))
                                        .WithUnit(new UnitQueryBuilder()
                                            .WithAllScalarFields())))
                            .WithIdentities(new BuildingIdentityUnionQueryBuilder()
                                .WithAllFields()
                                        .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())),
                        new BuildingFilter()),
                siteIdParameter)
            .WithParameter(siteIdParameter);

            var query = queryBuilder.Build().ToString();

            return query;
        }

        /// <summary>
        /// Creates a query that returns the building.
        /// </summary>
        /// <param name="buildingId">Building Id to search for.</param>
        /// <returns>A formatted graph query.</returns>
        public virtual string GetBuildingQuery(string buildingId)
        {
            var buildingIdParameter = new GraphQlQueryParameter<BuildingFilter>("buildingId", new BuildingFilter() { Id = new IdFilterExpressionInput() { Eq = buildingId } });

            var queryBuilder = new QueryQueryBuilder()
                .WithBuildings(
                    new BuildingQueryBuilder()
                        .WithAllScalarFields()
                        .WithIsAdjacentTo(new PlaceQueryBuilder()
                            .WithAllScalarFields()
                            .WithSpaceFragment(new SpaceQueryBuilder()
                                .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                                    .WithAllScalarFields()
                                            .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated()))))
                        .WithIsPartOf(new PlaceQueryBuilder()
                            .WithAllScalarFields())
                        .WithHasPoint(new PointQueryBuilder()
                            .WithAllScalarFields()
                                .WithIdentities(new PointIdentityUnionQueryBuilder()
                                    .WithAllScalarFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated()))
                                .WithUnit(new UnitQueryBuilder()
                                    .WithAllScalarFields()))
                        .WithFloors(new FloorQueryBuilder()
                            .WithAllScalarFields()
                            .WithIsAdjacentTo(new PlaceQueryBuilder()
                                .WithAllScalarFields()
                                .WithSpaceFragment(new SpaceQueryBuilder()
                                    .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                                        .WithAllScalarFields()
                                                .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated()))))
                            .WithIdentities(new FloorIdentityUnionQueryBuilder()
                                .WithAllScalarFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())))
                        .WithIdentities(new BuildingIdentityUnionQueryBuilder()
                            .WithAllFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())),
                    buildingIdParameter)
            .WithParameter(buildingIdParameter);

            var query = queryBuilder.Build().ToString();

            return query;
        }

        /// <summary>
        /// Creates a query that returns all the spaces associated to a building.
        /// </summary>
        /// <param name="buildingId">Building to search.</param>
        /// <returns>A formatted graph query.</returns>
        public virtual string GetBuildingPlacesQuery(string buildingId)
        {
            var buildingIdParameter = new GraphQlQueryParameter<BuildingFilter>("buildingId", new BuildingFilter() { Id = new IdFilterExpressionInput() { Eq = buildingId } });

            var queryBuilder = new QueryQueryBuilder()
                .WithBuildings(
                    new BuildingQueryBuilder()
                        .WithAllScalarFields()
                        .WithIsAdjacentTo(new PlaceQueryBuilder()
                            .WithAllScalarFields()
                            .WithSpaceFragment(new SpaceQueryBuilder()
                                .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                                    .WithAllScalarFields()
                                            .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated()))))
                        .WithFloors(new FloorQueryBuilder()
                            .WithAllScalarFields()
                            .WithIsAdjacentTo(new PlaceQueryBuilder()
                                .WithAllScalarFields()
                                .WithSpaceFragment(new SpaceQueryBuilder()
                                    .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                                        .WithAllScalarFields()
                                                .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated())
                                                .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                                    .WithAllScalarFields().WithDateCreated()))))
                            .WithIdentities(new FloorIdentityUnionQueryBuilder()
                                .WithAllScalarFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())))
                        .WithIdentities(new BuildingIdentityUnionQueryBuilder()
                            .WithAllFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())),
                    buildingIdParameter);

            var query = queryBuilder.Build().ToString();

            return query;
        }

        /// <summary>
        /// Creates a query that returns all the connectors associated to an org.
        /// </summary>
        /// <returns>A formatted graph query.</returns>
        public virtual string GetConnectorsQuery()
        {
            var queryBuilder = new QueryQueryBuilder()
                .WithConnectors(new ConnectorQueryBuilder()
                    .WithAllScalarFields());

            var query = queryBuilder.Build().ToString();

            return query;
        }

        /// <summary>
        /// Creates a query that returns all the connectors associated to a building.
        /// </summary>
        /// <param name="buildingId">Building to search.</param>
        /// <returns>A formatted graph query.</returns>
        public virtual string GetBuildingConnectorsQuery(string buildingId)
        {
            var buildingIdParameter = new GraphQlQueryParameter<BuildingFilter>("buildingId", new BuildingFilter() { Id = new IdFilterExpressionInput() { Eq = buildingId } });

            var queryBuilder = new QueryQueryBuilder()
                .WithBuildings(new BuildingQueryBuilder()
                    .WithConnectors(new ConnectorQueryBuilder()
                        .WithConnectorType(new ConnectorTypeQueryBuilder()
                            .WithAllScalarFields())
                        .WithAllScalarFields()),
                buildingIdParameter)
            .WithParameter(buildingIdParameter);

            var query = queryBuilder.Build().ToString();

            return query;
        }

        /// <inheritdoc/>
        public virtual string GetBuildingThingsQuery(string buildingDtId, string connectorId)
        {
            var buildingIdParameter = new GraphQlQueryParameter<BuildingFilter>("buildingId", new BuildingFilter() { Id = new IdFilterExpressionInput() { Eq = buildingDtId } });
            var exactTypeParameter = new GraphQlQueryParameter<ThingFilter>("exactType", new ThingFilter() { ExactType = new StringFilterExpressionInput() { Ne = "Thing" }, ConnectedDataSourceId = new StringFilterExpressionInput() { Eq = connectorId } });

            var queryBuilder = new QueryQueryBuilder()
                .WithBuildings(new BuildingQueryBuilder()
                    .WithThings(new ThingQueryBuilder()
                    .WithAllScalarFields()
                    .WithIdentities(new ThingIdentityUnionQueryBuilder()
                        .WithAllScalarFields()
                        .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                            .WithAllScalarFields().WithDateCreated())
                        .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                            .WithAllScalarFields().WithDateCreated())
                        .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                            .WithAllScalarFields().WithDateCreated())
                        .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                            .WithAllScalarFields().WithDateCreated())
                        .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                            .WithAllScalarFields().WithDateCreated())
                        .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                            .WithAllScalarFields().WithDateCreated())
                        .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                            .WithAllScalarFields().WithDateCreated())
                        .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                            .WithAllScalarFields().WithDateCreated())
                        .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                            .WithAllScalarFields().WithDateCreated())
                        .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                            .WithAllScalarFields().WithDateCreated()))
                    .WithHasDeviceModel(new DeviceModelQueryBuilder()
                        .WithAllScalarFields()
                        .WithManufacturedBy(new AgentQueryBuilder()
                            .WithAllScalarFields()))
                    .WithHasLocation(new PlaceQueryBuilder()
                        .WithAllScalarFields()
                        .WithBuildingFragment(new BuildingQueryBuilder()
                            .WithIdentities(new BuildingIdentityUnionQueryBuilder()
                                .WithAllScalarFields()
                                .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())))
                        .WithFloorFragment(new FloorQueryBuilder()
                            .WithIdentities(new FloorIdentityUnionQueryBuilder()
                                .WithAllScalarFields()
                                .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())))
                        .WithSpaceFragment(new SpaceQueryBuilder()
                            .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                                .WithAllScalarFields()
                                .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated()))))
                    .WithServes(new ServesUnionQueryBuilder()
                        .WithAllScalarFields()
                        .WithZoneFragment(new ZoneQueryBuilder()
                            .WithIdentities(new ZoneIdentityUnionQueryBuilder()
                                .WithAllScalarFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated()))
                            .WithIsPartOf(new PlaceQueryBuilder()
                                .WithAllScalarFields()
                                .WithSpaceFragment(new SpaceQueryBuilder()
                                    .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                                        .WithAllScalarFields()
                                            .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated()))))
                        .WithHasPart(new PlaceQueryBuilder()
                            .WithAllScalarFields()
                            .WithSpaceFragment(new SpaceQueryBuilder()
                                .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                                    .WithAllScalarFields())))))
                    .WithIsFedBy(new IsFedByUnionQueryBuilder()
                        .WithAllScalarFields()
                        .WithThingFragment(new ThingQueryBuilder()
                            .WithAllScalarFields()
                            .WithIdentities(new ThingIdentityUnionQueryBuilder()
                                .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated())
                                .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                    .WithAllScalarFields().WithDateCreated()))
                        .WithAllScalarFields())),
                        exactTypeParameter),
            buildingIdParameter)
            .WithParameter(exactTypeParameter)
            .WithParameter(buildingIdParameter);

            var query = queryBuilder.Build().ToString();

            return query;
        }

        /// <summary>
        /// Creates a query that returns all the points associated to a thing.
        /// </summary>
        /// <param name="thingDtIds">Things to search for.</param>
        /// <returns>A formatted graph query.</returns>
        public virtual string GetPointsForThingsQuery(IList<string> thingDtIds)
        {
            var thingIdParameter = new GraphQlQueryParameter<ThingFilter>("thingId", new ThingFilter() { Id = new IdFilterExpressionInput() { In = thingDtIds.ToArray() } });
            var exactTypeParameter = new GraphQlQueryParameter<PointFilter>("exactType", new PointFilter() { ExactType = new StringFilterExpressionInput() { Ne = "Point" } });

            var queryBuilder = new QueryQueryBuilder()
                .WithThings(new ThingQueryBuilder()
                    .WithAllScalarFields()
                    .WithIdentities(new ThingIdentityUnionQueryBuilder()
                        .WithAllScalarFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated()))
                    .WithHasPoint(new PointQueryBuilder()
                        .WithAllScalarFields()
                        .WithValueMap()
                        .WithIsBilledTo(new AccountQueryBuilder()
                            .WithAllScalarFields())
                        .WithIdentities(new PointIdentityUnionQueryBuilder().WithAllScalarFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated()))
                        .WithUnit(new UnitQueryBuilder().WithAllScalarFields()),
                exactTypeParameter),
                thingIdParameter)
                .WithParameter(thingIdParameter)
                .WithParameter(exactTypeParameter);

            var query = queryBuilder.Build().ToString();
            return query;
        }

        /// <summary>
        /// Creates a query that returns all spaces on a floor.
        /// </summary>
        /// <param name="floorId">floor to search.</param>
        /// <returns>A formatted graph query.</returns>
        public virtual string GetFloorQuery(string floorId)
        {
            var floorIdParameter = new GraphQlQueryParameter<FloorFilter>("floorId", new FloorFilter() { Id = new IdFilterExpressionInput() { Eq = floorId } });

            var queryBuilder = new QueryQueryBuilder()
            .WithFloors(new FloorQueryBuilder()
                .WithAllScalarFields()
                .WithIdentities(new FloorIdentityUnionQueryBuilder()
                    .WithAllFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated()))
                .WithIsAdjacentTo(new PlaceQueryBuilder()
                    .WithAllScalarFields()
                    .WithSpaceFragment(new SpaceQueryBuilder()
                        .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                            .WithAllScalarFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated()))))
                .WithHasPart(new PlaceQueryBuilder()
                    .WithAllScalarFields()
                    .WithIsAdjacentTo(new PlaceQueryBuilder()
                        .WithAllScalarFields()
                        .WithSpaceFragment(new SpaceQueryBuilder()
                            .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                                .WithAllScalarFields()
                                        .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated()))))
                    .WithSpaceFragment(new SpaceQueryBuilder()
                        .WithIsAdjacentTo(new PlaceQueryBuilder()
                            .WithAllScalarFields()
                            .WithSpaceFragment(new SpaceQueryBuilder()
                                .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                                    .WithAllScalarFields()
                                            .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated()))))
                        .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                            .WithAllScalarFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())))
                    .WithZoneFragment(new ZoneQueryBuilder()
                        .WithIsAdjacentTo(new PlaceQueryBuilder()
                            .WithAllScalarFields()
                            .WithSpaceFragment(new SpaceQueryBuilder()
                                .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                                    .WithAllScalarFields()
                                            .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated())
                                            .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                                .WithAllScalarFields().WithDateCreated()))))
                        .WithIdentities(new ZoneIdentityUnionQueryBuilder()
                            .WithAllScalarFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated()))
                        .WithAllScalarFields()))
                .WithZones(new ZoneQueryBuilder()
                    .WithAllScalarFields()
                    .WithIsAdjacentTo(new PlaceQueryBuilder()
                        .WithAllScalarFields()
                        .WithSpaceFragment(new SpaceQueryBuilder()
                            .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                                .WithAllScalarFields()
                                        .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated())
                                        .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                            .WithAllScalarFields().WithDateCreated()))))
                    .WithIdentities(new ZoneIdentityUnionQueryBuilder()
                        .WithAllScalarFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated()))
                    .WithHasPoint(new PointQueryBuilder()
                        .WithAllScalarFields()
                        .WithIdentities(new PointIdentityUnionQueryBuilder()
                            .WithAllScalarFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())))
                    .WithHasPart(new PlaceQueryBuilder()
                                    .WithAllScalarFields()
                                    .WithIsAdjacentTo(new PlaceQueryBuilder()
                                        .WithAllScalarFields()
                                        .WithSpaceFragment(new SpaceQueryBuilder()
                                            .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                                                .WithAllScalarFields()
                                                        .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                                            .WithAllScalarFields().WithDateCreated())
                                                        .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                                            .WithAllScalarFields().WithDateCreated())
                                                        .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                                            .WithAllScalarFields().WithDateCreated())
                                                        .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                                            .WithAllScalarFields().WithDateCreated())
                                                        .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                                            .WithAllScalarFields().WithDateCreated())
                                                        .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                                            .WithAllScalarFields().WithDateCreated())
                                                        .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                                            .WithAllScalarFields().WithDateCreated())
                                                        .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                                            .WithAllScalarFields().WithDateCreated())
                                                        .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                                            .WithAllScalarFields().WithDateCreated())
                                                        .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                                            .WithAllScalarFields().WithDateCreated()))))
                                    .WithSpaceFragment(new SpaceQueryBuilder()
                                        .WithIdentities(new SpaceIdentityUnionQueryBuilder()
                                            .WithAllScalarFields()
                                    .WithBaCnetObjectIdFragment(new BaCnetObjectIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithBaCnetVendorIdFragment(new BaCnetVendorIdQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailAddressIdentityFragment(new EmailAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithEmailIdentityFragment(new EmailIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithExternalIdentityFragment(new ExternalIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithFloorLevelIdentityFragment(new FloorLevelIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithGenericIdentityFragment(new GenericIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithNameIdentityFragment(new NameIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithPostalAddressIdentityFragment(new PostalAddressIdentityQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated())
                                    .WithSpaceCodeFragment(new SpaceCodeQueryBuilder()
                                        .WithAllScalarFields().WithDateCreated()))))),
                floorIdParameter)
            .WithParameter(floorIdParameter);

            var query = queryBuilder.Build().ToString();
            return query;
        }

        /// <inheritdoc />
        public virtual string GetConnectorMetadataQuery(string connectorId)
        {
            var connectorIdParameter = new GraphQlQueryParameter<ConnectorFilterInput>("id", new ConnectorFilterInput() { Id = new IdFilterExpressionInput() { Eq = connectorId } });

            var queryBuilder = new QueryQueryBuilder()
                .WithConnectors(new ConnectorQueryBuilder()
                    .WithAllScalarFields()
                    .WithConfig(),
             connectorIdParameter)
            .WithParameter(connectorIdParameter);

            var query = queryBuilder.Build().ToString();

            return query;
        }

        /// <summary>
        /// Try to get a Digital Twins Model Interface for a Mapped Exact type.
        /// </summary>
        /// <param name="exactType">The exact type of the twin from Mapped.</param>
        /// <param name="dtmi">The output DTMI if found, otherwise string.Empty.</param>
        /// <returns>true if found, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the exact type passed in is Null, String.Empty, or Whitespace.</exception>
        public bool TryGetDtmi(string exactType, out string dtmi)
        {
            dtmi = string.Empty;

            if (string.IsNullOrWhiteSpace(exactType))
            {
                throw new ArgumentNullException(nameof(exactType));
            }

            try
            {
                var root = model.RootElement.EnumerateArray();

                var element = root.FirstOrDefault(e => e.TryGetProperty("displayName", out var propertyName) && string.Compare(propertyName.ToString(), exactType, StringComparison.OrdinalIgnoreCase) == 0);
                if (element.ValueKind != JsonValueKind.Null && element.ValueKind != JsonValueKind.Undefined && element.TryGetProperty("@id", out var idProperty))
                {
                    dtmi = idProperty.ToString();
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting DTMI from Mapped DTDL for ExactType: '{exactType}'", exactType);
                return false;
            }

            return false;
        }

        private static JsonDocument LoadObjectModelJson()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("mapped_dtdl.json"));

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string result = reader.ReadToEnd();
                        return JsonDocument.Parse(result);
                    }
                }
                else
                {
                    throw new FileNotFoundException(resourceName);
                }
            }
        }
    }
}
