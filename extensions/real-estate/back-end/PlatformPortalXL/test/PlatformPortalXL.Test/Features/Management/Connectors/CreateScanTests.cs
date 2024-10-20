using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Features.Management;
using PlatformPortalXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Platform.Users;
using Willow.Api.DataValidation;
using System.Collections.Generic;

namespace PlatformPortalXL.Test.Features.Management.Connectors
{
    public class CreateScanTests : BaseInMemoryTest
    {
        public CreateScanTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task UserHasNoAccess_CreateScan_ReturnForbidden()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors/{connectorId}/scans", new CreateConnectorScanRequest());
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Theory]
        [InlineData(ScanStatus.New)]
        [InlineData(ScanStatus.Scanning)]
        public async Task ValidationFailed_CreateScan_ReturnUnprocessableEntity(ScanStatus incompleteScanStatus)
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var connector = Fixture.Build<Connector>()
                .With(x => x.Id, connectorId)
                .With(x => x.IsEnabled, true)
                .Create();
            var existingScans = Fixture.Build<ConnectorScan>()
                .With(x => x.Status, incompleteScanStatus)
                .CreateMany(1)
                .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors/{connectorId}")
                    .ReturnsJson(connector);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectors/{connectorId}/scans")
                    .ReturnsJson(existingScans);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors/{connectorId}/scans", new CreateConnectorScanRequest());
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var error = await response.Content.ReadAsAsync<ValidationError>();
                error.Items.Select(x => x.Name).Should().OnlyContain(x => x == "connectorId");
                error.Items.Select(x => x.Message).Should().BeEquivalentTo(new[]
                {
                    "Cannot create new Scan request while Connector is Enabled.",
                    "Cannot create new Scan request while other Scan requests not finished."
                });
            }
        }

        [Fact]
        public async Task ValidInput_CreateScan_ReturnCreatedScan()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var connectorTypeId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var connector = Fixture.Build<Connector>()
                .With(x => x.Id, connectorId)
                .With(x => x.IsEnabled, false)
                .With(x => x.ConnectorTypeId, connectorTypeId)
                .Create();
            var connectorType = Fixture.Build<ConnectorType>()
                .With(x => x.ScanConfigurationSchemaId, (Guid?)null)
                .Create();
            var existingScans = Fixture.Build<ConnectorScan>()
                .With(x => x.Status, ScanStatus.Finished)
                .CreateMany(1)
                .ToList();
            var userEmail = "current_user@willowinc.com";
            var user = Fixture.Build<User>()
                .With(x => x.Email, userEmail)
                .Create();
            var request = Fixture.Build<CreateConnectorScanRequest>()
                .Create();
            var createdScan = Fixture.Build<ConnectorScan>()
                .With(x => x.ErrorCount, (int?)null)
                .With(x => x.StartTime, (DateTime?)null)
                .With(x => x.EndTime, (DateTime?)null)
                .With(x => x.CreatedAt, DateTime.UtcNow.Date)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectortypes/{connectorTypeId}")
                    .ReturnsJson(connectorType);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors/{connectorId}")
                    .ReturnsJson(connector);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectors/{connectorId}/scans")
                    .ReturnsJson(existingScans);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(user);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Post, $"connectors/{connectorId}/scans"
                        , async message =>
                    {
                        var connectorRequest = await message.Content.ReadAsAsync<ConnectorScan>();
                        return connectorRequest.Message == request.Message &&
                               connectorRequest.DevicesToScan == request.DevicesToScan &&
                               connectorRequest.Configuration == request.Configuration &&
                               connectorRequest.CreatedBy == userEmail;
                    }
                        )
                    .ReturnsJson(createdScan);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors/{connectorId}/scans",
                    request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task InvalidScanConfiguration_CreateScan_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var connectorTypeId = Guid.NewGuid();
            var connector = Fixture.Build<Connector>()
                .With(x => x.Id, connectorId)
                .With(x => x.IsEnabled, false)
                .With(x => x.ConnectorTypeId, connectorTypeId)
                .Create();
            var request = new CreateConnectorScanRequest
            {
                Message = "Test",
                DevicesToScan = "100",
                Configuration = "{}"
            };
            var configurationSchemaId = Guid.NewGuid();
            var connectorType = Fixture.Build<ConnectorType>()
                .With(x => x.ScanConfigurationSchemaId, configurationSchemaId)
                .Create();
            var schemaColumns = new[]
            {
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "WhoisSegmentSize")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "TimeInterval")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "MinScanTime")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
            };
            var existingScans = Fixture.Build<ConnectorScan>()
                .With(x => x.Status, ScanStatus.Finished)
                .CreateMany(1)
                .ToList();
            var userId = Guid.NewGuid();
            var userEmail = "current_user@willowinc.com";
            var user = Fixture.Build<User>()
                .With(x => x.Email, userEmail)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectors/{connectorId}/scans")
                    .ReturnsJson(existingScans);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(user);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectortypes/{connectorTypeId}")
                    .ReturnsJson(connectorType);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"schemas/{configurationSchemaId}/SchemaColumns")
                    .ReturnsJson(schemaColumns);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors/{connectorId}")
                    .ReturnsJson(connector);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors/{connectorId}/scans", request);
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var error = await response.Content.ReadAsAsync<ValidationError>();
                error.Items.Should().BeEquivalentTo(new ValidationErrorItem[]{
                    new ValidationErrorItem
                    {
                        Name = "WhoisSegmentSize", Message = "WhoisSegmentSize should be greater than 0"
                    },
                    new ValidationErrorItem
                    {
                        Name = "MinScanTime", Message = "MinScanTime should be 60 or more"
                    },
                    new ValidationErrorItem
                    {
                        Name = "TimeInterval", Message = "TimeInterval should be greater than 0"
                    }
                });
            }
        }

        [Fact]
        public async Task ValidConfiguration_CreateScan_ReturnCreatedScan()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var connectorTypeId = Guid.NewGuid();
            var connector = Fixture.Build<Connector>()
                .With(x => x.Id, connectorId)
                .With(x => x.IsEnabled, false)
                .With(x => x.ConnectorTypeId, connectorTypeId)
                .Create();
            var request = new CreateConnectorScanRequest
            {
                Message = "Test",
                DevicesToScan = "100",
                Configuration = "{\"WhoisSegmentSize\":\"150\",\"MinScanTime\":60,\"TimeInterval\":1,\"InRangeOnly\":true}"
            };
            var configurationSchemaId = Guid.NewGuid();
            var connectorType = Fixture.Build<ConnectorType>()
                .With(x => x.ScanConfigurationSchemaId, configurationSchemaId)
                .Create();
            var schemaColumns = new[]
            {
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "WhoisSegmentSize")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "TimeInterval")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "MinScanTime")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
            };
            var existingScans = Fixture.Build<ConnectorScan>()
                .With(x => x.Status, ScanStatus.Finished)
                .CreateMany(1)
                .ToList();
            var userId = Guid.NewGuid();
            var userEmail = "current_user@willowinc.com";
            var user = Fixture.Build<User>()
                .With(x => x.Email, userEmail)
                .Create();
            var createdScan = Fixture.Build<ConnectorScan>()
                .With(x => x.ErrorCount, (int?)null)
                .With(x => x.StartTime, (DateTime?)null)
                .With(x => x.EndTime, (DateTime?)null)
                .With(x => x.CreatedAt, DateTime.UtcNow.Date)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectors/{connectorId}/scans")
                    .ReturnsJson(existingScans);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(user);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectortypes/{connectorTypeId}")
                    .ReturnsJson(connectorType);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"schemas/{configurationSchemaId}/SchemaColumns")
                    .ReturnsJson(schemaColumns);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors/{connectorId}")
                    .ReturnsJson(connector);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Post, $"connectors/{connectorId}/scans"
                        , async message =>
                    {
                        var connectorRequest = await message.Content.ReadAsAsync<ConnectorScan>();
                        return connectorRequest.Message == request.Message &&
                            connectorRequest.DevicesToScan == request.DevicesToScan &&
                            connectorRequest.Configuration == request.Configuration &&
                            connectorRequest.CreatedBy == userEmail;
                    }
                        )
                    .ReturnsJson(createdScan);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors/{connectorId}/scans", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task InvalidDecimalNumberScanConfiguration_CreateScan_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var connectorTypeId = Guid.NewGuid();
            var connector = Fixture.Build<Connector>()
                .With(x => x.Id, connectorId)
                .With(x => x.IsEnabled, false)
                .With(x => x.ConnectorTypeId, connectorTypeId)
                .Create();
            var request = new CreateConnectorScanRequest
            {
                Message = "Test",
                DevicesToScan = "100",
                Configuration = "{\"WhoisSegmentSize\":\"0.5\",\"MinScanTime\":60,\"TimeInterval\":1,\"InRangeOnly\":true}"
            };
            var configurationSchemaId = Guid.NewGuid();
            var connectorType = Fixture.Build<ConnectorType>()
                .With(x => x.ScanConfigurationSchemaId, configurationSchemaId)
                .Create();
            var schemaColumns = new[]
            {
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "WhoisSegmentSize")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "TimeInterval")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "MinScanTime")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
            };
            var existingScans = Fixture.Build<ConnectorScan>()
                .With(x => x.Status, ScanStatus.Finished)
                .CreateMany(1)
                .ToList();
            var userId = Guid.NewGuid();
            var userEmail = "current_user@willowinc.com";
            var user = Fixture.Build<User>()
                .With(x => x.Email, userEmail)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectors/{connectorId}/scans")
                    .ReturnsJson(existingScans);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(user);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectortypes/{connectorTypeId}")
                    .ReturnsJson(connectorType);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"schemas/{configurationSchemaId}/SchemaColumns")
                    .ReturnsJson(schemaColumns);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors/{connectorId}")
                    .ReturnsJson(connector);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors/{connectorId}/scans", request);
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var error = await response.Content.ReadAsAsync<ValidationError>();
                error.Items.Should().BeEquivalentTo(new List<ValidationErrorItem>() {
                    new() { Name = "WhoisSegmentSize", Message = "WhoisSegmentSize is invalid" }
                });
            }
        }
        
        [Fact]
        public async Task InvalidNegativeNumberScanConfiguration_CreateScan_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var connectorTypeId = Guid.NewGuid();
            var connector = Fixture.Build<Connector>()
                .With(x => x.Id, connectorId)
                .With(x => x.IsEnabled, false)
                .With(x => x.ConnectorTypeId, connectorTypeId)
                .Create();
            var request = new CreateConnectorScanRequest
            {
                Message = "Test",
                DevicesToScan = "100",
                Configuration = "{\"WhoisSegmentSize\":100,\"TimeInterval\":-1,\"MinScanTime\":60}"
            };
            var configurationSchemaId = Guid.NewGuid();
            var connectorType = Fixture.Build<ConnectorType>()
                .With(x => x.ScanConfigurationSchemaId, configurationSchemaId)
                .Create();
            var schemaColumns = new[]
            {
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "WhoisSegmentSize")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "TimeInterval")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "MinScanTime")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
            };
            var existingScans = Fixture.Build<ConnectorScan>()
                .With(x => x.Status, ScanStatus.Finished)
                .CreateMany(1)
                .ToList();
            var userId = Guid.NewGuid();
            var userEmail = "current_user@willowinc.com";
            var user = Fixture.Build<User>()
                .With(x => x.Email, userEmail)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectors/{connectorId}/scans")
                    .ReturnsJson(existingScans);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(user);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectortypes/{connectorTypeId}")
                    .ReturnsJson(connectorType);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"schemas/{configurationSchemaId}/SchemaColumns")
                    .ReturnsJson(schemaColumns);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors/{connectorId}")
                    .ReturnsJson(connector);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors/{connectorId}/scans", request);
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var error = await response.Content.ReadAsAsync<ValidationError>();
                error.Items.Should().BeEquivalentTo(new List<ValidationErrorItem>() {
                    new() { Name = "TimeInterval", Message = "TimeInterval should be greater than 0" }});
            }
        }

        [Fact]
        public async Task InvalidMinNumberScanConfiguration_CreateScan_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var connectorId = Guid.NewGuid();
            var connectorTypeId = Guid.NewGuid();
            var connector = Fixture.Build<Connector>()
                .With(x => x.Id, connectorId)
                .With(x => x.IsEnabled, false)
                .With(x => x.ConnectorTypeId, connectorTypeId)
                .Create();
            var request = new CreateConnectorScanRequest
            {
                Message = "Test",
                DevicesToScan = "100",
                Configuration = "{\"WhoisSegmentSize\":100,\"TimeInterval\":1,\"MinScanTime\":50}"
            };
            var configurationSchemaId = Guid.NewGuid();
            var connectorType = Fixture.Build<ConnectorType>()
                .With(x => x.ScanConfigurationSchemaId, configurationSchemaId)
                .Create();
            var schemaColumns = new[]
            {
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "WhoisSegmentSize")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "TimeInterval")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
                Fixture.Build<ConnectorTypeColumn>()
                    .With(x => x.Name, "MinScanTime")
                    .With(x => x.IsRequired, true)
                    .With(x => x.DataType, "Number")
                    .Create(),
            };
            var existingScans = Fixture.Build<ConnectorScan>()
                .With(x => x.Status, ScanStatus.Finished)
                .CreateMany(1)
                .ToList();
            var userId = Guid.NewGuid();
            var userEmail = "current_user@willowinc.com";
            var user = Fixture.Build<User>()
                .With(x => x.Email, userEmail)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(Guid.NewGuid(), Permissions.ManageConnectors, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectors/{connectorId}/scans")
                    .ReturnsJson(existingScans);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(user);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"connectortypes/{connectorTypeId}")
                    .ReturnsJson(connectorType);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"schemas/{configurationSchemaId}/SchemaColumns")
                    .ReturnsJson(schemaColumns);
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/connectors/{connectorId}")
                    .ReturnsJson(connector);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/connectors/{connectorId}/scans", request);
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var error = await response.Content.ReadAsAsync<ValidationError>();
                error.Items.Should().BeEquivalentTo(new List<ValidationErrorItem>() {
                    new() { Name = "MinScanTime", Message = "MinScanTime should be 60 or more" } });
            }
        }
    }
}
