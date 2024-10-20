using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.Features.Insights;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using Moq;
using Moq.Contrib.HttpClient;
using Willow.Platform.Users;
using Willow.Platform.Models;
using PlatformPortalXL.Features.Pilot;
using Autodesk.Forge.Model;

namespace PlatformPortalXL.Test.Features.Insights.Insights;

public class UpdateInsightsStatusV2Test : BaseInMemoryTest
{
	public UpdateInsightsStatusV2Test(ITestOutputHelper output) : base(output)
	{
	}

	[Fact]
	public async Task InsightsExist_UpdateInsightsStatus_ReturnsSuccess()
	{
		var siteId = Guid.NewGuid();
		var expectedInsights = Fixture.Build<Insight>()
			.With(x => x.SourceType, InsightSourceType.Willow)
			.CreateMany(10);

		using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
		using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
		{
            server.Arrange().GetInsightApi().SetupRequest(HttpMethod.Put, $"sites/{siteId}/insights/status")
                .ReturnsResponse(HttpStatusCode.NoContent);

            var response = await client.PostAsJsonAsync(
				$"v2/sites/{siteId}/insights/status",
				new UpdateInsightStatusRequest
				{
					Ids = expectedInsights.Select(x => x.Id).ToList(),
					Status = InsightStatus.Resolved
				});

			response.StatusCode.Should().Be(HttpStatusCode.NoContent);

		}
	}

    [Fact]
    public async Task InsightsExist_UpdateInsightsByScopeId_ReturnsSuccess()
    {
        var siteId = Guid.NewGuid();
        var customerId= Guid.NewGuid();
        var scopeId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();
        var expectedInsights = Fixture.Build<Insight>()
            .With(x => x.SourceType, InsightSourceType.Willow)
            .CreateMany(10);
        var userSites = Fixture.Build<Site>()
                                      .With(x => x.CustomerId, customerId)
                                      .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                                      .CreateMany(2).ToList();
        var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
        {
            server.Arrange().GetInsightApi().SetupRequest(HttpMethod.Put, $"sites/{siteId}/insights/status")
                .ReturnsResponse(HttpStatusCode.NoContent);

            server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                .ReturnsJson(expectedTwinDto);

            server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                .ReturnsJson(userSites);

            var response = await client.PostAsJsonAsync(
                $"v2/sites/{siteId}/insights/status",
                new UpdateInsightStatusRequest
                {
                    Ids = expectedInsights.Select(x => x.Id).ToList(),
                    Status = InsightStatus.Resolved,
                    ScopeId= scopeId
                });

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        }
    }

    [Fact]
    public async Task InsightsExist_UpdateInsightsByScopeId_UserHasNoAccess_ReturnsForbidden()
    {
        var siteId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var scopeId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();
        var expectedInsights = Fixture.Build<Insight>()
            .With(x => x.SourceType, InsightSourceType.Willow)
            .CreateMany(10);
        var userSites = Fixture.Build<Site>()
                                      .With(x => x.CustomerId, customerId)
                                      .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                                      .CreateMany(2).ToList();
        var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
        {

            server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                .ReturnsJson(expectedTwinDto);

            server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                .ReturnsJson(userSites);

            var response = await client.PostAsJsonAsync(
                $"v2/sites/{siteId}/insights/status",
                new UpdateInsightStatusRequest
                {
                    Ids = expectedInsights.Select(x => x.Id).ToList(),
                    Status = InsightStatus.Resolved,
                    ScopeId = scopeId
                });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        }
    }

    [Fact]
    public async Task UpdateInsight_InvalidScopeId_ReturnsUnauthorized()
    {
        var siteId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var scopeId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();
        var expectedInsights = Fixture.Build<Insight>()
            .With(x => x.SourceType, InsightSourceType.Willow)
            .CreateMany(10);
        var userSites = Fixture.Build<Site>()
                                      .With(x => x.CustomerId, customerId)
                                      .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
                                      .CreateMany(2).ToList();
        var expectedTwinDto = new List<TwinDto>();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
        {
            server.Arrange().GetInsightApi().SetupRequest(HttpMethod.Put, $"sites/{siteId}/insights/status")
                .ReturnsResponse(HttpStatusCode.NoContent);

            server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                .ReturnsJson(expectedTwinDto);

            server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                .ReturnsJson(userSites);

            var response = await client.PostAsJsonAsync(
                $"v2/sites/{siteId}/insights/status",
                new UpdateInsightStatusRequest
                {
                    Ids = expectedInsights.Select(x => x.Id).ToList(),
                    Status = InsightStatus.Resolved,
                    ScopeId = scopeId
                });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        }
    }

