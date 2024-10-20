using AutoFixture;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using PlatformPortalXL.Controllers;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Notification.Requests;
using PlatformPortalXL.Models;
using PlatformPortalXL.Models.NotificationTrigger;
using PlatformPortalXL.ServicesApi.NotificationTriggerApi.Request;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
namespace PlatformPortalXL.Test.Features.Notifications;

public class GetNotificationTriggerTests : BaseInMemoryTest
{
    public GetNotificationTriggerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UserIsUnAuthorized_ReturnsForbidden()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient())
        {
            var response = await client.GetAsync($"notifications/triggers/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
    [Fact]
    public async Task GetNotification_IdIsInvalid_ReturnNotFound()
    {
        var userId = Guid.NewGuid();
        var triggerId = Guid.NewGuid();
        NotificationTrigger trigger = null;
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid()))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(trigger);
            ;

            var response = await client.GetAsync($"notifications/triggers/{triggerId}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task GetNotification_PersonalTrigger_ReturnResponse()
    {
        var userId = Guid.NewGuid();
        var triggerId = Guid.NewGuid();
        var notification = Fixture.Build<NotificationTrigger>()
            .With(c => c.Id, triggerId)
            .With(c => c.Type, NotificationTriggerType.Personal)
            .With(c => c.CreatedBy, userId)
            .Without(c => c.SkillCategoryIds)
            .Without(c => c.PriorityIds).Create();
       
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClientWithPermissionOnSite(userId,Permissions.ViewSites,Guid.NewGuid()))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(notification);

            var response = await client.GetAsync($"notifications/triggers/{triggerId}");
            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            result.Should().BeEquivalentTo(NotificationTriggerDto.MapFrom(notification));
        }
    }

    [Fact]
    public async Task GetNotification_PersonalTrigger_UserIdIsInvalid_ReturnForbidden()
    {
        var userId = Guid.NewGuid();
        var triggerId = Guid.NewGuid();
        var notification = Fixture.Build<NotificationTrigger>()
            .With(c => c.Id, triggerId)
            .With(c => c.Type, NotificationTriggerType.Personal)
            .With(c => c.CreatedBy, Guid.NewGuid)
            .Without(c => c.SkillCategoryIds)
            .Without(c => c.PriorityIds).Create();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid()))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(notification);

            var response = await client.GetAsync($"notifications/triggers/{triggerId}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
    [Fact]
    public async Task GetNotification_WorkGroupTrigger_UserIsAdmin_ReturnResponse()
    {
        var userId = Guid.NewGuid();
        var triggerId = Guid.NewGuid();
        var notification = Fixture.Build<NotificationTrigger>()
            .With(c => c.Id, triggerId)
            .With(c => c.Type, NotificationTriggerType.Workgroup)
            .With(c => c.CreatedBy, userId)
            .Without(c => c.SkillCategoryIds)
            .Without(c => c.PriorityIds).Create();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId,username:"admin@test.com"))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(notification);

            var response = await client.GetAsync($"notifications/triggers/{triggerId}");
            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            result.Should().BeEquivalentTo(NotificationTriggerDto.MapFrom(notification));
        }
    }

    [Fact]
    public async Task GetNotification_WorkGroupTrigger_UserIsNotAdmin_ReturnForbidden()
    {
        var userId = Guid.NewGuid();
        var triggerId = Guid.NewGuid();
        var notification = Fixture.Build<NotificationTrigger>()
            .With(c => c.Id, triggerId)
            .With(c => c.Type, NotificationTriggerType.Workgroup)
            .With(c => c.CreatedBy, userId)
            .Without(c => c.SkillCategoryIds)
            .Without(c => c.PriorityIds).Create();

        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId, username: "test@test.com"))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(notification);

            var response = await client.GetAsync($"notifications/triggers/{triggerId}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }

}
