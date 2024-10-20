namespace Willow.LiveData.Core.Tests.UnitTests.Connector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NUnit.Framework;
    using Willow.LiveData.Core.Features.Connectors.DTOs;

    internal class ConnectorStatsDtoTests
    {
        [TestCase]
        public void ConnectorStatsDtoT_TotalTelemetryCount_Returns_Correct_Count()
        {
            //arrange
            var connectorCapabilitiesCountDto = new List<ConnectorTelemetryBucketDto>();
            for (int i = 0; i < 100; i++)
            {
                connectorCapabilitiesCountDto.Add(new ConnectorTelemetryBucketDto() { TotalTelemetryCount = 10 });
            }

            var dto = new ConnectorStatsDto();

            //action
            dto.Telemetry = connectorCapabilitiesCountDto;

            //assert
            dto.TotalTelemetryCount.Should().Be(1000);
        }
    }
}
