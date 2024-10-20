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

public class GetTicketsTests : BaseInMemoryTest
{
    public GetTicketsTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task TokenIsNotGiven_GetTickets_RequiresAuthorization()
    {
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient())
        {
            var result = await client.GetAsync($"api/mapped/sites/{Guid.NewGuid}/tickets");
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task TicketsExists_GetTickets_ReturnTheseTickets()
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
                                          .Without(x => x.Diagnostics)
                                          .With(x => x.TemplateId, (Guid?)null)
                                          .CreateMany(5)
                                          .ToList();

        existingTickets.AddRange(existingTicketsFromOtherSite);
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                  .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                  .ReturnsJson(Fixture.CreateMany<UserProfile>(3).ToList());
            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.Tickets.AddRange(existingTickets);
            db.SaveChanges();
            var result = await client.GetAsync($"api/mapped/sites/{siteId}/tickets");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            var response = await result.Content.ReadFromJsonAsync<GetTicketsResponse>();
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeTrue();
            response.Data.Count.Should().Be(5);

        }
    }

    [Fact]
    public async Task TicketsWithExternalIdExists_GetTickets_ReturnTickets()
    {
        var siteId = Guid.NewGuid();
        var externalId = Guid.NewGuid().ToString();

        var existingTicketWithExternalId = Fixture.Build<TicketEntity>()
                                      .With(x => x.SiteId, siteId)
                                      .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                      .With(x => x.ExternalId, externalId)
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
                                         .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                         .With(x => x.SiteId, siteId)
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
        existingTickets.Add(existingTicketWithExternalId);
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                  .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                  .ReturnsJson(Fixture.CreateMany<UserProfile>(3).ToList());

            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.Tickets.AddRange(existingTickets);
            db.SaveChanges();
            var result = await client.GetAsync($"api/mapped/sites/{siteId}/tickets?externalId={externalId}");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            var response = await result.Content.ReadFromJsonAsync<GetTicketsResponse>();
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeTrue();
            response.Data.Count.Should().Be(1);
            response.Data.First().ExternalId.Should().Be(externalId);
            MappedTicketDto.MapFrom(existingTicketWithExternalId).Should().BeEquivalentTo(response.Data.First());

        }
    }

    [Fact]
    public async Task TicketsFilteredByDateExists_GetTickets_ReturnTickets()
    {
        var siteId = Guid.NewGuid();
        var date = new DateTime(2023, 11, 1);

        var existingTicket1 = Fixture.Build<TicketEntity>()
                                      .With(x => x.SiteId, siteId)
                                      .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                      .With(x => x.CreatedDate, new DateTime(2023, 11, 2))
                                      .With(x => x.Priority, 4)
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Comments)
                                      .Without(x => x.Category)
                                      .Without(x => x.Tasks)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .With(x => x.TemplateId, (Guid?)null)
                                      .Create();

        var existingTicket2 = Fixture.Build<TicketEntity>()
                                    .With(x => x.SiteId, siteId)
                                    .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                    .With(x => x.CreatedDate, new DateTime(2023, 11, 5))
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
                                         .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                         .With(x => x.SiteId, siteId)
                                         .With(x => x.CreatedDate, new DateTime(2023, 10, 30))
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
        existingTickets.Add(existingTicket1);
        existingTickets.Add(existingTicket2);
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                  .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                  .ReturnsJson(Fixture.CreateMany<UserProfile>(3).ToList());
            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.Tickets.AddRange(existingTickets);
            db.SaveChanges();
            var result = await client.GetAsync($"api/mapped/sites/{siteId}/tickets?CreatedAfter={date.ToString("MM/dd/yyyy")}");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            var response = await result.Content.ReadFromJsonAsync<GetTicketsResponse>();
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeTrue();
            response.Data.Count.Should().Be(2);
            MappedTicketDto.MapFrom(existingTicket1).Id.Should().Be(response.Data.First(x => x.Id == existingTicket1.Id).Id);
            MappedTicketDto.MapFrom(existingTicket2).Id.Should().Be(response.Data.First(x => x.Id == existingTicket2.Id).Id);

        }
    }


    [Fact]
    public async Task TicketsFilteredByDateExists_GetTickets_ReturnTicketsAndSyncIdentities()
    {
        var siteId = Guid.NewGuid();
        var date = new DateTime(2023, 11, 1);

        var existingTicket1 = Fixture.Build<TicketEntity>()
                                      .With(x => x.SiteId, siteId)
                                      .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                      .With(x => x.CreatedDate, new DateTime(2023, 11, 2))
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

        var existingTicket2 = Fixture.Build<TicketEntity>()
                                    .With(x => x.SiteId, siteId)
                                    .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                    .With(x => x.CreatedDate, new DateTime(2023, 11, 5))
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
                                         .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                                         .With(x => x.SiteId, siteId)
                                         .With(x => x.CreatedDate, new DateTime(2023, 10, 30))
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
        existingTickets.Add(existingTicket1);
        existingTickets.Add(existingTicket2);

        // setup identities
        var responseUserProfile1 = Fixture.Build<UserProfile>()
                                      .With(x => x.Id, existingTicket1.CreatorId)
                                      .Create();

        var creator1 = MappedUserProfile.MapFromUserProfile(responseUserProfile1);
        var assignee1 = Fixture.Build<MappedAssignee>()
                              .With(x => x.Id, existingTicket1.AssigneeId)
                              .Create();

        var responseUserProfile2 = Fixture.Build<UserProfile>()
                                      .With(x => x.Id, existingTicket2.CreatorId)
                                      .Create();

        var creator2 = MappedUserProfile.MapFromUserProfile(responseUserProfile2);
        var assignee2 = Fixture.Build<MappedAssignee>()
                              .With(x => x.Id, existingTicket2.AssigneeId)
                              .Create();

        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                  .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                  .ReturnsJson(new List<UserProfile> { responseUserProfile1, responseUserProfile2 });

            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.Tickets.AddRange(existingTickets);

            db.ExternalProfiles.Add(new ExternalProfileEntity
            {
                Id = existingTicket1.AssigneeId.Value,
                Email = assignee1.Email,
                Name = assignee1.Name
            });

            db.ExternalProfiles.Add(new ExternalProfileEntity
            {
                Id = existingTicket2.AssigneeId.Value,
                Email = assignee2.Email,
                Name = assignee2.Name
            });

            db.SaveChanges();
            var result = await client.GetAsync($"api/mapped/sites/{siteId}/tickets?CreatedAfter={date.ToString("MM/dd/yyyy")}");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            var response = await result.Content.ReadFromJsonAsync<GetTicketsResponse>();
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeTrue();
            response.Data.Count.Should().Be(2);
            MappedTicketDto.MapFrom(existingTicket1).Id.Should().Be(response.Data.First(x => x.Id == existingTicket1.Id).Id);
            MappedTicketDto.MapFrom(existingTicket2).Id.Should().Be(response.Data.First(x => x.Id == existingTicket2.Id).Id);
            response.Data.First(x => x.Id == existingTicket1.Id).Creator.Should().BeEquivalentTo(creator1);
            response.Data.First(x => x.Id == existingTicket1.Id).Assignee.Should().BeEquivalentTo(assignee1);
            response.Data.First(x => x.Id == existingTicket2.Id).Creator.Should().BeEquivalentTo(creator2);
            response.Data.First(x => x.Id == existingTicket2.Id).Assignee.Should().BeEquivalentTo(assignee2);

        }
    }


}

