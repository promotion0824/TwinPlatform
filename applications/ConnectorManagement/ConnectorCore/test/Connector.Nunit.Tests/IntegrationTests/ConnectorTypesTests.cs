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

    public class ConnectorTypesTests
    {
        [Test]
        public async Task GetConnectorTypes_ReturnsConnectorTypes()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var types = await client.GetJsonAsync<List<ConnectorTypeEntity>>("connectortypes");
                types.Should().Contain(t => t.Id == Constants.ConnectorTypeId1);
                types.Should().Contain(t => t.Id == Constants.ConnectorTypeId2);
                types.Should().Contain(t => t.Id == Constants.ConnectorTypeId4);
            }
        }

        [Test]
        public async Task GetConnectorType_ReturnsConnectorType()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var connectorType = await client.GetJsonAsync<ConnectorTypeEntity>($"connectortypes/{Constants.ConnectorTypeId1}");
                connectorType.Should()
                    .BeEquivalentTo(ConnectorsTestData.Types.FirstOrDefault(c => c.Id == Constants.ConnectorTypeId1));
            }
        }

        [Test]
        public async Task GetConnectorType_WrongFormatGuid_Returns400()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync("connectortypes/5113b3bf-0de6-4a56-818e-7828f938b6f");
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Test]
        public async Task GetConnectorType_WrongGuid_Returns404()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync("connectortypes/5113b3bf-0de6-4a56-818e-7828f938b6fb");
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
