using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.AzureDataExplorer.Builders;
using Willow.AzureDataExplorer.Command;
using Willow.AzureDataExplorer.Infra;
using Willow.AzureDataExplorer.Ingest;
using Willow.AzureDataExplorer.Options;
using Willow.AzureDataExplorer.Query;
using Willow.AzureDigitalTwins.Api.DataQuality;
using Willow.AzureDigitalTwins.Api.Messaging.Configuration;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Api.Services.Hosted;
using Willow.AzureDigitalTwins.DataQuality.Api.Services;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.DataQuality.Model.Validation;
using Willow.DataQuality.Model.ValidationResults;
using Willow.Model.Async;
using Xunit;

namespace Willow.AzureDigitalTwins.Api.UnitTests.Services;
public class DataQualityAdxServiceTests
{
    private readonly DataQualityAdxService _dataQualityAdxService;
    private readonly Mock<IAzureDataExplorerInfra> _azureDataExplorerInfraMock;
    private readonly Mock<IAsyncService<TwinsValidationJob>> _asyncServiceMock;
    private readonly Mock<ILogger<DataQualityAdxService>> _loggerMock;
    private readonly Mock<IOptions<AzureDataExplorerOptions>> _azureDataExplorerOptionsMock;
    private readonly Mock<IClientBuilder> _clientBuilderMock;
    private readonly Mock<IAzureDataExplorerCommand> _azureDataExplorerCommandMock;
    private readonly Mock<IAzureDataExplorerQuery> _azureDataExplorerQueryCommandMock;
    private readonly Mock<IAzureDataExplorerIngest> _azureDataExplorerIngestMock;
    private readonly Mock<IAdxService> _adxService;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IDQRuleService> _ruleService;
    private readonly Mock<IJobsService> _jobsService;
    private readonly Mock<IBackgroundTaskQueue<TwinsValidationJob>> _backgroundQueue;
    private readonly Mock<IAzureDigitalTwinCacheProvider> _azureDigitalTwinCacheProvider;
    private readonly Mock<IMemoryCache> _memCacheMock;


    public DataQualityAdxServiceTests()
    {
        _azureDataExplorerInfraMock = new Mock<IAzureDataExplorerInfra>();
        _loggerMock = new Mock<ILogger<DataQualityAdxService>>();
        _azureDataExplorerOptionsMock = new Mock<IOptions<AzureDataExplorerOptions>>();
        _clientBuilderMock = new Mock<IClientBuilder>();
        _azureDataExplorerCommandMock = new Mock<IAzureDataExplorerCommand>();
        _azureDataExplorerQueryCommandMock = new Mock<IAzureDataExplorerQuery>();
        _azureDataExplorerIngestMock = new Mock<IAzureDataExplorerIngest>();
        _asyncServiceMock = new Mock<IAsyncService<TwinsValidationJob>>();
        _mapper = new Mock<IMapper>();
        _adxService = new Mock<IAdxService>();
        _ruleService = new Mock<IDQRuleService>();
        _azureDigitalTwinCacheProvider = new Mock<IAzureDigitalTwinCacheProvider>();
        _memCacheMock = new Mock<IMemoryCache>();
        _jobsService = new Mock<IJobsService>();
        _backgroundQueue = new Mock<IBackgroundTaskQueue<TwinsValidationJob>>();

        _azureDataExplorerOptionsMock.Setup(x => x.Value).Returns(new AzureDataExplorerOptions { DatabaseName = "database" });

        _dataQualityAdxService = new DataQualityAdxService(
            _azureDataExplorerInfraMock.Object,
            _loggerMock.Object,
            _azureDataExplorerOptionsMock.Object,
            _azureDataExplorerIngestMock.Object,
            _azureDataExplorerQueryCommandMock.Object,
            _mapper.Object,
            _memCacheMock.Object,
            new AzureDigitalTwinsSettings
            {
                Instance = new InstanceSettings { InstanceUri = new Uri("https://adt.com") }
            },
             _adxService.Object,
            _ruleService.Object,
            _azureDigitalTwinCacheProvider.Object,
            _jobsService.Object,
            _backgroundQueue.Object
            );
    }

    [Fact]
    public async Task CreateAdxDefaultInfra_ShouldCreateTable()
    {

        await _dataQualityAdxService.CreateAdxDefaultInfraAsync();

        _azureDataExplorerInfraMock.Verify(x => x.CreateTableAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<Tuple<string, string>>>(), It.IsAny<bool>()), Times.Exactly(1));

    }

    [Fact]
    public async Task InitDQAdxSettings_ShouldInitializeDatabaseSchema()
    {

        await _dataQualityAdxService.InitDQAdxSettingsAsync();

        _azureDataExplorerInfraMock.Verify(x => x.CreateTableAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<Tuple<string, string>>>(), It.IsAny<bool>()), Times.Exactly(1));

    }

    [Fact]
    public async Task AddDataToADX_VerifyIngest()
    {
        await _dataQualityAdxService.IngestDataToValidationTableAsync(new List<ValidationResults>() { new ValidationResults() { } });

        _azureDataExplorerIngestMock.Verify(x => x.IngestFromDataReaderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<ValidationResultsAdxDto>>()));
    }

    [Fact]
    public async Task Verify_ReadFromADX()
    {
        string[] dtids = { "MSFT-XX1", "BPY-XX2" };
        var query = QueryBuilder.Create().Select("ValidationResults");
        query.Where();
        (query as IQueryFilterGroup).PropertyIn("TwinDtId", dtids);

        await _dataQualityAdxService.GetTwinDataQualityResultsByIdAsync(dtids);
        _azureDataExplorerQueryCommandMock.Verify(x => x.CreatePagedQueryAsync(It.IsAny<string>(), It.IsAny<IQuerySelector>(), It.IsAny<int>(), true));
    }
}

