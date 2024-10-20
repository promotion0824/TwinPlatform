using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.SiteStructure.Sites
{
    public class CreateOrUpdateModuleGroupsPreferences : BaseInMemoryTest
    {
        public CreateOrUpdateModuleGroupsPreferences(ITestOutputHelper output) : base(output)
        { }

        [Fact]
        public async Task GivenValidInput_CreateOrUpdateModuleGroupsPreferences_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var user = Fixture.Create<User>();
            var sitePreferencesRequest = new { moduleGroups = new { } };
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(user.Id, "manage-sites", siteId))
            {
                server.Arrange().GetSiteApi()
                .SetupRequest(HttpMethod.Put, $"sites/{siteId}/preferences")
                .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/preferences/moduleGroups", sitePreferencesRequest);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateOrUpdateModuleGroupsPreferences_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/preferences/moduleGroups", new { moduleGroups = new { } });

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
