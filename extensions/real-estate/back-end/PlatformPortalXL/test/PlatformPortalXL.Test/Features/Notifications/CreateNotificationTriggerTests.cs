using AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Notification.Requests;
using PlatformPortalXL.Models;
using PlatformPortalXL.Models.NotificationTrigger;
using PlatformPortalXL.ServicesApi.NotificationTriggerApi.Request;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
namespace PlatformPortalXL.Test.Features.Notifications;

public class CreateNotificationTriggerTests : BaseInMemoryTest
{
    public CreateNotificationTriggerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UserIsUnAuthorized_ReturnsForbidden()
    {
        var request = Fixture.Build<CreateNotificationTriggerRequest>().Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient())
        {
            var response = await client.PostAsJsonAsync($"notifications/triggers", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }


    [Theory]
    [InlineData(NotificationTriggerFocus.Twin)]
    [InlineData(NotificationTriggerFocus.SkillCategory)]
    [InlineData(NotificationTriggerFocus.Skill)]
    public async Task CreateNotification_ValidRequest_ReturnResponse(NotificationTriggerFocus focus)
    {
        var userId = Guid.NewGuid();
        var location = new List<string>() { Guid.NewGuid().ToString() };
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, location)
            .With(c => c.Type, NotificationTriggerType.Personal)
            .Without(c => c.WorkGroupIds)
            .With(c => c.Focus, focus)
            .With(c => c.Channels, [NotificationTriggerChannel.InApp])
            .With(c => c.SkillCategories, focus == NotificationTriggerFocus.SkillCategory ? [(int)InsightType.Alert] : null)
            .With(c => c.Twins, focus == NotificationTriggerFocus.Twin ? new List<NotificationTriggerTwinDto> { new NotificationTriggerTwinDto { TwinName = "twin", TwinId = "twin" } } : null)
            .With(c => c.SkillIds, focus == NotificationTriggerFocus.Skill ? ["skill"] : null)
            .With(c => c.Priorities, [(int)InsightPriority.High, (int)InsightPriority.Low, (int)InsightPriority.Medium, (int)InsightPriority.Urgent])
            .Create();
        var notification=new NotificationTrigger()
        {
            Id = Guid.NewGuid(),
            IsEnabled = request.IsEnabled,
            Channels = request.Channels,
            CanUserDisableNotification = request.CanUserDisableNotification,
            SkillCategoryIds = request.SkillCategories?.Select(c=>(int)c).ToList(),
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow,
            Focus = request.Focus,
            Locations = request.Locations,
            PriorityIds = request.Priorities?.Select(c=>(int)c).ToList(),
            SkillIds = request.SkillIds,
            Source = request.Source,
            Twins = NotificationTriggerTwinDto.MapTo(request.Twins),
            Type = request.Type,
            WorkgroupIds = request.WorkGroupIds,
            UpdatedBy = null,
            UpdatedDate = null
        };
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId))
        {
            var apiRequest = new CreateNotificationTriggerApiRequest
            {
                Type = request.Type,
                Source = request.Source,
                Focus = request.Focus,
                Locations = request.Locations,
                IsEnabled = request.IsEnabled,
                CreatedBy = userId,
                CanUserDisableNotification = request.CanUserDisableNotification,
                WorkGroupIds = request.WorkGroupIds,
                SkillCategoryIds = request.SkillCategories,
                TwinCategoryIds = request.TwinCategoryIds,
                Twins = request.Twins,
                SkillIds = request.SkillIds,
                PriorityIds = request.Priorities,
                Channels = request.Channels
            };
            server.Arrange().GetNotificationApi()
                .SetupRequestWithExpectedBody(HttpMethod.Post, $"notifications/triggers", apiRequest)
                .ReturnsJson(notification);

            var response = await client.PostAsJsonAsync($"notifications/triggers", request);
            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            result.Should().BeEquivalentTo(NotificationTriggerDto.MapFrom(notification));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateNotification_LocationIsNullOrEmpty_ReturnResponse(bool locationIsEmpty)
    {
        var userId = Guid.NewGuid();
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, locationIsEmpty ? new List<string>() : null)
            .With(c => c.Type, NotificationTriggerType.Personal)
            .Without(c => c.WorkGroupIds)
            .With(c => c.Channels, new List<NotificationTriggerChannel> { NotificationTriggerChannel.InApp })
            .With(c => c.Focus, NotificationTriggerFocus.SkillCategory)
            .With(c => c.SkillCategories, [(int)InsightType.Alert ])
            .Without(c => c.Priorities)
            .Create();

        var notification = new NotificationTrigger()
        {
            Id = Guid.NewGuid(),
            IsEnabled = request.IsEnabled,
            Channels = request.Channels,
            CanUserDisableNotification = request.CanUserDisableNotification,
            SkillCategoryIds = request.SkillCategories?.Select(c => (int)c).ToList(),
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow,
            Focus = request.Focus,
            PriorityIds = request.Priorities?.Select(c => (int)c).ToList(),
            SkillIds = request.SkillIds,
            Source = request.Source,
            Twins = NotificationTriggerTwinDto.MapTo(request.Twins),
            Type = request.Type,
            WorkgroupIds = request.WorkGroupIds,
            UpdatedBy = null,
            UpdatedDate = null
        };

        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId))
        {
            var apiRequest = new CreateNotificationTriggerApiRequest
            {
                Type = request.Type,
                Source = request.Source,
                Focus = request.Focus,
                Locations = request.Locations,
                IsEnabled = request.IsEnabled,
                CreatedBy = userId,
                CanUserDisableNotification = request.CanUserDisableNotification,
                WorkGroupIds = request.WorkGroupIds,
                SkillCategoryIds = request.SkillCategories,
                TwinCategoryIds = request.TwinCategoryIds,
                Twins = request.Twins,
                SkillIds = request.SkillIds,
                PriorityIds = request.Priorities,
                Channels = request.Channels
            };
            server.Arrange().GetNotificationApi()
                .SetupRequestWithExpectedBody(HttpMethod.Post, $"notifications/triggers", apiRequest)
                .ReturnsJson(notification);

            var response = await client.PostAsJsonAsync($"notifications/triggers", request);

            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            result.Should().BeEquivalentTo(NotificationTriggerDto.MapFrom(notification));
        }
    }

    [Fact]
    public async Task CreateNotification_WorkGroupIsNullOrEmpty_ReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, new List<string>() { "location" })
            .With(c => c.Type, NotificationTriggerType.Workgroup)
            .Without(c => c.WorkGroupIds)
            .With(c => c.Channels, new List<NotificationTriggerChannel> { NotificationTriggerChannel.InApp })
            .With(c => c.Focus, NotificationTriggerFocus.SkillCategory)
            .With(c => c.SkillCategories, [(int)InsightType.Alert])
            .Without(c => c.Priorities)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId))
        {

            var response = await client.PostAsJsonAsync($"notifications/triggers", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }
    }
    [Fact]
    public async Task CreateNotification_WorkGroup_UserIsNotAdmin_ReturnForbidden()
    {
        var userId = Guid.NewGuid();
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, new List<string>() { "location" })
            .With(c => c.Type, NotificationTriggerType.Workgroup)
            .With(c => c.WorkGroupIds,workgroup)
            .With(c => c.Channels, new List<NotificationTriggerChannel> { NotificationTriggerChannel.InApp })
            .With(c => c.Focus, NotificationTriggerFocus.SkillCategory)
            .With(c => c.SkillCategories, [(int)InsightType.Alert])
            .Without(c => c.Priorities)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId))
        {

            var response = await client.PostAsJsonAsync($"notifications/triggers", request);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        }
    }
    [Fact]
    public async Task CreateNotification_WorkGroup_UserIsAdmin_ReturnResponse()
    {
        var userId = Guid.NewGuid();
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, new List<string>() { "location" })
            .With(c => c.Type, NotificationTriggerType.Workgroup)
            .With(c => c.WorkGroupIds, workgroup)
            .With(c => c.Channels, new List<NotificationTriggerChannel> { NotificationTriggerChannel.InApp })
            .With(c => c.Focus, NotificationTriggerFocus.SkillCategory)
            .With(c => c.SkillCategories, [(int)InsightType.Alert])
            .Without(c => c.Priorities)
            .Create();
        var notification = new NotificationTrigger()
        {
            Id = Guid.NewGuid(),
            IsEnabled = request.IsEnabled,
            Channels = request.Channels,
            CanUserDisableNotification = request.CanUserDisableNotification,
            SkillCategoryIds = request.SkillCategories?.Select(c => (int)c).ToList(),
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow,
            Focus = request.Focus,
            PriorityIds = request.Priorities?.Select(c => (int)c).ToList(),
            SkillIds = request.SkillIds,
            Source = request.Source,
            Twins = NotificationTriggerTwinDto.MapTo(request.Twins),
            Type = request.Type,
            WorkgroupIds = request.WorkGroupIds,
            UpdatedBy = null,
            UpdatedDate = null
        };
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId,username:"admin@test.com"))
        {

            var apiRequest = new CreateNotificationTriggerApiRequest
            {
                Type = request.Type,
                Source = request.Source,
                Focus = request.Focus,
                Locations = request.Locations,
                IsEnabled = request.IsEnabled,
                CreatedBy = userId,
                CanUserDisableNotification = request.CanUserDisableNotification,
                WorkGroupIds = request.WorkGroupIds,
                SkillCategoryIds = request.SkillCategories,
                TwinCategoryIds = request.TwinCategoryIds,
                Twins = request.Twins,
                SkillIds = request.SkillIds,
                PriorityIds = request.Priorities,
                Channels = request.Channels
            };
            server.Arrange().GetNotificationApi()
                .SetupRequestWithExpectedBody(HttpMethod.Post, $"notifications/triggers", apiRequest)
                .ReturnsJson(notification);

            var response = await client.PostAsJsonAsync($"notifications/triggers", request);
            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            result.Should().BeEquivalentTo(NotificationTriggerDto.MapFrom(notification));

        }
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateNotification_SkillCategoryIsNullOrEmpty_ReturnBadRequest(bool categoryIsEmpty)
    {
        var userId = Guid.NewGuid();
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, new List<string>() { "location" })
            .With(c => c.Type, NotificationTriggerType.Personal)
            .With(c => c.Channels, new List<NotificationTriggerChannel> { NotificationTriggerChannel.InApp })
            .With(c => c.Focus, NotificationTriggerFocus.SkillCategory)
            .With(c => c.SkillCategories, categoryIsEmpty ? new List<int>() : null)
            .Without(c => c.Priorities)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId))
        {
            var response = await client.PostAsJsonAsync($"notifications/triggers", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateNotification_SkillIsNullOrEmpty_ReturnBadRequest(bool skillIsEmpty)
    {
        var userId = Guid.NewGuid();
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, new List<string>() { "location" })
            .With(c => c.Type, NotificationTriggerType.Personal)
            .With(c => c.Channels, new List<NotificationTriggerChannel> { NotificationTriggerChannel.InApp })
            .With(c => c.Focus, NotificationTriggerFocus.Skill)
            .With(c => c.SkillIds, skillIsEmpty ? new List<string>() : null)
            .Without(c => c.Priorities)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId))
        {

            var response = await client.PostAsJsonAsync($"notifications/triggers", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateNotification_TwinIdIsNullOrEmpty_ReturnBadRequest(bool twinIdIsEmpty)
    {
        var userId = Guid.NewGuid();
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, new List<string>() { "location" })
            .With(c => c.Type, NotificationTriggerType.Personal)
            .With(c => c.Channels, new List<NotificationTriggerChannel> { NotificationTriggerChannel.InApp })
            .With(c => c.Focus, NotificationTriggerFocus.Twin)
            .With(c => c.Twins, twinIdIsEmpty ? new List<NotificationTriggerTwinDto>() : null)
            .Without(c => c.Priorities)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, Guid.NewGuid()))
        {
            var response = await client.PostAsJsonAsync($"notifications/triggers", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }
    }
}
