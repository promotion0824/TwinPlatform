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
using Willow.Platform.Models;

namespace PlatformPortalXL.Test.Features.Insights.Insights;

public class GetDiagnosticsSnapshotTests : BaseInMemoryTest
{
    public GetDiagnosticsSnapshotTests(ITestOutputHelper output) : base(output)
    {
    }
       
    [Fact]
    public async Task GetInsightDiagnosticTests_UserAssignedToNoSite_ReturnForbidden()
    {
        var userId=Guid.NewGuid();
        var userSites = (List<Site>)null;
        var insightId=Guid.NewGuid();

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid());

        server.Arrange().GetDirectoryApi()
            .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var response = await client.GetAsync($"insights/{insightId}/diagnostics/snapshot");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetInsightDiagnosticTests_InputIsValid_ReturnOk()
    {
        var userId = Guid.NewGuid();
        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
            .CreateMany(10).ToList(); ;

        var insightId = Guid.NewGuid();

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid());

        server.Arrange().GetDirectoryApi()
            .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var expectedResponse = Fixture.Build<DiagnosticsSnapshotDto>().Without(x => x.Diagnostics).Create();

        server.Arrange().GetInsightApi()
            .SetupRequest(HttpMethod.Get, $"insights/{insightId}/diagnostics/snapshot")
            .ReturnsJson(expectedResponse);

        var response = await client.GetAsync($"insights/{insightId}/diagnostics/snapshot");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsAsync<DiagnosticsSnapshotDto>();
        result.Should().BeEquivalentTo(expectedResponse);
    }
}
