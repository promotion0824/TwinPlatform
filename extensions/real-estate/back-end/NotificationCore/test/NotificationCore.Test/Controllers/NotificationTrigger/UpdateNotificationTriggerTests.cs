using AutoFixture;
using FluentAssertions;
using NotificationCore.Controllers.Requests;
using NotificationCore.Dto;
using NotificationCore.Entities;
using NotificationCore.Models;
using NotificationCore.Test.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Willow.Infrastructure;

namespace NotificationCore.Test.Controllers.NotificationTrigger;

public class UpdateNotificationTriggerTests : BaseInMemoryTest
{
    public UpdateNotificationTriggerTests(ITestOutputHelper output) : base(output)
    {
    }
    [Fact]
    public async Task UpdateNotification_ChangeLocation_ReturnResponse()
    {
        var userId = Guid.NewGuid();
      
        var request = new UpdateNotificationTriggerRequest()
        {
            Locations = ["l2"],
            UpdatedBy = userId
        };
        var triggerId= Guid.NewGuid();
        var entity = Fixture.Build<NotificationTriggerEntity>()
             .With(c => c.Locations,new List<LocationEntity>()
             {
                 new LocationEntity()
                 {
                     Id = "l1",
                     NotificationTriggerId = triggerId
                 }
             })
             .With(c => c.ChannelJson, new List<NotificationChannel> { NotificationChannel.InApp })
             .Without(c => c.PriorityJson)
             .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddAsync(entity);
             
            await db.SaveChangesAsync();
            var response = await client.PatchAsJsonAsync($"notifications/Triggers/{entity.Id}", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            var updatedNotification = NotificationTriggerEntity.MapTo(db.NotificationTriggers.Include(c => c.Locations).First(c => c.Id == entity.Id));
            updatedNotification.Locations.Should().BeEquivalentTo(request.Locations);
            updatedNotification.UpdatedBy.Should().Be(userId);
        }
    }
    [Fact]
    public async Task UpdateNotification_ChangeLocationToAll_ReturnResponse()
    {
        var userId = Guid.NewGuid();

        var request = new UpdateNotificationTriggerRequest()
        {
            AllLocation = true,
            UpdatedBy = userId
        };
        var triggerId = Guid.NewGuid();
        var entity = Fixture.Build<NotificationTriggerEntity>()
             .With(c => c.Locations, new List<LocationEntity>()
             {
                 new LocationEntity()
                 {
                     Id = "l1",
                     NotificationTriggerId = triggerId
                 }
             })
             .With(c => c.ChannelJson, new List<NotificationChannel> { NotificationChannel.InApp })
             .Without(c => c.PriorityJson)
             .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddAsync(entity);

            await db.SaveChangesAsync();
            var response = await client.PatchAsJsonAsync($"notifications/Triggers/{entity.Id}", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            var updatedNotification = NotificationTriggerEntity.MapTo(db.NotificationTriggers.Include(c => c.Locations).First(c => c.Id == entity.Id));
            updatedNotification.Locations.Should().BeNullOrEmpty();
        }
    }
    [Theory]
    [InlineData(NotificationType.Workgroup)]
    [InlineData(NotificationType.Personal)]
    public async Task UpdateNotification_ChangeType_ReturnResponse(NotificationType type)
    {
        var userId = Guid.NewGuid();
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = new UpdateNotificationTriggerRequest()
        {
            Type = type,
            WorkGroupIds = type == NotificationType.Workgroup ? workgroup : null,
            UpdatedBy = userId
        };
       var entity=Fixture.Build<NotificationTriggerEntity>()
            .With(c => c.Type, type== NotificationType.Personal?NotificationType.Workgroup:NotificationType.Personal)
            .With(c => c.ChannelJson, new List<NotificationChannel> {NotificationChannel.InApp})
            .Without(c=>c.WorkgroupSubscriptions)
            .Without(c=>c.PriorityJson)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddAsync(entity);
            if (type == NotificationType.Personal)
                await db.WorkgroupSubscriptions.AddRangeAsync(workgroup.Select(c => new WorkgroupSubscriptionEntity()
                {
                    NotificationTriggerId = entity.Id,
                    WorkgroupId = c
                }));
            else
            {
                 db.WorkgroupSubscriptions.RemoveRange(
                    db.WorkgroupSubscriptions.Where(c => c.NotificationTriggerId == entity.Id));
            }
            await db.SaveChangesAsync();
            var response = await client.PatchAsJsonAsync($"notifications/Triggers/{entity.Id}", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            var updatedNotification =NotificationTriggerEntity.MapTo(db.NotificationTriggers.Include(c=>c.Locations).Include(c=>c.WorkgroupSubscriptions).First(c => c.Id == entity.Id));
            updatedNotification.Type.Should().Be(type );
            if(type==NotificationType.Workgroup)
                updatedNotification.WorkgroupIds.Should().BeEquivalentTo(request.WorkGroupIds);
            else
                updatedNotification.WorkgroupIds.Should().BeNullOrEmpty();
            
            updatedNotification.UpdatedBy.Should().Be(userId);
        }
    }

