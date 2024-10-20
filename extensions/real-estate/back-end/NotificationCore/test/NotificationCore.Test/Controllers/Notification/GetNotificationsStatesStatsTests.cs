using AutoFixture;
using FluentAssertions;
using NotificationCore.Entities;
using NotificationCore.Models;
using NotificationCore.Test.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Willow.Batch;
using Xunit;
using Xunit.Abstractions;

namespace NotificationCore.Test.Controllers.Notification;

public class GetNotificationsStatesStatsTests : BaseInMemoryTest
{
    public GetNotificationsStatesStatsTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetNotificationsStatesStatsTests_ValidRequest()
    {
        var userId = Guid.NewGuid();

        var notificationEntities = Fixture.Build<NotificationEntity>()
            .With(x => x.PropertyBagJson, "{ \"entityId\":\"9650fa72-4cdd-43eb-bf5d-02f119605518\", \"source\":\"insight\",\"name\":\"Executive Conference Room 06-103\",\"twinId\":\"WIL-104BDFD-RM-06-103\",\"twinCategoryId\":\"dtmi:com:willowinc:Room;1\",\"skillCategoryId\":1,\"skillId\":\"sin-and-cos\",\"priority\":4,\"locations\":[\"WIL-104BDFD\"]}")
            .CreateMany(10);

        var notificationUsers = notificationEntities.Take(5).Select(x => Fixture.Build<NotificationUserEntity>()
            .With(c => c.UserId, userId)
            .With(x => x.NotificationId, x.Id)
            .With(x => x.State, NotificationUserState.New)
            .Create()).ToList();


        notificationUsers.AddRange(notificationEntities.Skip(5).Select(x => Fixture.Build<NotificationUserEntity>()
            .With(c => c.UserId, userId)
            .With(x => x.NotificationId, x.Id)
            .With(x => x.State, NotificationUserState.Cleared)
            .Create()));

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationsUsers.AddRangeAsync(notificationUsers);
            await db.SaveChangesAsync();

            var response = await client.PostAsJsonAsync($"notifications/states/stats", new List<FilterSpecificationDto>()
            {
                {
                    new FilterSpecificationDto()
                    {
                        Field = nameof(NotificationUser.UserId),
                        Operator = FilterOperators.EqualsLiteral,
                        Value = userId
                    }
                }
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<List<NotificationStatesStats>>();
            result.FirstOrDefault(x => x.State == NotificationUserState.New)?.Count.Should().Be(notificationUsers.Count(x => x.State == NotificationUserState.New));
        }
    }

    [Fact]
    public async Task GetNotificationsTests_FilterByDate_ValidRequest()
    {
        var userId = Guid.NewGuid();

        var notificationEntities = Fixture.Build<NotificationEntity>()
            .With(x => x.PropertyBagJson, "{ \"entityId\":\"9650fa72-4cdd-43eb-bf5d-02f119605518\", \"source\":\"insight\",\"name\":\"Executive Conference Room 06-103\",\"twinId\":\"WIL-104BDFD-RM-06-103\",\"twinCategoryId\":\"dtmi:com:willowinc:Room;1\",\"skillCategoryId\":1,\"skillId\":\"sin-and-cos\",\"priority\":4,\"locations\":[\"WIL-104BDFD\"]}")
            .CreateMany(10);

        var notificationUsers = notificationEntities.Take(5).Select(x => Fixture.Build<NotificationUserEntity>()
            .With(c => c.UserId, userId)
            .With(x => x.NotificationId, x.Id)
            .With(x => x.State, NotificationUserState.New)
            .With(x => x.ClearedDateTime, DateTime.UtcNow)
            .Create()).ToList();

        notificationUsers.AddRange(notificationEntities.Skip(5).Select(x => Fixture.Build<NotificationUserEntity>()
            .With(c => c.UserId, userId)
            .With(x => x.NotificationId, x.Id)
            .With(x => x.State, NotificationUserState.New)
            .With(x => x.ClearedDateTime, DateTime.UtcNow.AddDays(-10))
            .Create()));

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationsUsers.AddRangeAsync(notificationUsers);
            await db.SaveChangesAsync();

            var response = await client.PostAsJsonAsync($"notifications/states/stats", new List<FilterSpecificationDto>()
            {
                new FilterSpecificationDto()
                {
                    Field = nameof(NotificationUser.UserId),
                    Operator = FilterOperators.EqualsLiteral,
                    Value = userId
                },
                new FilterSpecificationDto()
                {
                    Field = nameof(NotificationUser.ClearedDateTime),
                    Operator = FilterOperators.GreaterThan,
                    Value = DateTime.UtcNow.AddDays(-5)
                }
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<List<NotificationStatesStats>>();
            result.FirstOrDefault(x => x.State == NotificationUserState.New)?.Count.Should().Be(5);
        }
    }
}
