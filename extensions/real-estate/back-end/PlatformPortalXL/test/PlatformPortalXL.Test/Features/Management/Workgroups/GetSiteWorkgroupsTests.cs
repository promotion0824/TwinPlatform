using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Platform.Users;

namespace PlatformPortalXL.Test.Features.Management.Workgroups
{
    public class GetSiteWorkgroupsTests : BaseInMemoryTest
    {
        public GetSiteWorkgroupsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetSiteWorkgroups_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewUsers, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/workgroups");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task WorkgroupsExist_GetSiteWorkgroups_ReturnsWorkgroupsList()
        {
            var siteId = Guid.NewGuid();
            var expectedWorkgroups = Fixture.CreateMany<Workgroup>(5).ToList();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewUsers, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/workgroups")
                    .ReturnsJson(expectedWorkgroups);

                var response = await client.GetAsync($"sites/{siteId}/workgroups");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<Workgroup>>();
                result.Should().BeEquivalentTo(WorkgroupSimpleDto.MapFromModels(expectedWorkgroups));
            }
        }
    }
}
