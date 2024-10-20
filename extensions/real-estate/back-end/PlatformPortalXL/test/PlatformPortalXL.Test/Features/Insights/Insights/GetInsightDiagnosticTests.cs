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
using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Features.Pilot;
using Willow.Platform.Models;

namespace PlatformPortalXL.Test.Features.Insights.Insights;

public class GetInsightDiagnosticTests : BaseInMemoryTest
{
    public GetInsightDiagnosticTests(ITestOutputHelper output) : base(output)
    {
    }


    [Fact]
    public async Task GetInsightDiagnosticTests_UserAssignedToNoSite_ReturnForbidden()
    {
        var userId=Guid.NewGuid();
        List<Site> userSites = null;
        var startDate = DateTime.Parse("2023-10-19");
        var endDate = DateTime.Parse("2023-11-03");
        var insightId=Guid.NewGuid();
        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid());
        server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var response = await client.GetAsync($"insights/{insightId}/occurrences/diagnostics?start={startDate.ToString("MM/dd/yyyy")}&end={endDate.ToString("MM/dd/yyyy")}&interval={00.00:10:00}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

    }

    [Fact]
    public async Task GetInsightDiagnosticTests_InputIsValid_ReturnOk()
    {
        var userId = Guid.NewGuid();
        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
            .CreateMany(10).ToList(); ;
        var startDate = DateTime.Parse("2023-10-19");
        var endDate = DateTime.Parse("2023-11-03");
        var insightId = Guid.NewGuid();
        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid());

        server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);
        var expectedResponse =Fixture.Build<InsightDiagnosticDto>().CreateMany(10).ToList();
        server.Arrange().GetInsightApi().SetupRequest(HttpMethod.Get, $"insights/{insightId}/occurrences/diagnostics?start={startDate}&end={endDate}&interval=00.00:10:00")
            .ReturnsJson(expectedResponse);
        var response = await client.GetAsync($"insights/{insightId}/occurrences/diagnostics?start={startDate.ToString("MM/dd/yyyy")}&end={endDate.ToString("MM/dd/yyyy")}&interval=00.00:10:00");
        var result = await response.Content.ReadAsAsync<List<InsightDiagnosticDto>>();

        result.Should().BeEquivalentTo(expectedResponse);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetInsightDiagnosticTests_WithScopeId_ReturnOk()
    {
        var userId = Guid.NewGuid();
        var scopeId = Guid.NewGuid().ToString();
        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
            .CreateMany(10).ToList();
        var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();

        var startDate = DateTime.Parse("2023-10-19");
        var endDate = DateTime.Parse("2023-11-03");
        var insightId = Guid.NewGuid();
        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid());
        server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
            .ReturnsJson(expectedTwinDto);
        server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);
        var expectedResponse = Fixture.Build<InsightDiagnosticDto>().CreateMany(10).ToList();
        server.Arrange().GetInsightApi().SetupRequest(HttpMethod.Get, $"insights/{insightId}/occurrences/diagnostics?start={startDate}&end={endDate}&interval=00.00:10:00")
            .ReturnsJson(expectedResponse);
        var response = await client.GetAsync($"insights/{insightId}/occurrences/diagnostics?start={startDate.ToString("MM/dd/yyyy")}&end={endDate.ToString("MM/dd/yyyy")}&interval=00.00:10:00&scopeId={scopeId}");
        var result = await response.Content.ReadAsAsync<List<InsightDiagnosticDto>>();

        result.Should().BeEquivalentTo(expectedResponse);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetInsightDiagnosticTests_WithScopeId_UserHasNoAccess_ReturnForbidden()
    {
        var userId = Guid.NewGuid();
        var scopeId = Guid.NewGuid().ToString();
        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
            .CreateMany(10).ToList();
        var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();

        var startDate = DateTime.Parse("2023-10-19");
        var endDate = DateTime.Parse("2023-11-03");
        var insightId = Guid.NewGuid();
        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);

        using var client = server.CreateClient(null, userId);

        server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
            .ReturnsJson(expectedTwinDto);

        server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var response = await client.GetAsync($"insights/{insightId}/occurrences/diagnostics?start={startDate.ToString("MM/dd/yyyy")}&end={endDate.ToString("MM/dd/yyyy")}&interval={00.00:10:00}&scopeId={scopeId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetInsightDiagnosticTests_ScopeIdIsInvalid_ReturnForbidden()
    {
        var userId = Guid.NewGuid();
        var scopeId = Guid.NewGuid().ToString();
        var expectedTwinDto = new List<TwinDto>();

        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
            .CreateMany(10).ToList(); ;
        var startDate = DateTime.Parse("2023-10-19");
        var endDate = DateTime.Parse("2023-11-03");
        var insightId = Guid.NewGuid();
        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid());
        server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
            .ReturnsJson(expectedTwinDto);
        server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var response = await client.GetAsync($"insights/{insightId}/occurrences/diagnostics?start={startDate.ToString("MM/dd/yyyy")}&end={endDate.ToString("MM/dd/yyyy")}&interval={00.00:10:00}&scopeId={scopeId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }


    [Theory]
    [InlineData("2023-10-03 12:00:00 AM", "")]
    [InlineData("", "2023-10-30 12:00:00 AM")]
    [InlineData("", "")]
    public async Task GetInsightDiagnostic_InsightHasDependencies_NullEndDateOrStartDate_ReturnsBadRequest(string startDateString, string endDateString)
    {
        var insightId = Guid.NewGuid();
        DateTime? startDate = startDateString == "" ? null : DateTime.Parse(startDateString);
        DateTime? endDate = endDateString == "" ? null : DateTime.Parse(endDateString);

        var userId = Guid.NewGuid();

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid());
        var url = $"insights/{insightId}/occurrences/diagnostics?interval=00.10:00:00";
        url += startDate.HasValue ? $"&start={startDate.Value.ToString("MM/dd/yyyy")}" : "";
        url += endDate.HasValue ? $"&start={endDate.Value.ToString("MM/dd/yyyy")}" : "";

        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
