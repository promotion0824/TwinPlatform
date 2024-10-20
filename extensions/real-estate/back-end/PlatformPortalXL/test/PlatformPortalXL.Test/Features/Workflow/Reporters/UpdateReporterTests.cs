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
using PlatformPortalXL.Features.Workflow;
using System.Net.Http.Json;

using Willow.Api.DataValidation;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Workflow.Reporters
{
    public class UpdateReporterTests : BaseInMemoryTest
    {
        public UpdateReporterTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ValidInput_UpdateReporter_ReturnsUpdatedReporter()
        {
            var siteId = Guid.NewGuid();
            var reporterId = Guid.NewGuid();
            var request = Fixture.Build<UpdateReporterRequest>()
                .With(x => x.Email, "test@site.com")
                .With(x => x.Phone, "+61 123455678")
                .Create();
            var updatedReporter = Fixture.Create<Reporter>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{siteId}/reporters/{reporterId}", request)
                    .ReturnsJson(updatedReporter);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/reporters/{reporterId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ReporterDto>();
                result.Should().BeEquivalentTo(ReporterDto.MapFromModel(updatedReporter));
            }
        }

        [Fact]
        public async Task MissingFields_UpdateReporter_ReturnsError()
        {
            var siteId = Guid.NewGuid();
            var reporterId = Guid.NewGuid();
            var request = Fixture.Build<UpdateReporterRequest>()
                .Without(x => x.Email)
                .Without(x => x.Phone)
                .Without(x => x.Name)
                .Without(x => x.Company)
                .Create();
            var updatedReporter = Fixture.Create<Reporter>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{siteId}/reporters/{reporterId}", request)
                    .ReturnsJson(updatedReporter);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/reporters/{reporterId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(3);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateReporter_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/reporters/{Guid.NewGuid()}", new UpdateReporterRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InvalidPhoneEmailInput_UpdateReporter_ReturnsError()
        {
            var siteId = Guid.NewGuid();
            var reporterId = Guid.NewGuid();
            var request = Fixture.Build<UpdateReporterRequest>()
                .With(x => x.Email, "Invalid Email")
                .With(x => x.Phone, "Invalid Phone")
                .With(x => x.Name)
                .With(x => x.Company)
                .Create();
            var updatedReporter = Fixture.Create<Reporter>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{siteId}/reporters/{reporterId}", request)
                    .ReturnsJson(updatedReporter);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/reporters/{reporterId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain($"Phone is invalid");
                result.Should().Contain($"Email is invalid");
            }
        }
    }
}