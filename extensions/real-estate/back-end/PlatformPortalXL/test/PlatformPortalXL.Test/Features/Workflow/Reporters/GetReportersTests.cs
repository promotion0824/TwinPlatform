using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Workflow.Reporters
{
    public class GetReportersTests : BaseInMemoryTest
    {
        public GetReportersTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SiteHasReporters_GetReporters_ReturnsThoseReporters()
        {
            var siteId = Guid.NewGuid();
            var expectedReporters = Fixture.CreateMany<Reporter>().ToList();
            var expectedUsers = Fixture.CreateMany<User>().ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/reporters")
                    .ReturnsJson(expectedReporters);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(expectedUsers);

                var response = await client.GetAsync($"sites/{siteId}/requestors");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<RequestorDto>>();

                var expectedResult = new List<RequestorDto>();
                expectedResult.AddRange(RequestorDto.MapFromModels(expectedReporters));
                expectedResult.AddRange(RequestorDto.MapFromModels(expectedUsers.FindAll(x => x.Status == UserStatus.Active)));

                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetReporters_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/requestors");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}