using AutoFixture;
using FluentAssertions;
using NotificationCore.Controllers.Requests;
using NotificationCore.Entities;
using NotificationCore.Models;
using NotificationCore.Test.Infrastructure;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace NotificationCore.Test.Controllers.NotificationTrigger;

public class BatchNotificationTriggerToggleTests : BaseInMemoryTest
{
    public BatchNotificationTriggerToggleTests(ITestOutputHelper output) : base(output)
    {
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BatchNotificationTriggerToggleTests_UserIsAdmin_ReturnResponse(bool currentMuted)
    {
        var request = new BatchNotificationTriggerToggleRequest
        {
            IsAdmin = true,
            Source = NotificationSource.Insight,
            UserId = Guid.NewGuid()
        };
        var notificationTriggers=Fixture.Build<NotificationTriggerEntity>().With(c => c.IsDefault, false).With(c=>c.IsMuted,currentMuted).CreateMany(10).ToList();
        notificationTriggers.AddRange(Fixture.Build<NotificationTriggerEntity>().With(x=>x.Type,NotificationType.Personal).With(c => c.IsDefault, false).With(c => c.IsMuted, currentMuted).With(x=>x.CreatedBy,request.UserId).CreateMany(3));
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null,userId:request.UserId))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddRangeAsync(notificationTriggers);
            await db.SaveChangesAsync();

            var response = await client.PostAsJsonAsync($"notifications/triggers/toggle",request);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var items = db.NotificationTriggers.Where(x =>
                !x.IsDefault && x.Source == request.Source && (x.Type == NotificationType.Workgroup ||
                (x.Type == NotificationType.Personal && x.CreatedBy == request.UserId))).All(x => x.IsMuted == !currentMuted).Should().BeTrue();

        }
    }


    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BatchNotificationTriggerToggleTests_UserIsNotAdmin_ReturnResponse(bool currentMuted)
    {
        var request = new BatchNotificationTriggerToggleRequest
        {
            IsAdmin = false,
            Source = NotificationSource.Insight,
            UserId = Guid.NewGuid(),
            WorkgroupIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]
        };
        var notificationTriggers = Fixture.Build<NotificationTriggerEntity>()
            .With(c=>c.IsDefault,false)
            .With(c => c.IsMuted, currentMuted)
            .CreateMany(10).ToList();

        var personalTriggers = Fixture.Build<NotificationTriggerEntity>()
            .With(x => x.Type, NotificationType.Personal)
            .With(c => c.IsDefault, false)
            .With(c => c.IsMuted, currentMuted)
            .With(x => x.CreatedBy, request.UserId)
            .CreateMany(3);
        notificationTriggers.AddRange(personalTriggers);

        var workgroupTriggers = Fixture.Build<NotificationTriggerEntity>()
            .With(x => x.Type, NotificationType.Workgroup)
            .With(c => c.IsDefault, false)
            .With(c => c.IsMuted, currentMuted)
            .With(x => x.CanUserDisableNotification, true)
            .Without(c=>c.NotificationSubscriptionOverrides)
            .CreateMany(3).ToList();
        workgroupTriggers.Add(Fixture.Build<NotificationTriggerEntity>()
            .With(x => x.Type, NotificationType.Workgroup)
            .With(c => c.IsDefault, false)
            .With(c => c.IsMuted, currentMuted)
            .With(x => x.CanUserDisableNotification, false)
            .Without(c => c.NotificationSubscriptionOverrides).Create());
        workgroupTriggers[0].NotificationSubscriptionOverrides = new List<NotificationSubscriptionOverrideEntity>()
        {
            new NotificationSubscriptionOverrideEntity()
            {
                UserId = request.UserId,
                IsEnabled = true,
                IsMuted = currentMuted,
                NotificationTriggerId = workgroupTriggers[0].Id
            }
        };

        foreach (var workgroupTrigger in workgroupTriggers)
        {
            workgroupTrigger.WorkgroupSubscriptions = new List<WorkgroupSubscriptionEntity>()
            {
                new WorkgroupSubscriptionEntity()
                {
                    WorkgroupId = request.WorkgroupIds[0],
                    NotificationTriggerId = workgroupTrigger.Id
                },
                new WorkgroupSubscriptionEntity()
                {
                    WorkgroupId = new Guid(),
                    NotificationTriggerId = workgroupTrigger.Id
                },
            };
        }
        notificationTriggers.AddRange(workgroupTriggers);

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null, userId: request.UserId))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationTriggers.AddRangeAsync(notificationTriggers);
            await db.SaveChangesAsync();

            var response = await client.PostAsJsonAsync($"notifications/triggers/toggle", request);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var personalIds = personalTriggers.Select(c => c.Id).ToList();
            var workgroupIds=workgroupTriggers.Select(c => c.Id).ToList();
           
            db.NotificationTriggers.Where(c => personalIds.Contains(c.Id)).All(c => c.IsMuted == !currentMuted).Should()
                .Be(true);
            db.NotificationTriggers.Where(c => !personalIds.Contains(c.Id)).All(c => c.IsMuted == currentMuted).Should()
                .Be(true);

           var overrideItems= db.NotificationSubscriptionOverrides.Where(c => workgroupIds.Contains(c.NotificationTriggerId)).ToList();
           overrideItems.Count.Should().Be(3);
        }
    }

}
