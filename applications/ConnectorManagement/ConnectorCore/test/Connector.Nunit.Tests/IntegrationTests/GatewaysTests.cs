namespace Connector.Nunit.Tests.IntegrationTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Connector.Nunit.Tests.Infrastructure.Extensions;
    using Connector.Nunit.Tests.TestData;
    using ConnectorCore.Entities;
    using FluentAssertions;
    using NUnit.Framework;
    using Snapshooter.NUnit;

    public class GatewaysTests
    {
        [Test]
        public async Task GetGatewaysForSite_ReturnsGateways()
        {
            using var client = IntegrationFixture.Server.CreateClientRandomUser();
            var gateways = await client.GetJsonAsync<List<GatewayEntity>>($"sites/{Constants.SiteIdDefault}/gateways");

            var sorted = gateways
                .Select(g =>
                {
                    g.Connectors = g.Connectors.OrderBy(i => i.Name).ToList();
                    return g;
                })
                .OrderBy(i => i.Id);

            sorted.MatchSnapshot(options => options.IgnoreAllFields("LastHeartbeatTime").IgnoreAllFields("LastUpdatedAt"));
        }

        [Test]
        public async Task GetGatewayById_ReturnsGateway()
        {
            using var client = IntegrationFixture.Server.CreateClientRandomUser();
            var gateway = await client.GetJsonAsync<GatewayEntity>($"sites/{Constants.SiteIdDefault}/gateways/{Constants.GatewayId1}");
            gateway.MatchSnapshot(options => options.IgnoreAllFields("LastHeartbeatTime").IgnoreAllFields("LastUpdatedAt"));
        }

        [Test]
        public async Task GetGatewayById_WrongFormatGuid_Returns400()
        {
            using var client = IntegrationFixture.Server.CreateClientRandomUser();
            var response = await client.GetAsync($"sites/{Constants.SiteIdDefault}/gateways/360d2af2-446b-4b1f-bb6f-20af553ea28");
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetGatewayById_WrongGuid_Returns404()
        {
            using var client = IntegrationFixture.Server.CreateClientRandomUser();
            var response = await client.GetAsync($"sites/{Constants.SiteIdDefault}/gateways/360d2af2-446b-4b1f-bb6f-20af553ea282");
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
