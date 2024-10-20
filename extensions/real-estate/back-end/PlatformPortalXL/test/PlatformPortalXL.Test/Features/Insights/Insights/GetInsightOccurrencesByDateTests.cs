using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.ServicesApi.InsightApi;

namespace PlatformPortalXL.Test.Features.Insights.Insights;

public class GetInsightOccurrencesByDateTests : BaseInMemoryTest
{
    public GetInsightOccurrencesByDateTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetInsightOccurrencesByDate_InputIsValid_ReturnResponse()
    {
      
        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(Guid.NewGuid(), Permissions.ViewSites, Guid.NewGuid());
        var spaceTwinId = "twinId";
        var startDate=DateTime.UtcNow.Date.AddDays(-1);
        var endDate=DateTime.UtcNow.Date;

        var expectedResponse = Fixture.Build<InsightOccurrencesCountByDateResponse>().Create();

        server.Arrange().GetInsightApi()
            .SetupRequest(HttpMethod.Get, $"insights/twin/{spaceTwinId}/insightOccurrencesByDate?startDate={startDate}&endDate={endDate}")
            .ReturnsJson(expectedResponse);

        var response = await client.GetAsync($"insights/twin/{spaceTwinId}/insightOccurrencesByDate?startDate={startDate.ToString("MM/dd/yyyy")}&endDate={endDate.ToString("MM/dd/yyyy")}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsAsync<InsightOccurrencesCountByDateResponse>();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetInsightOccurrencesByDate_EndDateIsNull_ReturnBadRequest()
    {

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(Guid.NewGuid(), Permissions.ViewSites, Guid.NewGuid());
        var spaceTwinId = "twinId";
        var startDate = DateTime.UtcNow.Date.AddDays(-1);
      
        var response = await client.GetAsync($"insights/twin/{spaceTwinId}/insightOccurrencesByDate?startDate={startDate.ToString("MM/dd/yyyy")}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    }

    [Fact]
    public async Task GetInsightOccurrencesByDate_StartDateIsNull_ReturnBadRequest()
    {

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(Guid.NewGuid(), Permissions.ViewSites, Guid.NewGuid());
        var spaceTwinId = "twinId";
        var endDate = DateTime.UtcNow.Date;

        var response = await client.GetAsync($"insights/twin/{spaceTwinId}/insightOccurrencesByDate?startDate=&endDate={endDate.ToString("MM/dd/yyyy")}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    }

    [Fact]
    public async Task GetInsightOccurrencesByDate_InputIsInValid_ReturnBadRequest()
    {

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(Guid.NewGuid(), Permissions.ViewSites, Guid.NewGuid());
        var spaceTwinId = "twinId";
        var startDate = DateTime.UtcNow.Date.AddDays(+1);
        var endDate = DateTime.UtcNow.Date;

        var response = await client.GetAsync($"insights/twin/{spaceTwinId}/insightOccurrencesByDate?startDate={startDate.ToString("MM/dd/yyyy")}&endDate={endDate.ToString("MM/dd/yyyy")}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    }

    [Fact]
    public async Task GetInsightOccurrencesByDate_UnauthorizedUser_ReturnUnauthorized()
    {
        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClient();
        {
            var spaceTwinId = "twinId";
            var startDate = DateTime.UtcNow.Date.AddDays(+1);
            var endDate = DateTime.UtcNow.Date;

            var response = await client.GetAsync($"insights/twin/{spaceTwinId}/insightOccurrencesByDate?startDate={startDate.ToString("MM/dd/yyyy")}&endDate={endDate.ToString("MM/dd/yyyy")}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }
    }
}
