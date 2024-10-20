// -----------------------------------------------------------------------
// <copyright file="AdtApiGraphManagerTests.cs" Company="Willow">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.TopologyIngestion.Mapped.Test
{
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.DigitalTwins.Core;
    using Microsoft.AspNetCore.JsonPatch.Operations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Willow.AzureDigitalTwins.SDK.Client;
    using Willow.Model.Adt;
    using Willow.Telemetry;
    using Willow.TopologyIngestion.AzureDigitalTwins;
    using Willow.TopologyIngestion.Interfaces;
    using Xunit;
    using Xunit.Abstractions;

    public class AdtApiGraphManagerTests
    {
        private readonly ITestOutputHelper output;
        private readonly Mock<ILogger<AdtApiGraphManager<IngestionManagerOptions>>> mockLogger;
        private readonly IOptions<IngestionManagerOptions> someOptions;
        private readonly Mock<IMeterFactory> mockMeterFactory;
        private readonly Mock<ITwinMappingIndexer> mockTwinMappingIndexer;
        private readonly Mock<ITwinsClient> mockTwinsClient;
        private readonly Mock<IRelationshipsClient> mockRelationshipsClient;
        private readonly Mock<IModelsClient> mockModelsClient;
        private readonly Mock<IMappingClient> mockMappingClient;
        private readonly IConfiguration configuration;

        private const string SiteId = "testSiteId";
        private const string BuildingId = "testBuildingId";
        private const string ConnectorId = "testConnectorId";

        public AdtApiGraphManagerTests(ITestOutputHelper output)
        {
            this.output = output;
            mockLogger = new Mock<ILogger<AdtApiGraphManager<IngestionManagerOptions>>>();
            someOptions = Options.Create(new IngestionManagerOptions());
            mockMeterFactory = new Mock<IMeterFactory>();
            mockTwinMappingIndexer = new Mock<ITwinMappingIndexer>();
            mockTwinsClient = new Mock<ITwinsClient>();
            mockRelationshipsClient = new Mock<IRelationshipsClient>();
            mockModelsClient = new Mock<IModelsClient>();
            mockMappingClient = new Mock<IMappingClient>();

            someOptions.Value.EnableUpdates = true;
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));
            configuration = new ConfigurationBuilder()
                .Build();
        }

        [Fact]
        public async Task GetSiteIdForBuildingWithSiteId_ReturnsSiteId()
        {
            const string buildingId = "1234";
            const string siteId = "9876";
            var twinWithRelationships = new TwinWithRelationships
            {
                Twin = new BasicDigitalTwin
                {
                    Id = buildingId,
                    Contents = new Dictionary<string, object>
                    {
                        { "siteID", siteId },
                    },
                },
            };

            var returnPage = new Page<TwinWithRelationships>();
            returnPage.Content = new List<TwinWithRelationships>
            {
                twinWithRelationships,
            };

            mockTwinsClient.Setup(t => t.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>())).ReturnsAsync(returnPage);

            var manager = new AdtApiGraphManager<IngestionManagerOptions>(mockLogger.Object,
                                                 someOptions,
                                                 mockTwinMappingIndexer.Object,
                                                 mockTwinsClient.Object,
                                                 mockRelationshipsClient.Object,
                                                 mockModelsClient.Object,
                                                 mockMeterFactory.Object,
                                                 mockMappingClient.Object,
                                                 new MetricsAttributesHelper(configuration));

            var result = await manager.GetSiteIdForBuilding(buildingId, CancellationToken.None);

            Assert.Equal(siteId, result);
        }

        [Fact]
        public async Task GetSiteIdForBuildingWithNoSiteId_ReturnsBuildingId()
        {
            const string buildingId = "1234";

            var twinWithRelationships = new TwinWithRelationships
            {
                Twin = new BasicDigitalTwin
                {
                    Id = buildingId,
                },
            };

            var returnPage = new Page<TwinWithRelationships>();
            returnPage.Content = new List<TwinWithRelationships>
            {
                twinWithRelationships,
            };

            mockTwinsClient.Setup(t => t.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>())).ReturnsAsync(returnPage);

            var manager = new AdtApiGraphManager<IngestionManagerOptions>(mockLogger.Object,
                                                 someOptions,
                                                 mockTwinMappingIndexer.Object,
                                                 mockTwinsClient.Object,
                                                 mockRelationshipsClient.Object,
                                                 mockModelsClient.Object,
                                                 mockMeterFactory.Object,
                                                 mockMappingClient.Object,
                                                 new MetricsAttributesHelper(configuration));

            var result = await manager.GetSiteIdForBuilding(buildingId, CancellationToken.None);

            Assert.Equal(buildingId, result);
        }

        [Fact]
        public async Task UploadGraph_NoData()
        {
            var twins = new Dictionary<string, BasicDigitalTwin>();
            var relationships = new Dictionary<string, BasicRelationship>();

            var manager = new AdtApiGraphManager<IngestionManagerOptions>(mockLogger.Object,
                                                 someOptions,
                                                 mockTwinMappingIndexer.Object,
                                                 mockTwinsClient.Object,
                                                 mockRelationshipsClient.Object,
                                                 mockModelsClient.Object,
                                                 mockMeterFactory.Object,
                                                 mockMappingClient.Object,
                                                 new MetricsAttributesHelper(configuration));

            await manager.UploadGraphAsync(twins, relationships, BuildingId, ConnectorId, false, CancellationToken.None);

            // Verify Twin Calls
            mockTwinsClient.Verify(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>()), Times.Never());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.IsAny<BasicDigitalTwin>(), null, It.IsAny<CancellationToken>()), Times.Never());

            // Verify Relationship Calls
            mockRelationshipsClient.Verify(x => x.GetRelationshipAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
            mockRelationshipsClient.Verify(x => x.UpsertRelationshipAsync(It.IsAny<BasicRelationship>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UploadGraph_1NewTwinAddedToMapping()
        {
            var twins = new Dictionary<string, BasicDigitalTwin>();
            var relationships = new Dictionary<string, BasicRelationship>();

            var newTwin = GetTwin("testtwinId");
            twins.Add(newTwin.Id, newTwin);

            var mappedEntryResponse = new MappedEntryResponse
            {
                Items = new List<MappedEntry> { },
            };

            mockMappingClient.Setup(x => x.GetMappedEntriesAsync(It.IsAny<MappedEntryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedEntryResponse);

            var returnPage = new Page<TwinWithRelationships>
            {
                Content = new List<TwinWithRelationships>(),
            };

            mockTwinsClient.Setup(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>())).ReturnsAsync(returnPage);

            var manager = new AdtApiGraphManager<IngestionManagerOptions>(mockLogger.Object,
                                                 someOptions,
                                                 mockTwinMappingIndexer.Object,
                                                 mockTwinsClient.Object,
                                                 mockRelationshipsClient.Object,
                                                 mockModelsClient.Object,
                                                 mockMeterFactory.Object,
                                                 mockMappingClient.Object,
                                                 new MetricsAttributesHelper(configuration));

            await manager.UploadGraphAsync(twins, relationships, BuildingId, ConnectorId, false, CancellationToken.None);

            // Verify Twin Calls
            mockTwinsClient.Verify(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.IsAny<BasicDigitalTwin>(), null, It.IsAny<CancellationToken>()), Times.Never());

            var createMappedEntry = new CreateMappedEntry
            {
                AuditInformation = null,
                ConnectorId = ConnectorId,
                Description = newTwin.Contents["description"].ToString(),
                MappedId = newTwin.Contents["externalID"].ToString(),
                MappedModelId = string.Empty,
                ModelInformation = null,
                Name = newTwin.Contents["name"].ToString(),
                ParentMappedId = string.Empty,
                ParentWillowId = string.Empty,
                Status = Status.Pending,
                StatusNotes = null,
                WillowId = newTwin.Id,
                WillowModelId = null,
                WillowParentRel = null,
                BuildingId = BuildingId,
            };

            // Since it is a new twin never seen before, it should be added to the Mappings Table
            mockMappingClient.Verify(x => x.CreateMappedEntryAsync(It.Is<CreateMappedEntry>(c => AreCreateMappedEntriesEqual(c, createMappedEntry)),
                                                                   It.IsAny<CancellationToken>()),
                                                                   Times.Once());

            // Verify Relationship Calls
            mockRelationshipsClient.Verify(x => x.GetRelationshipAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
            mockRelationshipsClient.Verify(x => x.UpsertRelationshipAsync(It.IsAny<BasicRelationship>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UploadGraph_1ExistingTwinPatchesTwin()
        {
            var twins = new Dictionary<string, BasicDigitalTwin>();
            var relationships = new Dictionary<string, BasicRelationship>();

            var existingTwin = GetTwin("testtwinId");

            var updatedTwin = GetTwin("testtwinId");

            // Add a new property to the twin to force a manual patch operation
            updatedTwin.Contents.Add("checkValue", "Test");

            // Add a new property to the twin to force a auto patch operation
            updatedTwin.Contents["externalID"] = "newTestExternalId";
            updatedTwin.Contents["alternateClassification"] = "newTestAlternateClassification";

            var mappedIds = new List<object>
            {
                new
                {
                    exactType = "ExternalIdentity",
                    scope = "ORG",
                    scopeId = "ORGYUULXQid",
                    value = "urn:test:testt6:asset:id:2230272",
                },
            };

            updatedTwin.Contents["mappedIds"] = mappedIds;

            var externalIds = new Dictionary<string, string>
            {
                { "extId1", "newTestExternalId1" },
                { "extId2", "newTestExternalId2" },
            };

            updatedTwin.Contents["externalIds"] = externalIds;

            updatedTwin.Contents["mappedConnectorId"] = "newMappedConnectorId";
            updatedTwin.Contents["stateText"] = "newStateText";
            updatedTwin.Contents["valueMap"] = "newValueMap";

            twins.Add(updatedTwin.Id, updatedTwin);

            var mappedEntryResponse = new MappedEntryResponse
            {
                Items = new List<MappedEntry> { },
            };

            mockMappingClient.Setup(x => x.GetMappedEntriesAsync(It.IsAny<MappedEntryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedEntryResponse);

            var returnPage = new Page<TwinWithRelationships>
            {
                Content = new List<TwinWithRelationships>()
                {
                    new TwinWithRelationships()
                    {
                        Twin = existingTwin,
                    },
                },
            };

            mockTwinsClient.Setup(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>())).ReturnsAsync(returnPage);

            var manager = new AdtApiGraphManager<IngestionManagerOptions>(mockLogger.Object,
                                                 someOptions,
                                                 mockTwinMappingIndexer.Object,
                                                 mockTwinsClient.Object,
                                                 mockRelationshipsClient.Object,
                                                 mockModelsClient.Object,
                                                 mockMeterFactory.Object,
                                                 mockMappingClient.Object,
                                                 new MetricsAttributesHelper(configuration));

            await manager.UploadGraphAsync(twins, relationships, BuildingId, ConnectorId, false, CancellationToken.None);

            // Verify Twin Calls
            mockTwinsClient.Verify(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.Is<BasicDigitalTwin>(x => x.Id == existingTwin.Id), null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.Is<BasicDigitalTwin>(x => x.Id == existingTwin.Id && x.Contents["externalID"] != null), null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.Is<BasicDigitalTwin>(x => x.Id == existingTwin.Id && x.Contents["externalID"].ToString() == "newTestExternalId"), null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.Is<BasicDigitalTwin>(x => x.Id == existingTwin.Id && x.Contents["alternateClassification"].ToString() == "newTestAlternateClassification"), null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.Is<BasicDigitalTwin>(x => x.Id == existingTwin.Id && x.Contents["mappedConnectorId"].ToString() == "newMappedConnectorId"), null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.Is<BasicDigitalTwin>(x => x.Id == existingTwin.Id && x.Contents["stateText"].ToString() == "newStateText"), null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.Is<BasicDigitalTwin>(x => x.Id == existingTwin.Id && x.Contents["externalIds"] != null), null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.Is<BasicDigitalTwin>(x => x.Id == existingTwin.Id && x.Contents["mappedIds"] != null), null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.Is<BasicDigitalTwin>(x => x.Id == existingTwin.Id && x.Contents["valueMap"] != null), null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.PatchTwinAsync(updatedTwin.Id, It.IsAny<IEnumerable<Operation>>(), null, It.IsAny<CancellationToken>()), Times.Never());

            // Since it is a update of an existing twin, it should not be added to the Mappings Table
            mockMappingClient.Verify(x => x.CreateMappedEntryAsync(It.IsAny<CreateMappedEntry>(),
                                                                   It.IsAny<CancellationToken>()),
                                                                   Times.Never());

            // Verify Relationship Calls
            mockRelationshipsClient.Verify(x => x.GetRelationshipAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
            mockRelationshipsClient.Verify(x => x.UpsertRelationshipAsync(It.IsAny<BasicRelationship>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UploadGraph_1ExistingTwinNoChangesDoesNotPatchTwin()
        {
            var twins = new Dictionary<string, BasicDigitalTwin>();
            var relationships = new Dictionary<string, BasicRelationship>();

            var existingTwin = GetTwin("testtwinId");

            var updatedTwin = GetTwin("testtwinId");
            twins.Add(updatedTwin.Id, updatedTwin);

            var mappedEntryResponse = new MappedEntryResponse
            {
                Items = new List<MappedEntry> { },
            };

            mockMappingClient.Setup(x => x.GetMappedEntriesAsync(It.IsAny<MappedEntryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedEntryResponse);

            var returnPage = new Page<TwinWithRelationships>
            {
                Content = new List<TwinWithRelationships>()
                {
                    new TwinWithRelationships()
                    {
                        Twin = existingTwin,
                    },
                },
            };

            mockTwinsClient.Setup(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>())).ReturnsAsync(returnPage);

            var manager = new AdtApiGraphManager<IngestionManagerOptions>(mockLogger.Object,
                                                 someOptions,
                                                 mockTwinMappingIndexer.Object,
                                                 mockTwinsClient.Object,
                                                 mockRelationshipsClient.Object,
                                                 mockModelsClient.Object,
                                                 mockMeterFactory.Object,
                                                 mockMappingClient.Object,
                                                 new MetricsAttributesHelper(configuration));

            await manager.UploadGraphAsync(twins, relationships, BuildingId, ConnectorId, false, CancellationToken.None);

            // Verify Twin Calls
            mockTwinsClient.Verify(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.IsAny<BasicDigitalTwin>(), null, It.IsAny<CancellationToken>()), Times.Never());
            mockTwinsClient.Verify(x => x.PatchTwinAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Operation>>(), null, It.IsAny<CancellationToken>()), Times.Never());

            // Since it is a update of an existing twin, it should not be added to the Mappings Table
            mockMappingClient.Verify(x => x.CreateMappedEntryAsync(It.IsAny<CreateMappedEntry>(),
                                                                   It.IsAny<CancellationToken>()),
                                                                   Times.Never());

            // Verify Relationship Calls
            mockRelationshipsClient.Verify(x => x.GetRelationshipAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
            mockRelationshipsClient.Verify(x => x.UpsertRelationshipAsync(It.IsAny<BasicRelationship>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UploadGraph_1MappedTwinDoesNotPatchTwin()
        {
            var twins = new Dictionary<string, BasicDigitalTwin>();
            var relationships = new Dictionary<string, BasicRelationship>();

            var existingTwin = GetTwin("testtwinId");

            var updatedTwin = GetTwin("testtwinId");
            twins.Add(updatedTwin.Id, updatedTwin);

            var mappedEntry = new MappedEntry
            {
                AuditInformation = null,
                ConnectorId = string.Empty,
                Description = existingTwin.Contents["description"].ToString(),
                MappedId = existingTwin.Contents["externalID"].ToString(),
                MappedModelId = string.Empty,
                ModelInformation = null,
                Name = existingTwin.Contents["name"].ToString(),
                ParentMappedId = string.Empty,
                ParentWillowId = string.Empty,
                Status = Status.Pending,
                StatusNotes = null,
                WillowId = null,
                WillowModelId = null,
                WillowParentRel = null,
            };

            var mappedEntryResponse = new MappedEntryResponse
            {
                Items = new List<MappedEntry> { mappedEntry },
            };

            var emptyMappedEntryResponse = new MappedEntryResponse
            {
                Items = new List<MappedEntry>(),
            };

            mockMappingClient.SetupSequence(x => x.GetMappedEntriesAsync(It.IsAny<MappedEntryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedEntryResponse)
                .ReturnsAsync(emptyMappedEntryResponse);

            var returnPage = new Page<TwinWithRelationships>
            {
                Content = new List<TwinWithRelationships>(),
            };

            mockTwinsClient.Setup(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>())).ReturnsAsync(returnPage);

            var manager = new AdtApiGraphManager<IngestionManagerOptions>(mockLogger.Object,
                                                 someOptions,
                                                 mockTwinMappingIndexer.Object,
                                                 mockTwinsClient.Object,
                                                 mockRelationshipsClient.Object,
                                                 mockModelsClient.Object,
                                                 mockMeterFactory.Object,
                                                 mockMappingClient.Object,
                                                 new MetricsAttributesHelper(configuration));

            await manager.UploadGraphAsync(twins, relationships, BuildingId, ConnectorId, false, CancellationToken.None);

            // Verify Twin Calls
            mockTwinsClient.Verify(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.IsAny<BasicDigitalTwin>(), null, It.IsAny<CancellationToken>()), Times.Never());
            mockTwinsClient.Verify(x => x.PatchTwinAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Operation>>(), null, It.IsAny<CancellationToken>()), Times.Never());

            // Since it is a update of an existing twin, it should not be added to the Mappings Table
            mockMappingClient.Verify(x => x.CreateMappedEntryAsync(It.IsAny<CreateMappedEntry>(),
                                                                   It.IsAny<CancellationToken>()),
                                                                   Times.Never());

            // Since it is a update of an existing mapping entry that is still pending, it should do an update of the Mapped entry
            mockMappingClient.Verify(x => x.UpdateMappedEntryAsync(It.IsAny<UpdateMappedEntry>(),
                                                                   It.IsAny<CancellationToken>()),
                                                                   Times.Once());

            // Verify Relationship Calls
            mockRelationshipsClient.Verify(x => x.GetRelationshipAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
            mockRelationshipsClient.Verify(x => x.UpsertRelationshipAsync(It.IsAny<BasicRelationship>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UploadGraph_1MappedTwinDoesNotUpdateIgnoredMapping()
        {
            var twins = new Dictionary<string, BasicDigitalTwin>();
            var relationships = new Dictionary<string, BasicRelationship>();

            var existingTwin = GetTwin("testtwinId");

            var updatedTwin = GetTwin("testtwinId");
            twins.Add(updatedTwin.Id, updatedTwin);

            var mappedEntry = new MappedEntry
            {
                AuditInformation = null,
                ConnectorId = string.Empty,
                Description = existingTwin.Contents["description"].ToString(),
                MappedId = existingTwin.Contents["externalID"].ToString(),
                MappedModelId = string.Empty,
                ModelInformation = null,
                Name = existingTwin.Contents["name"].ToString(),
                ParentMappedId = string.Empty,
                ParentWillowId = string.Empty,
                Status = Status.Ignore,
                StatusNotes = null,
                WillowId = null,
                WillowModelId = null,
                WillowParentRel = null,
            };

            var mappedEntryResponse = new MappedEntryResponse
            {
                Items = new List<MappedEntry> { mappedEntry },
            };

            var emptyMappedEntryResponse = new MappedEntryResponse
            {
                Items = new List<MappedEntry>(),
            };

            mockMappingClient.SetupSequence(x => x.GetMappedEntriesAsync(It.IsAny<MappedEntryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedEntryResponse)
                .ReturnsAsync(emptyMappedEntryResponse);

            var returnPage = new Page<TwinWithRelationships>
            {
                Content = new List<TwinWithRelationships>(),
            };

            mockTwinsClient.Setup(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>())).ReturnsAsync(returnPage);

            var manager = new AdtApiGraphManager<IngestionManagerOptions>(mockLogger.Object,
                                                 someOptions,
                                                 mockTwinMappingIndexer.Object,
                                                 mockTwinsClient.Object,
                                                 mockRelationshipsClient.Object,
                                                 mockModelsClient.Object,
                                                 mockMeterFactory.Object,
                                                 mockMappingClient.Object,
                                                 new MetricsAttributesHelper(configuration));

            await manager.UploadGraphAsync(twins, relationships, BuildingId, ConnectorId, false, CancellationToken.None);

            // Verify Twin Calls
            mockTwinsClient.Verify(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.IsAny<BasicDigitalTwin>(), null, It.IsAny<CancellationToken>()), Times.Never());
            mockTwinsClient.Verify(x => x.PatchTwinAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Operation>>(), null, It.IsAny<CancellationToken>()), Times.Never());

            // Since it is a update of an existing twin, it should not be added to the Mappings Table
            mockMappingClient.Verify(x => x.CreateMappedEntryAsync(It.IsAny<CreateMappedEntry>(),
                                                                   It.IsAny<CancellationToken>()),
                                                                   Times.Never());

            // Since it is a update of an existing mapping entry that is still pending, it should do an update of the Mapped entry
            mockMappingClient.Verify(x => x.UpdateMappedEntryAsync(It.IsAny<UpdateMappedEntry>(),
                                                                   It.IsAny<CancellationToken>()),
                                                                   Times.Never);

            // Verify Relationship Calls
            mockRelationshipsClient.Verify(x => x.GetRelationshipAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
            mockRelationshipsClient.Verify(x => x.UpsertRelationshipAsync(It.IsAny<BasicRelationship>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UploadGraph_1ApprovedMappedRecordCreatesTwin()
        {
            var twins = new Dictionary<string, BasicDigitalTwin>();
            var relationships = new Dictionary<string, BasicRelationship>();

            var existingTwin = GetTwin("testtwinId");

            var updatedTwin = GetTwin("testtwinId");
            twins.Add(updatedTwin.Id, updatedTwin);

            var mappedEntry = new MappedEntry
            {
                AuditInformation = null,
                ConnectorId = string.Empty,
                Description = existingTwin.Contents["description"].ToString(),
                MappedId = existingTwin.Contents["externalID"].ToString(),
                MappedModelId = string.Empty,
                ModelInformation = null,
                Name = existingTwin.Contents["name"].ToString(),
                ParentMappedId = string.Empty,
                ParentWillowId = string.Empty,
                Status = Status.Approved,
                StatusNotes = null,
                WillowId = null,
                WillowModelId = null,
                WillowParentRel = null,
            };

            var mappedEntryResponse = new MappedEntryResponse
            {
                Items = new List<MappedEntry> { mappedEntry },
            };

            var emptyMappedEntryResponse = new MappedEntryResponse
            {
                Items = new List<MappedEntry>(),
            };

            mockMappingClient.SetupSequence(x => x.GetMappedEntriesAsync(It.IsAny<MappedEntryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedEntryResponse)
                .ReturnsAsync(emptyMappedEntryResponse);

            var returnPage = new Page<TwinWithRelationships>
            {
                Content = new List<TwinWithRelationships>(),
            };

            mockTwinsClient.Setup(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>())).ReturnsAsync(returnPage);

            var manager = new AdtApiGraphManager<IngestionManagerOptions>(mockLogger.Object,
                                                 someOptions,
                                                 mockTwinMappingIndexer.Object,
                                                 mockTwinsClient.Object,
                                                 mockRelationshipsClient.Object,
                                                 mockModelsClient.Object,
                                                 mockMeterFactory.Object,
                                                 mockMappingClient.Object,
                                                 new MetricsAttributesHelper(configuration));

            await manager.UploadGraphAsync(twins, relationships, BuildingId, ConnectorId, false, CancellationToken.None);

            // Verify Twin Calls
            mockTwinsClient.Verify(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.IsAny<BasicDigitalTwin>(), null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.PatchTwinAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Operation>>(), null, It.IsAny<CancellationToken>()), Times.Never());

            // Since it is a update of an existing twin, it should not be added to the Mappings Table
            mockMappingClient.Verify(x => x.CreateMappedEntryAsync(It.IsAny<CreateMappedEntry>(),
                                                                   It.IsAny<CancellationToken>()),
                                                                   Times.Never());

            // Since it is a update of an existing mapping entry that is still pending, it should do an update of the Mapped entry
            mockMappingClient.Verify(x => x.UpdateMappedEntryAsync(It.IsAny<UpdateMappedEntry>(),
                                                                   It.IsAny<CancellationToken>()),
                                                                   Times.Once);

            // Verify Relationship Calls
            mockRelationshipsClient.Verify(x => x.GetRelationshipAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
            mockRelationshipsClient.Verify(x => x.UpsertRelationshipAsync(It.IsAny<BasicRelationship>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UploadGraph_1ApprovedMappedRecordCreatesNewTwin()
        {
            var twins = new Dictionary<string, BasicDigitalTwin>();
            var relationships = new Dictionary<string, BasicRelationship>();

            var existingTwin = GetTwin("testtwinId");

            var updatedTwin = GetTwin("testtwinId");
            twins.Add(updatedTwin.Id, updatedTwin);

            var dupExistingTwinId = existingTwin.Contents["externalID"].ToString() ?? string.Empty;

            var mappedEntry = new MappedEntry
            {
                AuditInformation = null,
                ConnectorId = string.Empty,
                Description = existingTwin.Contents["description"].ToString(),
                MappedId = existingTwin.Contents["externalID"].ToString(),
                MappedModelId = string.Empty,
                ModelInformation = null,
                Name = existingTwin.Contents["name"].ToString(),
                ParentMappedId = string.Empty,
                ParentWillowId = string.Empty,
                Status = Status.Approved,
                StatusNotes = null,
                WillowId = null,
                WillowModelId = null,
                WillowParentRel = null,
            };

            var mappedEntryResponse = new MappedEntryResponse
            {
                Items = new List<MappedEntry> { mappedEntry },
            };

            var emptyMappedEntryResponse = new MappedEntryResponse
            {
                Items = new List<MappedEntry>(),
            };

            mockMappingClient.SetupSequence(x => x.GetMappedEntriesAsync(It.IsAny<MappedEntryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedEntryResponse)
                .ReturnsAsync(emptyMappedEntryResponse);

            var returnPage = new Page<TwinWithRelationships>
            {
                Content = new List<TwinWithRelationships>(),
            };

            var dupeReturnTwin = new TwinWithRelationships() { Twin = GetTwin(dupExistingTwinId) };

            mockTwinsClient.Setup(x => x.GetTwinByIdAsync(dupExistingTwinId, null, null, It.IsAny<CancellationToken>())).ReturnsAsync(dupeReturnTwin);
            mockTwinsClient.Setup(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>())).ReturnsAsync(returnPage);
            someOptions.Value.EnableTwinReplace = true;

            var manager = new AdtApiGraphManager<IngestionManagerOptions>(mockLogger.Object,
                                                 someOptions,
                                                 mockTwinMappingIndexer.Object,
                                                 mockTwinsClient.Object,
                                                 mockRelationshipsClient.Object,
                                                 mockModelsClient.Object,
                                                 mockMeterFactory.Object,
                                                 mockMappingClient.Object,
                                                 new MetricsAttributesHelper(configuration));

            await manager.UploadGraphAsync(twins, relationships, BuildingId, ConnectorId, false, CancellationToken.None);

            // Verify Twin Calls
            mockTwinsClient.Verify(x => x.DeleteTwinsAndRelationshipsAsync(new List<string>() { dupExistingTwinId }, true, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.GetTwinsByIdsAsync(It.IsAny<IEnumerable<string>>(), null, null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.UpdateTwinAsync(It.IsAny<BasicDigitalTwin>(), null, It.IsAny<CancellationToken>()), Times.Once());
            mockTwinsClient.Verify(x => x.PatchTwinAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Operation>>(), null, It.IsAny<CancellationToken>()), Times.Never());

            // Since it is a update of an existing twin, it should not be added to the Mappings Table
            mockMappingClient.Verify(x => x.CreateMappedEntryAsync(It.IsAny<CreateMappedEntry>(),
                                                                   It.IsAny<CancellationToken>()),
                                                                   Times.Never());

            // Since it is a update of an existing mapping entry that is still pending, it should do an update of the Mapped entry
            mockMappingClient.Verify(x => x.UpdateMappedEntryAsync(It.IsAny<UpdateMappedEntry>(),
                                                                   It.IsAny<CancellationToken>()),
                                                                   Times.Once);

            // Verify Relationship Calls
            mockRelationshipsClient.Verify(x => x.GetRelationshipAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
            mockRelationshipsClient.Verify(x => x.UpsertRelationshipAsync(It.IsAny<BasicRelationship>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        private static bool AreCreateMappedEntriesEqual(CreateMappedEntry a, CreateMappedEntry b)
        {
            return a.AuditInformation == b.AuditInformation &&
                   a.ConnectorId == b.ConnectorId &&
                   a.Description == b.Description &&
                   a.MappedId == b.MappedId &&
                   a.MappedModelId == b.MappedModelId &&
                   a.ModelInformation == b.ModelInformation &&
                   a.Name == b.Name &&
                   a.ParentMappedId == b.ParentMappedId &&
                   a.ParentWillowId == b.ParentWillowId &&
                   a.Status == b.Status &&
                   a.StatusNotes == b.StatusNotes &&
                   a.WillowId == b.WillowId &&
                   a.WillowModelId == b.WillowModelId &&
                   a.WillowParentRel == b.WillowParentRel;
        }

        private BasicDigitalTwin GetTwin(string id)
        {
            return new BasicDigitalTwin
            {
                Id = id,
                Contents = new Dictionary<string, object>()
                    {
                        { "siteID", SiteId },
                        { "description", "test twin description" },
                        { "name", "test twin name" },
                        { "externalID", "test mapped id" },
                    },
            };
        }
    }
}
