using AutoFixture;
using FluentAssertions;
using NotificationCore.Entities;
using NotificationCore.Models;
using NotificationCore.Test.Infrastructure;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace NotificationCore.Test.Controllers.NotificationTrigger;

public class DeleteNotificationTriggerTests : BaseInMemoryTest
{
    public DeleteNotificationTriggerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task DeleteNotificationTrigger_InvalidId_ReturnNotFound()
    {
        var userId = Guid.NewGuid();
        var triggerId = Guid.NewGuid();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var response = await client.DeleteAsync($"notifications/Triggers/{triggerId}?userId={userId}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        }
    }
    [Theory]
    [InlineData(NotificationFocus.Twin)]
    [InlineData(NotificationFocus.TwinCategory)]
    [InlineData(NotificationFocus.SkillCategory)]
    [InlineData(NotificationFocus.Skill)]
    public async Task DeleteNotificationTrigger_Response(NotificationFocus focus)
    {
        var triggerId = Guid.NewGuid();
        var entity = Fixture.Build<NotificationTriggerEntity>()
            .With(c => c.Locations, new List<LocationEntity>()
            {
                new LocationEntity()
                {
                    Id = "loc1",
                    NotificationTriggerId = triggerId
                }
            })
            .With(c => c.WorkgroupSubscriptions, new List<WorkgroupSubscriptionEntity>()
            {
                new WorkgroupSubscriptionEntity()
                {
                    WorkgroupId = Guid.NewGuid(),
                    NotificationTriggerId = triggerId
                }
            })
            .With(c => c.Focus, focus)
            .With(c => c.TwinCategories, focus == NotificationFocus.TwinCategory ? new List<NotificationTriggerTwinCategoryEntity>()
            {
                new NotificationTriggerTwinCategoryEntity()
                {
                    CategoryId = "123",
                    NotificationTriggerId = triggerId
                }
            } : null)
            .With(c => c.SkillCategories, focus == NotificationFocus.SkillCategory ? new List<NotificationTriggerSkillCategoryEntity>()
            {
                new NotificationTriggerSkillCategoryEntity()
                {
                    CategoryId = 1,
                    NotificationTriggerId = triggerId
                }
            } : null)
            .With(c => c.Twins, focus == NotificationFocus.Twin ? new List<NotificationTriggerTwinEntity>()
            {
                new NotificationTriggerTwinEntity()
                {
                    TwinId = "123",
                    NotificationTriggerId = triggerId
                }
            } : null)
            .With(c => c.Skills, focus == NotificationFocus.Skill ? new List<NotificationTriggerSkillEntity>()
            {
                new NotificationTriggerSkillEntity()
                {
                    SkillId = "123",
                    NotificationTriggerId = triggerId
                }
            } : null)
            .With(c => c.Id, triggerId)
            .With(c => c.Type, NotificationType.Personal)
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

            var response = await client.DeleteAsync($"notifications/Triggers/{triggerId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        }

    }

}
