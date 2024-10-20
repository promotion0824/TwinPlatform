namespace Willow.MappedTelemetryAdaptor.Tests.Services;

using System.Diagnostics.Metrics;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Willow.Adx;
using Willow.MappedTelemetryAdaptor.Options;
using Willow.MappedTelemetryAdaptor.Services;
using Willow.Telemetry;
using Xunit;

public class IdMappingCacheServiceTests : IDisposable
{
    private readonly IAdxService adxService = Substitute.For<IAdxService>();
    private readonly IMetricsCollector metricsCollector = Substitute.For<IMetricsCollector>();
    private readonly Meter meter;
    private const string CacheMissInstrumentName = "MappingIdCacheMiss";

    public IdMappingCacheServiceTests()
    {
        meter = new Meter(Assembly.GetCallingAssembly().FullName!);
    }

    private IServiceProvider CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMetrics();
        serviceCollection.AddMemoryCache();
        return serviceCollection.BuildServiceProvider();
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

    [Fact]
    public void ReturnFromCacheIfFound()
    {
        var seedEternalId = "PNTDXT2JwxMMcS6iDhPZRt4kF";
        var expectedConnectorId = "CONRdBLQCNvWoKGUA4kRoUMoe";
        var seed = new Dictionary<string, string>
        {
            { seedEternalId, expectedConnectorId },
        };
        var service = GetCacheService(seed);
        var connectorId = service.GetConnectorId(seedEternalId);

        Assert.Equal(expectedConnectorId, connectorId);
        metricsCollector.DidNotReceive().TrackMetric(CacheMissInstrumentName, 1, MetricType.Counter, Arg.Any<string>(), Arg.Any<IDictionary<string, string>>());
    }

    [Fact]
    public void ReturnDefaultConnectorIdIfNotFoundInCache()
    {
        var service = GetCacheService();
        var metricCounter = 0;
        const string externalId = "externalId";
        metricsCollector.When(x => x.TrackMetric(CacheMissInstrumentName, 1, MetricType.Counter, Arg.Any<string>(), Arg.Any<IDictionary<string, string>>()))
            .Do(c => metricCounter++);

        adxService.QueryAsync<ConnectorIdMap>(Arg.Any<string>()).Returns([]);

        var connectorId = service.GetConnectorId(externalId);
        Assert.Equal(IdMapCacheService.DefaultMappedConnectorId, connectorId);
        Assert.Equal(1, metricCounter);
    }

    [Fact]
    public void ReturnDefaultIfEmpty()
    {
        var service = GetCacheService();

        var connectorId1 = service.GetConnectorId(string.Empty);
        Assert.Equal(IdMapCacheService.DefaultMappedConnectorId, connectorId1);
        metricsCollector.DidNotReceive().TrackMetric(CacheMissInstrumentName, 1, MetricType.Counter, Arg.Any<string>(), Arg.Any<IDictionary<string, string>>());
    }

    public void Dispose()
    {
        meter.Dispose();
        GC.SuppressFinalize(this);
    }
}