    [Theory]
    [InlineData(NotificationFocus.TwinCategory,NotificationFocus.Skill)]
    [InlineData(NotificationFocus.Skill,NotificationFocus.Twin)]
    [InlineData(NotificationFocus.SkillCategory,NotificationFocus.TwinCategory)]
    [InlineData(NotificationFocus.Twin,NotificationFocus.SkillCategory)]
    public async Task UpdateNotification_ChangeFocus_ReturnResponse(NotificationFocus newFocus, NotificationFocus prvFocus)
    {
        var userId = Guid.NewGuid();

        var request = new UpdateNotificationTriggerRequest()
        {
            Focus = newFocus,
            TwinCategoryIds = newFocus== NotificationFocus.TwinCategory ? new List<string> { "1" } : null,
            Twins = newFocus == NotificationFocus.Twin ? new List<NotificationTriggerTwinDto> { new NotificationTriggerTwinDto { TwinName = "twin", TwinId = "twin" } } : null,
            SkillCategoryIds = newFocus == NotificationFocus.SkillCategory ? new List<int> { 1 } : null,
            SkillIds = newFocus == NotificationFocus.Skill ? new List<string> { "1" } : null,
            UpdatedBy = userId
        };
        var triggerId=Guid.NewGuid();
        var entity = Fixture.Build<NotificationTriggerEntity>()
            .With(c=>c.Id, triggerId)
             .With(c => c.Focus, prvFocus )
             .With(c => c.ChannelJson, new List<NotificationChannel> { NotificationChannel.InApp })
             .With(c => c.SkillCategories, prvFocus == NotificationFocus.SkillCategory ? new List<NotificationTriggerSkillCategoryEntity>
             {
                 new ()
                 {
                     CategoryId = 2,NotificationTriggerId = triggerId
                 }
             } : null)
             .With(c => c.Twins, prvFocus == NotificationFocus.Twin ? new List<NotificationTriggerTwinEntity>
            {
                new ()
                {
                    TwinId = "2",NotificationTriggerId = triggerId
                }
            } : null)
             .With(c => c.TwinCategories, prvFocus == NotificationFocus.TwinCategory ? new List<NotificationTriggerTwinCategoryEntity>
            {
                new ()
                {
                    CategoryId = "2",NotificationTriggerId = triggerId
                }
            } : null)
             .With(c => c.Skills, prvFocus == NotificationFocus.Skill ? new List<NotificationTriggerSkillEntity>
            {
                new ()
                {
                    SkillId = "2",NotificationTriggerId = triggerId
                }
            } : null)
             .Without(c => c.PriorityJson)
             .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddAsync(entity);
            await db.SaveChangesAsync();

            var response = await client.PatchAsJsonAsync($"notifications/Triggers/{entity.Id}", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            var updatedNotification = NotificationTriggerEntity.MapTo(db.NotificationTriggers.Include(c => c.Locations)
                .Include(c => c.WorkgroupSubscriptions)
                .Include(c=>c.SkillCategories)
                .Include(c=>c.TwinCategories)
                .Include(c=>c.Twins)
                .Include(c=>c.Skills)
                .First(c => c.Id == entity.Id));
            updatedNotification.Focus.Should().Be(newFocus);
            switch (prvFocus)
            {
                case NotificationFocus.Skill:
                    updatedNotification.SkillIds.Should().BeNullOrEmpty();
                    break;
                case NotificationFocus.Twin:
                    updatedNotification.Twins.Should().BeNullOrEmpty();
                    break;
                case NotificationFocus.TwinCategory:
                    updatedNotification.TwinCategoryIds.Should().BeNullOrEmpty();
                    break;
                case NotificationFocus.SkillCategory:
                    updatedNotification.SkillCategoryIds.Should().BeNullOrEmpty();
                    break;
            }
            switch (newFocus)
            {
                case NotificationFocus.Skill:
                    updatedNotification.SkillIds.Should().BeEquivalentTo(request.SkillIds);
                    break;
                case NotificationFocus.Twin:
                    updatedNotification.Twins.Should().BeEquivalentTo(request.Twins);
                    break;
                case NotificationFocus.TwinCategory:
                    updatedNotification.TwinCategoryIds.Should().BeEquivalentTo(request.TwinCategoryIds);
                    break;
                case NotificationFocus.SkillCategory:
                    updatedNotification.SkillCategoryIds.Should().BeEquivalentTo(request.SkillCategoryIds);
                    break;
            }

            updatedNotification.UpdatedBy.Should().Be(userId);
        }
    }


