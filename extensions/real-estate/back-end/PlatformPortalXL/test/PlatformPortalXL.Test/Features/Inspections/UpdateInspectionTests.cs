using FluentAssertions;
using PlatformPortalXL.Features.Inspection;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using System.Net.Http;
using PlatformPortalXL.Dto;
using System.Linq;

using Willow.Api.DataValidation;
using Willow.Platform.Models;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Inspections
{
    public class UpdateInspectionTests : BaseInMemoryTest
    {
        private UpdateInspectionRequest _request = new UpdateInspectionRequest
        {
            Name             = "bob",
            StartDate        = "2011-11-11T11:11:11",
            Frequency = 4,
            FrequencyUnit = SchedulingUnit.Hours,
            Checks = new List<UpdateCheckRequest>
            {
                new UpdateCheckRequest
                {
                    Name = "bob",
                    Type = CheckType.Numeric,
                    TypeValue = "3",
                    DecimalPlaces = 2
                }
            }
        };

        public UpdateInspectionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateInspection_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/inspections/{Guid.NewGuid()}", _request);

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task SiteDoesNotExist_UpdateInspection_ReturnsNotFound()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<UpdateInspectionRequest>()
                                 .With(x=> x.StartDate, "2021-01-01T00:00:00")
                                 .Without(x=> x.EndDate)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson((Site)null);

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/inspections/{Guid.NewGuid()}", request);

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task MissingFields_UpdateInspection_ReturnsError()
        {
            var utcNow = DateTime.UtcNow;
            var site = Fixture.Create<Site>();
            var inspectionId = Guid.NewGuid();            
            var firstCheckRequest = Fixture.Build<UpdateCheckRequest>()
                                        .Without(c => c.Name)
                                        .Without(c => c.Type)
                                        .Without(c => c.TypeValue)
                                        .Without(c => c.DecimalPlaces)
                                        .Without(c => c.DependencyName)
                                        .Without(c => c.DependencyValue)
                                        .Without(c => c.PauseStartDate)
                                        .Create();

            var secondCheckRequest = Fixture.Build<UpdateCheckRequest>()
                                                .Without(c => c.Id)
                                                .With(c => c.Type, CheckType.Numeric)
                                                .Without(c => c.DecimalPlaces)
                                                .Without(c => c.PauseStartDate)
                                                .Create();
            _request.Checks = new List<UpdateCheckRequest> { firstCheckRequest, secondCheckRequest };

            var inspection = Fixture.Build<Inspection>()
                                            .With(i => i.Id, inspectionId)
                                            .With(i => i.StartDate, utcNow.AddDays(-1).ToString("s"))
                                            .Without(x=> x.EndDate)
                                            .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/inspections/{inspectionId}")
                    .ReturnsJson(inspection);

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/inspections/{inspectionId}", _request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(4);
            }
        }

        [Fact]
        public async Task MissingChecks_UpdateInspection_ReturnsError()
        {
            var utcNow = DateTime.UtcNow;
            var site = Fixture.Create<Site>();
            var inspectionId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInspectionRequest>()
                .Without(i => i.Checks)
                .Without(i => i.EndDate)
                .With( i=> i.StartDate, utcNow.ToString("s"))
                .Create();

            var inspection = Fixture.Build<Inspection>()
                                            .With(i => i.Id, inspectionId)
                                            .With(i => i.StartDate, utcNow.AddDays(1).ToString("s"))
                                            .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/inspections/{inspectionId}")
                    .ReturnsJson(inspection);

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/inspections/{inspectionId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Message.Should().Be("Checks is required");
            }
        }

        [Fact]
        public async Task OnlyPausedChecks_UpdateInspection_ReturnsUpdatedInspection()
        {
            var utcNow = DateTime.UtcNow;
            var site = Fixture.Create<Site>();
            var inspectionId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInspectionRequest>()
                                    .With(i => i.Frequency, 1)
                                    .Without(i => i.Checks)
                                    .Without(i => i.EndDate)
                                    .With( i=> i.StartDate, utcNow.ToString("s"))
                                    .Create();

            request.Checks = Fixture.Build<UpdateCheckRequest>()
                                    .Without(c => c.DependencyName)
                                    .Without(c => c.DependencyValue)
                                    .CreateMany(3).ToList();

            var updatedInspection = Fixture.Create<Inspection>();

            var inspection = Fixture.Build<Inspection>()
                                            .With(i => i.Id, inspectionId)
                                            .With(i => i.StartDate, utcNow.AddDays(1).ToString("s"))
                                            .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/inspections/{inspectionId}")
                    .ReturnsJson(inspection);

                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{site.Id}/inspections/{inspectionId}", request)
                    .ReturnsJson(updatedInspection);

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/inspections/{inspectionId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionDto>();
                result.Should().BeEquivalentTo(InspectionDto.MapFromModel(updatedInspection));
            }
        }

        [Fact]
        public async Task ValidInput_UpdateInspection_ReturnsUpdatedInspection()
        {
            var site = Fixture.Create<Site>();
            var inspectionId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var request = Fixture.Build<UpdateInspectionRequest>()
                                    .With(i => i.Frequency, 1)
                                    .Without(i => i.Checks)
                                    .With( i=> i.StartDate, utcNow.ToString("s"))
                                    .Without(i => i.EndDate)
                                    .Create();
            var listCheckRequest = Fixture.Build<UpdateCheckRequest>()
                                                .With(c => c.Type, CheckType.List)
                                                .With(c => c.TypeValue, "Yes|No|Auto")
                                                .Without(c => c.DecimalPlaces)
                                                .Without(c => c.DependencyName)
                                                .Without(c => c.DependencyValue)
                                                .Without(c => c.PauseStartDate)
                                                .Create();

            var numericCheckRequest = Fixture.Build<UpdateCheckRequest>()
                                                .Without(c => c.Id)
                                                .Without(c => c.PauseStartDate)
                                                .With(c => c.Type, CheckType.Numeric)
                                                .With(c => c.DecimalPlaces, 2)
                                                .With(c => c.DependencyName, listCheckRequest.Name)
                                                .With(c => c.DependencyValue, "No")
                                                .Create();
            var dateCheckRequest = Fixture.Build<UpdateCheckRequest>()
                                    .With(c => c.Type, CheckType.Date)
                                    .Without(c => c.TypeValue)
                                    .Without(c => c.DecimalPlaces)
                                    .Without(c => c.DependencyName)
                                    .Without(c => c.DependencyValue)
                                    .Without(c => c.PauseStartDate)
                                    .Create();
            request.Checks = new List<UpdateCheckRequest> { listCheckRequest, numericCheckRequest, dateCheckRequest };
            var updatedInspection = Fixture.Create<Inspection>();

            var inspection = Fixture.Build<Inspection>()
                                            .With(i => i.Id, inspectionId)
                                            .With(i => i.StartDate, utcNow.AddDays(1).ToString("s"))
                                            .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/inspections/{inspectionId}")
                    .ReturnsJson(inspection);

                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{site.Id}/inspections/{inspectionId}", request)
                    .ReturnsJson(updatedInspection);

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/inspections/{inspectionId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionDto>();
                result.Should().BeEquivalentTo(InspectionDto.MapFromModel(updatedInspection));
            }
        }

        [Fact]
        public async Task DuplicateChecksName_UpdateInspection_ReturnsError()
        {
            var site = Fixture.Create<Site>();
            var inspectionId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var request = Fixture.Build<UpdateInspectionRequest>()
                                    .Without(i => i.Checks)
                                    .With( i=> i.StartDate, utcNow.ToString("s"))
                                    .Without(i => i.EndDate)
                                    .Create();
            var firstCheckRequest = Fixture.Build<UpdateCheckRequest>()
                                                .With(c => c.Name, "CHECK")
                                                .With(c => c.Type, CheckType.List)
                                                .With(c => c.TypeValue, "Yes|No|Auto")
                                                .Without(c => c.DecimalPlaces)
                                                .Without(c => c.DependencyName)
                                                .Without(c => c.DependencyValue)
                                                .Without(c => c.PauseStartDate)
                                                .Create();

            var secondCheckRequest = Fixture.Build<UpdateCheckRequest>()
                                                .Without(c => c.Id)
                                                .Without(c => c.PauseStartDate)
                                                .With(c => c.Name, "check")
                                                .With(c => c.Type, CheckType.Numeric)
                                                .With(c => c.DecimalPlaces, 2)
                                                .With(c => c.DependencyName, firstCheckRequest.Name)
                                                .With(c => c.DependencyValue, "No")
                                                .Create();
            request.Checks = new List<UpdateCheckRequest> { firstCheckRequest, secondCheckRequest };
            var updatedInspection = Fixture.Create<Inspection>();

            var inspection = Fixture.Build<Inspection>()
                                            .With(i => i.Id, inspectionId)
                                            .With(i => i.StartDate, utcNow.AddDays(1).ToString("s"))
                                            .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/inspections/{inspectionId}")
                    .ReturnsJson(inspection);

                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{site.Id}/inspections/{inspectionId}", request)
                    .ReturnsJson(updatedInspection);

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/inspections/{inspectionId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Message.Should().Be("Name must be unique");
            }
        }

        [Fact]
        public async Task InvalidEndDate_UpdateInspection_ReturnsError()
        {
            var utcNow = DateTime.UtcNow;
            var site = Fixture.Create<Site>();
            var inspectionId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInspectionRequest>()
                                    .Without(i => i.Checks)
                                    .With(i => i.StartDate, utcNow.AddDays(1).ToString("s"))
                                    .With(i => i.EndDate, utcNow.ToString("s"))
                                    .Create();

            request.Checks = Fixture.Build<UpdateCheckRequest>()
                                    .Without(c => c.PauseStartDate)
                                    .Without(c => c.DependencyName)
                                    .Without(c => c.DependencyValue)
                                    .CreateMany(3).ToList();

            var inspection = Fixture.Build<Inspection>()
                                            .With(i => i.Id, inspectionId)
                                            .With(i => i.StartDate, utcNow.AddDays(1).ToString("s"))
                                            .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/inspections/{inspectionId}")
                    .ReturnsJson(inspection);

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/inspections/{inspectionId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Message.Should().Be("EndDate cannot be before start date");
            }
        }

        [Fact]
        public async Task ContainDuplicatesInTypeValueChecks_UpdateInspection_ReturnsError()
        {
            var utcNow = DateTime.UtcNow;
            var site = Fixture.Create<Site>();
            var inspectionId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInspectionRequest>()
                                    .Without(i => i.Checks)
                                    .With( i=> i.StartDate, utcNow.ToString("s"))
                                    .Without(i => i.EndDate)
                                    .With( i=> i.StartDate, utcNow.ToString("s"))
                                    .Create();
            var CheckRequest = Fixture.Build<UpdateCheckRequest>()
                                                .With(c => c.Type, CheckType.List)
                                                .With(c => c.TypeValue, "Yes|No|Auto|yEs")
                                                .Without(c => c.DecimalPlaces)
                                                .Without(c => c.DependencyName)
                                                .Without(c => c.DependencyValue)
                                                .Without(c => c.PauseStartDate)
                                                .Create();
            request.Checks = new List<UpdateCheckRequest> { CheckRequest };

            var inspection = Fixture.Build<Inspection>()
                                            .With(i => i.Id, inspectionId)
                                            .With(i => i.StartDate, utcNow.AddDays(1).ToString("s"))
                                            .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/inspections/{inspectionId}")
                    .ReturnsJson(inspection);

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/inspections/{inspectionId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Message.Should().Be("Checks[0].TypeValue contains duplicates");
            }
        }

        [Fact]
        public async Task PauseRootCheck_UpdateInspection_ReturnsUpdatedInspection()
        {
            var utcNow = DateTime.UtcNow;
            var site = Fixture.Create<Site>();
            var inspectionId = Guid.NewGuid();
            var request = Fixture.Build<UpdateInspectionRequest>()
                                    .With(i => i.Frequency, 1)
                                    .Without(i => i.Checks)
                                    .With( i=> i.StartDate, utcNow.ToString("s"))
                                    .Without(i => i.EndDate)
                                    .Create();

            var rootCheckRequest = Fixture.Build<UpdateCheckRequest>()
                                                .With(c => c.Type, CheckType.List)
                                                .With(c => c.TypeValue, "Yes|No|Auto")
                                                .Without(c => c.DependencyId)
                                                .Without(c => c.DecimalPlaces)
                                                .Without(c => c.DependencyName)
                                                .Without(c => c.DependencyValue)
                                                .With(c => c.PauseStartDate, utcNow)
                                                .With(c => c.PauseEndDate, utcNow.AddDays(3))
                                                .Create();

            var childCheckRequest = Fixture.Build<UpdateCheckRequest>()
                                    .With(c => c.Type, CheckType.List)
                                    .With(c => c.TypeValue, "Yes|No|Auto")
                                    .Without(c => c.DecimalPlaces)
                                    .Without(c => c.DependencyName)
                                    .Without(c => c.DependencyValue)
                                    .Without(c => c.PauseStartDate)
                                    .Without(c => c.PauseEndDate)
                                    .With(c => c.PauseStartDate, utcNow)
                                    .With(c => c.PauseEndDate, utcNow.AddDays(3))
                                    .With(c => c.DependencyId, rootCheckRequest.Id)
                                    .Create();

            request.Checks = new List<UpdateCheckRequest> { rootCheckRequest, childCheckRequest };

            var inspection = Fixture.Build<Inspection>()
                                            .With(i => i.Id, inspectionId)
                                            .With(i => i.StartDate, utcNow.AddDays(1).ToString("s"))
                                            .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/inspections/{inspectionId}")
                    .ReturnsJson(inspection);

                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{site.Id}/inspections/{inspectionId}", request)
                    .ReturnsJson(inspection);

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/inspections/{inspectionId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionDto>();
                result.Should().BeEquivalentTo(InspectionDto.MapFromModel(inspection));
            }
        }
    }
}
