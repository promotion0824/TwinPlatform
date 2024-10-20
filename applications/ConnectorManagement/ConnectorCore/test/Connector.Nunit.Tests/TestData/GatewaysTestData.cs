namespace Connector.Nunit.Tests.TestData
{
    using System;
    using System.Collections.Generic;
    using ConnectorCore.Entities;

    public class GatewaysTestData
    {
        public static List<GatewayEntity> Gateways = new List<GatewayEntity>
        {
            new GatewayEntity
            {
                Id = Constants.GatewayId1,
                Name = "Gateway 1",
                SiteId = Constants.SiteIdDefault,
                CustomerId = Constants.ClientIdDefault,
                Host = "abc123",
                IsEnabled = true,
                IsOnline = true,
                LastHeartbeatTime = DateTime.UtcNow,
                Connectors = new List<ConnectorEntity>
                {
                    ConnectorsTestData.Connectors[0],
                    ConnectorsTestData.Connectors[1],
                },
            },
            new GatewayEntity
            {
                Id = Constants.GatewayId2,
                Name = "Gateway 2",
                SiteId = Constants.SiteIdDefault,
                CustomerId = Constants.ClientIdDefault,
                Host = "abc123",
                IsEnabled = true,
                IsOnline = null,
                LastHeartbeatTime = DateTime.UtcNow,
                Connectors = new List<ConnectorEntity>
                {
                    ConnectorsTestData.Connectors[2],
                    ConnectorsTestData.Connectors[3],
                },
            },
            new GatewayEntity
            {
                Id = Constants.GatewayId3,
                Name = "Gateway 3",
                SiteId = Constants.SiteIdDefault,
                CustomerId = Constants.ClientIdDefault,
                Host = "abc123",
                IsEnabled = false,
                IsOnline = true,
                LastHeartbeatTime = DateTime.UtcNow,
                Connectors = new List<ConnectorEntity>
                {
                    ConnectorsTestData.Connectors[4]
                },
            },
            new GatewayEntity
            {
                Id = Constants.GatewayId4,
                Name = "Gateway 4",
                SiteId = Constants.SiteIdDefault,
                CustomerId = Constants.ClientIdDefault,
                Host = "abc123",
                IsEnabled = false,
                IsOnline = null,
                LastHeartbeatTime = DateTime.UtcNow,
                Connectors = null,
            },
        };
    }
}
