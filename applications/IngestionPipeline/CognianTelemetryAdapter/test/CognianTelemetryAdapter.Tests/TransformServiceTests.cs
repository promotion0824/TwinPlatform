namespace Willow.CognianTelemetryAdapter.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Moq;
using Willow.CognianTelemetryAdapter.Models;
using Willow.CognianTelemetryAdapter.Options;
using Willow.CognianTelemetryAdapter.Services;
using Xunit;

public class TransformServiceTests
{
    private readonly TransformService transformService;
    private readonly Mock<IOptions<CognianAdapterOption>> mockOptions = new();

    public TransformServiceTests()
    {
        mockOptions.Setup(o => o.Value).Returns(new CognianAdapterOption { ConnectorId = Guid.NewGuid().ToString() });
        transformService = new TransformService(mockOptions.Object);
    }

    [Theory]
    [InlineData("/presence", "presence", "true", "Device1-presence", true)]
    [InlineData("/presence", "presence", "false", "Device1-presence", false)]
    [InlineData("/lux", "lux", "100", "Device1-Lux", 100)]
    public void ProcessMessage_WhenGivenValidTelemetryData_ProcessesCorrectly(string topic, string key, string value, string expectedExternalId, object expectedValue)
    {
        // Arrange
        var inputMessage = new CognianTelemetryMessage(
            Topic: topic,
            Timestamp: 1609459200,
            Values: string.Empty,
            Telemetry: new Dictionary<string, object> { { key, value } },
            Origin: new CognianOrigin(new CognianOriginDevice("Device1")));

        // Act
        var result = transformService.ProcessMessage(inputMessage).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedValue, result[0].ScalarValue);
        Assert.Contains(expectedExternalId, result[0].ExternalId);
    }

    [Fact]
    public void ProcessMessage_WhenBLETagPresent_CreatesTelemetry()
    {
        // Arrange
        var bleTag = new CognianBLETag("123-456", 1, 2);
        var bleTagJson = JsonSerializer.Serialize(bleTag);
        var inputMessage = new CognianTelemetryMessage(
            Topic: "/BLETag",
            Timestamp: 1609459200,
            Values: null,
            Telemetry: new Dictionary<string, object> { { "BLETagId", bleTagJson } },
            Origin: new CognianOrigin(new CognianOriginDevice("Device1")));

        // Act
        var result = transformService.ProcessMessage(inputMessage).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("123-456-1-2-presence", result[0].ExternalId);
        Assert.Equal(1, result[0].ScalarValue);
    }

    [Fact]
    public void ProcessMessage_WhenGivenMultiSensorData_IncludesDetectedPersonsZone()
    {
        // Arrange
        var jsonArray = new JsonArray { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var jsonDocument = JsonDocument.Parse(jsonArray.ToString());
        var detectedPersonsZoneValues = jsonDocument.RootElement;
        var inputMessage = new CognianTelemetryMessage(
            Topic: "event/88889999-AAAA-2BBB-BCCC-DDDDEEEEFFFF/devices/device1/sensors/b827eb9166d5/multi",
            Timestamp: 1609459200,
            Values: null,
            Telemetry: new Dictionary<string, object>
            {
                { "temperatureC", 25.8 },
                { "humidityRelative", 44.7 },
                { "detectedPersons", 3 },
                { "detectionZonesPresent", 0 },
                { "detectedPersonsZone", detectedPersonsZoneValues },
            },
            Origin: new CognianOrigin(new CognianOriginDevice("device1.7")));

        // Act
        var results = transformService.ProcessMessage(inputMessage).ToList();

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.Equal(14, results.Count);

        // Check temperatureC
        var temperatureTelemetry = results.First(t => t.ExternalId == "device1.7-temperatureC");
        Assert.Equal(25.8, temperatureTelemetry.ScalarValue);

        // Check humidityRelative
        var humidityTelemetry = results.First(t => t.ExternalId == "device1.7-humidityRelative");
        Assert.Equal(44.7, humidityTelemetry.ScalarValue);

        // Check detectedPersons
        var personsTelemetry = results.First(t => t.ExternalId == "device1.7-detectedPersons");
        Assert.Equal(3, personsTelemetry.ScalarValue);

        // Check detectionZonesPresent
        var zonesPresentTelemetry = results.First(t => t.ExternalId == "device1.7-detectionZonesPresent");
        Assert.Equal(0, zonesPresentTelemetry.ScalarValue);

        // Check detectedPersonsZone
        var detectedPersonsZoneEntries = results.Where(t => t.ExternalId != null && t.ExternalId.StartsWith("device1.7-detectedPersonsZone[")).ToList();
        Assert.Equal(10, detectedPersonsZoneEntries.Count);

        for (int counter = 0; counter < detectedPersonsZoneEntries.Count; counter++)
        {
            var zoneTelemetry = detectedPersonsZoneEntries[counter];
            Assert.Equal(counter, int.TryParse(zoneTelemetry.ScalarValue!.ToString(), out int zone) ? zone : -1);
        }
    }

    [Fact]
    public void ProcessMessage_ShouldNotCreateTelemetry()
    {
        // Arrange
        var inputMessage = new CognianTelemetryMessage(
            Topic: null,
            Timestamp: 1609459200,
            Values: new List<PointValue>() { new PointValue { PointExternalId = "123", Timestamp = DateTime.UtcNow, Value = 22.5, } },
            Telemetry: null,
            Origin: null);

        // Act
        var result = transformService.ProcessMessage(inputMessage).ToList();

        // Assert
        Assert.Empty(result);
    }
}
