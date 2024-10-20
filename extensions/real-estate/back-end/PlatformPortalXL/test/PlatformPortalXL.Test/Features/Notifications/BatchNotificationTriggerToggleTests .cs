using PlatformPortalXL.Features.Notification.Requests;
using PlatformPortalXL.Models.NotificationTrigger;
using PlatformPortalXL.ServicesApi.NotificationTriggerApi.Request;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Test.MockServices;
using Willow.Tests.Infrastructure;
using Xunit.Abstractions;
using Xunit;
using Willow.Batch;

namespace PlatformPortalXL.Test.Features.Notifications;

public class BatchNotificationTriggerToggleTests : BaseInMemoryTest
{
    public BatchNotificationTriggerToggleTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UserIsUnAuthorized_ReturnsUnauthorized()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient())
        {
            var response = await client.PostAsJsonAsync($"notifications/triggers/toggle", new BatchNotificationTriggerToggleRequest
            {
                Source = NotificationTriggerSource.Insight
            });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }


    [Fact]
    public async Task BatchNotificationToggle_AdminUser_ReturnResponse()
    {
        var userId = Guid.NewGuid();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId,username:"admin@test.com"))
        {
            server.Arrange().GetNotificationApi().SetupRequestWithExpectedBody(HttpMethod.Post,
                $"notifications/triggers/toggle", new BatchNotificationTriggerToggleApiRequest()
                {
                    Source = NotificationTriggerSource.Insight,
                    UserId = userId,
                    IsAdmin = true,
                    WorkgroupIds = null
                }).ReturnsResponse(HttpStatusCode.NoContent);
            var response = await client.PostAsJsonAsync($"notifications/triggers/toggle", new BatchNotificationTriggerToggleRequest
            {
                Source = NotificationTriggerSource.Insight
            });


            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }

    [Fact]
    public async Task BatchNotificationToggle_NormalUser_ReturnResponse()
    {
        var userId = Guid.NewGuid();
        var workgroupIds = (await new MockUserAuthorizationService().GetApplicationGroupsByUserAsync(userId.ToString(), new BatchRequestDto())).Items.Select(x => x.Id).ToList();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId))
        {
            server.Arrange().GetNotificationApi().SetupRequestWithExpectedBody(HttpMethod.Post,
                $"notifications/triggers/toggle", new BatchNotificationTriggerToggleApiRequest()
                {
                    Source = NotificationTriggerSource.Insight,
                    UserId = userId,
                    IsAdmin = false,
                    WorkgroupIds = workgroupIds
                }).ReturnsResponse(HttpStatusCode.NoContent);
            var response = await client.PostAsJsonAsync($"notifications/triggers/toggle", new BatchNotificationTriggerToggleRequest
            {
                Source = NotificationTriggerSource.Insight
            });


            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }

}
