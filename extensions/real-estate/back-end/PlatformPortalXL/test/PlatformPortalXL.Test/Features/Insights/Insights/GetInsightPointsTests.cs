using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Insights.Insights;

public class GetInsightPointsTests : BaseInMemoryTest
{
    public GetInsightPointsTests(ITestOutputHelper output) : base(output)
    {
    }
    [Fact]
    public async Task UnauthorizedUser_GetInsightPoints_ReturnUnauthorized()
    {
        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClient();
        {
            var siteId = Guid.NewGuid();
            var insightId = Guid.NewGuid();
            var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/points");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }
    }
    [Fact]
    public async Task PointsExists_GetInsightPoints_ReturnsPoints()
    {
        var siteId = Guid.NewGuid();
        var insightId = Guid.NewGuid();
        var expectedPoints = Fixture.Create<InsightPointsDto>();


        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId);
        {

            server.Arrange().GetInsightApi()
                .SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{insightId}/points")
                .ReturnsJson(expectedPoints);

            var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/points");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<InsightPointsDto>();

            result.Should().BeEquivalentTo(expectedPoints);

        }
    }
}

