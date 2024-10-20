namespace Willow.CognianTelemetryAdapter.Tests;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Willow.CognianTelemetryAdapter.Metrics;
using Willow.CognianTelemetryAdapter.Models;
using Willow.CognianTelemetryAdapter.Options;
using Willow.CognianTelemetryAdapter.Services;
using Willow.LiveData.Pipeline;
using Xunit;

public class TelemetryProcessorTests
{
    private readonly TelemetryProcessor processor;
    private readonly Mock<ITransformService> mockTransformService = new();
    private readonly Mock<ISender> mockSender = new();
    private readonly Mock<ILogger<TelemetryProcessor>> mockLogger = new();
    private readonly Mock<HealthCheckTelemetryProcessor> mockHealthCheckTelemetryProcessor = new();
    private readonly Mock<IMetricsCollector> metricsCollector = new();

    public TelemetryProcessorTests()
    {
        var cognianSettings = Microsoft.Extensions.Options.Options.Create(new CognianAdapterOption() { ConnectorId = Guid.NewGuid().ToString() });
        var service = new TransformService(cognianSettings);
        processor = new TelemetryProcessor(mockLogger.Object, mockTransformService.Object, metricsCollector.Object, mockSender.Object, mockHealthCheckTelemetryProcessor.Object);

        mockTransformService
            .Setup(x => x.ProcessMessage(It.IsAny<CognianTelemetryMessage>()))
            .Returns((CognianTelemetryMessage input) => service.ProcessMessage(input));

        mockTransformService
            .Setup(x => x.ProcessMessages(It.IsAny<IEnumerable<CognianTelemetryMessage>>()))
            .Returns((IEnumerable<CognianTelemetryMessage> inputMessages) =>
                inputMessages.SelectMany(message => service.ProcessMessage(message)).ToList());
    }

    [Fact]
    public async Task ProcessAsync_ReadsJsonAndProcessesData()
    {
        // Arrange
        var inputMessage = new CognianTelemetryMessage(
            Topic: "/presence",
            Timestamp: 1609459200,
            Values: string.Empty,
            Telemetry: new Dictionary<string, object> { { "presence", "true" } },
            Origin: new CognianOrigin(new CognianOriginDevice("Device1")));

        var capturedTelemetry = new List<IEnumerable<Telemetry>>();
        mockSender.Setup(x => x.SendAsync(It.IsAny<IEnumerable<Telemetry>>(), It.IsAny<CancellationToken>()))
                  .Callback<IEnumerable<Telemetry>, CancellationToken>((telemetry, token) => capturedTelemetry.Add(telemetry))
                  .Returns(Task.CompletedTask);

        // Act
        var (succeeded, failed, skipped) = await processor.ProcessAsync(batch: [inputMessage]!, CancellationToken.None);

        // Assert
        Assert.True(succeeded > 0);
        mockSender.Verify(x => x.SendAsync(It.IsAny<IEnumerable<Telemetry>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }
}
