using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.SiteStructure.Sites
{
    public class GetSitePreferencesTests : BaseInMemoryTest
    {
        public GetSitePreferencesTests(ITestOutputHelper output) : base(output)
        { }

        [Fact]
        public async Task SitePreferencesExist_GetTimeMachinePreferences_ReturnsThoseSitePreferences()
        {
            var siteId = Guid.NewGuid();
            var user = Fixture.Create<User>();
            var preferences = new { TimeMachine = new { } };
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(user.Id, "view-sites", siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/preferences")
                    .ReturnsJson(preferences);

                var response = await client.GetAsync($"sites/{siteId}/preferences/timeMachine");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("{}");
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetTimeMachinePreferences_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/preferences/timeMachine");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task SitePreferencesExist_GetModuleGroupsPreferences_ReturnsThoseSitePreferences()
        {
            var siteId = Guid.NewGuid();
            var user = Fixture.Create<User>();
            var preferences = new { TimeMachine = new { }, ModuleGroups = new { } };
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(user.Id, "view-sites", siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/preferences")
                    .ReturnsJson(preferences);

                var response = await client.GetAsync($"sites/{siteId}/preferences/moduleGroups");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("{}");
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetModuleGroupsPreferences_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/preferences/moduleGroups");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}