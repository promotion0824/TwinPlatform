using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Insights.Insights;

public class GetLiveDataByExternalIdTests : BaseInMemoryTest
{
    public GetLiveDataByExternalIdTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UnauthorizedUser_GetLiveDataByExternalId_ReturnUnauthorized()
    {
        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClient();

        var siteId = Guid.NewGuid();
        var externalId = Fixture.Create<string>();
        var response = await client.GetAsync($"sites/{siteId}/livedata/impactScores/{externalId}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UserDoesNotHaveCorrectPermission_GetLiveDataByExternalId_ReturnsForbidden()
    {
        var siteId = Guid.NewGuid();
        var externalId = Fixture.Create<string>();

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId);

        var response = await client.GetAsync($"sites/{siteId}/livedata/impactScores/{externalId}?start=2022-10-01&end=2023-08-01");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ImpactScoresLiveDataExist_GetLiveDataByExternalId_ReturnsImpactScoresLiveData()
    {
        var siteId = Guid.NewGuid();
        var externalId = Fixture.Create<string>();
        var customerId = Guid.NewGuid();

        var startDateTime = new DateTime(2023, 12, 12, 12, 12, 12);
        var endDateTime = startDateTime.AddDays(1);
        var connectorId = ServerFixtureConfigurations.RulesEngineConnectorId;

        var expectedResult = Fixture.Build<ImpactScoresLiveData>()
                                    .With(x => x.ExternalId, externalId)
                                    .Create();

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId);

        server.Arrange().GetSiteApi()
             .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
             .ReturnsJson(new Site { Id = siteId, CustomerId = customerId });

        var startDateTimeUtc = HttpUtility.UrlEncode(startDateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
        var endDateTimeUtc = HttpUtility.UrlEncode(endDateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
        var liveDataUrl = $"api/telemetry/point/analog/{connectorId}/{externalId}?clientId={customerId}&startUtc={startDateTimeUtc}&endUtc={endDateTimeUtc}";
        server.Arrange().GetLiveDataApi()
            .SetupRequest(HttpMethod.Get, liveDataUrl)
            .ReturnsJson(expectedResult.TimeSeriesData);

        var response = await client.GetAsync($"sites/{siteId}/livedata/impactScores/{externalId}?start={startDateTimeUtc}&end={endDateTimeUtc}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsAsync<ImpactScoresLiveData>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task ImpactScoresLiveDataExist_GetLiveDataByScopeId_ReturnsImpactScoresLiveData()
    {
        var siteId = Guid.NewGuid();
        var externalId = Fixture.Create<string>();
        var customerId = Guid.NewGuid();
        var scopeId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var startDateTime = new DateTime(2023, 12, 12, 12, 12, 12);
        var endDateTime = startDateTime.AddDays(1);
        var connectorId = ServerFixtureConfigurations.RulesEngineConnectorId;

        var expectedResult = Fixture.Build<ImpactScoresLiveData>()
                                    .With(x => x.ExternalId, externalId)
                                    .Create();

        var userSites = Fixture.Build<Site>()
                                     .With(x => x.CustomerId, customerId)
                                     .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                                     .CreateMany(2).ToList();
        var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId);

        server.Arrange().GetSiteApi()
             .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto.First().SiteId}")
             .ReturnsJson(new Site { Id = expectedTwinDto.First().SiteId.Value, CustomerId = customerId });

        server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                .ReturnsJson(expectedTwinDto);

        server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var startDateTimeUtc = HttpUtility.UrlEncode(startDateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
        var endDateTimeUtc = HttpUtility.UrlEncode(endDateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
        var liveDataUrl = $"api/telemetry/point/analog/{connectorId}/{externalId}?clientId={customerId}&startUtc={startDateTimeUtc}&endUtc={endDateTimeUtc}";

        server.Arrange().GetLiveDataApi()
            .SetupRequest(HttpMethod.Get, liveDataUrl)
            .ReturnsJson(expectedResult.TimeSeriesData);

        var response = await client.GetAsync($"sites/{siteId}/livedata/impactScores/{externalId}?start={startDateTimeUtc}&end={endDateTimeUtc}&scopeId={scopeId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsAsync<ImpactScoresLiveData>();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task ImpactScoresLiveDataExist_WithScopeId_UserHasNoAccess_ReturnsForbidden()
    {
        var siteId = Guid.NewGuid();
        var externalId = Fixture.Create<string>();
        var customerId = Guid.NewGuid();
        var scopeId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var startDateTime = new DateTime(2023, 12, 12, 12, 12, 12);
        var endDateTime = startDateTime.AddDays(1);


        var userSites = Fixture.Build<Site>()
                                     .With(x => x.CustomerId, customerId)
                                     .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                                     .CreateMany(2).ToList();

        var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId);

        server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                .ReturnsJson(expectedTwinDto);

        server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var startDateTimeUtc = HttpUtility.UrlEncode(startDateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
        var endDateTimeUtc = HttpUtility.UrlEncode(endDateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));

        var response = await client.GetAsync($"sites/{siteId}/livedata/impactScores/{externalId}?start={startDateTimeUtc}&end={endDateTimeUtc}&scopeId={scopeId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ImpactScoresLiveDataExist_ScopeIdIsInvalid_ReturnsForbidden()
    {
        var siteId = Guid.NewGuid();
        var externalId = Fixture.Create<string>();
        var customerId = Guid.NewGuid();
        var scopeId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var startDateTime = new DateTime(2023, 12, 12, 12, 12, 12);
        var endDateTime = startDateTime.AddDays(1);
        var connectorId = ServerFixtureConfigurations.RulesEngineConnectorId;

        var userSites = Fixture.Build<Site>()
                                     .With(x => x.CustomerId, customerId)
                                     .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                                     .CreateMany(2).ToList();
        var expectedTwinDto = new List<TwinDto>();

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId);

        server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                .ReturnsJson(expectedTwinDto);

        server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var startDateTimeUtc = HttpUtility.UrlEncode(startDateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
        var endDateTimeUtc = HttpUtility.UrlEncode(endDateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));

        var response = await client.GetAsync($"sites/{siteId}/livedata/impactScores/{externalId}?start={startDateTimeUtc}&end={endDateTimeUtc}&scopeId={scopeId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

