using AutoFixture;
using Azure.Core;
using FluentAssertions;
using NotificationCore.Dto;
using NotificationCore.Entities;
using NotificationCore.Models;
using NotificationCore.Test.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Willow.Batch;
using Xunit;
using Xunit.Abstractions;

namespace NotificationCore.Test.Controllers.NotificationTrigger;

public class GetNotificationTriggersTests: BaseInMemoryTest
{
    public GetNotificationTriggersTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetNotificationsTriggers_ValidRequest_ReturnResponse()
    {
        var entities = Fixture.Build<NotificationTriggerEntity>()
            .With(c => c.ChannelJson, new List<NotificationChannel> { NotificationChannel.InApp })
            .With(c => c.PriorityJson, new List<int> { 1 })
            .CreateMany(5);

        foreach(var entity in entities)
        {
            entity.Locations = new List<LocationEntity>()
            {
                new LocationEntity()
                {
                    Id = "l1",
                    NotificationTriggerId = entity.Id
                }
            };
        }

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddRangeAsync(entities);
            await db.SaveChangesAsync();

            var response = await client.PostAsJsonAsync($"notifications/triggers/all", new BatchRequestDto());
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<BatchDto<NotificationTriggerDto>>();
            result.Items.Count().Should().Be(5);
        }
    }

    [Fact]
    public async Task GetNotificationsTriggers_Focus_ReturnResponse()
    {
        var entities = Fixture.Build<NotificationTriggerEntity>()
            .With(c => c.ChannelJson, new List<NotificationChannel> { NotificationChannel.InApp })
            .With(c => c.PriorityJson, new List<int> { 1 })
        .CreateMany(1);

        var workroupTriggerIds = entities.Where(x => x.Type == NotificationType.Workgroup).Select(x => x.Id).ToList();
        var workgroupSubscribers = entities.Where(x => x.Type == NotificationType.Workgroup)
            .Select(x => Fixture.Build<WorkgroupSubscriptionEntity>().With(e => e.NotificationTriggerId, x.Id).Create()
        );

        var focusGroups = entities.GroupBy(x => x.Focus);

        var skills = new List<NotificationTriggerSkillEntity>();
        if (focusGroups.Any(x => x.Key == NotificationFocus.Skill))
        {
            skills = entities.Where(x => x.Focus == NotificationFocus.Skill)
                .SelectMany(x => Fixture.Build<NotificationTriggerSkillEntity>()
                    .With(e => e.NotificationTriggerId, x.Id)
                    .Without(e => e.NotificationTrigger).CreateMany(3)).ToList();
        }

        var twins = new List<NotificationTriggerTwinEntity>();
        if (focusGroups.Any(x => x.Key == NotificationFocus.Twin))
        {
            twins = entities.Where(x => x.Focus == NotificationFocus.Twin)
                .SelectMany(x => Fixture.Build<NotificationTriggerTwinEntity>()
                    .With(e => e.NotificationTriggerId, x.Id)
                    .Without(e => e.NotificationTrigger).CreateMany(3)).ToList();
        }

        var twinCategories = new List<NotificationTriggerTwinCategoryEntity>();
        if (focusGroups.Any(x => x.Key == NotificationFocus.TwinCategory))
        {
            twinCategories = entities.Where(x => x.Focus == NotificationFocus.TwinCategory)
                .SelectMany(x => Fixture.Build<NotificationTriggerTwinCategoryEntity>()
                    .With(e => e.NotificationTriggerId, x.Id)
                    .Without(e => e.NotificationTrigger).CreateMany(3)).ToList();
        }

        var skillCategories = new List<NotificationTriggerSkillCategoryEntity>();
        if (focusGroups.Any(x => x.Key == NotificationFocus.SkillCategory))
        {
            skillCategories = entities.Where(x => x.Focus == NotificationFocus.SkillCategory)
                .SelectMany(x => Fixture.Build<NotificationTriggerSkillCategoryEntity>()
                    .With(e => e.NotificationTriggerId, x.Id)
                    .Without(e => e.NotificationTrigger).CreateMany(3)).ToList();
        }

        foreach (var entity in entities)
        {
            entity.Locations = new List<LocationEntity>()
            {
                new LocationEntity()
                {
                    Id = "l1",
                    NotificationTriggerId = entity.Id
                }
            };
        }

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddRangeAsync(entities);
            await db.NotificationTriggerSkills.AddRangeAsync(skills);
            await db.NotificationTriggerSkillCategories.AddRangeAsync(skillCategories);
            await db.NotificationTriggerTwins.AddRangeAsync(twins);
            await db.NotificationTriggerTwinCategories.AddRangeAsync(twinCategories);
            await db.SaveChangesAsync();

            var response = await client.PostAsJsonAsync($"notifications/triggers/all", new BatchRequestDto());
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<BatchDto<NotificationTriggerDto>>();
            result.Items.Count().Should().Be(entities.Count());

            result.Items.Where(x => x.Focus == NotificationFocus.Skill).ToList().Count().Should().Be(skills.Count());
        }
    }

    [Fact]
    public async Task GetNotificationsTriggers_FilterByUser_ReturnResponse()
    {
        var nonadminUser1Id = Guid.NewGuid();
        var nonadminUser2Id = Guid.NewGuid();
        var adminUser1Id = Guid.NewGuid();
        var adminUser2Id = Guid.NewGuid();
        var workgroup1Id = Guid.NewGuid();
        var workgroup2Id = Guid.NewGuid();

        var nonadminUser1Entities = Fixture.Build<NotificationTriggerEntity>()
            .With(x => x.Type, NotificationType.Personal)
            .With(x => x.CreatedBy, nonadminUser1Id)
            .CreateMany(5);

        var nonadminUser2Entities = Fixture.Build<NotificationTriggerEntity>()
            .With(x => x.Type, NotificationType.Personal)
            .With(x => x.CreatedBy, nonadminUser2Id)
            .CreateMany(5);

        var adminUser1Entities = Fixture.Build<NotificationTriggerEntity>()
            //.With(x => x.Type, NotificationType.Workgroup)
            .With(x => x.CreatedBy, adminUser1Id)
            .CreateMany(10);

        var adminUser2Entities = Fixture.Build<NotificationTriggerEntity>()
            //.With(x => x.Type, NotificationType.Workgroup)
            .With(x => x.CreatedBy, adminUser2Id)
            .CreateMany(10);

        foreach(var entity in adminUser1Entities.Where(x => x.Type == NotificationType.Workgroup))
        {
            entity.WorkgroupSubscriptions = [
                new WorkgroupSubscriptionEntity()
                {
                    NotificationTriggerId = entity.Id,
                    WorkgroupId = workgroup1Id
                }
            ];
        }

        foreach (var entity in adminUser2Entities.Where(x => x.Type == NotificationType.Workgroup))
        {
            entity.WorkgroupSubscriptions = [
                new WorkgroupSubscriptionEntity()
                {
                    NotificationTriggerId = entity.Id,
                    WorkgroupId = workgroup2Id
                }
            ];
        }

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddRangeAsync(nonadminUser1Entities);
            await db.NotificationTriggers.AddRangeAsync(nonadminUser2Entities);
            await db.NotificationTriggers.AddRangeAsync(adminUser1Entities);
            await db.NotificationTriggers.AddRangeAsync(adminUser2Entities);
            await db.SaveChangesAsync();

            var response = await client.PostAsJsonAsync($"notifications/triggers/all", new BatchRequestDto()
            {
                FilterSpecifications =
                [
                    new FilterSpecificationDto()
                    {
                        Field = nameof(NotificationTriggerDto.CreatedBy),
                        Operator = FilterOperators.ContainedIn,
                        Value = new List<Guid>() { nonadminUser1Id }
                    }
                ]
            });
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<BatchDto<NotificationTriggerDto>>();
            result.Items.Count().Should().Be(5);
        }
    }

    [Fact]
    public async Task GetNotificationsTriggers_FilterByUserIdAndWorkgroupId_ReturnResponse()
    {
        var nonadminUser1Id = Guid.NewGuid();
        var nonadminUser2Id = Guid.NewGuid();
        var adminUser1Id = Guid.NewGuid();
        var adminUser2Id = Guid.NewGuid();
        var workgroup1Id = Guid.NewGuid();
        var workgroup2Id = Guid.NewGuid();

        var nonadminUser1Entities = Fixture.Build<NotificationTriggerEntity>()
            .With(x => x.Type, NotificationType.Personal)
            .With(x => x.CreatedBy, nonadminUser1Id)
            .CreateMany(5);

        var nonadminUser2Entities = Fixture.Build<NotificationTriggerEntity>()
            .With(x => x.Type, NotificationType.Personal)
            .With(x => x.CreatedBy, nonadminUser2Id)
            .CreateMany(5);

        var adminUser1Entities = Fixture.Build<NotificationTriggerEntity>()
            //.With(x => x.Type, NotificationType.Workgroup)
            .With(x => x.CreatedBy, adminUser1Id)
            .CreateMany(10);

        var adminUser2Entities = Fixture.Build<NotificationTriggerEntity>()
            //.With(x => x.Type, NotificationType.Workgroup)
            .With(x => x.CreatedBy, adminUser2Id)
            .CreateMany(10);

        foreach (var entity in adminUser1Entities.Where(x => x.Type == NotificationType.Workgroup))
        {
            entity.WorkgroupSubscriptions = [
                new WorkgroupSubscriptionEntity()
                {
                    NotificationTriggerId = entity.Id,
                    WorkgroupId = workgroup1Id
                }
            ];
        }

        foreach (var entity in adminUser2Entities.Where(x => x.Type == NotificationType.Workgroup))
        {
            entity.WorkgroupSubscriptions = [
                new WorkgroupSubscriptionEntity()
                {
                    NotificationTriggerId = entity.Id,
                    WorkgroupId = workgroup2Id
                }
            ];
        }

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddRangeAsync(nonadminUser1Entities);
            await db.NotificationTriggers.AddRangeAsync(nonadminUser2Entities);
            await db.NotificationTriggers.AddRangeAsync(adminUser1Entities);
            await db.NotificationTriggers.AddRangeAsync(adminUser2Entities);
            await db.SaveChangesAsync();

            var response = await client.PostAsJsonAsync($"notifications/triggers/all", new BatchRequestDto()
            {
                FilterSpecifications =
                [
                    new FilterSpecificationDto()
                    {
                        Field = "WorkgroupSubscriptions[WorkgroupId]",
                        Operator = FilterOperators.ContainedIn,
                        Value = new List<Guid>() { workgroup1Id }
                    }
                ]
            });
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<BatchDto<NotificationTriggerDto>>();
            result.Items.Count().Should().Be(adminUser1Entities.Count(x => x.WorkgroupSubscriptions.Any(y => y.WorkgroupId == workgroup1Id)));
        }
    }

    [Fact]
    public async Task GetNotificationsTriggers_FilterByAdmin_ReturnResponse()
    {
        var nonadminUser1Id = Guid.NewGuid();
        var nonadminUser2Id = Guid.NewGuid();
        var adminUser1Id = Guid.NewGuid();
        var adminUser2Id = Guid.NewGuid();
        var workgroup1Id = Guid.NewGuid();
        var workgroup2Id = Guid.NewGuid();

        var nonadminUser1Entities = Fixture.Build<NotificationTriggerEntity>()
            .With(x => x.Type, NotificationType.Personal)
            .With(x => x.CreatedBy, nonadminUser1Id)
            .CreateMany(5);

        var nonadminUser2Entities = Fixture.Build<NotificationTriggerEntity>()
            .With(x => x.Type, NotificationType.Personal)
            .With(x => x.CreatedBy, nonadminUser2Id)
            .CreateMany(5);

        var adminUser1Entities = Fixture.Build<NotificationTriggerEntity>()
            //.With(x => x.Type, NotificationType.Workgroup)
            .With(x => x.CreatedBy, adminUser1Id)
            .CreateMany(10);

        var adminUser2Entities = Fixture.Build<NotificationTriggerEntity>()
            //.With(x => x.Type, NotificationType.Workgroup)
            .With(x => x.CreatedBy, adminUser2Id)
            .CreateMany(10);

        foreach (var entity in adminUser1Entities.Where(x => x.Type == NotificationType.Workgroup))
        {
            entity.WorkgroupSubscriptions = [
                new WorkgroupSubscriptionEntity()
                {
                    NotificationTriggerId = entity.Id,
                    WorkgroupId = workgroup1Id
                }
            ];
        }

        foreach (var entity in adminUser2Entities.Where(x => x.Type == NotificationType.Workgroup))
        {
            entity.WorkgroupSubscriptions = [
                new WorkgroupSubscriptionEntity()
                {
                    NotificationTriggerId = entity.Id,
                    WorkgroupId = workgroup2Id
                }
            ];
        }

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddRangeAsync(nonadminUser1Entities);
            await db.NotificationTriggers.AddRangeAsync(nonadminUser2Entities);
            await db.NotificationTriggers.AddRangeAsync(adminUser1Entities);
            await db.NotificationTriggers.AddRangeAsync(adminUser2Entities);
            await db.SaveChangesAsync();

            var response = await client.PostAsJsonAsync($"notifications/triggers/all", new BatchRequestDto()
            {
                FilterSpecifications =
                [
                    new FilterSpecificationDto()
                    {
                        Field = "WorkgroupSubscriptions[WorkgroupId],CreatedBy",
                        Operator = FilterOperators.ContainedIn,
                        Value = new List<Guid>() { workgroup1Id, workgroup2Id, adminUser1Id }
                    }
                ]
            });
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<BatchDto<NotificationTriggerDto>>();
            result.Items.Count().Should().Be(adminUser1Entities.Count() + adminUser2Entities.Count(x => x.Type == NotificationType.Workgroup));
        }
    }

    [Fact]
    public async Task GetNotificationsTriggers_IsEnabledForUser_ReturnResponse()
    {
        var nonadminUser1Id = Guid.NewGuid();
        var adminUser1Id = Guid.NewGuid();
        var workgroup1Id = Guid.NewGuid();

        var nonadminUser1Entities = Fixture.Build<NotificationTriggerEntity>()
            .With(x => x.Type, NotificationType.Personal)
            .With(x => x.CreatedBy, nonadminUser1Id)
            .CreateMany(5);

        var adminUser1Entities = Fixture.Build<NotificationTriggerEntity>()
            .With(x => x.CreatedBy, adminUser1Id)
            .CreateMany(10);

        foreach (var entity in adminUser1Entities.Where(x => x.Type == NotificationType.Workgroup))
        {
            entity.WorkgroupSubscriptions = [
                new WorkgroupSubscriptionEntity()
                {
                    NotificationTriggerId = entity.Id,
                    WorkgroupId = workgroup1Id
                }
            ];

            entity.NotificationSubscriptionOverrides = [
                new NotificationSubscriptionOverrideEntity()
                {
                    NotificationTriggerId = entity.Id,
                    UserId = nonadminUser1Id,
                    IsEnabled = false
                }
            ];
        }

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddRangeAsync(nonadminUser1Entities);
            await db.NotificationTriggers.AddRangeAsync(adminUser1Entities);
            await db.SaveChangesAsync();


            var response = await client.PostAsJsonAsync($"notifications/triggers/all", new BatchRequestDto()
            {
                FilterSpecifications =
                [
                    new FilterSpecificationDto()
                    {
                        Field = "WorkgroupSubscriptions[WorkgroupId],CreatedBy",
                        Operator = FilterOperators.ContainedIn,
                        Value = new List<Guid>() { workgroup1Id, nonadminUser1Id }
                    }
                ]
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<BatchDto<NotificationTriggerDto>>();

            result.Items.Count(x => (x.Type == NotificationType.Personal) && !x.IsEnabledForUser.HasValue).Should().Be(nonadminUser1Entities.Count());

            result.Items.Count(x => (x.Type == NotificationType.Workgroup) && (x.IsEnabledForUser == false)).Should()
                .Be(adminUser1Entities.Count(x => (x.Type == NotificationType.Workgroup) && x.NotificationSubscriptionOverrides.Any(x => x.UserId == nonadminUser1Id && !x.IsEnabled)));
        }
    }
}
