// -----------------------------------------------------------------------
// <copyright file="MappedGraphIngestionProcessorTests.cs" Company="Willow">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.TopologyIngestion.Mapped.Test
{
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Net.Http;
    using Azure.Security.KeyVault.Secrets;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Willow.Security.KeyVault;
    using Willow.Telemetry;
    using Xunit;
    using Xunit.Abstractions;

    public class MappedGeneratedGraphManagerTests
    {
        private readonly ITestOutputHelper output;
        private Mock<ISecretManager> mockSecretManager;
        private readonly IConfiguration configuration;

        public MappedGeneratedGraphManagerTests(ITestOutputHelper output)
        {
            this.output = output;
            mockSecretManager = new Mock<ISecretManager>();
            var secret = new KeyVaultSecret("MappedToken", "Test");

            mockSecretManager.Setup(sm => sm.GetSecretAsync("MappedToken")).ReturnsAsync(secret);

            configuration = new ConfigurationBuilder()
                .Build();
        }

        [Fact]
        public void GetOrganizationQuery_ReturnsString()
        {
            var mockLogger = new Mock<ILogger<MappedGeneratedGraphManager>>();
            var mockClientFactory = new Mock<IHttpClientFactory>();
            var someOptions = Options.Create(new MappedIngestionManagerOptions());
            var mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            var manager = new MappedGeneratedGraphManager(mockLogger.Object, mockClientFactory.Object, someOptions, mockMeterFactory.Object, mockSecretManager.Object, new MetricsAttributesHelper(configuration));

            var result = manager.GetOrganizationQuery();

            Assert.False(string.IsNullOrEmpty(result));

            output.WriteLine(result);
        }

        [Fact]
        public void GetAccountsQuery_ReturnsString()
        {
            var mockLogger = new Mock<ILogger<MappedGeneratedGraphManager>>();
            var mockClientFactory = new Mock<IHttpClientFactory>();
            var someOptions = Options.Create(new MappedIngestionManagerOptions());
            var mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            var manager = new MappedGeneratedGraphManager(mockLogger.Object, mockClientFactory.Object, someOptions, mockMeterFactory.Object, mockSecretManager.Object, new MetricsAttributesHelper(configuration));

            var result = manager.GetAccountsQuery();

            Assert.False(string.IsNullOrEmpty(result));

            output.WriteLine(result);
        }

        [Fact]
        public void GetBuildingsForSiteQuery_ReturnsString()
        {
            var mockLogger = new Mock<ILogger<MappedGeneratedGraphManager>>();
            var mockClientFactory = new Mock<IHttpClientFactory>();
            var someOptions = Options.Create(new MappedIngestionManagerOptions());
            var mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            var manager = new MappedGeneratedGraphManager(mockLogger.Object, mockClientFactory.Object, someOptions, mockMeterFactory.Object, mockSecretManager.Object, new MetricsAttributesHelper(configuration));

            var result = manager.GetBuildingsForSiteQuery("SITEFhJhVrRwyDLAmQ6M3HUBN6");

            Assert.False(string.IsNullOrEmpty(result));

            output.WriteLine(result);
        }

        [Fact]
        public void GetBuildingPlacesQuery_ReturnsString()
        {
            var mockLogger = new Mock<ILogger<MappedGeneratedGraphManager>>();
            var mockClientFactory = new Mock<IHttpClientFactory>();
            var someOptions = Options.Create(new MappedIngestionManagerOptions());
            var mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            var manager = new MappedGeneratedGraphManager(mockLogger.Object, mockClientFactory.Object, someOptions, mockMeterFactory.Object, mockSecretManager.Object, new MetricsAttributesHelper(configuration));

            var result = manager.GetBuildingPlacesQuery("BLDGR3ojEpkygWiUgfJ6ZEND3t");

            Assert.False(string.IsNullOrEmpty(result));

            output.WriteLine(result);
        }

        [Fact]
        public void GetConnectorsQuery_ReturnsString()
        {
            var mockLogger = new Mock<ILogger<MappedGeneratedGraphManager>>();
            var mockClientFactory = new Mock<IHttpClientFactory>();
            var someOptions = Options.Create(new MappedIngestionManagerOptions());
            var mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            var manager = new MappedGeneratedGraphManager(mockLogger.Object, mockClientFactory.Object, someOptions, mockMeterFactory.Object, mockSecretManager.Object, new MetricsAttributesHelper(configuration));

            var result = manager.GetConnectorsQuery();

            Assert.False(string.IsNullOrEmpty(result));

            output.WriteLine(result);
        }

        [Fact]
        public void GetBuildingsThingsQuery_ReturnsString()
        {
            var mockLogger = new Mock<ILogger<MappedGeneratedGraphManager>>();
            var mockClientFactory = new Mock<IHttpClientFactory>();
            var someOptions = Options.Create(new MappedIngestionManagerOptions());
            var mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            var manager = new MappedGeneratedGraphManager(mockLogger.Object, mockClientFactory.Object, someOptions, mockMeterFactory.Object, mockSecretManager.Object, new MetricsAttributesHelper(configuration));

            var result = manager.GetBuildingThingsQuery("BLDGR3ojEpkygWiUgfJ6ZEND3t", "CONRdBLQCNvWoKGUA4kRoUMoe");

            Assert.False(string.IsNullOrEmpty(result));

            output.WriteLine(result);
        }

        [Fact]
        public void GetPointsForThingQuery_ReturnsString()
        {
            var mockLogger = new Mock<ILogger<MappedGeneratedGraphManager>>();
            var mockClientFactory = new Mock<IHttpClientFactory>();
            var someOptions = Options.Create(new MappedIngestionManagerOptions());
            var mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            var manager = new MappedGeneratedGraphManager(mockLogger.Object, mockClientFactory.Object, someOptions, mockMeterFactory.Object, mockSecretManager.Object, new MetricsAttributesHelper(configuration));

            var result = manager.GetPointsForThingsQuery(new List<string>() { "1234" });

            Assert.False(string.IsNullOrEmpty(result));

            output.WriteLine(result);
        }

        [Fact]
        public void GetFloorQuery_ReturnsString()
        {
            var mockLogger = new Mock<ILogger<MappedGeneratedGraphManager>>();
            var mockClientFactory = new Mock<IHttpClientFactory>();
            var someOptions = Options.Create(new MappedIngestionManagerOptions());
            var mockMeterFactory = new Mock<IMeterFactory>();
            mockMeterFactory.Setup(mf => mf.Create(It.IsAny<MeterOptions>())).Returns(new Meter("Test"));

            var manager = new MappedGeneratedGraphManager(mockLogger.Object, mockClientFactory.Object, someOptions, mockMeterFactory.Object, mockSecretManager.Object, new MetricsAttributesHelper(configuration));

            var result = manager.GetFloorQuery("1234");

            Assert.False(string.IsNullOrEmpty(result));

            output.WriteLine(result);
        }
    }
}
