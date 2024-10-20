using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;

using Willow.Api.DataValidation;
using Willow.Platform.Models;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Workflow.Reporters
{
    public class CreateReporterTests : BaseInMemoryTest
    {
        public CreateReporterTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task MissingFields_CreateReporter_ReturnsError()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<UpdateReporterRequest>()
                .Without(x => x.Email)
                .Without(x => x.Phone)
                .Without(x => x.Name)
                .Without(x => x.Company)
                .Create();
            var createdReporter = Fixture.Create<Reporter>();
            var expectedRequestToWorkflowApi = new WorkflowCreateReporterRequest
            {
                CustomerId = site.CustomerId,
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                Company = request.Company
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{site.Id}/reporters", expectedRequestToWorkflowApi)
                    .ReturnsJson(createdReporter);

                var response = await client.PostAsJsonAsync($"sites/{site.Id}/reporters", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(3);
            }
        }

        [Fact]
        public async Task ValidInput_CreateReporter_ReturnsCreatedReporter()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateReporterRequest>()
                .With(x => x.Phone, "+1713131212")
                .With(x => x.Email, "test@site.com")
                .Create();
            var createdReporter = Fixture.Create<Reporter>();
            var expectedRequestToWorkflowApi = new WorkflowCreateReporterRequest
            {
                CustomerId = site.CustomerId,
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                Company = request.Company
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{site.Id}/reporters", expectedRequestToWorkflowApi)
                    .ReturnsJson(createdReporter);

                var response = await client.PostAsJsonAsync($"sites/{site.Id}/reporters", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ReporterDto>();
                result.Should().BeEquivalentTo(ReporterDto.MapFromModel(createdReporter));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateReporter_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/reporters", new CreateReporterRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InvalidInput_CreateReporter_ReturnsErrors()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateReporterRequest>()
                .With(x => x.Phone, "Invalid Phone")
                .With(x => x.Email, "Invalid Email")
                .Without(x => x.Company)
                .Without(x => x.Name)
                .Create();
            var createdReporter = Fixture.Create<Reporter>();
            var expectedRequestToWorkflowApi = new WorkflowCreateReporterRequest
            {
                CustomerId = site.CustomerId,
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                Company = request.Company
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{site.Id}/reporters", expectedRequestToWorkflowApi)
                    .ReturnsJson(createdReporter);

                var response = await client.PostAsJsonAsync($"sites/{site.Id}/reporters", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain($"Email is invalid");
                result.Should().Contain($"Phone is invalid");
                result.Should().Contain($"Name is required");
            }
        }
    }
}