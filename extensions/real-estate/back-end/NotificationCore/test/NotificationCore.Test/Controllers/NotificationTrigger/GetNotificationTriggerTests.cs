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
using Willow.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace NotificationCore.Test.Controllers.NotificationTrigger;

public class GetNotificationTriggerTests: BaseInMemoryTest
{
    public GetNotificationTriggerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetNotification_ValidRequest_ReturnResponse()
    {
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
            .With(c=>c.Id,triggerId)
            .With(c => c.ChannelJson, new List<NotificationChannel> { NotificationChannel.InApp })
            .With(c => c.PriorityJson, new List<int> { 1 })
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
            var response = await client.GetAsync($"notifications/Triggers/{entity.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            result.Id.Should().Be(triggerId);
            result.Locations.Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task GetNotification_InvalidId_ReturnResponse()
    {
        var entity = Fixture.Build<NotificationTriggerEntity>()
            .With(c=>c.Id,Guid.NewGuid)
            .With(c => c.ChannelJson, new List<NotificationChannel> { NotificationChannel.InApp })
            .With(c => c.PriorityJson, new List<int> { 1 })
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
            var response = await client.GetAsync($"notifications/Triggers/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

}
