using System;
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
using System.Collections.Generic;

namespace PlatformPortalXL.Test.Features.Management.Connectors
{
    public class UpdateConnectorTests : BaseInMemoryTest
    {
        public UpdateConnectorTests(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public async Task UserHasNoAccess_UpdateConnector_ReturnForbidden()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/connectors/{connectorId}", new UpdateConnectorRequest());
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
                        
        [Fact]
        public async Task InvalidConnectorConfiguration_UpdateConnector_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var connectorTypeId = Guid.NewGuid();
            var request = new UpdateConnectorRequest { Configuration = "{}"};
            var configurationSchemaId = Guid.NewGuid();
            var existingConnector = Fixture.Build<Connector>()
                .With(x => x.Id, connectorId)
                .With(x => x.ConnectorTypeId, connectorTypeId)
                .Create();
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
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors/{connectorId}")
                    .ReturnsJson(existingConnector);

                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectortypes/{connectorTypeId}")
                    .ReturnsJson(connectorType);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"schemas/{configurationSchemaId}/SchemaColumns")
                    .ReturnsJson(schemaColumns);
                
                var response = await client.PutAsJsonAsync($"sites/{siteId}/connectors/{connectorId}", request);
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().BeEquivalentTo(new List<ValidationErrorItem>() {
                    new() {Name = "RequiredColumn", Message = "RequiredColumn is required"}});
            }
        }
        
        [Fact]
        public async Task ValidInputOnlyConfiguration_UpdateConnector_UpdatesConnector()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var connectorTypeId = Guid.NewGuid();
            var request = new UpdateConnectorRequest
            {
                Configuration = "{\"UserName\":\"blabla\", \"Password\": 1}",
                Name = "test2"
            };
            var existingConnector = Fixture.Build<Connector>()
                .With(x => x.Id, connectorId)
                .With(x => x.ConnectorTypeId, connectorTypeId)
                .With(x => x.Name, "test1")
                .Create();
            var updatedConnector =  Fixture.Build<Connector>()
                .With(x => x.Id, connectorId)
                .With(x => x.Name, "test2")
                .Create();
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
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors/{connectorId}")
                    .ReturnsJson(existingConnector);
                connectorApi
                    .SetupRequest(HttpMethod.Get, $"connectortypes/{connectorTypeId}")
                    .ReturnsJson(connectorType);
                connectorApi
                    .SetupRequest(HttpMethod.Get, $"schemas/{configurationSchemaId}/SchemaColumns")
                    .ReturnsJson(schemaColumns);

                connectorApi
                    .SetupRequest(HttpMethod.Put, "connectors", async message =>
                    {
                        var requestContent = await message.Content.ReadAsStringAsync();
                        var parsedRequest = ParseFromFormData(requestContent);
                        return connectorId == parsedRequest.Id &&
                               request.Configuration == parsedRequest.Configuration  &&
                               request.Name == parsedRequest.Name;
                    })
                    .ReturnsJson(updatedConnector);

                var expectedConnectorLogRecord = Fixture.Build<ConnectorLogRecord>()
                                                                     .With(x => x.ConnectorId, updatedConnector.Id)
                                                                     .With(x => x.StartTime, expectedLastUpdated)
                                                                     .With(x => x.CreatedAt, expectedLastUpdated)
                                                                     .With(x => x.EndTime, expectedLastUpdated.AddMinutes(5))
                                                                     .With(x => x.PointCount, 1000)
                                                                     .With(x => x.ErrorCount, 0)
                                                                     .Create();

                connectorApi.SetupRequest(HttpMethod.Get, $"connectors/{updatedConnector.Id}/logs/latest?count=1&includeErrors={true}&source=Connector")
                                   .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecord });

                var response = await client.PutAsJsonAsync($"sites/{siteId}/connectors/{connectorId}", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ConnectorDto>();
                var mappedConnectorDto = ConnectorDto.MapFrom(updatedConnector);
                mappedConnectorDto.Status = PortfolioDashboardConnectorStatus.MapStatus(updatedConnector, expectedConnectorLogRecord, true);

                result.Should().BeEquivalentTo(mappedConnectorDto);
            }
        }

        [Fact]
        public async Task ValidInputWithoutConfiguration_UpdateConnector_UpdatesConnector()
        {
            var connectorName = "Test1";
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var connectorTypeId = Guid.NewGuid();
            var request = new UpdateConnectorRequest
            {
                Name = connectorName,
                ErrorThreshold = 20,
                IsLoggingEnabled = true
            };
            var existingConnector = Fixture.Build<Connector>()
                .With(x => x.Id, connectorId)
                .With(x => x.ConnectorTypeId, connectorTypeId)
                .With(x => x.Name, connectorName)
                .Create();
            var updatedConnector =  Fixture.Build<Connector>()
                .With(x => x.Id, connectorId)
                .With(x => x.Name, connectorName)
                .Create();

            var expectedLastUpdated = DateTime.UtcNow;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite( Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var connectorApi = server.Arrange().GetConnectorApi();

                connectorApi
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors/{connectorId}")
                    .ReturnsJson(existingConnector);

                connectorApi
                    .SetupRequest(HttpMethod.Put, "connectors", async message =>
                    {
                        var requestContent = await message.Content.ReadAsStringAsync();
                        var parsedRequest = ParseFromFormData(requestContent);
                        return connectorId == parsedRequest.Id;
                    })
                    .ReturnsJson(updatedConnector);

                var expectedConnectorLogRecord = Fixture.Build<ConnectorLogRecord>()
                                                                   .With(x => x.ConnectorId, updatedConnector.Id)
                                                                   .With(x => x.StartTime, expectedLastUpdated)
                                                                   .With(x => x.CreatedAt, expectedLastUpdated)
                                                                   .With(x => x.EndTime, expectedLastUpdated.AddMinutes(5))
                                                                   .With(x => x.PointCount, 1000)
                                                                   .With(x => x.ErrorCount, 0)
                                                                   .Create();

                connectorApi.SetupRequest(HttpMethod.Get, $"connectors/{updatedConnector.Id}/logs/latest?count=1&includeErrors={true}&source=Connector")
                                   .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecord });

                var response = await client.PutAsJsonAsync($"sites/{siteId}/connectors/{connectorId}", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ConnectorDto>();
                var mappedConnectorDto = ConnectorDto.MapFrom(updatedConnector);
                mappedConnectorDto.Status = PortfolioDashboardConnectorStatus.MapStatus(updatedConnector, expectedConnectorLogRecord, true);

                result.Should().BeEquivalentTo(mappedConnectorDto);
            }
        }

        private static Connector ParseFromFormData(string formData)
        {
            var parameters = formData.Split("&")
                .Select(x => x.Split("="))
                .ToDictionary(x => x[0], x => x[1]);
            var instance = new Connector();
            var propertiesByName = typeof(Connector).Properties().ToDictionary(x => x.Name);
            foreach (var (propertyName, propertyValue) in parameters)
            {
                var property = propertiesByName[propertyName];
                if (property.PropertyType == typeof(Guid))
                {
                    property.SetValue(instance, Guid.Parse(propertyValue));
                }else if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(instance, bool.Parse(propertyValue));
                }else if (property.PropertyType == typeof(int))
                {
                    property.SetValue(instance, int.Parse(propertyValue));
                }else if (property.PropertyType == typeof(string))
                {
                    property.SetValue(instance, WebUtility.UrlDecode(propertyValue));
                }
                else
                {
                    throw new NotSupportedException($"Type {property.PropertyType} is not supported");
                }
            }

            return instance;
        }

    }
}
