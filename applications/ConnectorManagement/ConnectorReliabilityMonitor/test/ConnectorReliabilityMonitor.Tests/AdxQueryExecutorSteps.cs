namespace Willow.ConnectorReliabilityMonitor.Tests;

using Microsoft.Extensions.Logging;
using Moq;
using TechTalk.SpecFlow;
using Willow.Adx;
using Willow.Telemetry;

[Binding]
internal class AdxQueryExecutorSteps
{
    private readonly Mock<IAdxService> adxServiceMock = new();
    private readonly Mock<IMetricsCollector> metricsCollectorMock = new();
    private readonly Mock<ILogger<AdxQueryExecutor>> loggerMock = new();
    private readonly Mock<IHealthMetricsRepository> healthMetricsMock = new();
    private AdxQueryExecutor adxQueryExecutor;
    private Dictionary<string, string> dimensions = new();
    private string connectorName = string.Empty;

    [Given(@"a query is configured in AdxQueryExecutor")]
    public void GivenAQueryIsConfiguredInAdxQueryExecutor()
    {
        const long count = 100L;

        this.adxServiceMock.Setup(x => x.QueryAsync<long>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new[] { count });

        this.adxQueryExecutor = new AdxQueryExecutor(this.adxServiceMock.Object, this.metricsCollectorMock.Object, this.loggerMock.Object, this.healthMetricsMock.Object);
    }

    [When(@"the query is executed")]
    public async Task WhenTheQueryIsExecuted()
    {
        await this.adxQueryExecutor.ExecuteQueriesAsync(string.Empty, this.dimensions, CancellationToken.None);
    }

    [Given(@"I have a Connector configured with ""(.*)""")]
    public void GivenIHaveAConnectorConfiguredWith(string connectorConfig)
    {
        var pairs = connectorConfig.Split(',');
        this.dimensions = new Dictionary<string, string>();
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split(':');
            this.dimensions.Add(keyValue[0], keyValue[1]);

            if (keyValue[0] == "Name")
            {
                this.connectorName = keyValue[1];
            }
        }
    }

    [Then(@"the result should contain the configured ConnectorName")]
    public void ThenTheResultShouldContainTheConfiguredConnectorName()
    {
        this.loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"[{this.connectorName}]") && v.ToString().Contains("query completed in") && v.ToString().Contains("Result: 100")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeast(1));
    }
}
