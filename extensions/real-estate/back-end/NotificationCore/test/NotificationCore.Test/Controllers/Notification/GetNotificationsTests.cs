using AutoFixture;
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

namespace NotificationCore.Test.Controllers.Notification;

public class GetNotificationsTests : BaseInMemoryTest
{
    public GetNotificationsTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetNotificationsTests_ValidRequest()
    {
        var userId = Guid.NewGuid();

        var notificationEntity = Fixture.Build<NotificationEntity>()
            .With(x => x.PropertyBagJson, "{ \"entityId\":\"9650fa72-4cdd-43eb-bf5d-02f119605518\", \"source\":\"insight\",\"name\":\"Executive Conference Room 06-103\",\"twinId\":\"WIL-104BDFD-RM-06-103\",\"twinCategoryId\":\"dtmi:com:willowinc:Room;1\",\"skillCategoryId\":1,\"skillId\":\"sin-and-cos\",\"priority\":4,\"locations\":[\"WIL-104BDFD\"]}")
            .Create();

        var notificationUsers = Fixture.Build<NotificationUserEntity>()
            .With(c => c.UserId, userId)
            .With(x => x.Notification, notificationEntity)
            .With(x => x.NotificationId, notificationEntity.Id)
            .CreateMany(1);

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

            var response = await client.PostAsJsonAsync($"notifications/all", new BatchRequestDto()
            {
                FilterSpecifications =
                [
                    new FilterSpecificationDto()
                    {
                        Field = nameof(NotificationUser.UserId),
                        Operator = FilterOperators.EqualsLiteral,
                        Value = userId
                    }
                ]
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<BatchDto<NotificationUserDto>>();
            result.Items.Count().Should().Be(1);

            result.Items.Should().BeEquivalentTo(NotificationUserDto.MapFrom(NotificationUser.MapFrom(notificationUsers)));
        }
    }

    [Fact]
    public async Task GetNotificationsTests_FilterByUser_ValidRequest()
    {
        var userId = Guid.NewGuid();

        var notificationEntity = Fixture.Build<NotificationEntity>()
            .With(x => x.PropertyBagJson, "{ \"entityId\":\"9650fa72-4cdd-43eb-bf5d-02f119605518\", \"source\":\"insight\",\"name\":\"Executive Conference Room 06-103\",\"twinId\":\"WIL-104BDFD-RM-06-103\",\"twinCategoryId\":\"dtmi:com:willowinc:Room;1\",\"skillCategoryId\":1,\"skillId\":\"sin-and-cos\",\"priority\":4,\"locations\":[\"WIL-104BDFD\"]}")
            .Create();

        var notificationUsers = Fixture.Build<NotificationUserEntity>()
            .With(x => x.Notification, notificationEntity)
            .With(x => x.NotificationId, notificationEntity.Id)
            .CreateMany(1);

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

            var response = await client.PostAsJsonAsync($"notifications/all", new BatchRequestDto()
            {
                FilterSpecifications =
                [
                    new FilterSpecificationDto()
                    {
                        Field = nameof(NotificationUser.UserId),
                        Operator = FilterOperators.EqualsLiteral,
                        Value = userId
                    }
                ]
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<BatchDto<NotificationUserDto>>();
            result.Items.Count().Should().Be(0);
        }
    }

    [Fact]
    public async Task GetNotificationsTests_Sorted_ValidRequest()
    {
        var userId = Guid.NewGuid();

        var notificationEntity = Fixture.Build<NotificationEntity>()
            .With(x => x.PropertyBagJson, "{ \"entityId\":\"9650fa72-4cdd-43eb-bf5d-02f119605518\", \"source\":\"insight\",\"name\":\"Executive Conference Room 06-103\",\"twinId\":\"WIL-104BDFD-RM-06-103\",\"twinCategoryId\":\"dtmi:com:willowinc:Room;1\",\"skillCategoryId\":1,\"skillId\":\"sin-and-cos\",\"priority\":4,\"locations\":[\"WIL-104BDFD\"]}")
            .Create();

        var notificationUsers = Fixture.Build<NotificationUserEntity>()
            .With(c => c.UserId, userId)
            .With(x => x.NotificationId, notificationEntity.Id)
            .CreateMany(3);

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

            var response = await client.PostAsJsonAsync($"notifications/all", new BatchRequestDto()
            {
                FilterSpecifications =
                [
                    new FilterSpecificationDto()
                    {
                        Field = nameof(NotificationUserDto.UserId),
                        Operator = FilterOperators.EqualsLiteral,
                        Value = userId
                    }
                ],
                SortSpecifications =
                [
                    new SortSpecificationDto()
                    {
                         Field = "Notification.CreatedDateTime",
                         Sort = SortSpecificationDto.DESC
                    }
                ]
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<BatchDto<NotificationUserDto>>();
            result.Items.Count().Should().Be(3);

            result.Items.Should().BeEquivalentTo(NotificationUserDto.MapFrom(NotificationUser.MapFrom(notificationUsers.OrderByDescending(x => x.Notification.CreatedDateTime))));
        }
    }

    [Fact]
    public async Task GetNotificationsTests_FilterByState_ValidRequest()
    {
        var userId = Guid.NewGuid();

        var notificationUsers = Fixture.Build<NotificationUserEntity>()
            .With(x => x.UserId, userId)
            .CreateMany(10);

        foreach (var notificationUser in notificationUsers)
        {
            notificationUser.Notification = Fixture.Build<NotificationEntity>()
            .With(x => x.PropertyBagJson, "{ \"entityId\":\"9650fa72-4cdd-43eb-bf5d-02f119605518\", \"source\":\"insight\",\"name\":\"Executive Conference Room 06-103\",\"twinId\":\"WIL-104BDFD-RM-06-103\",\"twinCategoryId\":\"dtmi:com:willowinc:Room;1\",\"skillCategoryId\":1,\"skillId\":\"sin-and-cos\",\"priority\":4,\"locations\":[\"WIL-104BDFD\"]}")
            .Create();
        }

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationsUsers.AddRangeAsync(notificationUsers);
            await db.SaveChangesAsync();

            var response = await client.PostAsJsonAsync($"notifications/all", new BatchRequestDto()
            {
                FilterSpecifications =
                [
                    new FilterSpecificationDto()
                    {
                        Field = nameof(NotificationUser.UserId),
                        Operator = FilterOperators.EqualsLiteral,
                        Value = userId
                    },
                    new FilterSpecificationDto()
                    {
                        Field = nameof(NotificationUser.State),
                        Operator = FilterOperators.EqualsLiteral,
                        Value = NotificationUserState.New
                    }
                ]
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<BatchDto<NotificationUserDto>>();
            result.Items.Count().Should().Be(notificationUsers.Count(x => x.State == NotificationUserState.New));
        }
    }

    [Fact]
    public async Task GetNotificationsTests_FilterByTwinName_ValidRequest()
    {
        var userId = Guid.NewGuid();

        var notificationUsers = Fixture.Build<NotificationUserEntity>()
            .With(x => x.UserId, userId)
            .CreateMany(10);

        foreach (var notificationUser in notificationUsers)
        {
            notificationUser.Notification = Fixture.Build<NotificationEntity>()
            .With(x => x.PropertyBagJson, "{ \"entityId\":\"9650fa72-4cdd-43eb-bf5d-02f119605518\", \"source\":\"insight\",\"name\":\"Executive Conference Room 06-103\",\"twinId\":\"WIL-104BDFD-RM-06-103\",\"twinCategoryId\":\"dtmi:com:willowinc:Room;1\",\"skillCategoryId\":1,\"skillId\":\"sin-and-cos\",\"priority\":4,\"locations\":[\"WIL-104BDFD\"]}")
            .Create();
        }

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            await db.NotificationsUsers.AddRangeAsync(notificationUsers);
            await db.SaveChangesAsync();

            var response = await client.PostAsJsonAsync($"notifications/all", new BatchRequestDto()
            {
                FilterSpecifications =
                [
                    new FilterSpecificationDto()
                    {
                        Field = nameof(NotificationUser.UserId),
                        Operator = FilterOperators.EqualsLiteral,
                        Value = userId
                    },
                    new FilterSpecificationDto()
                    {
                        Field = nameof(NotificationUser.Notification.PropertyBagJson),
                        Operator = FilterOperators.Contains,
                        Value = "104BDFD"
                    }
                ]
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsAsync<BatchDto<NotificationUserDto>>();
            result.Items.Count().Should().Be(notificationUsers.Count(x => x.Notification.PropertyBagJson.Contains("104BDFD")));
        }
    }
}
