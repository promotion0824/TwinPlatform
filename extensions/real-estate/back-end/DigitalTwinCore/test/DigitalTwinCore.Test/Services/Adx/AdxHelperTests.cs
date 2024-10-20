using System;
using AutoFixture.Xunit2;
using Azure.DigitalTwins.Core;
using DigitalTwinCore.Constants;
using DigitalTwinCore.Dto.Adx;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DigitalTwinCore.Services.Adx;
using Microsoft.Extensions.Caching.Memory;
using DigitalTwinCore.Services.AdtApi;
using Kusto.Data.Common;
using Microsoft.Extensions.Options;


namespace DigitalTwinCore.Test.Services.Adx
{
    public class AdxHelperTests
    {
        private readonly Mock<IOptions<AzureDataExplorerSettings>> _azureDataExplorerSettings = new();
        private readonly Mock<IAdtApiService> _adtApiService = new();
        private readonly Mock<IMemoryCache> _memoryCache = new();
        private readonly Mock<ICacheEntry> _cacheEntry = new();
        private readonly Mock<ILogger<AdxHelper>> _logger = new();
        private readonly Mock<IDigitalTwinService> _service = new();
        private readonly Guid _siteId = Guid.NewGuid();

        private readonly IAdxHelper _sut;

        public AdxHelperTests()
        {
            var settings = new AzureDataExplorerSettings()
            {
                BlobStorage = new Willow.Api.AzureStorage.BlobStorageConfig()
                {
                    AccountKey = "",

                },
                Cluster = new AzureDataExplorerClusterSettings() { Name = "", }
            };

            _azureDataExplorerSettings.SetupGet(x => x.Value).Returns(settings);                      

            _memoryCache
                .Setup(mc => mc.CreateEntry(It.IsAny<object>()))
                .Returns(_cacheEntry.Object);

            _sut = new AdxHelper(_memoryCache.Object, _adtApiService.Object, _azureDataExplorerSettings.Object, _logger.Object);
        }

        [Fact]
        public void QueueExport_NoErrors_Success()
        {
            var result = _sut.QueueExport(_siteId, _service.Object);

            result.Status.Should().Be(ExportStatus.Queued);
            result.SiteId.Should().Be(_siteId);
            result.HasErrors.Should().BeFalse();
        }

        [Theory]
        [AutoData]
        public async System.Threading.Tasks.Task Model_Table_Creation_Should_Not_Run_Twice(
            [Frozen] Mock<ICslAdminProvider> cslAdminProviderMock,
            [Frozen] Mock<IDigitalTwinService> digitalTwinService,
            AdtModel adtModel)
        {
            var commandResponse = new[] {new AdxTableSchemaResponse
            {
                Schema = "Column:Type"
            }};

            var commandReader = Helpers.CreateDataReader(commandResponse);
            cslAdminProviderMock
                .Setup(x => x.ExecuteControlCommandAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync(commandReader.Object);

            object cslAdminProvider = cslAdminProviderMock.Object;

            _memoryCache
                .Setup(mc => mc.TryGetValue("CslAdminProvider", out cslAdminProvider))
                .Returns(true);

            await _sut.AppendModel(digitalTwinService.Object, adtModel);
            await _sut.AppendModel(digitalTwinService.Object, adtModel);

            const string showCommand = ".show table Models cslschema";
            cslAdminProviderMock.Verify(x => x.ExecuteControlCommandAsync(It.IsAny<string>(), showCommand, null), Times.Once);
        }

        [Theory]
        [AutoData]
        public async System.Threading.Tasks.Task AppendTwin_Should_Not_Create_Tables(
            [Frozen] Mock<ICslAdminProvider> cslAdminProviderMock,
            [Frozen] Mock<IDigitalTwinService> digitalTwinService,
            BasicDigitalTwin twin)
        {
            twin.Contents.Add(Properties.UniqueId, Guid.NewGuid());
            var commandResponse = new[] {new AdxTableSchemaResponse
            {
                Schema = "Column:Type"
            }};

            var commandReader = Helpers.CreateDataReader(commandResponse);
            cslAdminProviderMock
                .Setup(x => x.ExecuteControlCommandAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync(commandReader.Object);

            object cslAdminProvider = cslAdminProviderMock.Object;

            _memoryCache
                .Setup(mc => mc.TryGetValue("CslAdminProvider", out cslAdminProvider))
                .Returns(true);

            await _sut.AppendTwin(digitalTwinService.Object, twin, true);
            await _sut.AppendTwin(digitalTwinService.Object, twin, true);

            const string showCommand = ".show table Twins cslschema";
            cslAdminProviderMock.Verify(x => x.ExecuteControlCommandAsync(It.IsAny<string>(), showCommand, null), Times.Never);
        }

        [Theory]
        [AutoData]
        public async System.Threading.Tasks.Task Relationship_Table_Creation_Should_Not_Run_Twice(
            [Frozen] Mock<ICslAdminProvider> cslAdminProviderMock,
            [Frozen] Mock<IDigitalTwinService> digitalTwinService,
            BasicRelationship relationship)
        {
            var commandResponse = new[] {new AdxTableSchemaResponse
            {
                Schema = "Column:Type"
            }};

            var commandReader = Helpers.CreateDataReader(commandResponse);
            cslAdminProviderMock
                .Setup(x => x.ExecuteControlCommandAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync(commandReader.Object);

            object cslAdminProvider = cslAdminProviderMock.Object;

            _memoryCache
                .Setup(mc => mc.TryGetValue("CslAdminProvider", out cslAdminProvider))
                .Returns(true);

            await _sut.AppendRelationship(digitalTwinService.Object, relationship);
            await _sut.AppendRelationship(digitalTwinService.Object, relationship);

            const string showCommand = ".show table Relationships cslschema";
            cslAdminProviderMock.Verify(x => x.ExecuteControlCommandAsync(It.IsAny<string>(), showCommand, null), Times.Once);
        }
    }
}