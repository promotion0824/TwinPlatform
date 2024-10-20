using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Features.Inspection;
using PlatformPortalXL.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Api.DataValidation;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Inspections
{
    public class UpdateInspectionZonesTests : BaseInMemoryTest
    {
        public UpdateInspectionZonesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateInspectionZone_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var inspectionZoneId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/inspectionZones/{inspectionZoneId}", new CreateInspectionZoneRequest { Name = "bob" });

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InspectonZoneExists_UpdateInspectionZone_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var inspectionZoneId = Guid.NewGuid();
            var request = new UpdateInspectionZoneRequest { Name = "Zone1" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/zones?includeStatistics=False")
                    .ReturnsJson(new InspectionZone[0]);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/zones/{inspectionZoneId}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/inspectionZones/{inspectionZoneId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task NameIsEmpty_UpdateInspectionZone_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var inspectionZoneId = Guid.NewGuid();
            var missingNameRequest = new UpdateInspectionZoneRequest();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/zones?includeStatistics=False")
                    .ReturnsJson(new InspectionZone[0]);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/zones/{inspectionZoneId}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/inspectionZones/{inspectionZoneId}", missingNameRequest);
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Name.Should().Be(nameof(missingNameRequest.Name));
                result.Items[0].Message.Should().Contain("Name is required");
            }
        }

        [Fact]
        public async Task NameTooLong_UpdateInspectionZone_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var inspectionZoneId = Guid.NewGuid();
            var request = Fixture.Build<CreateInspectionZoneRequest>()
                .With(x => x.Name, new string('n', 201))
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/zones?includeStatistics=False")
                    .ReturnsJson(new InspectionZone[0]);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/zones/{inspectionZoneId}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/inspectionZones/{inspectionZoneId}", request);
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Name.Should().Be(nameof(request.Name));
                result.Items[0].Message.Should().Contain("length");
            }
        }

        [Fact]
        public async Task DuplicateName_UpdateInspectionZone_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var inspectionZoneId = Guid.NewGuid();
            var request = Fixture.Create<CreateInspectionZoneRequest>();
            var duplicateNameZone = Fixture.Build<InspectionZone>().With(x => x.Name, request.Name).Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/zones?includeStatistics=False")
                    .ReturnsJson(new InspectionZone[] { duplicateNameZone });
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/zones/{inspectionZoneId}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/inspectionZones/{inspectionZoneId}", request);
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Name.Should().Be(nameof(request.Name));
                result.Items[0].Message.Should().Contain("Duplicate");
            }
        }
    }
}
