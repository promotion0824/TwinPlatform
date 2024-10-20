using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.ServicesApi.InsightApi;

namespace PlatformPortalXL.Test.Features.Insights.Insights;

public class GetActiveInsightCountByModelIdTests : BaseInMemoryTest
{
    public GetActiveInsightCountByModelIdTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetActiveInsightCountByModelIdTests_InputIsValid_ReturnResponse()
    {
      
        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(Guid.NewGuid(), Permissions.ViewSites, Guid.NewGuid());
        var spaceTwinId = "twinId";
        var limit=5;
        var expectedResponse = Fixture.Build<ActiveInsightCountByModelIdDto>().CreateMany(limit).ToList();

        server.Arrange().GetInsightApi()
            .SetupRequest(HttpMethod.Get, $"insights/twin/{spaceTwinId}/activeInsightCountsByTwinModel?limit={limit}")
            .ReturnsJson(expectedResponse);

        var response = await client.GetAsync($"insights/twin/{spaceTwinId}/activeInsightCountsByTwinModel?limit={limit}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsAsync<List<ActiveInsightCountByModelIdDto>>();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetActiveInsightCountByModelIdTests_UnauthorizedUser_ReturnUnauthorized()
    {
        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClient();
        {
            var spaceTwinId = "twinId";
           
            var response = await client.GetAsync($"insights/twin/{spaceTwinId}/activeInsightCountsByTwinModel");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }
    }
}
