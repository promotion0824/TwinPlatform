namespace Willow.LiveData.TelemetryDataQuality.Tests;

using LazyCache;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.LiveData.TelemetryDataQuality.Models;
using Willow.LiveData.TelemetryDataQuality.Options;
using Willow.LiveData.TelemetryDataQuality.Services;
using Willow.LiveData.TelemetryDataQuality.Services.Abstractions;
using Willow.Telemetry;

[Binding]
public class TwinServiceStepDefinitions
{
    private TwinDetails? requestedTwin;
    private TwinsService? twinService;
    private readonly MemoryCacheService<TwinDetails> memoryCacheService = new(new CachingService());
    private readonly IMetricsCollector metricsCollector = Substitute.For<IMetricsCollector>();
    private readonly IOptions<TwinsCacheOption> options = Substitute.For<IOptions<TwinsCacheOption>>();
    private readonly ITwinsClient twinsClient = Substitute.For<ITwinsClient>();
    private readonly ILogger<TwinsService> logger = Substitute.For<ILogger<TwinsService>>();
    private readonly HealthCheckTwinsApi healthCheckTwinsApi = Substitute.For<HealthCheckTwinsApi>();

    [Given(@"I have twins in the cache")]
    public void GivenIHaveTwinsInTheCache(Table table)
    {
        var twins = table.CreateSet<TwinDetails>();
        this.twinService = new TwinsService(
            this.options,
            this.twinsClient,
            this.memoryCacheService,
            this.metricsCollector,
            this.healthCheckTwinsApi,
            this.logger);

        foreach (var twin in twins)
        {
            var cacheKey = $"{Constants.TwinsCachePrefix}-{twin.ExternalId}";
            this.memoryCacheService.SetAsync(cacheKey, twin);
        }
    }

    [When(@"I call GetTwin with Id '(.*)'")]
    public async Task WhenICallGetTwinWithId(string externalId)
    {
        var twinsCacheService = this.twinService;
        if (twinsCacheService != null)
        {
            this.requestedTwin = await twinsCacheService.GetTwin(externalId);
        }
    }

    [Then(@"I should get the following twin:")]
    public void ThenIShouldGetTheFollowingTwin(Table table)
    {
        var expectedTwin = table.CreateInstance<TwinDetails>();

        Assert.Multiple(
            () =>
        {
            Assert.That(this.requestedTwin, Is.Not.Null);
            Assert.That(this.requestedTwin, Is.EqualTo(expectedTwin));
        });
    }

    [Then(@"I should get no twin")]
    public void ThenIShouldGetNoTwin()
    {
        Assert.That(this.requestedTwin, Is.Null);
    }
}
