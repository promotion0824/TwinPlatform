using AutoFixture;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using PlatformPortalXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;
using Willow.Batch;
using PlatformPortalXL.Models.Notification;
using System.Collections.Generic;
using System.Linq;
namespace PlatformPortalXL.Test.Features.Notifications;

public class GetNotificationStatesStatsTests : BaseInMemoryTest
{
    public GetNotificationStatesStatsTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetNotificationStatesStats_Response()
    {
        var userId = Guid.NewGuid();

        var stats = Fixture.Build<NotificationStatesStats>()
            .CreateMany(5).ToList();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid()))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Post, $"notifications/states/stats")
                .ReturnsJson(stats);

            var response = await client.PostAsJsonAsync($"notifications/states/stats", new List<FilterSpecificationDto>());

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<List<NotificationStatesStats>>();
            result.Should().BeEquivalentTo(stats);
        }
    }
}
