using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;
using PlatformPortalXL.Features.Scopes;

namespace PlatformPortalXL.Test.Features.Twins
{
    public class GetSitesByScopeIdTests : BaseInMemoryTest
    {
        public GetSitesByScopeIdTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetSitesByScopeId_ReturnsOk()
        {
            var userId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();

            var userSites = Fixture.Build<Site>()
                .CreateMany(10).ToList();

            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>()
                .With(y => y.SiteId, x.Id)
                .Without(x => x.CustomProperties)
                .Without(x => x.Metadata)
                .Create()).ToList();

            using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
            using var client = server.CreateClient(null, userId);

            server.Arrange().GetDirectoryApi()
                .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                .ReturnsJson(userSites);

            server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                .ReturnsJson(expectedTwinDto);

            var response = await client.PostAsJsonAsync($"scopes/sites", new GetScopeSitesRequest() { Scope = new ScopeRequest() { DtId = scopeId } });
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<List<TwinDto>>();
            result.Count.Should().Be(expectedTwinDto.Count);
            result.Should().BeEquivalentTo(expectedTwinDto);

        }

        [Fact]
        public async Task GetSitesByScopeId_UserHasAccessToNoSites_ReturnsNoContent()
        {
            var userId = Guid.NewGuid();
            var sites = new List<Site>();
            var scopeId = Guid.NewGuid().ToString();
            using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
            using var client = server.CreateClient(null, userId);

            server.Arrange().GetDirectoryApi()
                .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                .ReturnsJson(sites);

            var response = await client.PostAsJsonAsync($"scopes/sites", new GetScopeSitesRequest() { Scope = new ScopeRequest() { DtId = scopeId } });

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        }

    }
}