    [Theory]
    [InlineData(NotificationFocus.TwinCategory)]
    [InlineData(NotificationFocus.Skill)]
    [InlineData(NotificationFocus.SkillCategory)]
    [InlineData(NotificationFocus.Twin)]
    public async Task UpdateNotification_ChangeFocus_InvalidRequest_ReturnBadRequest(NotificationFocus newFocus)
    {
        var userId = Guid.NewGuid();

        var request = new UpdateNotificationTriggerRequest()
        {
            Focus = newFocus,
            UpdatedBy = userId
        };
     
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

           
            var response = await client.PatchAsJsonAsync($"notifications/Triggers/{Guid.NewGuid()}", request);

            if (newFocus == NotificationFocus.TwinCategory)
            {
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
            else
            {
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }
    }

    [Fact]
    public async Task UpdateNotification_ChangeType_InvalidRequest_ReturnBadRequest()
    {
        var userId = Guid.NewGuid();

        var request = new UpdateNotificationTriggerRequest()
        {
            Type = NotificationType.Workgroup,
            UpdatedBy = userId
        };

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);


            var response = await client.PatchAsJsonAsync($"notifications/Triggers/{Guid.NewGuid()}", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }
    }

    [Theory]
    [InlineData(NotificationFocus.TwinCategory)]
    [InlineData(NotificationFocus.Skill)]
    [InlineData(NotificationFocus.SkillCategory)]
    [InlineData(NotificationFocus.Twin)]
    public async Task UpdateNotification_ChangeFocusValues_ReturnResponse(NotificationFocus currentFocus)
    {
        var userId = Guid.NewGuid();

        var request = new UpdateNotificationTriggerRequest()
        {
            Focus = currentFocus,
            TwinCategoryIds = currentFocus == NotificationFocus.TwinCategory ? new List<string> { "1" } : null,
            Twins = currentFocus == NotificationFocus.Twin ? new List<NotificationTriggerTwinDto> { new NotificationTriggerTwinDto { TwinName = "twin", TwinId = "twin" } } : null,
            SkillCategoryIds = currentFocus == NotificationFocus.SkillCategory ? new List<int> { 1 } : null,
            SkillIds = currentFocus == NotificationFocus.Skill ? new List<string> { "1" } : null,
            UpdatedBy = userId
        };
        var triggerId = Guid.NewGuid();
        var entity = Fixture.Build<NotificationTriggerEntity>()
            .With(c => c.Id, triggerId)
             .With(c => c.Focus, currentFocus)
             .With(c => c.ChannelJson, new List<NotificationChannel> { NotificationChannel.InApp })
             .With(c => c.SkillCategories, currentFocus == NotificationFocus.SkillCategory ? new List<NotificationTriggerSkillCategoryEntity>
             {
                 new ()
                 {
                     CategoryId = 2,NotificationTriggerId = triggerId
                 }
             } : null)
             .With(c => c.Twins, currentFocus == NotificationFocus.Twin ? new List<NotificationTriggerTwinEntity>
            {
                new ()
                {
                    TwinId = "2",NotificationTriggerId = triggerId
                }
            } : null)
             .With(c => c.TwinCategories, currentFocus == NotificationFocus.TwinCategory ? new List<NotificationTriggerTwinCategoryEntity>
            {
                new ()
                {
                    CategoryId = "2",NotificationTriggerId = triggerId
                }
            } : null)
             .With(c => c.Skills, currentFocus == NotificationFocus.Skill ? new List<NotificationTriggerSkillEntity>
            {
                new ()
                {
                    SkillId = "2",NotificationTriggerId = triggerId
                }
            } : null)
             .Without(c => c.PriorityJson)
             .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddAsync(entity);
            await db.SaveChangesAsync();

            var response = await client.PatchAsJsonAsync($"notifications/Triggers/{entity.Id}", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            var updatedNotification = NotificationTriggerEntity.MapTo(db.NotificationTriggers.Include(c => c.Locations)
                .Include(c => c.WorkgroupSubscriptions)
                .Include(c => c.SkillCategories)
                .Include(c => c.TwinCategories)
                .Include(c => c.Twins)
                .Include(c => c.Skills)
                .First(c => c.Id == entity.Id));
            updatedNotification.Focus.Should().Be(currentFocus);

            switch (currentFocus)
            {
                case NotificationFocus.Skill:
                    updatedNotification.SkillIds.Should().BeEquivalentTo(request.SkillIds);
                    break;
                case NotificationFocus.Twin:
                    updatedNotification.Twins.Should().BeEquivalentTo(request.Twins);
                    break;
                case NotificationFocus.TwinCategory:
                    updatedNotification.TwinCategoryIds.Should().BeEquivalentTo(request.TwinCategoryIds);
                    break;
                case NotificationFocus.SkillCategory:
                    updatedNotification.SkillCategoryIds.Should().BeEquivalentTo(request.SkillCategoryIds);
                    break;
            }

            updatedNotification.UpdatedBy.Should().Be(userId);
        }
    }