    [Fact]
	public async Task AppInsightsExistWithoutSourceId_UpdateInsightsStatus_ReturnsSuccess()
	{
        var userId = Guid.NewGuid();
		var siteId = Guid.NewGuid();
		var expectedInsights = Fixture.Build<Insight>()
			.With(x => x.SourceType, InsightSourceType.App)
			.Without(x => x.SourceId)
			.CreateMany(10);

		using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
        {
            server.Arrange().GetInsightApi().SetupRequest(HttpMethod.Put, $"sites/{siteId}/insights/status")
                .ReturnsResponse(HttpStatusCode.NoContent);

            var response = await client.PostAsJsonAsync(
                $"v2/sites/{siteId}/insights/status",
                new UpdateInsightStatusRequest
                {
                    Ids = expectedInsights.Select(x => x.Id).ToList(),
                    Status = InsightStatus.Ignored
                });

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        }
	}

	[Fact]
	public async Task AppInsightsExistWithSourceId_UpdateInsightsStatus_ReturnsSuccess()
	{
		var siteId = Guid.NewGuid();
		var expectedInsights = Fixture.Build<Insight>()
			.With(x => x.SourceType, InsightSourceType.App)
			.CreateMany(10);

		using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
		using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
		{
            server.Arrange().GetInsightApi().SetupRequest(HttpMethod.Put, $"sites/{siteId}/insights/status")
                .ReturnsResponse(HttpStatusCode.NoContent);
            var response = await client.PostAsJsonAsync(
				$"v2/sites/{siteId}/insights/status",
				new UpdateInsightStatusRequest
				{
					Ids = expectedInsights.Select(x => x.Id).ToList(),
					Status = InsightStatus.InProgress
				});

			response.StatusCode.Should().Be(HttpStatusCode.NoContent);

		}
	}

	[Fact]
	public async Task UserDoesNotHaveCorrectPermission_UpdateInsightStatus_ReturnsForbidden()
	{
		var siteId = Guid.NewGuid();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
		using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
		{
			var response = await client.PostAsJsonAsync($"v2/sites/{siteId}/insights/status", new UpdateInsightStatusRequest(){Ids =new List<Guid>{Guid.NewGuid()}, Status = InsightStatus.InProgress});

			response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
		}
	}

	[Fact]
	public async Task InsightsExist_UpdateInsightsStatusToDeleted_ReturnsSuccess()
	{
		var siteId = Guid.NewGuid();
		var expectedInsights = Fixture.Build<Insight>()
			.With(x => x.SourceType, InsightSourceType.Willow)
			.CreateMany(10);
		var expectedUser = Fixture.Build<User>()
			.With(x=>x.Email,$"{Guid.NewGuid()}@willowinc.com")
			.With(x => x.Id, Guid.NewGuid)
			.Create();
		using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
		using (var client = server.CreateClientWithPermissionOnSite(expectedUser.Id, Permissions.ViewSites, siteId))
		{

			server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{expectedUser.Id}")
				.ReturnsJson(expectedUser);
            server.Arrange().GetInsightApi().SetupRequest(HttpMethod.Put, $"sites/{siteId}/insights/status")
                .ReturnsResponse(HttpStatusCode.NoContent);
            var response = await client.PostAsJsonAsync(
				$"v2/sites/{siteId}/insights/status",
				new UpdateInsightStatusRequest
				{
					Ids = expectedInsights.Select(x => x.Id).ToList(),
					Status = InsightStatus.Deleted
				});

			response.StatusCode.Should().Be(HttpStatusCode.NoContent);

		}
	}

	[Fact]
	public async Task UserDoesNotHaveCorrectPermission_UpdateInsightStatusToDeleted_ReturnsForbidden()
	{
		var siteId = Guid.NewGuid();
		var expectedUser = Fixture.Build<User>()
			.With(x => x.Email, $"{Guid.NewGuid()}@microsoft.com")
			.With(x => x.Id, Guid.NewGuid)
			.Create();
		using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
		using (var client = server.CreateClientWithPermissionOnSite(expectedUser.Id, Permissions.ViewSites, siteId))
		{
			server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{expectedUser.Id}")
				.ReturnsJson(expectedUser);
			var response = await client.PostAsJsonAsync($"v2/sites/{siteId}/insights/status", new UpdateInsightStatusRequest() { Ids = new List<Guid> { Guid.NewGuid() }, Status = InsightStatus.Deleted });

			response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
		}
	}
}
