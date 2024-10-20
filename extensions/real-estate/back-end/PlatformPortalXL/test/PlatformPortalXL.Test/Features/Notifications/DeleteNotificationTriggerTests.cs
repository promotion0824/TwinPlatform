using AutoFixture;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.Models.NotificationTrigger;
namespace PlatformPortalXL.Test.Features.Notifications;

public class DeleteNotificationTriggerTests : BaseInMemoryTest
{
    public DeleteNotificationTriggerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UserIsUnAuthorized_ReturnsUnauthorized()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient())
        {
            var response = await client.DeleteAsync($"notifications/triggers/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }


    [Fact]
    public async Task DeleteNotification_IdIsInvalid_ReturnNotFound()
    {
        var userId = Guid.NewGuid();
        var triggerId = Guid.NewGuid();
        NotificationTrigger trigger = null;
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId))
        {
            server.Arrange().GetNotificationApi().SetupRequest(HttpMethod.Delete,
                $"notifications/triggers/{triggerId}?userId={userId}").ReturnsResponse(HttpStatusCode.NoContent);
            server.Arrange().GetNotificationApi().SetupRequest(HttpMethod.Get,
                $"notifications/triggers/{triggerId}").ReturnsJson(trigger);

            var response = await client.DeleteAsync($"notifications/triggers/{triggerId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteNotificationTrigger_PersonalTriggerUserIdIsInvalid_ReturnForbidden(bool isAdminUser)
    {
        var userId = Guid.NewGuid();
        var triggerId = Guid.NewGuid();
        var notification = Fixture.Build<NotificationTrigger>()
            .With(c => c.Id, triggerId)
            .With(c => c.Type, NotificationTriggerType.Personal)
            .Without(c => c.SkillCategoryIds)
            .Without(c => c.PriorityIds).Create();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId, username:isAdminUser?"admin@test.com":"test@test.com"))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(notification);

            var response = await client.DeleteAsync($"notifications/triggers/{triggerId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }

    [Fact]
    public async Task DeleteNotificationTrigger_WorkgroupTriggerUserIsNotAdmin_ReturnForbidden()
    {
        var userId = Guid.NewGuid();
        var triggerId = Guid.NewGuid();
        var notification = Fixture.Build<NotificationTrigger>()
            .With(c => c.Id, triggerId)
            .With(c => c.Type, NotificationTriggerType.Workgroup)
            .Without(c => c.SkillCategoryIds)
            .Without(c => c.PriorityIds).Create();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(notification);

            var response = await client.DeleteAsync($"notifications/triggers/{triggerId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }

    [Theory]
    [InlineData(NotificationTriggerType.Workgroup)]
    [InlineData(NotificationTriggerType.Personal)]
    public async Task DeleteNotificationTrigger_UserAdmin_DeleteTrigger(NotificationTriggerType type)
    {
        var userId = Guid.NewGuid();
        var triggerId = Guid.NewGuid();
        var notification = Fixture.Build<NotificationTrigger>()
            .With(c => c.Id, triggerId)
            .With(c => c.Type, type)
            .With(c=>c.CreatedBy,userId)
            .Without(c => c.SkillCategoryIds)
            .Without(c => c.PriorityIds).Create();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId, username: "admin@test.com"))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(notification);
            server.Arrange().GetNotificationApi().SetupRequest(HttpMethod.Delete,
                $"notifications/triggers/{triggerId}").ReturnsResponse(HttpStatusCode.NoContent);
            var response = await client.DeleteAsync($"notifications/triggers/{triggerId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }

    [Fact]
    public async Task DeleteNotificationTrigger_PersonalTrigger_ValidUserId_DeleteTrigger()
    {
        var userId = Guid.NewGuid();
        var triggerId = Guid.NewGuid();
        var notification = Fixture.Build<NotificationTrigger>()
            .With(c => c.Id, triggerId)
            .With(c => c.CreatedBy, userId)
            .With(c => c.Type, NotificationTriggerType.Personal)
            .Without(c => c.SkillCategoryIds)
            .Without(c => c.PriorityIds).Create();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId, username: "admin@test.com"))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(notification);
            server.Arrange().GetNotificationApi().SetupRequest(HttpMethod.Delete,
                $"notifications/triggers/{triggerId}").ReturnsResponse(HttpStatusCode.NoContent);
            var response = await client.DeleteAsync($"notifications/triggers/{triggerId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }

}
