using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;
using Willow.Batch;
using System.Collections.Generic;
using PlatformPortalXL.Features.Insights;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.ServicesApi.InsightApi;
using Willow.Platform.Users;
using Google.Apis.Requests;


namespace PlatformPortalXL.Test.Features.Insights.Insights;

public class GetInsightFilterTests: BaseInMemoryTest
{
    public GetInsightFilterTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetInsightFilter_BySiteId_ReturnsFilter()
    {
        var userId = Guid.NewGuid();
        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
            .CreateMany(2).ToList();
        var request = new GetInsightFilterRequest()
        {
            SiteIds = new List<Guid> { userSites[0].Id }
        };
        var expectedFilter = Fixture.Build<InsightFilterDto>().Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null, userId))
        {
            server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                .ReturnsJson(userSites);

            server.Arrange().GetInsightApi()
                .SetupRequestWithExpectedBody(HttpMethod.Post, "insights/filters",new GetInsightFilterApiRequest
                {
                    SiteIds = request.SiteIds
                })
                .ReturnsJson(expectedFilter);

            var response = await client.PostAsJsonAsync($"insights/filters", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<InsightFilterDto>();
            result.Should().BeEquivalentTo(expectedFilter);
        }
    }

    [Fact]
    public async Task GetInsightFilter_ByScopeId_ReturnsFilter()
    {
        var userId = Guid.NewGuid();
        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
            .CreateMany(2).ToList();
        var request = new GetInsightFilterRequest()
        {
            ScopeId = "scopeId"
        };
        var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();

        var expectedFilter = Fixture.Build<InsightFilterDto>().Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null, userId))
        {
            server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                .ReturnsJson(userSites);
            server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                .ReturnsJson(expectedTwinDto);
            server.Arrange().GetInsightApi()
                .SetupRequestWithExpectedBody(HttpMethod.Post, "insights/filters", new GetInsightFilterApiRequest
                {
                    SiteIds = userSites.Select(x => x.Id).ToList()
                })
                .ReturnsJson(expectedFilter);

            var response = await client.PostAsJsonAsync($"insights/filters", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<InsightFilterDto>();
            result.Should().BeEquivalentTo(expectedFilter);
        }
    }

    [Fact]
    public async Task GetInsightFilter_WithoutScopeIdAndSiteIds_ReturnsFilter()
    {
        var userId = Guid.NewGuid();
        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
            .CreateMany(2).ToList();
        var request = new GetInsightFilterRequest();


        var expectedFilter = Fixture.Build<InsightFilterDto>().Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null, userId))
        {
            server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                .ReturnsJson(userSites);

            server.Arrange().GetInsightApi()
                .SetupRequestWithExpectedBody(HttpMethod.Post, "insights/filters", new GetInsightFilterApiRequest
                {
                    SiteIds = userSites.Select(c=>c.Id).ToList()
                })
                .ReturnsJson(expectedFilter);

            var response = await client.PostAsJsonAsync($"insights/filters", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<InsightFilterDto>();
            result.Should().BeEquivalentTo(expectedFilter);
        }
    }
    [Fact]
    public async Task UserDoesNotHaveCorrectPermissionForSite_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
            .CreateMany(2).ToList();
        var request = new GetInsightFilterRequest()
        {
            SiteIds = [Guid.NewGuid()]
        };


        var expectedFilter = Fixture.Build<InsightFilterDto>().Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null, userId))
        {
            server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                .ReturnsJson(userSites);

            server.Arrange().GetInsightApi()
                .SetupRequestWithExpectedBody(HttpMethod.Post, "insights/filters", new GetInsightFilterApiRequest
                {
                    SiteIds = request.SiteIds
                })
                .ReturnsJson(expectedFilter);

            var response = await client.PostAsJsonAsync($"insights/filters", request);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        }
    }
}
