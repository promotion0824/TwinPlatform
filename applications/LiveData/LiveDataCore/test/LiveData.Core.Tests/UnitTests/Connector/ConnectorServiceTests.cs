namespace Willow.LiveData.Core.Tests.UnitTests.Connector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using NSubstitute;
    using NUnit.Framework;
    using Willow.LiveData.Core.Features.Connectors.ConnectionTypeRule;
    using Willow.LiveData.Core.Features.Connectors.DTOs;
    using Willow.LiveData.Core.Features.Connectors.Helpers;
    using Willow.LiveData.Core.Features.Connectors.Interfaces;
    using Willow.LiveData.Core.Features.Connectors.Models;
    using Willow.LiveData.Core.Features.Connectors.Services;
    using Willow.LiveData.Core.Infrastructure.Configuration;
    using Willow.LiveData.Core.Infrastructure.Extensions;

    internal class ConnectorServiceTests
    {
        private IOptions<TelemetryConfiguration> telemetryOptions;

        [TestCase("dd66b500-cac5-4201-951b-bee0655a65df", "bebaec82-b429-4de2-a9de-af1fbdee46c2", "2022-01-01 01:10", "2022-01-10 01:10")]
        public void ConnectorService_GetConnectorStatusAsync_Runs_Once(
                                                                Guid clientId,
                                                                string connectorId,
                                                                DateTime start,
                                                                DateTime end)
        {
            //arrange
            var adxConnectorRepositoryMock = new Mock<IAdxConnectorRepository>();
            var connectorIds = new List<string>() { connectorId };
            var connectorStatRequest = new ConnectorList() { ConnectorIds = connectorIds };
            var loggerMock = new Mock<ILogger<ConnectorTypeFactory>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            telemetryOptions = Substitute.For<IOptions<TelemetryConfiguration>>();
            telemetryOptions.Value.Returns(new TelemetryConfiguration());
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IVmConnectorType)))
                .Returns(new VmConnectorType(telemetryOptions));
            var connectorTypeFactory = new ConnectorTypeFactory(loggerMock.Object, serviceProviderMock.Object);
            IConnectorStatsResultHelper connectorStatsResultHelper = new ConnectorStatsResultHelper(connectorTypeFactory);

            adxConnectorRepositoryMock.Setup(x => x.GetConnectorStatusAsync(clientId, connectorIds.GetValidGuids())).ReturnsAsync(new List<ConnectorStats> { new() });
            adxConnectorRepositoryMock.Setup(x => x.GetTelemetryCountByLastXHoursAsync(clientId, connectorIds.GetValidGuids(), start, end, false))
                                      .ReturnsAsync(new List<ConnectorTelemetryBucket> { new() });
            adxConnectorRepositoryMock.Setup(x => x.GetConnectorStateOvertimeAsync(clientId, connectorIds.GetValidGuids(), start, end))
                                      .ReturnsAsync(new List<ConnectorState> { new() { ConnectorId = Guid.Parse(connectorId) } });
            var connectorService = new ConnectorService(connectorStatsResultHelper, adxConnectorRepositoryMock.Object);

            //act
            var result = connectorService.GetConnectorStatusAsync(clientId,
                                                                  connectorStatRequest,
                                                                  start,
                                                                  end,
                                                                  "false")
                                                                 .Result;

            //assert
            result.Data.Should().NotBeNull();
            result.Data.Count().Should().NotBe(0);
            adxConnectorRepositoryMock.Verify(x => x.GetConnectorStatusAsync(clientId, connectorIds.GetValidGuids()), Times.Once());
            adxConnectorRepositoryMock.Verify(x => x.GetTelemetryCountByLastXHoursAsync(clientId, connectorIds.GetValidGuids(), start, end, false), Times.Once());
            adxConnectorRepositoryMock.Verify(x => x.GetConnectorStateOvertimeAsync(clientId, connectorIds.GetValidGuids(), start, end), Times.Once());
        }

        [TestCase("dd66b500-cac5-4201-951b-bee0655a65df", "bebaec82-b429-4de2-a9de-af1fbdee46c2", "2022-01-01 01:10", "2022-01-10 01:10")]
        public void ConnectorService_GetConnectorStatusAsync_Returns_Valid_Results(
                                                                Guid clientId,
                                                                string connectorId,
                                                                DateTime start,
                                                                DateTime end)
        {
            //arrange
            var adxConnectorRepositoryMock = new Mock<IAdxConnectorRepository>();
            var newGuid = Guid.NewGuid();
            var connectorIds = new List<string> { connectorId };
            var connectorStatRequest = new ConnectorList() { ConnectorIds = connectorIds };
            var connectorStat = new List<ConnectorStats>()
            {
                new() { ConnectorId = Guid.Parse(connectorId) },
                new() { ConnectorId = newGuid },
            };
            var capabilitiesCount = new List<ConnectorTelemetryBucket>
            {
                new() { ConnectorId = newGuid.ToString() },
            };
            for (int i = 0; i < 20; i++)
            {
                capabilitiesCount.Add(new ConnectorTelemetryBucket { ConnectorId = connectorId });
            }

            var connectorStates = new List<ConnectorState>
            {
                new() { ConnectorId = Guid.Parse(connectorId), Interval = 300 },
            };

            for (int i = 0; i < 7; i++)
            {
                connectorStates.Add(new ConnectorState() { ConnectorId = Guid.Parse(connectorId), Interval = 300 });
            }

            var loggerMock = new Mock<ILogger<ConnectorTypeFactory>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            telemetryOptions = Substitute.For<IOptions<TelemetryConfiguration>>();
            telemetryOptions.Value.Returns(new TelemetryConfiguration());
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IVmConnectorType)))
                .Returns(new VmConnectorType(telemetryOptions));
            var connectorTypeFactory = new ConnectorTypeFactory(loggerMock.Object, serviceProviderMock.Object);
            var connectorStatsResultHelper = new ConnectorStatsResultHelper(connectorTypeFactory);
            adxConnectorRepositoryMock.Setup(x => x.GetConnectorStatusAsync(clientId, connectorIds.GetValidGuids())).ReturnsAsync(connectorStat);
            adxConnectorRepositoryMock.Setup(x => x.GetTelemetryCountByLastXHoursAsync(clientId, connectorIds.GetValidGuids(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), false))
                                      .ReturnsAsync(capabilitiesCount);
            adxConnectorRepositoryMock.Setup(x => x.GetConnectorStateOvertimeAsync(clientId, connectorIds.GetValidGuids(), start, end)).ReturnsAsync(connectorStates);
            var connectorService = new ConnectorService(connectorStatsResultHelper, adxConnectorRepositoryMock.Object);

            //act
            var result = connectorService.GetConnectorStatusAsync(clientId,
                                                                  connectorStatRequest,
                                                                  start,
                                                                  end,
                                                                  "false")
                                                                 .Result;

            //assert
            result.Data.Should().NotBeNull();
            var connectorCountResult = result.Data.Single(x => x.ConnectorId.ToString() == connectorId);
            connectorCountResult.Telemetry.Count().Should().Be(20);
            connectorCountResult.Status.Should().BeNull();
            adxConnectorRepositoryMock.Verify(x => x.GetConnectorStatusAsync(clientId, connectorIds.GetValidGuids()), Times.Once());
            adxConnectorRepositoryMock.Verify(x => x.GetTelemetryCountByLastXHoursAsync(clientId, connectorIds.GetValidGuids(), start, end, false), Times.Once());
            adxConnectorRepositoryMock.Verify(x => x.GetConnectorStateOvertimeAsync(clientId, connectorIds.GetValidGuids(), start, end), Times.Once());
        }
    }
}
