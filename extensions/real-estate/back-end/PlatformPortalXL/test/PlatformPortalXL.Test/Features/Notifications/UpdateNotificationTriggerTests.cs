using AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using PlatformPortalXL.Features.Notification.Requests;
using PlatformPortalXL.Models;
using PlatformPortalXL.Models.NotificationTrigger;
using PlatformPortalXL.ServicesApi.NotificationTriggerApi.Request;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Dto;
namespace PlatformPortalXL.Test.Features.Notifications;

public class UpdateNotificationTriggerTests : BaseInMemoryTest
{
    public UpdateNotificationTriggerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UserIsUnAuthorized_ReturnsForbidden()
    {
        var request = Fixture.Build<CreateNotificationTriggerRequest>().Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient())
        {
            var response = await client.PatchAsJsonAsync($"notifications/triggers/{Guid.NewGuid()}", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }


    [Theory]
    //[InlineData(NotificationTriggerFocus.Twin)]
    //[InlineData(NotificationTriggerFocus.SkillCategory)]
    //[InlineData(NotificationTriggerFocus.Skill)]
    [InlineData(NotificationTriggerFocus.TwinCategory)]
    public async Task UpdateNotification_ValidRequest_changeFocus_ReturnResponse(NotificationTriggerFocus focus)
    {
        var userId = Guid.NewGuid();

        var request =  new UpdateNotificationTriggerRequest()
            {
                Focus = focus,
                SkillCategories = focus == NotificationTriggerFocus.SkillCategory ? [(int)InsightType.Alert] : null ,
                TwinCategoryIds = focus == NotificationTriggerFocus.TwinCategory ? ["twin"] : null,
                Twins = focus == NotificationTriggerFocus.Twin ? new List<NotificationTriggerTwinDto> { new NotificationTriggerTwinDto { TwinName = "twin", TwinId = "twin" } } : null ,
                SkillIds = focus == NotificationTriggerFocus.Skill ? ["skill"] : null
            };
       

        var triggerId= Guid.NewGuid();
        var notification=new NotificationTrigger()
        {
            Id = triggerId,
            Type = NotificationTriggerType.Personal,
            SkillCategoryIds = request.SkillCategories?.Select(c=>(int)c).ToList(),
            Focus = request.Focus.Value,
            SkillIds = request.SkillIds,
            Twins = NotificationTriggerTwinDto.MapTo(request.Twins),
            TwinCategoryIds = request.TwinCategoryIds,
            WorkgroupIds = request.WorkGroupIds,
            UpdatedBy = userId,
            CreatedBy = userId

        };
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null,  userId))
        {
            var apiRequest = new UpdateNotificationTriggerApiRequest()
            {
                Type = request.Type,
                Source = request.Source,
                Focus = request.Focus,
                Locations = request.Locations,
                IsEnabled = request.IsEnabled,
                UpdatedBy = userId,
                CanUserDisableNotification = request.CanUserDisableNotification,
                WorkGroupIds = request.WorkGroupIds,
                SkillCategoryIds = request.SkillCategories?.Select(c => (int)c).ToList(),
                TwinCategoryIds = request.TwinCategoryIds,
                Twins = request.Twins,
                SkillIds = request.SkillIds,
                PriorityIds = request.Priorities?.Select(c => (int)c).ToList(),
                Channels = request.Channels,
                IsEnabledForUser = request.IsEnabledForUser
            };
            server.Arrange().GetNotificationApi()
                .SetupRequestWithExpectedBody(HttpMethod.Patch, $"notifications/triggers/{triggerId}", apiRequest).ReturnsResponse(HttpStatusCode.NoContent);
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(notification);

            var response = await client.PatchAsJsonAsync($"notifications/triggers/{triggerId}", request);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
    [Theory]
    [InlineData(NotificationTriggerFocus.Twin)]
    [InlineData(NotificationTriggerFocus.SkillCategory)]
    [InlineData(NotificationTriggerFocus.Skill)]
    public async Task UpdateNotification_InvalidValidRequest_changeFocus_ReturnBadRequest(NotificationTriggerFocus focus)
    {
        var userId = Guid.NewGuid();

        var request = new UpdateNotificationTriggerRequest()
        {
            Focus = focus,
            SkillCategories = null,
            TwinCategoryIds =  null,
            Twins = null,
            SkillIds = null
        };


        var triggerId = Guid.NewGuid();
       
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId))
        {
          
            var response = await client.PatchAsJsonAsync($"notifications/triggers/{triggerId}", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task UpdateNotification_ChangeTypeToWorkGroup_UserIsNotAdmin_ReturnForbidden()
    {
        var userId = Guid.NewGuid();
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Type, NotificationTriggerType.Workgroup)
            .With(c => c.WorkGroupIds, workgroup)
            .With(c => c.Channels, new List<NotificationTriggerChannel> { NotificationTriggerChannel.InApp })
            .With(c => c.Focus, NotificationTriggerFocus.SkillCategory)
            .With(c => c.SkillCategories, [(int)InsightType.Alert])
            .Without(c => c.Priorities)
            .Create();
        var triggerId = Guid.NewGuid();
        var notification = new NotificationTrigger()
        {
            Id = triggerId,
            Type = NotificationTriggerType.Personal,
            SkillCategoryIds = request.SkillCategories?.Select(c => (int)c).ToList(),
            Focus = NotificationTriggerFocus.SkillCategory,
            WorkgroupIds = request.WorkGroupIds,
            UpdatedBy = userId,
            CreatedBy=userId

        };
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(notification);

            var response = await client.PostAsJsonAsync($"notifications/triggers", request);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        }
    }

