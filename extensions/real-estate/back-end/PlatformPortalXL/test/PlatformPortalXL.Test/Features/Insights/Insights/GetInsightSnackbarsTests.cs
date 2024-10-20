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
using PlatformPortalXL.ServicesApi.InsightApi;
using Willow.Platform.Models;
using System.Net.Http.Json;
using Willow.Batch;

namespace PlatformPortalXL.Test.Features.Insights.Insights;

public class GetInsightSnackbarsTests : BaseInMemoryTest
{
    public GetInsightSnackbarsTests(ITestOutputHelper output) : base(output)
    {
    }
       
    [Fact]
    public async Task GetInsightSnackbarsTests_UserAssignedToNoSite_ReturnForbidden()
    {
        var userId=Guid.NewGuid();
        var userSites = (List<Site>)null;
         
        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid());

        server.Arrange().GetDirectoryApi()
            .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var response = await client.PostAsJsonAsync($"insights/snackbars/status", new List<FilterSpecificationDto>());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetInsightSnackbarsTests_InputIsValid_ReturnOk()
    {
        var userId = Guid.NewGuid();
        var userSites = Fixture.Build<Site>()
            .With(x => x.Features, new SiteFeatures() { IsInsightsDisabled = false })
            .CreateMany(2).ToList(); ;

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid());

        server.Arrange().GetDirectoryApi()
            .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
            .ReturnsJson(userSites);

        var expectedResponse = Fixture.Build<InsightSnackbarByStatus>()
            .With(x => x.SourceType, InsightSourceType.App)
            .CreateMany(2);

        server.Arrange().GetInsightApi()
            .SetupRequest(HttpMethod.Post, $"insights/snackbars/status")
            .ReturnsJson(expectedResponse);

        var response = await client.PostAsJsonAsync($"insights/snackbars/status", new List<FilterSpecificationDto>());
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadAsAsync<List<InsightSnackbarByStatus>>();
        result.Should().BeEquivalentTo(expectedResponse);
    }
}
