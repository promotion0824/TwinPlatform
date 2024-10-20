using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using WorkflowCore.Services.MappedIntegration.Dtos;
using WorkflowCore.Services.MappedIntegration.Dtos.Responses;
using WorkflowCore.Services.MappedIntegration.Models;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.MappedIntegration;

public class GetTicketTests : BaseInMemoryTest
{
    public GetTicketTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task TokenIsNotGiven_GetTicket_RequiresAuthorization()
    {
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient())
        {
            var result = await client.GetAsync($"api/mapped/sites/{Guid.NewGuid}/tickets/{Guid.NewGuid}");
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task TicketNotExists_GetTicket_ReturnNotFound()
    {
        var siteId = Guid.NewGuid();



        var existingTickets = Fixture.Build<TicketEntity>()
                                      .With(x => x.SiteId, siteId)
                                      .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                      .With(x => x.Priority, 4)
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Comments)
                                      .Without(x => x.Category)
                                      .Without(x => x.Tasks)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .With(x => x.TemplateId, (Guid?)null)
                                      .CreateMany(5)
                                      .ToList();

        var existingTicketsFromOtherSite = Fixture.Build<TicketEntity>()
                                          .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                          .With(x => x.Priority, 4)
                                          .Without(x => x.Attachments)
                                          .Without(x => x.Comments)
                                          .Without(x => x.Category)
                                          .Without(x => x.Tasks)
                                          .Without(x => x.JobType)
                                          .Without(x => x.Diagnostics)
                                          .With(x => x.TemplateId, (Guid?)null)
                                          .CreateMany(5)
                                          .ToList();

        existingTickets.AddRange(existingTicketsFromOtherSite);
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.Tickets.AddRange(existingTickets);
            db.SaveChanges();
            var result = await client.GetAsync($"api/mapped/sites/{siteId}/tickets/{Guid.NewGuid()}");
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var response = await result.Content.ReadAsStringAsync();
            response.Should().Contain("Ticket not found");

        }
    }

    [Fact]
    public async Task TicketExists_GetTicket_ReturnThisTicket()
    {
        var siteId = Guid.NewGuid();

        var existingTicket = Fixture.Build<TicketEntity>()
                                      .With(x => x.SiteId, siteId)
                                      .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                      .With(x => x.Priority, 4)
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Comments)
                                      .Without(x => x.Category)
                                      .Without(x => x.Tasks)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .With(x => x.TemplateId, (Guid?)null)
                                      .Create();

        var existingTickets = Fixture.Build<TicketEntity>()
                                      .With(x => x.SiteId, siteId)
                                      .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                      .With(x => x.Priority, 4)
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Comments)
                                      .Without(x => x.Category)
                                      .Without(x => x.Tasks)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .With(x => x.TemplateId, (Guid?)null)
                                      .CreateMany(5)
                                      .ToList();

        var existingTicketsFromOtherSite = Fixture.Build<TicketEntity>()
                                          .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                          .With(x => x.Priority, 4)
                                          .Without(x => x.Attachments)
                                          .Without(x => x.Comments)
                                          .Without(x => x.Category)
                                          .Without(x => x.Tasks)
                                          .Without(x => x.JobType)
                                          .Without(x => x.Diagnostics)
                                          .With(x => x.TemplateId, (Guid?)null)
                                          .CreateMany(5)
                                          .ToList();

        var userProfiles = Fixture.CreateMany<UserProfile>(3).ToList();
        userProfiles[0].Id = existingTicket.CreatorId;


        existingTickets.AddRange(existingTicketsFromOtherSite);
        existingTickets.Add(existingTicket);

        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                  .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                  .ReturnsJson(userProfiles);
            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.Tickets.AddRange(existingTickets);
            db.SaveChanges();
            var result = await client.GetAsync($"api/mapped/sites/{siteId}/tickets/{existingTicket.Id}");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            var response = await result.Content.ReadFromJsonAsync<GetTicketResponse>();
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeTrue();
            MappedTicketDto.MapFrom(existingTicket).Should()
                                                    .BeEquivalentTo(response.Data,
                                                        config=>config.Excluding(x=>x.Creator));
            var mappedUserProfile = MappedUserProfile.MapFromUserProfile(userProfiles.Where(x => x.Id == existingTicket.CreatorId).First());
            response.Data.Creator.Should().BeEquivalentTo(mappedUserProfile);


        }
    }

    [Fact]
    public async Task TicketExists_GetTicket_ReturnThisTicketAndSyncIdentities()
    {
        var siteId = Guid.NewGuid();

        var existingTicket = Fixture.Build<TicketEntity>()
                                      .With(x => x.SiteId, siteId)
                                      .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                      .With(x => x.Priority, 4)
                                      .With(x => x.AssigneeType, AssigneeType.CustomerUser)
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Comments)
                                      .Without(x => x.Category)
                                      .Without(x => x.Tasks)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .With(x => x.TemplateId, (Guid?)null)
                                      .Create();

        var existingTickets = Fixture.Build<TicketEntity>()
                                      .With(x => x.SiteId, siteId)
                                      .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                      .With(x => x.Priority, 4)
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Comments)
                                      .Without(x => x.Category)
                                      .Without(x => x.Tasks)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .With(x => x.TemplateId, (Guid?)null)
                                      .CreateMany(5)
                                      .ToList();

        var existingTicketsFromOtherSite = Fixture.Build<TicketEntity>()
                                          .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                          .With(x => x.Priority, 4)
                                          .Without(x => x.Attachments)
                                          .Without(x => x.Comments)
                                          .Without(x => x.Category)
                                          .Without(x => x.Tasks)
                                          .Without(x => x.JobType)
                                          .Without(x => x.Diagnostics)
                                          .With(x => x.TemplateId, (Guid?)null)
                                          .CreateMany(5)
                                          .ToList();



        var responseUserProfile = Fixture.Build<UserProfile>()
                                        .With(x => x.Id, existingTicket.CreatorId)
                                        .Create();

        var creator = MappedUserProfile.MapFromUserProfile(responseUserProfile);
        var assignee = Fixture.Build<MappedAssignee>()
                              .With(x => x.Id, existingTicket.AssigneeId)
                              .Create();


        existingTickets.AddRange(existingTicketsFromOtherSite);
        existingTickets.Add(existingTicket);
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                  .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                  .ReturnsJson(new List<UserProfile> { responseUserProfile });

            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.Tickets.AddRange(existingTickets);
            db.ExternalProfiles.Add(new ExternalProfileEntity
            {
                Id = existingTicket.AssigneeId.Value,
                Email = assignee.Email,
                Name = assignee.Name
            });
            db.SaveChanges();


            var result = await client.GetAsync($"api/mapped/sites/{siteId}/tickets/{existingTicket.Id}");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            var response = await result.Content.ReadFromJsonAsync<GetTicketResponse>();
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeTrue();
            MappedTicketDto.MapFrom(existingTicket).Should().BeEquivalentTo(response.Data, config =>
            {
                config.Excluding(x => x.Creator);
                config.Excluding(x => x.Assignee);
                return config;
            });
            response.Data.Creator.Should().BeEquivalentTo(creator);
            response.Data.Assignee.Should().BeEquivalentTo(assignee);


        }
    }

    [Fact]
    public async Task ClosedTicketExists_GetTicket_ReturnThisTicketAndSyncIdentities()
    {
        var siteId = Guid.NewGuid();

        var existingTicket = Fixture.Build<TicketEntity>()
                                      .With(x => x.SiteId, siteId)
                                      .With(x => x.Status, (int)TicketStatusEnum.ClosedCompleted)
                                      .With(x => x.Priority, 4)
                                      .With(x => x.AssigneeType, AssigneeType.CustomerUser)
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Comments)
                                      .Without(x => x.Category)
                                      .Without(x => x.Tasks)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .With(x => x.TemplateId, (Guid?)null)
                                      .Create();

        var existingTickets = Fixture.Build<TicketEntity>()
                                      .With(x => x.SiteId, siteId)
                                      .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                      .With(x => x.Priority, 4)
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Comments)
                                      .Without(x => x.Category)
                                      .Without(x => x.Tasks)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .With(x => x.TemplateId, (Guid?)null)
                                      .CreateMany(5)
                                      .ToList();

        var existingTicketsFromOtherSite = Fixture.Build<TicketEntity>()
                                          .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                          .With(x => x.Priority, 4)
                                          .Without(x => x.Attachments)
                                          .Without(x => x.Comments)
                                          .Without(x => x.Category)
                                          .Without(x => x.Tasks)
                                          .Without(x => x.JobType)
                                          .Without(x => x.Diagnostics)
                                          .With(x => x.TemplateId, (Guid?)null)
                                          .CreateMany(5)
                                          .ToList();

        var existingAuditTrail = Fixture.Build<AuditTrailEntity>()
                                        .With(x => x.SourceId, Guid.NewGuid())
                                        .With(x => x.ColumnName, nameof(TicketEntity.Status))
                                        .With(x => x.TableName, nameof(TicketEntity))
                                        .With(x => x.NewValue, ((int)TicketStatusEnum.ClosedCompleted).ToString())
                                        .With(x => x.RecordID, existingTicket.Id)
                                        .With(x=> x.Timestamp, DateTime.UtcNow.AddDays(3))
                                        .Create();


        var creatorUserProfile = Fixture.Build<UserProfile>()
                                        .With(x => x.Id, existingTicket.CreatorId)
                                        .Create();

        var closedByUserProfile = Fixture.Build<UserProfile>()
                                        .With(x => x.Id, existingAuditTrail.SourceId)
                                        .Create();

        var creator = MappedUserProfile.MapFromUserProfile(creatorUserProfile);
        var closedBy = MappedUserProfile.MapFromUserProfile(closedByUserProfile);

        var assignee = Fixture.Build<MappedAssignee>()
                              .With(x => x.Id, existingTicket.AssigneeId)
                              .Create();

        var existingStatus = Fixture.Build<TicketStatusEntity>()
                                    .With(x => x.Status, TicketStatusEnum.ClosedCompleted.ToString())
                                    .With(x => x.StatusCode, (int)TicketStatusEnum.ClosedCompleted)
                                    .With(x => x.Tab, TicketTabs.CLOSED)
                                    .Create();


        existingTickets.AddRange(existingTicketsFromOtherSite);
        existingTickets.Add(existingTicket);
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                  .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                  .ReturnsJson(new List<UserProfile> { creatorUserProfile, closedByUserProfile });

            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.Tickets.AddRange(existingTickets);
            db.TicketStatuses.Add(existingStatus);
            db.AuditTrails.Add(existingAuditTrail);
            db.ExternalProfiles.Add(new ExternalProfileEntity
            {
                Id = existingTicket.AssigneeId.Value,
                Email = assignee.Email,
                Name = assignee.Name
            });

            db.SaveChanges();
  

            var result = await client.GetAsync($"api/mapped/sites/{siteId}/tickets/{existingTicket.Id}");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            var response = await result.Content.ReadFromJsonAsync<GetTicketResponse>();
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeTrue();
            MappedTicketDto.MapFrom(existingTicket).Should().BeEquivalentTo(response.Data, config =>
            {
                config.Excluding(x => x.Creator);
                config.Excluding(x => x.Assignee);
                config.Excluding(x => x.ClosedBy);
                return config;
            });
            response.Data.Creator.Should().BeEquivalentTo(creator);
            response.Data.Assignee.Should().BeEquivalentTo(assignee);
            response.Data.ClosedBy.Should().BeEquivalentTo(closedBy);
        }
    }

}

