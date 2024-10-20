using AutoFixture;
using FluentAssertions;
using NotificationCore.Controllers.Requests;
using NotificationCore.Dto;
using NotificationCore.Entities;
using NotificationCore.Models;
using NotificationCore.Test.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace NotificationCore.Test.Controllers.NotificationTrigger;

public class CreateNotificationTriggerTests: BaseInMemoryTest
{
    public CreateNotificationTriggerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Theory]
    [InlineData(NotificationFocus.Twin)]
    [InlineData(NotificationFocus.SkillCategory)]
    [InlineData(NotificationFocus.Skill)]
    [InlineData(NotificationFocus.TwinCategory)]
    public async Task CreateWorkGroupNotification_ValidRequest_ReturnResponse(NotificationFocus focus)
    {
        var userId = Guid.NewGuid();
        var location = new List<string>() { Guid.NewGuid().ToString() };
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, location)
            .With(c => c.Type, NotificationType.Workgroup)
            .With(c => c.WorkGroupIds, workgroup)
            .With(c=>c.Focus,focus)
            .With(c=>c.Channels,new List<NotificationChannel> { NotificationChannel.InApp })
            .With(c => c.SkillCategoryIds, focus == NotificationFocus.SkillCategory ? new List<int> { 1 } : null)
            .With(c => c.Twins, focus == NotificationFocus.Twin ? new List<NotificationTriggerTwinDto> { new NotificationTriggerTwinDto{TwinName = "twin",TwinId = "twin"} } : null)
            .With(c => c.SkillIds, focus == NotificationFocus.Skill ? new List<string> { "skill" } : null)
            .With(c => c.TwinCategoryIds, focus == NotificationFocus.TwinCategory ? new List<string> { "twinCategory" } : null)
            .With(c=>c.PriorityIds, new List<int>{1,2,3,4})
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var response = await client.PostAsJsonAsync($"notifications/Triggers", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            var createdNotification =NotificationTriggerEntity.MapTo(db.NotificationTriggers.Include(c=>c.Locations).First(c => c.Id == result.Id));
            result.Type.Should().Be( createdNotification.Type);
            result.Source.Should().Be(createdNotification.Source);
            result.Focus.Should().Be(createdNotification.Focus);
            result.Channels.Count().Should().Be(1);
            result.Channels.First().Should().Be(NotificationChannel.InApp);
            result.Locations.Should().BeEquivalentTo(createdNotification.Locations);
            result.Twins.Should().BeEquivalentTo(focus == NotificationFocus.Twin ? new List<NotificationTriggerTwinDto> { new NotificationTriggerTwinDto { TwinName = "twin", TwinId = "twin" } } : null);
            result.SkillIds.Should().BeEquivalentTo(focus == NotificationFocus.Skill ? new List<string> { "skill" } : null);
            result.SkillCategoryIds.Should().BeEquivalentTo(focus == NotificationFocus.SkillCategory ? new List<int> { 1 } : null);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateNotification_InValidRequest_LocationIsNullOrEmpty_ReturnResponse(bool locationIsEmpty)
    {
        var userId = Guid.NewGuid();
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, locationIsEmpty?new List<string>():null)
            .With(c => c.Type, NotificationType.Workgroup)
            .With(c => c.WorkGroupIds, workgroup)
            .With(c => c.Channels, new List<NotificationChannel> { NotificationChannel.InApp })
            .With(c => c.Focus, NotificationFocus.SkillCategory)
            .With(c => c.SkillCategoryIds,  new List<int> { 1 } )
            .Without(c => c.PriorityIds)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var response = await client.PostAsJsonAsync($"notifications/Triggers", request);
            var result = await response.Content.ReadAsAsync<NotificationTriggerDto>();
            var createdNotification = NotificationTriggerEntity.MapTo(db.NotificationTriggers.Include(c => c.Locations).First(c => c.Id == result.Id));
            createdNotification.Locations.Should().BeNullOrEmpty();

        }
    }

    [Fact]
    public async Task CreateNotification_InValidRequest_WorkGroupIsNullOrEmpty_ReturnBadRequest()
    {
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, new List<string>() { "location"})
            .With(c => c.Type, NotificationType.Workgroup)
            .Without(c => c.WorkGroupIds)
            .With(c => c.Channels, new List<NotificationChannel> { NotificationChannel.InApp })
            .With(c => c.Focus, NotificationFocus.SkillCategory)
            .With(c => c.SkillCategoryIds, new List<int> { 1 })
            .Without(c => c.PriorityIds)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var response = await client.PostAsJsonAsync($"notifications/Triggers", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateNotification_InValidRequest_SkillCategoryIsNullOrEmpty_ReturnBadRequest(bool categoryIsEmpty)
    {
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, new List<string>() { "location" })
            .With(c => c.Type, NotificationType.Workgroup)
            .With(c => c.WorkGroupIds, workgroup)
            .With(c => c.Channels, new List<NotificationChannel> { NotificationChannel.InApp })
            .With(c => c.Focus, NotificationFocus.SkillCategory)
            .With(c => c.SkillCategoryIds, categoryIsEmpty? new List<int> ():null)
            .Without(c => c.PriorityIds)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var response = await client.PostAsJsonAsync($"notifications/Triggers", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateNotification_ValidRequest_TwinCategoryIsNullOrEmpty_ReturnBadRequest(bool categoryIsEmpty)
    {
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, new List<string>() { "location" })
            .With(c => c.Type, NotificationType.Workgroup)
            .With(c => c.WorkGroupIds, workgroup)
            .With(c => c.Channels, new List<NotificationChannel> { NotificationChannel.InApp })
            .With(c => c.Focus, NotificationFocus.TwinCategory)
            .With(c => c.TwinCategoryIds, categoryIsEmpty ? new List<string>() : null)
            .Without(c => c.PriorityIds)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var response = await client.PostAsJsonAsync($"notifications/Triggers", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateNotification_InValidRequest_SkillIsNullOrEmpty_ReturnBadRequest(bool skillIsEmpty)
    {
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, new List<string>() { "location" })
            .With(c => c.Type, NotificationType.Workgroup)
            .With(c => c.WorkGroupIds, workgroup)
            .With(c => c.Channels, new List<NotificationChannel> { NotificationChannel.InApp })
            .With(c => c.Focus, NotificationFocus.Skill)
            .With(c => c.SkillIds, skillIsEmpty ? new List<string>() : null)
            .Without(c => c.PriorityIds)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var response = await client.PostAsJsonAsync($"notifications/Triggers", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateNotification_InValidRequest_TwinIdIsNullOrEmpty_ReturnBadRequest(bool twinIdIsEmpty)
    {
        var workgroup = new List<Guid>() { Guid.NewGuid() };
        var request = Fixture.Build<CreateNotificationTriggerRequest>()
            .With(c => c.Locations, new List<string>() { "location" })
            .With(c => c.Type, NotificationType.Workgroup)
            .With(c => c.WorkGroupIds, workgroup)
            .With(c => c.Channels, new List<NotificationChannel> { NotificationChannel.InApp })
            .With(c => c.Focus, NotificationFocus.Twin)
            .With(c => c.Twins, twinIdIsEmpty ? new List<NotificationTriggerTwinDto>() : null)
            .Without(c => c.PriorityIds)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var utcNow = DateTime.UtcNow;
            var serverArrangement = server.Arrange();
            serverArrangement.SetCurrentDateTime(utcNow);

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var response = await client.PostAsJsonAsync($"notifications/Triggers", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }
    }
}
