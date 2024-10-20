namespace Connector.Nunit.Tests.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Connector.Nunit.Tests.Infrastructure.Extensions;
    using Connector.Nunit.Tests.TestData;
    using ConnectorCore.Dtos;
    using ConnectorCore.Entities;
    using FluentAssertions;
    using NUnit.Framework;

    public class ConnectorsTests
    {
        [Test]
        public async Task GetConnectors_ReturnsConnectors()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var connectors = await client.GetJsonAsync<List<ConnectorEntity>>("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/connectors");
                connectors.Should().Contain(c => c.Id == Constants.ConnectorId1);
                connectors.Should().Contain(c => c.Id == Constants.ConnectorId2);
                connectors.Should().Contain(c => c.Id == Constants.ConnectorId5);
            }
        }

        [Test]
        public async Task GetConnector_ReturnsConnector()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var connector = await client.GetJsonAsync<ConnectorEntity>("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/connectors/360d2af2-446b-4b1f-bb6f-20af553ea289");
                connector.Id.Should().Be(Guid.Parse("360d2af2-446b-4b1f-bb6f-20af553ea289"));
            }
        }

        [Test]
        public async Task GetConnectorById_ReturnsConnector()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var connector = await client.GetJsonAsync<ConnectorEntity>($"connectors/{Constants.ConnectorId1}");
                connector.Id.Should().Be(Constants.ConnectorId1);
                connector.IsEnabled.Should().BeTrue();
                connector.IsLoggingEnabled.Should().BeTrue();
                connector.ErrorThreshold.Should().Be(5);
            }
        }

        [Test]
        public async Task GetConnector_WrongFormatGuid_Returns400()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/connectors/360d2af2-446b-4b1f-bb6f-20af553ea28");
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Test]
        public async Task GetConnector_WrongGuid_Returns404()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/connectors/360d2af2-446b-4b1f-bb6f-20af553ea282");
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Test]
        public async Task CreateConnectorNoSiteId_ReturnsNewConnector()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var newConnector = new ConnectorEntity
                {
                    Name = "Creating Schema",
                    ClientId = Guid.NewGuid(),
                    SiteId = Guid.Parse("6a8cb6ef-f23b-4608-a08b-0b779fd616cb"),
                    Configuration = @"{""Name"": ""NameValue""}",
                    ConnectorTypeId = Constants.ConnectorTypeId1,
                };
                var response = await client.PostFormAsync<ConnectorEntity>("connectors", newConnector);
                response.Id.Should().NotBe(Guid.Empty);
                response.SiteId.Should().Be(Guid.Parse("6a8cb6ef-f23b-4608-a08b-0b779fd616cb"));
                response.Name.Should().Be(newConnector.Name);
            }
        }

        [Test]
        public async Task GetConnectorByTypeId_ReturnsConnector()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var connector = await client.GetJsonAsync<ConnectorEntity>($"sites/{Constants.SiteIdDefault}/connectors/bytype/{Constants.ConnectorTypeId4}");
                connector.Id.Should().Be(Constants.ConnectorId5);
                connector.IsEnabled.Should().BeTrue();
                connector.IsLoggingEnabled.Should().BeTrue();
            }
        }

        [Test]
        public async Task GetConnectorByTypeId_MoreThanOneResult_ReturnsNotFound()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync($"sites/{Constants.SiteIdDefault}/connectors/bytype/{Constants.ConnectorTypeId1}");
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            }
        }

        [Test]
        public async Task DisableConnectorById_ReturnsOk()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.PostAsync($"connectors/{Constants.ConnectorId6}/disable", null);
                response.IsSuccessStatusCode.Should().BeTrue();
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var connector = await client.GetJsonAsync<ConnectorEntity>($"connectors/{Constants.ConnectorId6}");
                connector.Id.Should().Be(Constants.ConnectorId6);
                connector.IsEnabled.Should().BeFalse();
            }
        }

        [Test]
        public async Task EnableConnectorById_ReturnsOk()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.PostAsync($"connectors/{Constants.ConnectorId7}/enable", null);
                response.IsSuccessStatusCode.Should().BeTrue();
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var connector = await client.GetJsonAsync<ConnectorEntity>($"connectors/{Constants.ConnectorId7}");
                connector.Id.Should().Be(Constants.ConnectorId7);
                connector.IsEnabled.Should().BeTrue();
            }
        }

        [Test]
        public async Task DisableConnectorById_WrongGuid_ReturnsNotFound()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.PostAsync($"connectors/5fd6fbbd-21e1-4881-b87c-4e0e45b3ab05/disable", null);
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Test]
        public async Task EnableConnectorById_WrongGuid_ReturnsNotFound()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.PostAsync($"connectors/5fd6fbbd-21e1-4881-b87c-4e0e45b3ab05/enable", null);
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Test]
        public async Task GetConnectorForImportValidation_ReturnsOk()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetJsonAsync<ConnectorForImportValidationDto>($"sites/{Constants.SiteIdDefault}/connectors/{Constants.ConnectorIdForValidation}/forImportValidation");
                response.SiteId.Should().Be(Constants.SiteIdDefault);
                response.EntityIdByPointId.Should().BeEmpty();
                response.AllPointTypes.Should().BeEquivalentTo(new[] { 0, 1, 2, 3, 4, 5 });
                response.AllTagNames.Should().OnlyHaveUniqueItems();
                response.AllExternalPointsForSiteExcludingConnector.Should().NotBeEmpty();
                response.DeviceSchemaColumns.Should().NotBeEmpty();
                response.PointSchemaColumns.Should().NotBeEmpty();
                response.AllDeviceIds.Should().NotBeEmpty();
                response.AllEquipmentIds.Should().BeEmpty();
            }
        }

        [Test]
        public async Task GetConnectorForImportValidation_NotFirstImport_ReturnsOk()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetJsonAsync<ConnectorForImportValidationDto>($"sites/{Constants.SiteIdDefault}/connectors/{Constants.ConnectorIdForValidationNotFirst}/forImportValidation");
                response.SiteId.Should().Be(Constants.SiteIdDefault);
                response.EntityIdByPointId.Should().NotBeEmpty();
                response.AllPointTypes.Should().BeEquivalentTo(new[] { 0, 1, 2, 3, 4, 5 });
                response.AllTagNames.Should().OnlyHaveUniqueItems();
                response.AllExternalPointsForSiteExcludingConnector.Should().NotBeEmpty();
                response.DeviceSchemaColumns.Should().NotBeEmpty();
                response.PointSchemaColumns.Should().NotBeEmpty();
                response.AllDeviceIds.Should().NotBeEmpty();
                response.AllEquipmentIds.Should().NotBeEmpty();
            }
        }

        [Test]
        public async Task ExportByCustomerId_ReturnsOk()
        {
            using var client = IntegrationFixture.Server.CreateClientRandomUser();
            var customerId = Constants.ClientIdDefault;
            var response = await client.PostAsync($"customers/{customerId}/export", null);
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
