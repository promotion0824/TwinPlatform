using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Management;
using PlatformPortalXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Api.DataValidation;
using Willow.Platform.Models;

namespace PlatformPortalXL.Test.Features.Management.Connectors
{
    public class CreateConnectorTests : BaseInMemoryTest
    {
        public CreateConnectorTests(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public async Task UserHasNoAccess_CreateConnector_ReturnForbidden()
        {
            var siteId = Guid.NewGuid();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors", new CreateConnectorRequest());
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
        [Fact]
        public async Task IncorrectConnectionType_CreateConnector_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var request = new CreateConnectorRequest
            {
                Configuration = "{}", Name = "test", ConnectionType = "blabla", ConnectorTypeId = Guid.NewGuid()
            };
            var configurationSchemaId = Guid.NewGuid();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectortypes/{request.ConnectorTypeId}")
                    .ReturnsJson(new ConnectorType{ConnectorConfigurationSchemaId = configurationSchemaId});
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"schemas/{configurationSchemaId}/SchemaColumns")
                    .ReturnsJson(new List<ConnectorTypeColumn>());

                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors", request);
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().BeEquivalentTo(new List<ValidationErrorItem>() {
                    new() {Name = nameof(request.ConnectionType), Message = "Connection Type is out of allowed values"}});
            }
        }
        
        [Fact]
        public async Task IncorrectInputWithEmptyConnectorType_CreateConnector_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var request = new CreateConnectorRequest
            {
                Configuration = "{}", Name = "", ConnectionType = "", ConnectorTypeId = Guid.Empty
            };
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {

                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors", request);
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().BeEquivalentTo(new List<ValidationErrorItem>() {
                    new() {Name = nameof(request.Name), Message = "Name is required"},
                    new() {Name = nameof(request.ConnectionType), Message = "Connection Type is required"},
                    new() {Name = "ConnectorType", Message = "Connector Type is required"}
                    });
            }
        }
        [Fact]
        public async Task InvalidConnectorConfiguration_CreateConnector_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var site = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .Create();
            var connectorTypeId = Guid.NewGuid();
            var request = new CreateConnectorRequest
            {
                Configuration = "{}", Name = "test", ConnectionType = "iotedge", ConnectorTypeId = connectorTypeId
            };
            var configurationSchemaId = Guid.NewGuid();
            var connectorType = Fixture.Build<ConnectorType>()
                .With(x => x.ConnectorConfigurationSchemaId, configurationSchemaId)
                .Create();
            var schemaColumns = new[]
            {
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "RequiredColumn")
                    .With(x => x.IsRequired, true)
                    .Create(),
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "OptionalColumn")
                    .With(x => x.IsRequired, false)
                    .Create(),
            };
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectortypes/{connectorTypeId}")
                    .ReturnsJson(connectorType);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"schemas/{configurationSchemaId}/SchemaColumns")
                    .ReturnsJson(schemaColumns);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors", request);
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().BeEquivalentTo(new List<ValidationErrorItem>() {
                    new() {Name = "RequiredColumn", Message = "RequiredColumn is required"}});
            }
        }

        [Fact]
        public async Task ValidInput_CreateConnector_CreatesConnector()
        {
            var siteId = Guid.NewGuid();
            var site = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .Create();
            var connectorTypeId = Guid.NewGuid();
            var request = new CreateConnectorRequest
            {
                Configuration = "{\"UserName\":\"blabla\", \"Password\": 1}", Name = "test", ConnectionType = "iotedge", ConnectorTypeId = connectorTypeId
            };
            var createdConnector = Fixture.Build<Connector>().Create();
            var configurationSchemaId = Guid.NewGuid();
            var connectorType = Fixture.Build<ConnectorType>()
                .With(x => x.ConnectorConfigurationSchemaId, configurationSchemaId)
                .Create();
            var schemaColumns = Fixture.Build<ConnectorTypeColumn>()
                .With(x => x.Name, "UserName")
                .With(x => x.IsRequired, true)
                .CreateMany(1)
                .ToList();

            var expectedLastUpdated = DateTime.UtcNow;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var connectorApi = server.Arrange().GetConnectorApi();

                connectorApi
                    .SetupRequest(HttpMethod.Get, $"connectortypes/{connectorTypeId}")
                    .ReturnsJson(connectorType);
                connectorApi
                    .SetupRequest(HttpMethod.Get, $"schemas/{configurationSchemaId}/SchemaColumns")
                    .ReturnsJson(schemaColumns);
                server.Arrange().GetSiteApi()
                     .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);
                connectorApi
                    .SetupRequest(HttpMethod.Post, "connectors")
                    .ReturnsJson(createdConnector);

                var expectedConnectorLogRecord = Fixture.Build<ConnectorLogRecord>()
                                                                       .With(x => x.ConnectorId, createdConnector.Id)
                                                                       .With(x => x.StartTime, expectedLastUpdated)
                                                                       .With(x => x.CreatedAt, expectedLastUpdated)
                                                                       .With(x => x.EndTime, expectedLastUpdated.AddMinutes(5))
                                                                       .With(x => x.PointCount, 1000)
                                                                       .With(x => x.ErrorCount, 0)
                                                                       .Create();

                connectorApi.SetupRequest(HttpMethod.Get, $"connectors/{createdConnector.Id}/logs/latest?count=1&includeErrors={true}&source=Connector")
                                   .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecord });

                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ConnectorDto>();
                var mappedConnectorDto = ConnectorDto.MapFrom(createdConnector);
                mappedConnectorDto.Status = PortfolioDashboardConnectorStatus.MapStatus(createdConnector, expectedConnectorLogRecord, true);
                
                result.Should().BeEquivalentTo(mappedConnectorDto);
            }
        }
    }
}