    [Fact]
    public async Task UpdateNotification_ChangeTypeToWorkGroup_UserIsAdmin_ReturnResponse()
    {
        var userId = Guid.NewGuid();
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = Fixture.Build<UpdateNotificationTriggerRequest>()
            .With(c => c.Type, NotificationTriggerType.Workgroup)
            .With(c => c.WorkGroupIds, workgroup)
            .With(c => c.Channels, new List<NotificationTriggerChannel> { NotificationTriggerChannel.InApp })
            .With(c => c.Focus, NotificationTriggerFocus.SkillCategory)
            .With(c => c.SkillCategories, [(int)InsightType.Alert])
            .Without(c => c.Priorities)
            .Create();
        var triggerId = Guid.NewGuid();
        var notification = new NotificationTrigger()
        {
            Id = triggerId,
            Type = NotificationTriggerType.Personal,
            SkillCategoryIds = request.SkillCategories?.Select(c => (int)c).ToList(),
            Focus = NotificationTriggerFocus.SkillCategory,
            WorkgroupIds = request.WorkGroupIds,
            UpdatedBy = userId,
            CreatedBy = userId

        };
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId,username:"admin@test.com"))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(notification);

            var apiRequest = new UpdateNotificationTriggerApiRequest()
            {
                Type = request.Type,
                Source = request.Source,
                Focus = request.Focus,
                Locations = request.Locations,
                IsEnabled = request.IsEnabled,
                UpdatedBy = userId,
                CanUserDisableNotification = request.CanUserDisableNotification,
                WorkGroupIds = request.WorkGroupIds,
                SkillCategoryIds = request.SkillCategories?.Select(c => (int)c).ToList(),
                TwinCategoryIds = request.TwinCategoryIds,
                Twins = request.Twins,
                SkillIds = request.SkillIds,
                PriorityIds = request.Priorities?.Select(c => (int)c).ToList(),
                Channels = request.Channels,
                IsEnabledForUser = request.IsEnabledForUser
            };
            server.Arrange().GetNotificationApi()
                .SetupRequestWithExpectedBody(HttpMethod.Patch, $"notifications/triggers/{triggerId}", apiRequest).ReturnsResponse(HttpStatusCode.NoContent);

            var response = await client.PatchAsJsonAsync($"notifications/triggers/{triggerId}", request);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }

    [Fact]
    public async Task UpdateNotification_ChangeTypeToPersonal_UserIsNotAdmin_ReturnForbidden()
    {
        var userId = Guid.NewGuid();
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = Fixture.Build<UpdateNotificationTriggerRequest>()
            .With(c => c.Type, NotificationTriggerType.Personal)
            .With(c => c.WorkGroupIds, workgroup)
            .With(c => c.Channels, new List<NotificationTriggerChannel> { NotificationTriggerChannel.InApp })
            .With(c => c.Focus, NotificationTriggerFocus.SkillCategory)
            .With(c => c.SkillCategories, [(int)InsightType.Alert])
            .Without(c => c.Priorities)
            .Create();
        var triggerId = Guid.NewGuid();
        var notification = new NotificationTrigger()
        {
            Id = triggerId,
            Type = NotificationTriggerType.Workgroup,
            SkillCategoryIds = request.SkillCategories?.Select(c => (int)c).ToList(),
            Focus = NotificationTriggerFocus.SkillCategory,
            WorkgroupIds = request.WorkGroupIds,
            UpdatedBy = userId,
            CreatedBy=userId

        };
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(notification);

       
            var response = await client.PatchAsJsonAsync($"notifications/triggers/{triggerId}", request);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        }
    }

    [Fact]
    public async Task UpdateNotification_UserHasNoAccess_ReturnForbidden()
    {
        var userId = Guid.NewGuid();
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = Fixture.Build<UpdateNotificationTriggerRequest>()
            .With(c => c.Type, NotificationTriggerType.Personal)
            .With(c => c.WorkGroupIds, workgroup)
            .With(c => c.Channels, new List<NotificationTriggerChannel> { NotificationTriggerChannel.InApp })
            .With(c => c.Focus, NotificationTriggerFocus.SkillCategory)
            .With(c => c.SkillCategories, [(int)InsightType.Alert])
            .Without(c => c.Priorities)
            .Create();
        var triggerId = Guid.NewGuid();
        var notification = new NotificationTrigger()
        {
            Id = triggerId,
            Type = NotificationTriggerType.Personal,
            SkillCategoryIds = request.SkillCategories?.Select(c => (int)c).ToList(),
            Focus = NotificationTriggerFocus.SkillCategory,
            WorkgroupIds = request.WorkGroupIds,
            CreatedBy = Guid.NewGuid()

        };
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(notification);


            var response = await client.PatchAsJsonAsync($"notifications/triggers/{triggerId}", request);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        }
    }
    [Fact]
    public async Task UpdateNotification_WorkGroupTriggers_UserCannotDisable_IsEnabledForUserIsTrue_ReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = new UpdateNotificationTriggerRequest
        {
            IsEnabledForUser = false
        };
        var triggerId = Guid.NewGuid();
        var notification = new NotificationTrigger()
        {
            Id = triggerId,
            Type = NotificationTriggerType.Workgroup,
            SkillCategoryIds = request.SkillCategories?.Select(c => (int)c).ToList(),
            Focus = NotificationTriggerFocus.SkillCategory,
            WorkgroupIds = workgroup,
            CreatedBy = Guid.NewGuid(),
            CanUserDisableNotification = false
        };
        using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
        using (var client = server.CreateClient(null, userId,username:"admin@test.com"))
        {
            server.Arrange().GetNotificationApi()
                .SetupRequest(HttpMethod.Get, $"notifications/triggers/{triggerId}")
                .ReturnsJson(notification);


            var response = await client.PatchAsJsonAsync($"notifications/triggers/{triggerId}", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }
    }
}
