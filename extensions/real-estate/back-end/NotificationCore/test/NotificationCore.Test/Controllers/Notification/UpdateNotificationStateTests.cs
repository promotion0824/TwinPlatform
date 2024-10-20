using AutoFixture;
using FluentAssertions;
using NotificationCore.Controllers.Requests;
using NotificationCore.Dto;
using NotificationCore.Entities;
using NotificationCore.Models;
using NotificationCore.Test.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Willow.Batch;
using Xunit;
using Xunit.Abstractions;

namespace NotificationCore.Test.Controllers.Notification;

public class UpdateNotificationStateTests : BaseInMemoryTest
{
    public UpdateNotificationStateTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UpdateNotificationState_ValidRequest()
    {
        var userId = Guid.NewGuid();
        var state = NotificationUserState.Cleared;

        var notificationUsers = Fixture.Build<NotificationUserEntity>()
            .With(c => c.UserId, userId)
            .CreateMany(10);

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationsUsers.AddRangeAsync(notificationUsers);
            await db.SaveChangesAsync();

            var notificationsIds = notificationUsers.Take(4).Select(x => x.NotificationId).ToList();

            var response = await client.PutAsJsonAsync($"notifications/state", new UpdateNotificationStateRequest()
            {
                UserId = userId,
                NotificationIds = notificationsIds,
                State = state
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<NotificationStatesStats>();
            result.Count.Should().Be(4);
            result.State.Should().Be(state);

            var updatedNotificaitons = db.NotificationsUsers.Where(x => x.UserId == userId && notificationsIds.Contains(x.NotificationId)).ToList();
            Assert.True(updatedNotificaitons.All(x => x.State == state));
        }
    }

    [Fact]
    public async Task UpdateNotificationState_UserHasNoNotifications()
    {
        var userId = Guid.NewGuid();
        var state = NotificationUserState.Cleared;

        var notificationUsers = Fixture.Build<NotificationUserEntity>()
            .With(c => c.UserId, userId)
            .CreateMany(10);

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationsUsers.AddRangeAsync(notificationUsers);
            await db.SaveChangesAsync();

            var notificationsIds = notificationUsers.Take(4).Select(x => x.NotificationId).ToList();

            var response = await client.PutAsJsonAsync($"notifications/state", new UpdateNotificationStateRequest()
            {
                UserId = Guid.NewGuid(),
                NotificationIds = notificationsIds,
                State = state
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<NotificationStatesStats>();
            result.Count.Should().Be(0);
            result.State.Should().Be(state);

            var updatedNotificaitonUsers = db.NotificationsUsers.Where(x => x.UserId == userId && notificationsIds.Contains(x.NotificationId)).ToList();
            foreach(var notification in updatedNotificaitonUsers)
            {
                Assert.True(notification.State == notificationUsers.FirstOrDefault(x => x.NotificationId == notification.NotificationId)?.State);
            }
        }
    }

    [Fact]
    public async Task UpdateNotificationState_SetCleratedDateTime()
    {
        var userId = Guid.NewGuid();
        var state = NotificationUserState.Cleared;

        var notificationUsers = Fixture.Build<NotificationUserEntity>()
            .With(c => c.UserId, userId)
            .Without(c => c.ClearedDateTime)
            .CreateMany(10);

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationsUsers.AddRangeAsync(notificationUsers);
            await db.SaveChangesAsync();

            var notificationsIds = notificationUsers.Take(4).Select(x => x.NotificationId).ToList();

            var response = await client.PutAsJsonAsync($"notifications/state", new UpdateNotificationStateRequest()
            {
                UserId = userId,
                NotificationIds = notificationsIds,
                State = state
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<NotificationStatesStats>();
            result.Count.Should().Be(4);
            result.State.Should().Be(state);

            var updatedNotificaitons = db.NotificationsUsers.Where(x => x.UserId == userId && notificationsIds.Contains(x.NotificationId)).ToList();
            Assert.True(updatedNotificaitons.All(x => x.State == state));
            Assert.True(updatedNotificaitons.All(x => x.ClearedDateTime > utcNow.AddMinutes(-2)));
        }
    }
}
