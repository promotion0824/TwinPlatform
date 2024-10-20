using AutoFixture;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Models.NotificationTrigger;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Json;
using Willow.Batch;
using System.Linq;
namespace PlatformPortalXL.Test.Features.Notifications;

public class GetNotificationTriggersTests : BaseInMemoryTest
{
    public GetNotificationTriggersTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetNotificationTriggers_Response()
    {
        var userId = Guid.NewGuid();

        var triggers = Fixture.Build<NotificationTrigger>()
            .CreateMany(5);

        var apiResponse = new BatchDto<NotificationTrigger>() { After = 0, Before = 0, Items = triggers.ToArray(), Total = triggers.Count() };
        var expectedResponse = new BatchDto<NotificationTriggerDto>() { After = 0, Before = 0, Items = NotificationTriggerDto.MapFrom(triggers).ToArray(), Total = triggers.Count() };

        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid()))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Post, $"notifications/triggers/all")
                .ReturnsJson(apiResponse);

            var response = await client.PostAsJsonAsync($"notifications/triggers/all", new BatchRequestDto());
            var result = await response.Content.ReadAsAsync<BatchDto<NotificationTriggerDto>>();
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
