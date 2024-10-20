// -----------------------------------------------------------------------
// <copyright file="MappedGraphIngestionProcessorTests.cs" Company="Willow">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.TopologyIngestion.Mapped.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.DigitalTwins.Core;
    using Azure.Security.KeyVault.Secrets;
    using DTDLParser;
    using global::Mapped.Ontologies.Mappings.OntologyMapper;
    using global::Mapped.Ontologies.Mappings.OntologyMapper.Mapped;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Willow.Security.KeyVault;
    using Willow.Telemetry;
    using Willow.TopologyIngestion.Interfaces;
    using Xunit;
    using Xunit.Abstractions;

    public class MappedGraphIngestionProcessorTests
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ITestOutputHelper output;
#pragma warning restore IDE0052 // Remove unread private members

        private readonly Mock<ILogger<MockedIngestionProcessor<IngestionManagerOptions>>> mockLogger;
        private readonly Mock<IInputGraphManager> mockInputGraphManager;
        private readonly Mock<IOutputGraphManager> mockOutputGraphManager;
        private readonly OntologyMappingManager ontologyMappingManager;
        private readonly IOptions<IngestionManagerOptions> mockOptions;
        private readonly Mock<IMeterFactory> mockMeterFactory;
        private readonly IConfiguration configuration;
        private readonly Mock<ISecretManager> mockSecretManager;

        public MappedGraphIngestionProcessorTests(ITestOutputHelper output)
        {
            this.output = output;

            mockLogger = new Mock<ILogger<MockedIngestionProcessor<IngestionManagerOptions>>>();

            mockSecretManager = new Mock<ISecretManager>();
            var secret = new KeyVaultSecret("MappedToken", "Test");
            mockSecretManager.Setup(sm => sm.GetSecretAsync("MappedToken")).ReturnsAsync(secret);

            mockInputGraphManager = new Mock<IInputGraphManager>();

            var spaceDtmi = "dtmi:org:w3id:rec:Space;1";
            var spaceWithBoxDtmi = "dtmi:org:w3id:rec:SpaceWithBox;1";
            var spaceWithUnitDtmi = "dtmi:org:w3id:rec:SpaceWithUnit;1";

            mockInputGraphManager.Setup(m => m.TryGetDtmi("Space", out spaceDtmi)).Returns(true);
            mockInputGraphManager.Setup(m => m.TryGetDtmi("SpaceWithBox", out spaceWithBoxDtmi)).Returns(true);
            mockInputGraphManager.Setup(m => m.TryGetDtmi("SpaceWithUnit", out spaceWithUnitDtmi)).Returns(true);

            mockOptions = Options.Create(new MappedIngestionManagerOptions());

            mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            mockOutputGraphManager = new Mock<IOutputGraphManager>();
            mockOutputGraphManager.Setup(m => m.GetSiteIdForBuilding(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("xxxxxx");
            mockOutputGraphManager.Setup(m => m.Errors).Returns(new Dictionary<string, string>());

            var listOfDtdlFiles = new List<string>
                {
                    "TopologyIngestion.Test.TestData.Box.json",
                    "TopologyIngestion.Test.TestData.Space.json",
                    "TopologyIngestion.Test.TestData.SpaceWithBox.json",
                    "TopologyIngestion.Test.TestData.SpaceWithUnit.json",
                };

            mockOutputGraphManager.Setup(m => m.GetModelsAsync(CancellationToken.None)).ReturnsAsync(LoadDtdl(listOfDtdlFiles));

            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMappingAsync()).ReturnsAsync(GetTestMappings);

            ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            configuration = new ConfigurationBuilder()
                .Build();
        }

        [Fact]
        public void GetInputInterfaceDtmi_ReturnsNull_WhenNotFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            var dtmi = mockedIngestionProcessor.TestInputInterfaceDtmi("invalidType");
            Assert.Null(dtmi);
        }

        [Fact]
        public void GetInputInterfaceDtmi_ReturnsDtmi_WhenFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            var dtmi = mockedIngestionProcessor.TestInputInterfaceDtmi("Space");
            Assert.NotNull(dtmi);
        }

        [Fact]
        public void GetOutputRelationshipType_ReturnsString_WhenFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            var expectedRelationshipType = "isA";
            var outputRelationshipType = mockedIngestionProcessor.TestGetOutputRelationshipType("hasA");

            Assert.Equal(expectedRelationshipType, outputRelationshipType);
        }

        [Fact]
        public void GetOutputRelationshipType_ReturnsInputValue_WhenNotFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            var inputRelationshipType = "wasA";
            var outputRelationshipType = mockedIngestionProcessor.TestGetOutputRelationshipType(inputRelationshipType);
            Assert.Equal(inputRelationshipType, outputRelationshipType);
        }

        [Fact]
        public async Task TryGetOutputInterfaceDtmi_ReturnsValue_WhenFoundInTargetOntology()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            var inputDtmi = new Dtmi("dtmi:org:w3id:rec:Space;1");
            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);

            var result = mockedIngestionProcessor.TestTryGetOutputInterfaceDtmi(inputDtmi, out var outputDtmi);
            Assert.True(result);
            Assert.NotNull(outputDtmi);
            Assert.Equal(inputDtmi.ToString(), outputDtmi?.ToString());
        }

        [Fact]
        public async Task TryGetOutputInterfaceDtmi_ReturnsValue_WhenFoundInInterfaceRemap()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            var inputDtmi = new Dtmi("dtmi:twin:main:CleaningRoom;1");
            var expectedOutputInterface = new Dtmi("dtmi:org:w3id:rec:Space;1");
            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);

            var result = mockedIngestionProcessor.TestTryGetOutputInterfaceDtmi(inputDtmi, out var outputDtmi);
            Assert.True(result);
            Assert.NotNull(outputDtmi);
            Assert.Equal(expectedOutputInterface.ToString(), outputDtmi?.ToString());
        }

        [Fact]
        public async Task TryGetOutputInterfaceDtmi_ReturnsNull_WhenNotFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            var inputDtmi = new Dtmi("dtmi:twin:main:SpaceShip;1");
            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);

            var result = mockedIngestionProcessor.TestTryGetOutputInterfaceDtmi(inputDtmi, out var outputDtmi);
            Assert.False(result);
            Assert.Null(outputDtmi);
        }

        [Fact]
        public async Task GetTwin_ReturnsNull_WhenInputInterfaceNotFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);
            Dictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElement();
            string basicDtId = string.Empty;
            string interfaceType = "SpaceShip";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTwin_ReturnsNull_WhenOutputInterfaceNotFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);
            Dictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElement();
            string basicDtId = string.Empty;
            string interfaceType = "Box";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTwin_ReturnsDtmi_WhenOutputMappingFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            var expectedDtmi = "dtmi:org:w3id:rec:Space;1";
            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);
            Dictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElement();
            string basicDtId = "CLSKkDFMgbojZZ54MorD6B11R";
            string interfaceType = "Space";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);
            Assert.NotNull(result);
            Assert.Equal(expectedDtmi, result?.ToString());
            Assert.Single(twins);
            Assert.Equal(basicDtId, twins.First().Key);
            Assert.Equal(basicDtId, twins.First().Value.Id);
            Assert.Equal(expectedDtmi, twins.First().Value.Metadata.ModelId);
            Assert.Equal(2, twins.First().Value.Contents.Count);
            Assert.Equal("AV 31", twins.First().Value.Contents["name"].ToString());
        }

        [Fact]
        public async Task GetTwin_FillsProperty_WhenFillPropertySpecified()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            var expectedDtmi = "dtmi:org:w3id:rec:Space;1";
            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);
            Dictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElementWithEmptyName();
            string basicDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            string interfaceType = "Space";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);
            Assert.NotNull(result);
            Assert.Equal(expectedDtmi, result?.ToString());
            Assert.Single(twins);
            Assert.Equal(basicDtId, twins.First().Key);
            Assert.Equal(basicDtId, twins.First().Value.Id);
            Assert.Equal(expectedDtmi, twins.First().Value.Metadata.ModelId);
            Assert.Equal(2, twins.First().Value.Contents.Count);
            Assert.Equal("test", twins.First().Value.Contents["name"].ToString());
        }

        [Fact]
        public async Task GetTwin_FillsComponent_WhenComponentNotSpecified()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            var expectedDtmi = "dtmi:org:w3id:rec:SpaceWithBox;1";
            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);
            Dictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElementWithEmptyName();
            string basicDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            string interfaceType = "SpaceWithBox";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);
            Assert.NotNull(result);
            Assert.Equal(expectedDtmi, result?.ToString());
            Assert.Single(twins);
            Assert.Equal(basicDtId, twins.First().Key);
            Assert.Equal(basicDtId, twins.First().Value.Id);
            Assert.Equal(expectedDtmi, twins.First().Value.Metadata.ModelId);
            Assert.Equal(3, twins.First().Value.Contents.Count);
            Assert.Equal("test", twins.First().Value.Contents["name"].ToString());
            Assert.Equal("{ \"$metadata\": {} }", twins.First().Value.Contents["box"].ToString());
        }

        [Fact]
        public async Task GetTwin_ProjectsProperty_WhenPropertyProjectionSpecified()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            var expectedDtmi = "dtmi:org:w3id:rec:Space;1";
            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);
            Dictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElementWithMappingKey();
            string basicDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            string interfaceType = "Space";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);
            Assert.NotNull(result);
            Assert.Equal(expectedDtmi, result?.ToString());
            Assert.Single(twins);
            Assert.Equal(basicDtId, twins.First().Key);
            Assert.Equal(basicDtId, twins.First().Value.Id);
            Assert.Equal(expectedDtmi, twins.First().Value.Metadata.ModelId);
            Assert.Equal(3, twins.First().Value.Contents.Count);

            var contents = twins.First().Value.Contents as IDictionary<string, object>;
            var externalIds = contents["externalIds"] as Dictionary<string, string>;
            Assert.NotNull(externalIds);
            Assert.Equal("12345", externalIds?["mappingKey"]);
        }

        [Fact]
        public async Task GetTwin_ProjectsProperty_WhenPropertyProjectionForMultiplePropertiesSpecified()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            var expectedDtmi = "dtmi:org:w3id:rec:Space;1";
            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);
            Dictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElementWithMultipleKeys();
            string basicDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            string interfaceType = "Space";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);
            Assert.NotNull(result);
            Assert.Equal(expectedDtmi, result?.ToString());
            Assert.Single(twins);
            Assert.Equal(basicDtId, twins.First().Key);
            Assert.Equal(basicDtId, twins.First().Value.Id);
            Assert.Equal(expectedDtmi, twins.First().Value.Metadata.ModelId);
            Assert.Equal(3, twins.First().Value.Contents.Count);

            var contents = twins.First().Value.Contents as IDictionary<string, object>;
            var externalIds = contents["externalIds"] as Dictionary<string, string>;
            Assert.NotNull(externalIds);
            Assert.Equal("12345", externalIds?["mappingKey"]);
            Assert.Equal("678", externalIds?["deviceId"]);
        }

        [Fact]
        public async Task GetTwin_ObjectTransform_WhenValidObjectTransformSpecified()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            var expectedDtmi = "dtmi:org:w3id:rec:SpaceWithUnit;1";
            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);
            Dictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElementWithMultipleKeys();
            string basicDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            string interfaceType = "SpaceWithUnit";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);

            Assert.NotNull(result);
            Assert.Equal(expectedDtmi, result?.ToString());
            Assert.Single(twins);
            Assert.Equal(basicDtId, twins.First().Key);
            Assert.Equal(basicDtId, twins.First().Value.Id);
            Assert.Equal(expectedDtmi, twins.First().Value.Metadata.ModelId);
            Assert.Equal(4, twins.First().Value.Contents.Count);

            var contents = twins.First().Value.Contents as IDictionary<string, object>;
            var externalIds = contents["externalIds"] as Dictionary<string, string>;
            Assert.NotNull(externalIds);
            Assert.Equal("12345", externalIds?["mappingKey"]);
            Assert.Equal("678", externalIds?["deviceId"]);
            Assert.Equal("A", contents["unit"]);
        }

        [Fact]
        public async Task GetTwin_ObjectTransform_WhenNullObjectSpecified()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            var expectedDtmi = "dtmi:org:w3id:rec:SpaceWithUnit;1";
            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);
            Dictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElementWithNullUnit();
            string basicDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            string interfaceType = "SpaceWithUnit";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);

            Assert.NotNull(result);
            Assert.Equal(expectedDtmi, result?.ToString());
            Assert.Single(twins);
            Assert.Equal(basicDtId, twins.First().Key);
            Assert.Equal(basicDtId, twins.First().Value.Id);
            Assert.Equal(expectedDtmi, twins.First().Value.Metadata.ModelId);
            Assert.Equal(3, twins.First().Value.Contents.Count);

            var contents = twins.First().Value.Contents as IDictionary<string, object>;
            var externalIds = contents["externalIds"] as Dictionary<string, string>;
            Assert.NotNull(externalIds);
            Assert.Equal("12345", externalIds?["mappingKey"]);
            Assert.Equal("678", externalIds?["deviceId"]);
            Assert.False(contents.TryGetValue("unit", out _));
        }

        [Fact]
        public async Task GetRelationship_GetsRelationship_WhenValidRelationship()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            Dictionary<string, BasicRelationship> relationships = new Dictionary<string, BasicRelationship>();
            string sourceDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            var inputSourceDtmi = new Dtmi("dtmi:org:w3id:rec:Space;1");
            var inputRelationshipType = "isLocationOf";
            string targetDtId = "CLSKkDFMgbojZZ54MorD6B11R";
            string targetInterfaceType = "Space";
            Dictionary<string, object> relationshipProperties = new Dictionary<string, object>
            {
                { "testKey", "testValue" },
            };

            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);

            mockedIngestionProcessor.TestGetRelationship(relationships, sourceDtId, inputSourceDtmi, inputRelationshipType, targetDtId, targetInterfaceType, relationshipProperties);

            Assert.Single(relationships);
            var outputRelationship = relationships.First();

            Assert.NotNull(outputRelationship.Value);
            Assert.Equal(inputRelationshipType, outputRelationship.Value.Name);
            Assert.Equal(sourceDtId, outputRelationship.Value.SourceId);
            Assert.Equal(targetDtId, outputRelationship.Value.TargetId);
            Assert.Equal($"{sourceDtId}-{targetDtId}-{outputRelationship.Value.Name}", outputRelationship.Value.Id);
            Assert.NotNull(outputRelationship.Value.Id);
            Assert.Equal("testKey", outputRelationship.Value.Properties.First().Key);
            Assert.Equal("testValue", outputRelationship.Value.Properties.First().Value);
        }

        [Fact]
        public async Task GetRelationship_GetsRelationship_WhenRemappedRelationship()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            Dictionary<string, BasicRelationship> relationships = new Dictionary<string, BasicRelationship>();
            string sourceDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            var inputSourceDtmi = new Dtmi("dtmi:org:w3id:rec:Space;1");
            var inputRelationshipType = "hasA";
            string targetDtId = "CLSKkDFMgbojZZ54MorD6B11R";
            string targetInterfaceType = "Space";
            Dictionary<string, object> relationshipProperties = new Dictionary<string, object>();

            var expectedRelationshipType = "isA";

            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);

            mockedIngestionProcessor.TestGetRelationship(relationships, sourceDtId, inputSourceDtmi, inputRelationshipType, targetDtId, targetInterfaceType, relationshipProperties);

            Assert.Single(relationships);
            var outputRelationship = relationships.First();

            Assert.NotNull(outputRelationship.Value);
            Assert.Equal(expectedRelationshipType, outputRelationship.Value.Name);
            Assert.Equal(sourceDtId, outputRelationship.Value.SourceId);
            Assert.Equal(targetDtId, outputRelationship.Value.TargetId);
            Assert.Equal($"{sourceDtId}-{targetDtId}-{outputRelationship.Value.Name}", outputRelationship.Value.Id);
            Assert.NotNull(outputRelationship.Value.Id);
        }

        [Fact]
        public async Task GetRelationship_GetsRelationship_WhenRemappedRelationshipReversed()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object,
                                                                        new DefaultGraphNamingManager(),
                                                                        mockOptions,
                                                                        mockMeterFactory.Object,
                                                                        new MetricsAttributesHelper(configuration));

            Dictionary<string, BasicRelationship> relationships = new Dictionary<string, BasicRelationship>();
            string sourceDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            var inputSourceDtmi = new Dtmi("dtmi:org:w3id:rec:Space;1");
            var inputRelationshipType = "hasPoint";
            string targetDtId = "CLSKkDFMgbojZZ54MorD6B11R";
            string targetInterfaceType = "Space";
            Dictionary<string, object> relationshipProperties = new Dictionary<string, object>();

            var expectedRelationshipType = "isPointOf";

            await mockedIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);

            mockedIngestionProcessor.TestGetRelationship(relationships, sourceDtId, inputSourceDtmi, inputRelationshipType, targetDtId, targetInterfaceType, relationshipProperties);

            Assert.Single(relationships);
            var outputRelationship = relationships.First();

            Assert.NotNull(outputRelationship.Value);
            Assert.Equal(expectedRelationshipType, outputRelationship.Value.Name);
            Assert.Equal(sourceDtId, outputRelationship.Value.TargetId);
            Assert.Equal(targetDtId, outputRelationship.Value.SourceId);
            Assert.Equal($"{targetDtId}-{sourceDtId}-{outputRelationship.Value.Name}", outputRelationship.Value.Id);
            Assert.NotNull(outputRelationship.Value.Id);
        }

        [Fact]
        public async Task IngestFromApi_NoData()
        {
            var mockLogger = new Mock<ILogger<MappedGraphIngestionProcessor<IngestionManagerOptions>>>();

            var mockInputGraphManager = new Mock<IInputGraphManager>();

            var mockOutputGraphManager = new Mock<IOutputGraphManager>();
            mockOutputGraphManager.Setup(m => m.GetSiteIdForBuilding(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("xxxxxx");
            mockOutputGraphManager.Setup(m => m.Errors).Returns(new Dictionary<string, string>());

            var mockOntologyMappingManager = new Mock<IOntologyMappingManager>();
            var configuration = new ConfigurationBuilder().Build();

            var graphNamingManager = new DefaultGraphNamingManager();

            var someOptions = Options.Create(new MappedIngestionManagerOptions());
            var mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            var graphIngestionProcessor = new MappedGraphIngestionProcessor<IngestionManagerOptions>(mockLogger.Object, mockInputGraphManager.Object, mockOntologyMappingManager.Object, mockOutputGraphManager.Object, graphNamingManager, someOptions, mockMeterFactory.Object, new MetricsAttributesHelper(configuration));

            await graphIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);
        }

        [Fact]
        public async Task SyncOrganization_OneBuilding()
        {
            var mockLogger = new Mock<ILogger<MappedGraphIngestionProcessor<IngestionManagerOptions>>>();
            var mockLogger2 = new Mock<ILogger<MappedGeneratedGraphManager>>();
            var mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            var configuration = new ConfigurationBuilder().Build();
            var someOptions = Options.Create(new MappedIngestionManagerOptions());

            var mockInputGraphManager = new MockInputGraphManager(mockLogger2.Object, new Mock<IHttpClientFactory>().Object, someOptions, mockMeterFactory.Object, mockSecretManager.Object, new MetricsAttributesHelper(configuration));

            var listOfDtdlFiles = new List<string>
                {
                    "TopologyIngestion.Test.Willow.Ontology.DTDLv3.jsonld",
                };

            var mockOutputGraphManager = new Mock<IOutputGraphManager>();
            mockOutputGraphManager.Setup(m => m.Errors).Returns(new Dictionary<string, string>());

            mockOutputGraphManager.Setup(m => m.GetSiteIdForBuilding(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("xxxxxx");
            mockOutputGraphManager.Setup(m => m.GetModelsAsync(CancellationToken.None)).ReturnsAsync(LoadDtdl(listOfDtdlFiles));

            var ontologyMappingManager = new OntologyMappingManager(new MappedHttpOntologyMappingLoader(mockLogger.Object, "https://mapped.com/ontologies/mapping/Mapped2Willow/latest.json"));

            var graphNamingManager = new DefaultGraphNamingManager();

            var graphIngestionProcessor = new MappedGraphIngestionProcessor<IngestionManagerOptions>(mockLogger.Object, mockInputGraphManager, ontologyMappingManager, mockOutputGraphManager.Object, graphNamingManager, someOptions, mockMeterFactory.Object, new MetricsAttributesHelper(configuration));

            await graphIngestionProcessor.SyncOrganizationAsync(false, CancellationToken.None);
        }

        [Fact]
        public async Task SyncConnectors_OneBuilding()
        {
            var mockLogger = new Mock<ILogger<MappedGraphIngestionProcessor<IngestionManagerOptions>>>();
            var mockLogger2 = new Mock<ILogger<MappedGeneratedGraphManager>>();
            var mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            var configuration = new ConfigurationBuilder().Build();
            var someOptions = Options.Create(new MappedIngestionManagerOptions());

            var mockInputGraphManager = new MockInputGraphManager(mockLogger2.Object, new Mock<IHttpClientFactory>().Object, someOptions, mockMeterFactory.Object, mockSecretManager.Object, new MetricsAttributesHelper(configuration));

            var listOfDtdlFiles = new List<string>
                {
                    "TopologyIngestion.Test.Willow.Ontology.DTDLv3.jsonld",
                };

            var mockOutputGraphManager = new Mock<IOutputGraphManager>();
            mockOutputGraphManager.Setup(m => m.Errors).Returns(new Dictionary<string, string>());

            var buildingTwin = new BasicDigitalTwin
            {
                Id = "BLDGFw1BBrbH346UFTmTjZGdLE",
            };

            buildingTwin.Metadata.ModelId = "dtmi:com:willowinc:Building;1";

            mockOutputGraphManager.Setup(m => m.GetSiteIdForMappedBuildingId(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("xxxxxx");
            mockOutputGraphManager.Setup(m => m.GetModelsAsync(CancellationToken.None)).ReturnsAsync(LoadDtdl(listOfDtdlFiles));
            mockOutputGraphManager.Setup(m => m.GetTwinForMappedId(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(buildingTwin);

            var ontologyMappingManager = new OntologyMappingManager(new MappedHttpOntologyMappingLoader(mockLogger.Object, "https://mapped.com/ontologies/mapping/Mapped2Willow/latest.json"));

            var graphNamingManager = new DefaultGraphNamingManager();

            var graphIngestionProcessor = new MappedGraphIngestionProcessor<IngestionManagerOptions>(mockLogger.Object, mockInputGraphManager, ontologyMappingManager, mockOutputGraphManager.Object, graphNamingManager, someOptions, mockMeterFactory.Object, new MetricsAttributesHelper(configuration));

            await graphIngestionProcessor.SyncConnectorsAsync("BLDGFw1BBrbH346UFTmTjZGdLE", false, CancellationToken.None);
        }

        [Fact]
        public async Task SyncSpatial_OneBuilding()
        {
            var mockLogger = new Mock<ILogger<MappedGraphIngestionProcessor<IngestionManagerOptions>>>();
            var mockLogger2 = new Mock<ILogger<MappedGeneratedGraphManager>>();
            var mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            var configuration = new ConfigurationBuilder().Build();
            var someOptions = Options.Create(new MappedIngestionManagerOptions());

            var mockInputGraphManager = new MockInputGraphManager(mockLogger2.Object, new Mock<IHttpClientFactory>().Object, someOptions, mockMeterFactory.Object, mockSecretManager.Object, new MetricsAttributesHelper(configuration));

            var listOfDtdlFiles = new List<string>
                {
                    "TopologyIngestion.Test.Willow.Ontology.DTDLv3.jsonld",
                };

            var mockOutputGraphManager = new Mock<IOutputGraphManager>();
            mockOutputGraphManager.Setup(m => m.Errors).Returns(new Dictionary<string, string>());

            var buildingTwin = new BasicDigitalTwin
            {
                Id = "BLDGL1LaWzU32uzyhABLM35s2B",
            };

            buildingTwin.Metadata.ModelId = "dtmi:com:willowinc:Building;1";

            mockOutputGraphManager.Setup(m => m.GetSiteIdForMappedBuildingId(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("xxxxxx");
            mockOutputGraphManager.Setup(m => m.GetModelsAsync(CancellationToken.None)).ReturnsAsync(LoadDtdl(listOfDtdlFiles));
            mockOutputGraphManager.Setup(m => m.GetTwinForMappedId(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(buildingTwin);

            var ontologyMappingManager = new OntologyMappingManager(new MappedHttpOntologyMappingLoader(mockLogger.Object, "https://mapped.com/ontologies/mapping/Mapped2Willow/latest.json"));

            var graphNamingManager = new DefaultGraphNamingManager();

            var graphIngestionProcessor = new MappedGraphIngestionProcessor<IngestionManagerOptions>(mockLogger.Object, mockInputGraphManager, ontologyMappingManager, mockOutputGraphManager.Object, graphNamingManager, someOptions, mockMeterFactory.Object, new MetricsAttributesHelper(configuration));

            await graphIngestionProcessor.SyncSpatialAsync("BLDGFw1BBrbH346UFTmTjZGdLE", false, CancellationToken.None);
        }

        [Fact]
        public async Task SyncThings_OneBuilding_OneConnector()
        {
            var mockLogger = new Mock<ILogger<MappedGraphIngestionProcessor<IngestionManagerOptions>>>();
            var mockLogger2 = new Mock<ILogger<MappedGeneratedGraphManager>>();
            var mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            var configuration = new ConfigurationBuilder().Build();
            var someOptions = Options.Create(new MappedIngestionManagerOptions());

            var mockInputGraphManager = new MockInputGraphManager(mockLogger2.Object, new Mock<IHttpClientFactory>().Object, someOptions, mockMeterFactory.Object, mockSecretManager.Object, new MetricsAttributesHelper(configuration));

            var listOfDtdlFiles = new List<string>
                {
                    "TopologyIngestion.Test.Willow.Ontology.DTDLv3.jsonld",
                };

            var mockOutputGraphManager = new Mock<IOutputGraphManager>();
            mockOutputGraphManager.Setup(m => m.Errors).Returns(new Dictionary<string, string>());

            var buildingTwin = new BasicDigitalTwin
            {
                Id = "BLDGL1LaWzU32uzyhABLM35s2B",
            };

            buildingTwin.Metadata.ModelId = "dtmi:com:willowinc:Building;1";

            mockOutputGraphManager.Setup(m => m.GetSiteIdForMappedBuildingId(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("xxxxxx");
            mockOutputGraphManager.Setup(m => m.GetModelsAsync(CancellationToken.None)).ReturnsAsync(LoadDtdl(listOfDtdlFiles));
            mockOutputGraphManager.Setup(m => m.GetTwinForMappedId(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(buildingTwin);

            var ontologyMappingManager = new OntologyMappingManager(new MappedHttpOntologyMappingLoader(mockLogger.Object, "https://mapped.com/ontologies/mapping/Mapped2Willow/latest.json"));

            var graphNamingManager = new DefaultGraphNamingManager();

            var graphIngestionProcessor = new MappedGraphIngestionProcessor<IngestionManagerOptions>(mockLogger.Object, mockInputGraphManager, ontologyMappingManager, mockOutputGraphManager.Object, graphNamingManager, someOptions, mockMeterFactory.Object, new MetricsAttributesHelper(configuration));

            await graphIngestionProcessor.SyncThingsAsync("BLDGFw1BBrbH346UFTmTjZGdLE", "CONRdBLQCNvWoKGUA4kRoUMoe", false, CancellationToken.None);
        }

        [Fact]
        public async Task SyncPoints_OneBuilding_OneConnector()
        {
            var mockLogger = new Mock<ILogger<MappedGraphIngestionProcessor<IngestionManagerOptions>>>();
            var mockLogger2 = new Mock<ILogger<MappedGeneratedGraphManager>>();
            var mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            var configuration = new ConfigurationBuilder().Build();
            var someOptions = Options.Create(new MappedIngestionManagerOptions());

            var mockInputGraphManager = new MockInputGraphManager(mockLogger2.Object, new Mock<IHttpClientFactory>().Object, someOptions, mockMeterFactory.Object, mockSecretManager.Object, new MetricsAttributesHelper(configuration));

            var listOfDtdlFiles = new List<string>
                {
                    "TopologyIngestion.Test.Willow.Ontology.DTDLv3.jsonld",
                };

            var mockOutputGraphManager = new Mock<IOutputGraphManager>();
            mockOutputGraphManager.Setup(m => m.Errors).Returns(new Dictionary<string, string>());

            var buildingTwin = new BasicDigitalTwin
            {
                Id = "BLDGL1LaWzU32uzyhABLM35s2B",
            };

            buildingTwin.Metadata.ModelId = "dtmi:com:willowinc:Building;1";

            mockOutputGraphManager.Setup(m => m.GetSiteIdForMappedBuildingId(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("xxxxxx");
            mockOutputGraphManager.Setup(m => m.GetModelsAsync(CancellationToken.None)).ReturnsAsync(LoadDtdl(listOfDtdlFiles));
            mockOutputGraphManager.Setup(m => m.GetTwinForMappedId(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(buildingTwin);

            var ontologyMappingManager = new OntologyMappingManager(new MappedHttpOntologyMappingLoader(mockLogger.Object, "https://mapped.com/ontologies/mapping/Mapped2Willow/latest.json"));

            var graphNamingManager = new DefaultGraphNamingManager();

            var graphIngestionProcessor = new MappedGraphIngestionProcessor<IngestionManagerOptions>(mockLogger.Object, mockInputGraphManager, ontologyMappingManager, mockOutputGraphManager.Object, graphNamingManager, someOptions, mockMeterFactory.Object, new MetricsAttributesHelper(configuration));

            await graphIngestionProcessor.SyncPointsAsync("BLDGFw1BBrbH346UFTmTjZGdLE", "CONRdBLQCNvWoKGUA4kRoUMoe", false, CancellationToken.None);
        }

        private static List<string> LoadDtdl(List<string> fileNames)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            var jsonTexts = new List<string>();

            foreach (var fileName in fileNames)
            {
                var resourceName = resources.Single(str => str.EndsWith(fileName));

                using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string? result = reader.ReadToEnd();

                            jsonTexts.Add(result);
                        }
                    }
                }
            }

            return jsonTexts;
        }

        private static JsonElement GetJsonElement()
        {
            var doc = JsonDocument.Parse("{ \"id\": \"CLSKkDFMgbojZZ54MorD6B11R\", \"name\": \"AV 31\", \"exactType\": \"TemperatureAlarmSetpoint\" }");
            return doc.RootElement;
        }

        private static JsonElement GetJsonElementWithEmptyName()
        {
            var doc = JsonDocument.Parse("{ \"id\": \"CLSKkDFMgbojZZ54MorD6B11P\", \"name\": null, \"description\": \"test\", \"exactType\": \"TemperatureAlarmSetpoint\" }");
            return doc.RootElement;
        }

        private static JsonElement GetJsonElementWithMappingKey()
        {
            var doc = JsonDocument.Parse("{ \"id\": \"CLSKkDFMgbojZZ54MorD6B11P\", \"exactType\": \"TemperatureAlarmSetpoint\", \"mappingKey\": \"12345\" }");
            return doc.RootElement;
        }

        private static JsonElement GetJsonElementWithMultipleKeys()
        {
            var doc = JsonDocument.Parse("{ \"id\": \"CLSKkDFMgbojZZ54MorD6B11P\", \"exactType\": \"SpaceWithBox\", \"mappingKey\": \"12345\", \"deviceId\": \"678\", \"unit\": { \"id\": \"A\" } }");
            return doc.RootElement;
        }

        private static JsonElement GetJsonElementWithNullUnit()
        {
            var doc = JsonDocument.Parse("{ \"id\": \"CLSKkDFMgbojZZ54MorD6B11P\", \"exactType\": \"SpaceWithBox\", \"mappingKey\": \"12345\", \"deviceId\": \"678\", \"unit\": null }");
            return doc.RootElement;
        }

        private OntologyMapping GetTestMappings()
        {
            var ontologyMapping = new OntologyMapping();

            ontologyMapping.Header.InputOntologies.Add(new Ontology { DtdlVersion = "v2", Name = "twin", Version = "1.0" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v2", Name = "org1", Version = "1.1" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v3", Name = "org2", Version = "1.2" });
            ontologyMapping.InterfaceRemaps.Add(new DtmiRemap { InputDtmi = "dtmi:twin:main:CleaningRoom;1", OutputDtmi = "dtmi:org:w3id:rec:Space;1" });
            ontologyMapping.InterfaceRemaps.Add(new DtmiRemap { InputDtmi = "dtmi:twin:main:SpaceWithUnit;1", OutputDtmi = "dtmi:org:w3id:rec:SpaceWithUnit;1" });
            ontologyMapping.RelationshipRemaps.Add(new RelationshipRemap { InputRelationship = "hasA", OutputRelationship = "isA" });
            ontologyMapping.RelationshipRemaps.Add(new RelationshipRemap { InputRelationship = "hasPoint", OutputRelationship = "isPointOf", ReverseRelationshipDirection = true });
            ontologyMapping.FillProperties.Add(new FillProperty { InputPropertyNames = new List<string>() { "name", "description" }, OutputDtmiFilter = ".*", OutputPropertyName = "name" });
            ontologyMapping.PropertyProjections.Add(new PropertyProjection { InputPropertyNames = new List<string> { "mappingKey", "deviceId" }, OutputDtmiFilter = ".*", IsOutputPropertyCollection = true, OutputPropertyName = "externalIds" });
            ontologyMapping.ObjectTransformations.Add(new ObjectTransformation { InputProperty = "unit", InputPropertyName = "id", OutputPropertyName = "unit", Priority = 1, OutputDtmiFilter = ".*" });
            return ontologyMapping;
        }
    }

    public class MockInputGraphManager : MappedGeneratedGraphManager
    {
        private const string AccountsQuery = "query{accounts{connectedDataSourceId,description,exactType,id,mappingKey,name,hasProvider{connectedDataSourceId,description,exactType,id,mappingKey,name,identities{__typename,...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}}},identities{__typename,...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}}}}";
        private const string OrganizationQuery = "{sites{connectedDataSourceId,description,exactType,id,mappingKey,name,identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}}}}\r\n";
        private const string ConnectorsQuery = "query{connectors{baseUrl,configHash,connectorTypeId,direction,id,lastUpdatedBy,name,state,stateDetails,userId,workflowId}}";
        private const string SiteBuildingsQuery = "query($siteId:SiteFilter={id:{eq:\"SITEFhJhVrRwyDLAmQ6M3HUBN6\"}}){sites(filter:$siteId){connectedDataSourceId,description,exactType,id,mappingKey,name,buildings(filter:{}){connectedDataSourceId,description,exactType,id,mappingKey,name,floors{connectedDataSourceId,description,exactType,id,level,mappingKey,name,identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}}},identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}}}}}";
        private const string BuildingFloorsQuery = "Getting topology from mapped. query($buildingId:BuildingFilter={id:{eq:\"BLDGFw1BBrbH346UFTmTjZGdLE\"}}){buildings(filter:$buildingId){connectedDataSourceId,description,exactType,id,mappingKey,name,floors{connectedDataSourceId,description,exactType,id,level,mappingKey,name,identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}}},identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}}}}";
        private const string BuildingConnectorsQuery = "query($buildingId:BuildingFilter={id:{eq:\"BLDGFw1BBrbH346UFTmTjZGdLE\"}}){buildings(filter:$buildingId){connectors{connectorType{description,direction,id,name,taskQueue,version},baseUrl,configHash,connectorTypeId,direction,id,lastUpdatedBy,name,state,stateDetails,userId,workflowId}}}";
        private const string FloorSpacesQuery = "query($floorId:FloorFilter={id:{eq:\"FLRPXsee48TzQAtGre2yVERmT\"}}){floors(filter:$floorId){connectedDataSourceId,description,exactType,id,level,mappingKey,name,identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}},hasPart{__typename,connectedDataSourceId,description,exactType,id,mappingKey,name,...on Space{identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}}},...on Zone{identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}},connectedDataSourceId,description,exactType,id,mappingKey,name}},zones{connectedDataSourceId,description,exactType,id,mappingKey,name,identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}},points{connectedDataSourceId,datatype,description,exactType,id,mappingKey,name,unused,identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}}},hasPart{__typename,connectedDataSourceId,description,exactType,id,mappingKey,name,...on Space{identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}}}}}}}";
        private const string BuildingThingsQuery = "query($exactType:ThingFilter={connectedDataSourceId:{eq:\"CONRdBLQCNvWoKGUA4kRoUMoe\"},exactType:{ne:\"Thing\"}},$buildingId:BuildingFilter={id:{eq:\"BLDGFw1BBrbH346UFTmTjZGdLE\"}}){buildings(filter:$buildingId){things(filter:$exactType){connectedDataSourceId,description,exactType,firmwareVersion,id,isVirtual,mappingKey,name,seeAlso,serialNumber,identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}},hasDeviceModel{connectedDataSourceId,description,exactType,id,mappingKey,name,manufacturedBy{connectedDataSourceId,description,exactType,id,mappingKey,name}},hasLocation{__typename,connectedDataSourceId,description,exactType,id,mappingKey,name,...on Building{identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}}},...on Space{identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}}}},serves{connectedDataSourceId,description,exactType,id,mappingKey,name,identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}},hasPart{__typename,connectedDataSourceId,description,exactType,id,mappingKey,name,...on Space{identities{__typename}}}},isFedBy{__typename,...on Thing{connectedDataSourceId,description,exactType,firmwareVersion,id,isVirtual,mappingKey,name,seeAlso,serialNumber,identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}}}}}}}";
        private const string ThingPointsQuery = "query($thingId:ThingFilter={id:{in:[\"THG23MPxkWeQqV6Cdvo1uRSCt\",\"THG27J2XBsBfDYySCkUG2TAZY\",\"THG27V6kpZDu9j2SeNVKBuyLu\",\"THG2AQydgvKVtmKNQwzuxgEcq\",\"THG2AWV69bcMjF64qsTrfQuYW\",\"THG2GWREcqdDFrXx1MWqLfxP4\",\"THG2HQdqohujt2rx2dJUW6ZRj\",\"THG2Jtg4KebyHhub1F3XX9mg4\",\"THG2KWo6iYhaZ1hbh7U9oBtSs\",\"THG2LtPZpJsDBVbET8iMJaQKU\",\"THG2RfPKHEm8JRRTY7NYDMDPa\",\"THG2TNZKrqNU8WVPzFT2tTWNS\",\"THG2VDP8Gz8MchkiMhuCahofi\",\"THG2cRhq4RQ6fZ5cNKP21Qjzy\",\"THG2de31vukSmvSNBjAesahyw\",\"THG2kVCuE7VCMAXAEK5inwqs4\",\"THG2nLMbrYb3Nny1zjSqanFTQ\",\"THG2taJYmJeZ8kkkAeqXiGvXp\",\"THG32myiAhemt5CuasjaAYRVi\",\"THG3AzLWzXA1YpVsbd8BF8S8t\",\"THG3GA9L3s2NtQq2qHrenzwUb\",\"THG3NeZ5ikyQpfFTJ3n2UG8PB\",\"THG3PVPkgiJQmdG4eLGknh2ba\",\"THG3PysWubNCYBF2MiXMCVNg2\",\"THG3UPvkHZsPB8MD39poyHEX9\",\"THG3bt6EDLBJc15w9Rk3Yxxoi\",\"THG3cg1Uss4xszjxSbY4yDAsp\",\"THG3fRYJ66pcKEFEZLkKu7MyX\",\"THG3hvqn7UoDFmGm8j3QcpGpQ\",\"THG3iLEZwDcNSS2Zi8cQjvASh\",\"THG3jMYM85QrJYdvPFjNT8STr\",\"THG3qS4yXoxbnXN8Cgb86S7oE\",\"THG3shdJdpQEBNp6Yf5rSxYvd\",\"THG3vz6zKgz4J8naRGwXbHdmU\",\"THG3zx1BYWS3EJyQpfopDZqXN\",\"THG484z3XtTGZocGmf7JpctF9\",\"THG4C5K7JECsAFHueigZYzcSB\",\"THG4CXh3rLAXMdixqZwhTDmAS\",\"THG4PNitq5jCqdHNbsvx7XgjN\",\"THG4RP56UuTmCQCnsxpsnJ1x3\",\"THG4VH88e6bxnGs8EtjcTUaVk\",\"THG4kN7PFBKdUpyuuj21VwXUR\",\"THG4mex7zEcdSSX9m6NUQ1JrB\",\"THG4pZnpCd5d6Ez7MGfgrg8Wk\",\"THG4q2ti4WN6fmPe3cdow5fJT\",\"THG4ryA1ypDZJm4omjWEvLzvf\",\"THG4toRhZixfgA4u4BwzfkMeV\",\"THG4yqwkHbx1q84Rw995s56Ey\",\"THG51yVdHxa7Ter4fKjvmfBfw\",\"THG5EcKSyRskrFvEnHjh5KYa7\",\"THG5GX26W7oLq97Wcff4XZTVZ\",\"THG5JAgDWW6QC25gt7gfvodkN\",\"THG5JX5qBpyMtbFACRAuyECp9\",\"THG5JY6wbzdPAXi6dczJa8qHk\",\"THG5Ku3MNcWA5xFNpR1hQ8Nch\",\"THG5RZTPtbQ1sLNSwabDYHcM7\",\"THG5TVtG8kzfjFrYttJMFGYLw\",\"THG5dSj4qyqXNb2yEEDiRcePr\",\"THG5ghTxa4xF6Kyus2X88gTCi\",\"THG5jXDpTbjZur7q9Rs4ksUwZ\",\"THG5kSbRR27FgjSWpG77TNaa4\",\"THG5vTP3BXesaSuKqz9ehER2g\",\"THG5xjjsqpUg4dsc2Dd4KxFyK\",\"THG64TqyMgjaiQ1cu1tPaWYjg\",\"THG661qLRe1F8G9Dqg2GmKHLX\",\"THG69hHvGnnFoazcnwqs7R8o6\",\"THG6AQwFPdcAgBdJm7jyn1DCT\",\"THG6Dc8NTQMd22JPe9yeKe8aA\",\"THG6EHW18LUebdfvBumukS6g7\",\"THG6FjxHZxRn7usooDo3SKUnA\",\"THG6GxdzZrsECWYUaBm9HhkxY\",\"THG6HQFo1vzEutA56zZcbudy3\",\"THG6JDD7wMxaY3T2qhYPGdW4j\",\"THG6JiGtvCeq7d1AUKN3WGBja\",\"THG6Lk1V4UEYeuUYPA9Gjuyav\",\"THG6R2ocrANLtVKR2Aha8Zz6B\",\"THG6SzMzfkoH1dCMMhStFyQGw\",\"THG6V99Bb76Gu5V81maVW75WH\",\"THG6VGxbnK79FMjsy36nZum4s\",\"THG6WzSrBRvaW489LZTnJiVKg\",\"THG6X7YJK4Kcy8d8s4jbFrvia\",\"THG6YKiZr7VvUCfq8ivJxzo5C\",\"THG6cUUMd3dibMJCxSpuArL4z\",\"THG6kJ2gFmCStvCvytzNcMSNj\",\"THG6kQGbvVksqyHKY7Tg5yjDL\",\"THG6mtFVovTU1m7aSAwT5KXXG\",\"THG6mvVzyAq2UkEvpDsfBo23z\",\"THG6nCEqfWeZ5xNGMWvDTYdfe\",\"THG6ooKKkHviKbcS82zzVPGQg\",\"THG6wsRzBfc4vtpB7PnySofyf\",\"THG6zsrB1h6kZfd59kf7dUF18\",\"THG72kYrMZfH9anuyNgRN1MN2\",\"THG75khapb4pzKmisyDBcpF9v\",\"THG79Hv9ZX4Z4UK4nFz1k3vRZ\",\"THG7DXeJGdbrLA5PjhDk3hzmh\",\"THG7Gs6q9YqMJZi9hoR6ML3Jt\",\"THG7J5V9LexatqYn7ns2VXVfD\",\"THG7JM2Vcx2wMYgeevg8MUGVX\",\"THG7JSiDwa7VXyoEhzrpov3p5\",\"THG7Pm4tpN9N6nQCe76Joy9rC\"]}},$exactType:PointFilter={exactType:{ne:\"Point\"}}){things(filter:$thingId){connectedDataSourceId,description,exactType,firmwareVersion,id,isVirtual,mappingKey,name,seeAlso,serialNumber,identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}},points(filter:$exactType){connectedDataSourceId,datatype,description,exactType,id,mappingKey,name,unused,valueMap,identities{__typename,...on BACnetObjectId{id,scope,scopeId,value},...on BACnetVendorId{id,scope,scopeId,value},...on EmailAddressIdentity{id,scope,scopeId,value},...on EmailIdentity{id,scope,scopeId,value},...on ExternalIdentity{id,scope,scopeId,value},...on FloorLevelIdentity{id,scope,scopeId,value},...on GenericIdentity{id,scope,scopeId,value},...on NameIdentity{id,scope,scopeId,value},...on PostalAddressIdentity{id,scope,scopeId,value},...on SpaceCode{id,scope,scopeId,value}},unit{description,id,name}}}}";

        public MockInputGraphManager(ILogger<MappedGeneratedGraphManager> logger, IHttpClientFactory httpClientFactory, IOptions<MappedIngestionManagerOptions> options, IMeterFactory meterFactory, ISecretManager secretManager, MetricsAttributesHelper metricsAttributesHelper)
            : base(logger, httpClientFactory, options, meterFactory, secretManager, metricsAttributesHelper)
        {
        }

        public override string GetOrganizationQuery()
        {
            return OrganizationQuery;
        }

        public override string GetAccountsQuery()
        {
            return AccountsQuery;
        }

        public override string GetConnectorsQuery()
        {
            return ConnectorsQuery;
        }

        public override string GetBuildingsForSiteQuery(string siteId)
        {
            return SiteBuildingsQuery;
        }

        public override string GetBuildingConnectorsQuery(string buildingDtId)
        {
            return BuildingConnectorsQuery;
        }

        public override string GetBuildingQuery(string buildingId)
        {
            return BuildingFloorsQuery;
        }

        public override string GetBuildingPlacesQuery(string buildingId)
        {
            return FloorSpacesQuery;
        }

        public override string GetBuildingThingsQuery(string buildingDtId, string connectorId)
        {
            return BuildingThingsQuery;
        }

        public override string GetFloorQuery(string floorId)
        {
            return FloorSpacesQuery;
        }

        public override string GetPointsForThingsQuery(IList<string> thingDtIds)
        {
            return ThingPointsQuery;
        }

        public override Task<JsonDocument?> GetTwinGraphAsync(string query, CancellationToken cancellationToken)
        {
            switch (query)
            {
                case SiteBuildingsQuery:
                    {
                        return Task.FromResult(GetDocumentFromResource("siteBuildings.json"));
                    }

                case AccountsQuery:
                    {
                        return Task.FromResult(GetDocumentFromResource("accounts.json"));
                    }

                case OrganizationQuery:
                    {
                        return Task.FromResult(GetDocumentFromResource("organization.json"));
                    }

                case BuildingConnectorsQuery:
                    {
                        return Task.FromResult(GetDocumentFromResource("buildingConnectors.json"));
                    }

                case FloorSpacesQuery:
                    {
                        return Task.FromResult(GetDocumentFromResource("floorSpaces.json"));
                    }

                case BuildingFloorsQuery:
                    {
                        return Task.FromResult(GetDocumentFromResource("buildingFloors.json"));
                    }

                case BuildingThingsQuery:
                    {
                        return Task.FromResult(GetDocumentFromResource("buildingThings.json"));
                    }

                case ThingPointsQuery:
                    {
                        return Task.FromResult(GetDocumentFromResource("thingPoints.json"));
                    }

                case ConnectorsQuery:
                    {
                        return Task.FromResult(GetDocumentFromResource("connectors.json"));
                    }

                default:
                    throw new Exception($"Query not found in test data. {query}");
            }
        }

        private static JsonDocument? GetDocumentFromResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resource = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourceName));
            var jsonDocument = null as JsonDocument;
            using (Stream? stream = assembly.GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string result = reader.ReadToEnd();
                        var organizationReader = new Utf8JsonReader(Encoding.UTF8.GetBytes(result));
                        _ = JsonDocument.TryParseValue(ref organizationReader, out jsonDocument);
                    }
                }
                else
                {
                    throw new FileNotFoundException(resourceName);
                }
            }

            return jsonDocument;
        }
    }

    public class MockedIngestionProcessor<TOptions> : MappedGraphIngestionProcessor<TOptions>
        where TOptions : IngestionManagerOptions
    {
        public MockedIngestionProcessor(ILogger<MockedIngestionProcessor<TOptions>> logger,
                                        IInputGraphManager inputGraphManager,
                                        IOntologyMappingManager ontologyMappingManager,
                                        IOutputGraphManager outputGraphManager,
                                        IGraphNamingManager graphNamingManager,
                                        IOptions<TOptions> options,
                                        IMeterFactory meterFactory,
                                        MetricsAttributesHelper metricsAttributesHelper)
            : base(logger, inputGraphManager, ontologyMappingManager, outputGraphManager, graphNamingManager, options, meterFactory, metricsAttributesHelper)
        {
        }

        public Dtmi? TestInputInterfaceDtmi(string interfaceType)
        {
            return GetInputInterfaceDtmi(interfaceType);
        }

        public string TestGetOutputRelationshipType(string inputRelationshipType)
        {
            return GetOutputRelationshipType(inputRelationshipType).Item1;
        }

        public bool TestTryGetOutputInterfaceDtmi(Dtmi inputDtmi, out Dtmi? outputDtmi)
        {
            return TryGetOutputInterfaceDtmi(inputDtmi, out outputDtmi);
        }

        public Dtmi? TestGetTwin(Dictionary<string, BasicDigitalTwin> twins,
                      JsonElement targetElement,
                      string basicDtId,
                      string interfaceType)
        {
            return AddTwin(twins, targetElement, basicDtId, interfaceType);
        }

        public void TestGetRelationship(Dictionary<string, BasicRelationship> relationships,
                              string sourceElementId,
                              Dtmi? inputSourceDtmi,
                              string? inputRelationshipType,
                              string targetDtId,
                              string targetInterfaceType,
                              Dictionary<string, object> relationshipProperties)
        {
            AddRelationship(relationships, sourceElementId, inputSourceDtmi, inputRelationshipType, targetDtId, targetInterfaceType, relationshipProperties);
        }
    }
}
