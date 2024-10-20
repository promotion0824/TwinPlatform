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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using PlatformPortalXL.Features.Pilot;
using Willow.Platform.Models;
using Willow.Workflow.Models;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets;

public class GetTicketStatisticsTest : BaseInMemoryTest
{
    public GetTicketStatisticsTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetTicketStatisticsTest_UserAssignedToNoSite_ReturnForbidden()
    {
        var userId=Guid.NewGuid();
        var userSites = (List<Site>)null;

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid());

        server.Arrange().GetDirectoryApi()
            .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var response = await client.PostAsJsonAsync($"tickets/statistics", new List<string>());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTicketStatisticsTest_InputIsValid_ReturnOk()
    {
        var userId = Guid.NewGuid();
        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
            .CreateMany(2).ToList(); ;


        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid());

        server.Arrange().GetDirectoryApi()
            .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var expectedResponse = new TicketStatisticsResponse
        {
            StatisticsByPriority = userSites
                .Select(c => Fixture.Build<TicketStatisticsByPriority>().With(d => d.Id, c.Id).Create()).ToList(),
            StatisticsByStatus = userSites
                .Select(c => Fixture.Build<TicketStatisticsByStatus>().With(d => d.Id, c.Id).Create()).ToList()
        };

        server.Arrange().GetWorkflowApi()
            .SetupRequest(HttpMethod.Post, $"tickets/statistics")
            .ReturnsJson(expectedResponse);

        var response = await client.PostAsJsonAsync($"tickets/statistics",new List<string>());
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsAsync<TicketStatisticsResponse>();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetTicketStatisticsTest_InputIsValid_WithScopeId_ReturnOk()
    {
        var userId = Guid.NewGuid();
        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
            .CreateMany(4).ToList(); ;

        var expectedTwinDto = new List<TwinDto>
        {
            new TwinDto() { Id = "scope1", SiteId = userSites[1].Id },
            new TwinDto() { Id = "scope2", SiteId = userSites[2].Id }
        };

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid());

        server.Arrange().GetDirectoryApi()
            .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);
        server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
            .ReturnsJson(expectedTwinDto);
        var expectedResponse = new TicketStatisticsResponse
        {
            StatisticsByPriority = userSites
                .Select(c => Fixture.Build<TicketStatisticsByPriority>().With(d => d.Id, c.Id).Create()).ToList(),
            StatisticsByStatus = userSites
                .Select(c => Fixture.Build<TicketStatisticsByStatus>().With(d => d.Id, c.Id).Create()).ToList()
        };

        server.Arrange().GetWorkflowApi()
            .SetupRequest(HttpMethod.Post, $"tickets/statistics")
            .ReturnsJson(expectedResponse);

        var response = await client.PostAsJsonAsync($"tickets/statistics",new List<string>() { "scope1", "scope2" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsAsync<TicketStatisticsResponse>();
        result.Should().BeEquivalentTo(expectedResponse);
    }
}
