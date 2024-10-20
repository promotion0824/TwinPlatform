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
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Features.Pilot;
using Willow.Platform.Models;
using PlatformPortalXL.ServicesApi.InsightApi;

namespace PlatformPortalXL.Test.Features.Insights.Insights;

public class GetInsightMapViewTests : BaseInMemoryTest
{
    public GetInsightMapViewTests(ITestOutputHelper output) : base(output)
    {
    }


    [Fact]
    public async Task GetInsightMapViewTests_UserAssignedToNoSite_ReturnEmpty()
    {
        var userId=Guid.NewGuid();
        List<Site> userSites = null;

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid());
        server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var response = await client.GetAsync($"insights/mapview");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

    }


    [Fact]
    public async Task GetInsightMapViewTests_WithScopeId_ReturnOk()
    {
        var userId = Guid.NewGuid();
        var scopeId = Guid.NewGuid().ToString();

        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
            .CreateMany(10).ToList();

        var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();
        expectedTwinDto[0].SiteId = userSites[0].Id;

        var expectedMapView = Fixture.Build<InsightMapViewResponse>()
           .With(c => c.SourceType, InsightSourceType.Willow)
           .With(c => c.Type, InsightType.Alert)
           .Without(c => c.SourceId)
           .CreateMany(10).ToList();

        var expectedResponse = InsightMapViewDto.MapFromModels(expectedMapView);

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClient(null, userId);

        server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
            .ReturnsJson(expectedTwinDto);

        server.Arrange().GetInsightApi().SetupRequest(HttpMethod.Get, $"insights/mapview?siteIds={expectedTwinDto[0].SiteId}")
            .ReturnsJson(expectedMapView);

        var response = await client.GetAsync($"insights/mapview?scopeId={scopeId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsAsync<List<InsightMapViewDto>>();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetInsightMapViewTests_WithScopeId_UserHasNoAccess_ReturnForbidden()
    {
        var userId = Guid.NewGuid();
        var scopeId = Guid.NewGuid().ToString();

        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures{ IsInsightsDisabled = false })
            .CreateMany(10).ToList();

        var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();

        var expectedMapView = Fixture.Build<InsightMapViewResponse>()
            .With(c => c.Type, InsightType.Alert)
            .CreateMany(10).ToList();

        var expectedResponse = InsightMapViewDto.MapFromModels(expectedMapView);

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClient(null, userId);

        server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
            .ReturnsJson(expectedTwinDto);

        server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var response = await client.GetAsync($"insights/mapview?scopeId={scopeId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetInsightMapViewTests_ScopeIdIsInvalid_ReturnForbidden()
    {
        var userId = Guid.NewGuid();
        var scopeId = Guid.NewGuid().ToString();
        var expectedTwinDto = new List<TwinDto>();

        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
            .CreateMany(10).ToList(); ;

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid());
        server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
            .ReturnsJson(expectedTwinDto);
        server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var response = await client.GetAsync($"insights/mapview?scopeId={scopeId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

    }
}
