namespace Willow.MappedTelemetryAdaptor.Tests;

using System.Diagnostics.Metrics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Willow.Adx;
using Willow.MappedTelemetryAdaptor.Options;
using Willow.MappedTelemetryAdaptor.Services;
using Willow.Telemetry;
using Xunit;

[Binding]
public class IdMappingCacheServiceStepDefinitions : IDisposable
{
    private readonly IAdxService adxService = Substitute.For<IAdxService>();
    private readonly IMetricsCollector metricsCollector = Substitute.For<IMetricsCollector>();
    private IdMapCacheService cacheService;
    private readonly Meter meter = new("Test");
    private const string CacheMissInstrumentName = "MappingIdCacheMiss";
    private string externalIdInput = string.Empty;
    private string connectorIdResult = string.Empty;
    private long metricResult;

    public IdMappingCacheServiceStepDefinitions()
    {
        cacheService = GetCacheService();
    }

    [Given("External id is \"([^\"]*)\"")]
    public void GivenExternalIdIs(string externalId)
    {
        externalIdInput = externalId;
    }

    [Given(@"in the cache external id (.*) with connector (.*) is present")]
    public void GivenInTheCacheExternalIdWithConnectorIsPresent(string externalId, string connectorId)
    {
        var seed = new Dictionary<string, string>
        {
            { externalId, connectorId },
        };

        cacheService = GetCacheService(seed);
    }

    [When(@"it does the look up")]
    public void WhenItDoesTheLookUp()
    {
        connectorIdResult = cacheService.GetConnectorId(externalIdInput);
    }

    [Then(@"it should return (.*)")]
    public void ThenItShouldReturn(string connectorId)
    {
        Assert.Equal(connectorIdResult, connectorId);
    }

    private IdMapCacheService GetCacheService(IDictionary<string, string>? cacheData = null)
    {
        var serviceProvider = CreateServiceProvider();
        var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();

        foreach (var data in cacheData ?? new Dictionary<string, string>())
        {
            memoryCache.Set(data.Key, data.Value);
        }

        var cacheOption = Microsoft.Extensions.Options.Options.Create(new IdMappingCacheOption());
        return new IdMapCacheService(cacheOption, adxService, memoryCache, metricsCollector, NullLogger<IdMapCacheService>.Instance);
    }

    private IServiceProvider CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMetrics();
        serviceCollection.AddMemoryCache();
        return serviceCollection.BuildServiceProvider();
    }

    [Given(@"is not found in the cache")]
    public void GivenIsNotFoundInTheCache()
    {
        cacheService = GetCacheService();
    }

    [Given(@"the cache doesn't have the id")]
    public void GivenTheCacheDoesntHaveTheId()
    {
        cacheService = GetCacheService();
        metricsCollector.When(x => x.TrackMetric(CacheMissInstrumentName, 1, MetricType.Counter, Arg.Any<string>(), Arg.Any<IDictionary<string, string>>()))
            .Do(c => metricResult++);
        adxService.QueryAsync<ConnectorIdMap>(Arg.Any<string>()).Returns([]);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        meter.Dispose();
    }

    [Then("^metric should be \"([^\"]*)\"$")]
    public void ThenMetricShouldBe(string p0)
    {
        if (string.IsNullOrEmpty(p0))
        {
            metricsCollector.DidNotReceive().TrackMetric(CacheMissInstrumentName, 1, MetricType.Counter, Arg.Any<string>(), Arg.Any<IDictionary<string, string>>());
            return;
        }

        Assert.Equal(int.Parse(p0), metricResult);
    }
}
