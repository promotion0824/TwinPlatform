using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Models;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.SiteStructure.Sites
{
    public class CreateOrUpdateTimeMachinePreferencesByScopeTests : BaseInMemoryTest
    {
        public CreateOrUpdateTimeMachinePreferencesByScopeTests(ITestOutputHelper output) : base(output)
        { }

        [Fact]
        public async Task GivenValidInput_CreateOrUpdateTimeMachinePreferencesByScope_ReturnsNoContent()
        {
            var scopeId = "scope-dtid";
            var user = Fixture.Create<User>();
            var userSites = Fixture.Build<Site>()
                .CreateMany(2).ToList();
            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();
            expectedTwinDto[0].SiteId = userSites[0].Id;
            var sitePreferencesRequest = new { timeMachineblablabla = new {} };
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{user.Id}/sites?permissionId={Permissions.ManageSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                server.Arrange().GetSiteApi()
                .SetupRequest(HttpMethod.Put, $"scopes/{scopeId}/preferences")
                .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"scopes/{scopeId}/preferences/timeMachine", sitePreferencesRequest);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateOrUpdateTimeMachinePreferencesByScope_ReturnsForbidden()
        {
            var scopeId = "scope-dtid";
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .CreateMany(2).ToList();
            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ManageSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                var response = await client.PutAsJsonAsync($"scopes/{scopeId}/preferences/timeMachine", new { timeMachineblablabla = new { } });

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
