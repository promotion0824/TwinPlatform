namespace Willow.LiveData.TelemetryStreaming.Tests;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using TechTalk.SpecFlow;
using Willow.LiveData.TelemetryStreaming.Metrics;
using Willow.LiveData.TelemetryStreaming.Models;
using Willow.LiveData.TelemetryStreaming.Services;

[Binding]
public class TelemetryProcessorStepDefinitions
{
    private Pipeline.Telemetry telemetry;
    private readonly Mock<ISubscriptionService> subscriptionServiceMock = new();
    private readonly Mock<IManagedMqttClient> managedMqttClientMock = new();
    private readonly Mock<IMetricsCollector> metricsCollector = new();
    private int enqueueCallCount;
    private OutputTelemetry? result;

    [BeforeScenario]
    public void Reset()
    {
        enqueueCallCount = 0;
    }

    [Given(@"I have telemetry")]
    public void GivenIHaveTelemetry()
    {
        telemetry = new()
        {
            Altitude = 1,
            ConnectorId = "64cb468d-0229-4956-8ddf-c586be43edd0",
            ExternalId = "701625AO0",
            DtId = "1234",
            EnqueuedTimestamp = DateTime.UtcNow,
            SourceTimestamp = DateTime.UtcNow,
            Latitude = 1,
            Longitude = 1,
            ScalarValue = 1d,
        };
    }

    [Given(@"the connector ID is null")]
    public void GivenTheConnectorIDIsNull()
    {
        telemetry = new()
        {
            Altitude = 1,
            ConnectorId = null,
            ExternalId = "701625AO0",
            DtId = "1234",
            EnqueuedTimestamp = DateTime.UtcNow,
            SourceTimestamp = DateTime.UtcNow,
            Latitude = 1,
            Longitude = 1,
            ScalarValue = 1d,
        };
    }

    [Given(@"the external ID is null")]
    public void GivenTheExternalIDIsNull()
    {
        telemetry = new()
        {
            Altitude = 1,
            ConnectorId = "64cb468d-0229-4956-8ddf-c586be43edd0",
            ExternalId = null,
            DtId = "1234",
            EnqueuedTimestamp = DateTime.UtcNow,
            SourceTimestamp = DateTime.UtcNow,
            Latitude = 1,
            Longitude = 1,
            ScalarValue = 1d,
        };
    }

    [Given(@"the scalar value is a string")]
    public void GivenTheScalarValueIsAString()
    {
        telemetry = new()
        {
            Altitude = 1,
            ConnectorId = "64cb468d-0229-4956-8ddf-c586be43edd0",
            ExternalId = "701625AO0",
            DtId = "1234",
            EnqueuedTimestamp = DateTime.UtcNow,
            SourceTimestamp = DateTime.UtcNow,
            Latitude = 1,
            Longitude = 1,
            ScalarValue = "Not a double",
        };
    }

    [Given(@"I do not have a matching subscription")]
    public void GivenIDoNotHaveAMatchingSubscription()
    {
        subscriptionServiceMock.Setup(m => m.GetSubscriptions(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(Array.Empty<Subscription>);
    }

    [Given(@"I have a matching subscription")]
    public void GivenIHaveAMatchingSubscription()
    {
        subscriptionServiceMock.Setup(m => m.GetSubscriptions("64cb468d-0229-4956-8ddf-c586be43edd0", "701625AO0")).ReturnsAsync(
        [
            new()
            {
                ExternalId = "701625AO0",
                ConnectorId = "64cb468d-0229-4956-8ddf-c586be43edd0",
                SubscriberId = "test-sub",
            },
        ]);
    }

    [When(@"I call ProcessAsync")]
    public async Task WhenICallProcessAsync()
    {
        managedMqttClientMock.Setup(m => m.EnqueueAsync(It.IsAny<MqttApplicationMessage>())).Callback((MqttApplicationMessage message) =>
        {
            enqueueCallCount++;
            result = JsonSerializer.Deserialize<OutputTelemetry>(message.PayloadSegment);
        });

        ILogger<TelemetryProcessor> logger = new LoggerFactory().CreateLogger<TelemetryProcessor>();

        TelemetryProcessor process = new(subscriptionServiceMock.Object, managedMqttClientMock.Object, metricsCollector.Object, logger);

        await process.ProcessAsync(telemetry);
    }

    [Then(@"the telemetry is not processed")]
    public void ThenTheTelemetryIsNotProcessed()
    {
        Assert.Equal(0, enqueueCallCount);
    }

    [Then(@"the telemetry is processed")]
    public void ThenTheTelemetryIsProcessed()
    {
        Assert.Equal(1, enqueueCallCount);

        Assert.NotNull(result);

        Assert.Equal(telemetry.SourceTimestamp, result.Value.SourceTimestamp);
        Assert.Equal(telemetry.EnqueuedTimestamp, result.Value.EnqueuedTimestamp);
        Assert.Equal(telemetry.ExternalId, result.Value.ExternalId);
        Assert.Equal(telemetry.ConnectorId.ToString(), result.Value.ConnectorId);
        Assert.Equal(telemetry.ScalarValue, result.Value.Value);
        Assert.Null(result.Value.Metadata);
    }
}