    [Theory]
    [InlineData(NotificationType.Workgroup)]
    [InlineData(NotificationType.Personal)]
    public async Task UpdateNotification_ChangeWorkgroupIds_ReturnResponse(NotificationType type)
    {
        var userId = Guid.NewGuid();
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = new UpdateNotificationTriggerRequest()
        {
            Type = type,
            WorkGroupIds = type == NotificationType.Workgroup ? workgroup : null,
            UpdatedBy = userId
        };
        var triggerId=Guid.NewGuid();
        var entity = Fixture.Build<NotificationTriggerEntity>()
            .With(c=>c.Id, triggerId)
             .With(c => c.Type, type )
             .With(c => c.ChannelJson, new List<NotificationChannel> { NotificationChannel.InApp })
             .With(c=>c.WorkgroupSubscriptions,type==NotificationType.Personal?null:new List<WorkgroupSubscriptionEntity>()
             {
                 new WorkgroupSubscriptionEntity()
                 {
                     NotificationTriggerId = triggerId,
                     WorkgroupId = Guid.NewGuid()
                 }
             })
             .Without(c => c.PriorityJson)
             .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddAsync(entity);
            
            await db.SaveChangesAsync();
            var response = await client.PatchAsJsonAsync($"notifications/Triggers/{entity.Id}", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            var updatedNotification = NotificationTriggerEntity.MapTo(db.NotificationTriggers.Include(c => c.Locations).Include(c => c.WorkgroupSubscriptions).First(c => c.Id == entity.Id));
            updatedNotification.Type.Should().Be(type);
            if (type == NotificationType.Workgroup)
                updatedNotification.WorkgroupIds.Should().BeEquivalentTo(request.WorkGroupIds);
            else
                updatedNotification.WorkgroupIds.Should().BeNullOrEmpty();

            updatedNotification.UpdatedBy.Should().Be(userId);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateNotification_DisableTheTrigger_ReturnResponse(bool isEnable)
    {
        var userId = Guid.NewGuid();
        var request = new UpdateNotificationTriggerRequest()
        {
            IsEnabled = isEnable,
            UpdatedBy = userId
        };
        var triggerId = Guid.NewGuid();
        var entity = Fixture.Build<NotificationTriggerEntity>()
            .With(c => c.Id, triggerId)
            .With(c=>c.IsEnabled,!isEnable)
            .With(c => c.ChannelJson, new List<NotificationChannel> { NotificationChannel.InApp })
            .Without(c => c.PriorityJson)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddAsync(entity);

            await db.SaveChangesAsync();
            var response = await client.PatchAsJsonAsync($"notifications/Triggers/{entity.Id}", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            var updatedNotification = NotificationTriggerEntity.MapTo(db.NotificationTriggers.First(c => c.Id == entity.Id));
            updatedNotification.IsEnabled.Should().Be(isEnable);
            updatedNotification.UpdatedBy.Should().Be(userId);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateNotification_DisableTheTriggerForUser_NoUserOverride_ReturnResponse(bool isEnable)
    {
        var userId = Guid.NewGuid();
        var request = new UpdateNotificationTriggerRequest()
        {
            IsEnabledForUser = isEnable,
            UpdatedBy = userId
        };
        var triggerId = Guid.NewGuid();
        var entity = Fixture.Build<NotificationTriggerEntity>()
            .With(c => c.Id, triggerId)
            .With(c => c.IsEnabled, true)
            .With(c => c.ChannelJson, new List<NotificationChannel> { NotificationChannel.InApp })
            .Without(c => c.PriorityJson)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddAsync(entity);

            await db.SaveChangesAsync();
            var response = await client.PatchAsJsonAsync($"notifications/Triggers/{entity.Id}", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            await response.Content.ReadAsAsync<NotificationTriggerDto>();
            var updatedNotification = NotificationTriggerEntity.MapTo(db.NotificationTriggers.Include(c => c.NotificationSubscriptionOverrides).First(c => c.Id == entity.Id));
            if(entity.Type==NotificationType.Personal)
                updatedNotification.IsEnabled.Should().Be(isEnable);
            else
            {
                updatedNotification.NotificationSubscriptionOverrides
                    .FirstOrDefault(c => c.UserId == request.UpdatedBy && c.IsEnabled == isEnable).Should().NotBeNull();
            }
            updatedNotification.UpdatedBy.Should().Be(userId);
        }
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateNotification_DisableTheTriggerForUser_WithUserOverride_ReturnResponse(bool isEnable)
    {
        var userId = Guid.NewGuid();
        var request = new UpdateNotificationTriggerRequest()
        {
            IsEnabledForUser = isEnable,
            UpdatedBy = userId
        };
        var triggerId = Guid.NewGuid();
        var entity = Fixture.Build<NotificationTriggerEntity>()
            .With(c => c.Id, triggerId)
            .With(c => c.IsEnabled, true)
            .With(c => c.ChannelJson, new List<NotificationChannel> { NotificationChannel.InApp })
            .Without(c => c.PriorityJson)
            .Create();
        entity.NotificationSubscriptionOverrides = new List<NotificationSubscriptionOverrideEntity>()
        {
            new NotificationSubscriptionOverrideEntity()
            {
                NotificationTriggerId = triggerId,
                UserId = userId,
                IsEnabled = !isEnable
            }
        };
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddAsync(entity);

            await db.SaveChangesAsync();
            var response = await client.PatchAsJsonAsync($"notifications/Triggers/{entity.Id}", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            await response.Content.ReadAsAsync<NotificationTriggerDto>();
            var updatedNotification = NotificationTriggerEntity.MapTo(db.NotificationTriggers.Include(c => c.NotificationSubscriptionOverrides).First(c => c.Id == entity.Id));
            if (entity.Type == NotificationType.Personal)
                updatedNotification.IsEnabled.Should().Be(isEnable);
            else
            {
                updatedNotification.NotificationSubscriptionOverrides
                    .SingleOrDefault(c => c.UserId == request.UpdatedBy && c.IsEnabled == isEnable).Should().NotBeNull();
            }
            updatedNotification.UpdatedBy.Should().Be(userId);
        }
    }
}
