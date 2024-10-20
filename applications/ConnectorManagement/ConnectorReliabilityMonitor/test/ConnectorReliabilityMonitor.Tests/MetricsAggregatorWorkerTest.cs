namespace Willow.ConnectorReliabilityMonitor.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Xunit.Abstractions;

public class MetricsAggregatorWorkerTest
{
    private readonly Mock<ILogger<MetricsAggregatorWorker>> mockLogger;
    private readonly Mock<IAdxQueryExecutor> mockAdxQueryExecutor;
    private readonly Mock<IConnectorApplicationBuilder> mockConnectorApplicationBuilder;
    private readonly IOptions<AdxQueryConfig> config;
    private readonly MetricsAggregatorWorker worker;
    private readonly ITestOutputHelper output;

    public MetricsAggregatorWorkerTest(ITestOutputHelper output)
    {
        this.output = output;
        mockLogger = new Mock<ILogger<MetricsAggregatorWorker>>();
        mockAdxQueryExecutor = new Mock<IAdxQueryExecutor>();
        mockConnectorApplicationBuilder = new Mock<IConnectorApplicationBuilder>();

        config = Options.Create(new AdxQueryConfig { ConnectorUpdateIntervalSeconds = 1 });
        worker = new MetricsAggregatorWorker(
            mockLogger.Object,
            mockAdxQueryExecutor.Object,
            mockConnectorApplicationBuilder.Object,
            config);
    }

    [Fact]
    public async Task ConnectorReEnabled_SetsUpNewTimer()
    {
        // Arrange
        var tcs = new TaskCompletionSource();
        var stoppingTokenSource = new CancellationTokenSource();
        var connectorsEnabled = new List<ConnectorApplicationDto>
            {
                new ConnectorApplicationDto { Id = "1", Name = "Connector", Interval = 1, IsEnabled = true },
            };
        var connectorsDisabled = new List<ConnectorApplicationDto>
            {
                new ConnectorApplicationDto { Id = "1", Name = "DisabledConnector", Interval = 1, IsEnabled = false },
            };
        var connectorsReEnabled = new List<ConnectorApplicationDto>
            {
                new ConnectorApplicationDto { Id = "1", Name = "ReEnabledConnector", Interval = 1, IsEnabled = true },
            };

        int callCount = 0;

        mockAdxQueryExecutor.Setup(m => m.ExecuteQueriesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .Callback<string, Dictionary<string, string>, CancellationToken>((connectorName, dimensions, token) =>
                {
                    output.WriteLine($"ExecuteQueriesAsync called for connector: {DateTime.UtcNow} {connectorName}");
                })
                .Returns(Task.CompletedTask);

        mockConnectorApplicationBuilder.Setup(m => m.GetConnectorsAsync())
                  .Returns(() =>
                  {
                      callCount++;
                      switch (callCount)
                      {
                          case 2:
                              return Task.FromResult<IEnumerable<ConnectorApplicationDto>>(connectorsDisabled);
                          case > 2:
                              if (callCount == 4)
                              {
                                  tcs.SetResult();
                              }

                              return Task.FromResult<IEnumerable<ConnectorApplicationDto>>(connectorsReEnabled);
                          default:
                              return Task.FromResult<IEnumerable<ConnectorApplicationDto>>(connectorsEnabled);
                      }
                  });

        // Act
        var workerTask = worker.StartAsync(stoppingTokenSource.Token);

        await tcs.Task;
        stoppingTokenSource.Cancel();
        await workerTask;

        // Assert
        mockAdxQueryExecutor.Verify(m => m.ExecuteQueriesAsync("DisabledConnector", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
        mockAdxQueryExecutor.Verify(m => m.ExecuteQueriesAsync("ReEnabledConnector", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        mockConnectorApplicationBuilder.Verify(m => m.GetConnectorsAsync(), Times.AtLeastOnce);
        mockLogger.Verify(m => m.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.AtLeastOnce);
    }
}
