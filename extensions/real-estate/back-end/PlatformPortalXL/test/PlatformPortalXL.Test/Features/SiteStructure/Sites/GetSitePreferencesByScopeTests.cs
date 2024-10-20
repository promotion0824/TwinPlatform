using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Models;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.SiteStructure.Sites
{
    public class GetSitePreferencesByScopeTests : BaseInMemoryTest
    {
        public GetSitePreferencesByScopeTests(ITestOutputHelper output) : base(output)
        { }

        [Fact]
        public async Task SitePreferencesExist_GetTimeMachinePreferencesByScope_ReturnsThoseSitePreferences()
        {
            var scopeId = "scope-dtid";
            var user = Fixture.Create<User>();
            var userSites = Fixture.Build<Site>()
                .CreateMany(2).ToList();
            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();
            expectedTwinDto[0].SiteId = userSites[0].Id;
            var preferences = new { TimeMachine = new { } };
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{user.Id}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"scopes/{scopeId}/preferences")
                    .ReturnsJson(preferences);

                var response = await client.GetAsync($"scopes/{scopeId}/preferences/timeMachine");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("{}");
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetTimeMachinePreferencesByScope_ReturnsForbidden()
        {
            var scopeId = "scope-dtid";
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .CreateMany(2).ToList();
            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                var response = await client.GetAsync($"scopes/{scopeId}/preferences/timeMachine");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
