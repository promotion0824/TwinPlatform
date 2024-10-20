using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PlatformPortalXL.Features.Twins;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Dynamic;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Test.Infrastructure;

namespace PlatformPortalXL.Test.Features.Twins
{
    public class GetTwinTests : BaseInMemoryTest
    {
        public GetTwinTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ValidInput_GetTwin_ReturnsThisTwin()
        {
            var siteId = Guid.NewGuid();
            var twinId = "twin123";
            var twin = Fixture.Build<TwinDto>().Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}")
                    .ReturnsJson(twin);

                var response = await client.GetAsync($"sites/{siteId}/twins/{twinId}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<object>();
                var expectedResult = JsonSerializer.Serialize(twin, new JsonSerializerOptions { WriteIndented = true });
                result.ToString().Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task ValidInput_GetTwinV2_ReturnsThisTwin()
        {
            var siteId = Guid.NewGuid();
            var twinId = "twin123";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionsOnSite(null, new[] { Permissions.ViewSites, Permissions.ManageSites }, siteId))
            {
                var twin = JsonSerializerHelper.Deserialize<Dictionary<string, object>>(JsonSerializerHelper.Serialize(Fixture.Build<TwinDto>().Create()));
                twin.Add("siteID", siteId);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}")
                    .ReturnsJson(twin);

                var response = await client.GetAsync($"v2.0/sites/{siteId}/twins/{twinId}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<object>();
                var expectedResult = JsonSerializer.Serialize(new {twin, permissions = new { edit = true }}, new JsonSerializerOptions { WriteIndented = true });
                result.ToString().Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task ValidInput_GetTwinV2_RemovesHiddenProperty()
        {
            var siteId = Guid.NewGuid();
            var twinId = "twin123";

            dynamic twin = new ExpandoObject();
            twin.Id = twinId;

            // We expect registrationID to be removed because it's listed as a hidden property
            twin.registrationID = "123";

            twin.metadata = new ExpandoObject() as dynamic;
            twin.metadata.modelId = "some model";
            twin.etag = "123";
            twin.siteID = siteId;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionsOnSite(null, new[] { Permissions.ViewSites, Permissions.ManageSites }, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}")
                    .ReturnsJson(twin as object);

                var response = await client.GetAsync($"v2.0/sites/{siteId}/twins/{twinId}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<object>();

                (twin as IDictionary<string, object>).Remove("registrationID");
                var expectedResult = JsonSerializer.Serialize(
                    new {
                      twin,
                      permissions = new { edit = true }
                    },
                    new JsonSerializerOptions { WriteIndented = true }
                );

                result.ToString().Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task ValidInput_GetTwin_ReturnsTwinWithEtag()
        {
            var siteId = Guid.NewGuid();
            var twinId = "twin123";
            var modelId = "dtmi:com:willowinc:Company;1";
            var properties = new Dictionary<string, string> { { "prop", "123" } };
            dynamic twin = new ExpandoObject();
            twin.Id = twinId;
            twin.metadata = new ExpandoObject() as dynamic;
            twin.metadata.modelId = modelId;
            twin.etag = "123";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}")
                    .ReturnsJson(twin as object);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/models/{modelId}/properties")
                    .ReturnsJson(properties);

                var response = await client.GetAsync($"sites/{siteId}/twins/{twinId}?includeModel=true");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<object>();
                twin.model = properties;
                var expectedResult = JsonSerializer.Serialize(twin, new JsonSerializerOptions { WriteIndented = true });
                result.ToString().Should().BeEquivalentTo(expectedResult);
                response.Headers.First().Value.Should().BeEquivalentTo(twin.etag);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetTwin_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/twins/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Theory]
        [CustomAutoData]
        public async Task Search_ReturnsValidResult(Guid siteId, string searchTerm, TwinSearchResponse searchResponse, Site[] userSites, Guid userId)
        {
            using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
            using var client = server.CreateClient(null, userId);

            server.Arrange().GetDirectoryApi()
                .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                .ReturnsJson(userSites);

            var siteIdsQuery = string.Join("&", userSites.Select(x => $"siteIds={x.Id}"));

            server.Arrange().GetDigitalTwinApi()
                .SetupRequest(HttpMethod.Get, $"search?{siteIdsQuery}&term={searchTerm}&page={1}")
                .ReturnsJson(searchResponse);

			var floors = searchResponse.Twins.Select(x => Fixture.Build<Floor>().With(f => f.Id, x.FloorId).Create()).ToList();

			server.Arrange().GetSiteApi()
				.SetupRequest(HttpMethod.Get, $"sites/floors?floorIds={string.Join("&floorIds=", floors.Select(x => x.Id))}")
				.ReturnsJson(floors);

            var outRelationshipFloors = searchResponse.Twins
                .SelectMany(x => x.OutRelationships)
                .Select(x => Fixture.Build<Floor>().With(f => f.Id, x.FloorId).Create())
                .Distinct()
                .ToList();

            server.Arrange().GetSiteApi()
                .SetupRequest(HttpMethod.Get, $"sites/floors?floorIds={string.Join("&floorIds=", outRelationshipFloors.Select(x => x.Id))}")
                .ReturnsJson(outRelationshipFloors);

            var response = await client.GetAsync($"twins/search?term={searchTerm}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<JsonElement>();

            var expectedNextPage = $"/twins/search?Term={searchTerm}&{siteIdsQuery}&QueryId={searchResponse.QueryId}&Page={searchResponse.NextPage}";
            result.GetProperty("nextPage").GetString().Should().BeEquivalentTo(expectedNextPage);
        }


        [Theory]
        [CustomAutoData]
        public async Task SearchWithScope_ReturnsValidResult(string searchTerm, TwinSearchResponse searchResponse, Site[] userSites, Guid userId, string scopeId)
        {
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).Take(1).ToList();
            var searchSiteId = expectedTwinDto.FirstOrDefault().SiteId;

            using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
            using var client = server.CreateClient(null, userId);

            server.Arrange().GetDirectoryApi()
                .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                .ReturnsJson(userSites);

            server.Arrange().GetDigitalTwinApi()
                .SetupRequest(HttpMethod.Post, $"scopes/sites")
                .ReturnsJson(expectedTwinDto);

            server.Arrange().GetDigitalTwinApi()
                .SetupRequest(HttpMethod.Get, $"search?siteIds={searchSiteId}&term={searchTerm}&page={1}")
                .ReturnsJson(searchResponse);

            var floors = searchResponse.Twins.Select(x => Fixture.Build<Floor>().With(f => f.Id, x.FloorId).Create()).ToList();

            server.Arrange().GetSiteApi()
                .SetupRequest(HttpMethod.Get, $"sites/floors?floorIds={string.Join("&floorIds=", floors.Select(x => x.Id))}")
                .ReturnsJson(floors);

            var outRelationshipFloors = searchResponse.Twins
                .SelectMany(x => x.OutRelationships)
                .Select(x => Fixture.Build<Floor>().With(f => f.Id, x.FloorId).Create())
                .Distinct()
                .ToList();

            server.Arrange().GetSiteApi()
                .SetupRequest(HttpMethod.Get, $"sites/floors?floorIds={string.Join("&floorIds=", outRelationshipFloors.Select(x => x.Id))}")
                .ReturnsJson(outRelationshipFloors);

            var response = await client.GetAsync($"twins/search?term={searchTerm}&scopeId={scopeId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var expectedNextPage = $"/twins/search?Term={searchTerm}&SiteIds={searchSiteId}&QueryId={searchResponse.QueryId}&Page={searchResponse.NextPage}";
            var result = await response.Content.ReadAsAsync<JsonElement>();

            result.GetProperty("nextPage").GetString().Should().Be(expectedNextPage);
        }

        [Fact]
        public async Task InvalidTwinId_GetTwinV2_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var twinId = "twin123";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var twin = JsonSerializerHelper.Deserialize<Dictionary<string, object>>(JsonSerializerHelper.Serialize(Fixture.Build<TwinDto>().Create()));
                twin.Add("siteID", Guid.NewGuid());

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}")
                    .ReturnsJson(twin);

                var response = await client.GetAsync($"v2.0/sites/{siteId}/twins/{twinId}");
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

    }
}
