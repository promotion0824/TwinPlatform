using AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using WorkflowCore.Services.MappedIntegration.Dtos.Requests;
using WorkflowCore.Services.MappedIntegration.Dtos.Responses;
using WorkflowCore.Services.MappedIntegration.Models;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http;
using FluentAssertions;

namespace WorkflowCore.Test.Features.MappedIntegration;

public class ReadOnlyUpsertTicketsTests : BaseInMemoryTest
{
    public ReadOnlyUpsertTicketsTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task ValidCreateTicketEvent_UpsertTickets_ReturnsOk()
    {
        var creator = Fixture.Build<MappedUserProfile>()
                                .With(x => x.Email, "aa@aa.comm")
                                .With(x => x.Phone, "1234567890")
                                .Create();

        var ticketData = Fixture.Build<TicketData>()
                                        .With(x => x.Status, "COMPLETED")
                                        .With(x => x.AssigneeType, AssigneeType.NoAssignee.ToString())
                                        .With(x => x.Priority, "pe-emergency-onsite w/i 2 hours")
                                        .With(x => x.Creator, creator)
                                        .Without(x => x.TwinId)
                                        .Without(x => x.Summary)
                                        .Without(x => x.Description)
                                        .Without(x => x.DueDate)
                                        .Without(x => x.ExternalCreatedDate)
                                        .Without(x => x.ExternalUpdatedDate)
                                        .Without(x => x.ResolvedDate)
                                        .Without(x => x.RequestType)
                                        .Without(x => x.JobType)
                                        .Without(x => x.ServiceNeeded)
                                        .Without(x => x.Solution)
                                        .Without(x => x.ClosedBy)
                                        .Create();

        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Create.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithReadOnlyMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                   .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                   .ReturnsJson(Fixture.CreateMany<UserProfile>(3).ToList());

            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);
            var db = server.Assert().GetDbContext<WorkflowContext>();
            var ticket = db.Tickets.Where(x => x.ExternalId == ticketData.ExternalId);
            
            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            ticket.Should().NotBeEmpty();
            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();
            response.Data.TicketId.Should().Be(ticket.FirstOrDefault().Id);
        }
    }


    [Fact]
    public async Task ValidCreateTickeUserAssignee_UpsertTickets_ReturnsOk()
    {
        var creator = Fixture.Build<MappedUserProfile>()
                                .With(x => x.Email, "aa@aa.comm")
                                .With(x => x.Phone, "1234567890")
                                .Create();

        var assignee = Fixture.Build<MappedAssignee>()
                               .With(x => x.Email, "aa@willowinc.com")
                               .Create();

        var ticketData = Fixture.Build<TicketData>()
                                        .With(x => x.Status, "COMPLETED")
                                        .With(x => x.AssigneeType, AssigneeType.CustomerUser.ToString())
                                        .With(x => x.Assignee, assignee)
                                        .With(x => x.Priority, "pe-emergency-onsite w/i 2 hours")
                                        .With(x => x.Creator, creator)
                                        .Without(x => x.TwinId)
                                        .Without(x => x.Summary)
                                        .Without(x => x.Description)
                                        .Without(x => x.DueDate)
                                        .Without(x => x.ExternalCreatedDate)
                                        .Without(x => x.ExternalUpdatedDate)
                                        .Without(x => x.ResolvedDate)
                                        .Without(x => x.RequestType)
                                        .Without(x => x.JobType)
                                        .Without(x => x.ServiceNeeded)
                                        .Without(x => x.Solution)
                                        .Without(x => x.ClosedBy)
                                        .Create();

        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Create.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithReadOnlyMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                   .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                   .ReturnsJson(Fixture.CreateMany<UserProfile>(3).ToList());

            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);
            var db = server.Assert().GetDbContext<WorkflowContext>();
            var ticket = db.Tickets.Where(x => x.ExternalId == ticketData.ExternalId);

            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            ticket.Should().NotBeEmpty();
            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();
            response.Data.TicketId.Should().Be(ticket.FirstOrDefault().Id);
        }
    }

    [Fact]
    public async Task ValidCreateTickeWithWorkgroup_UpsertTickets_ReturnsOk()
    {
        var creator = Fixture.Build<MappedUserProfile>()
                                .With(x => x.Email, "aa@aa.comm")
                                .With(x => x.Phone, "1234567890")
                                .Create();

        var workGroup = Fixture.Build<MappedWorkgroup>()
                               .Without(x => x.Assignees)
                               .Create();

        var ticketData = Fixture.Build<TicketData>()
                                        .With(x => x.Status, "COMPLETED")
                                        .With(x => x.AssigneeType, AssigneeType.WorkGroup.ToString())
                                        .With(x => x.AssigneeWorkgroup, workGroup)
                                        .With(x => x.Priority, "pe-emergency-onsite w/i 2 hours")
                                        .With(x => x.Creator, creator)
                                        .Without(x => x.TwinId)
                                        .Without(x => x.Summary)
                                        .Without(x => x.Description)
                                        .Without(x => x.DueDate)
                                        .Without(x => x.ExternalCreatedDate)
                                        .Without(x => x.ExternalUpdatedDate)
                                        .Without(x => x.ResolvedDate)
                                        .Without(x => x.RequestType)
                                        .Without(x => x.JobType)
                                        .Without(x => x.ServiceNeeded)
                                        .Without(x => x.Solution)
                                        .Without(x => x.ClosedBy)
                                        .Create();

        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Create.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithReadOnlyMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                   .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                   .ReturnsJson(Fixture.CreateMany<UserProfile>(3).ToList());

            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);
            var db = server.Assert().GetDbContext<WorkflowContext>();
            var ticket = db.Tickets.Where(x => x.ExternalId == ticketData.ExternalId);

            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            ticket.Should().NotBeEmpty();
            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();
            response.Data.TicketId.Should().Be(ticket.FirstOrDefault().Id);
        }
    }

    [Fact]
    public async Task CreateTicketWithUserAssigneeTypeAndWithoutAssaignee_UpsertTickets_ReturnsErrors()
    {
        var creator = Fixture.Build<MappedUserProfile>()
                                .With(x => x.Email, "aa@aa.comm")
                                .With(x => x.Phone, "1234567890")
                                .Create();

        var ticketData = Fixture.Build<TicketData>()
                                        .With(x => x.Status, "COMPLETED")
                                        .With(x => x.AssigneeType, AssigneeType.CustomerUser.ToString())
                                        .With(x => x.Priority, "pe-emergency-onsite w/i 2 hours")
                                        .With(x => x.Creator, creator)
                                        .Without(x => x.TwinId)
                                        .Without(x => x.Summary)
                                        .Without(x => x.Description)
                                        .Without(x => x.DueDate)
                                        .Without(x => x.ExternalCreatedDate)
                                        .Without(x => x.ExternalUpdatedDate)
                                        .Without(x => x.ResolvedDate)
                                        .Without(x => x.RequestType)
                                        .Without(x => x.JobType)
                                        .Without(x => x.ServiceNeeded)
                                        .Without(x => x.Solution)
                                        .Without(x => x.ClosedBy)
                                        .Without(x => x.Assignee)
                                        .Create();

        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Create.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithReadOnlyMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                   .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                   .ReturnsJson(Fixture.CreateMany<UserProfile>(3).ToList());

            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);
            var db = server.Assert().GetDbContext<WorkflowContext>();
            var ticket = db.Tickets.Where(x => x.ExternalId == ticketData.ExternalId);

            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.IsSuccess.Should().BeFalse();
            response.ErrorList.Should().NotBeEmpty();
            response.ErrorList.Should().Contain("Assignee details (name and email) are required when the Assignee type is CustomerUser");
           
        }
    }

    [Fact]
    public async Task ValidUpdateTickeUserAssignee_UpsertTickets_ReturnsOk()
    {
       

        var creator = Fixture.Build<MappedUserProfile>()
                                .With(x => x.Email, "aa@aa.comm")
                                .With(x => x.Phone, "1234567890")
                                .Create();

        var assignee = Fixture.Build<MappedAssignee>()
                               .With(x => x.Email, "aa@willowinc.com")
                               .Create();

        var ticketData = Fixture.Build<TicketData>()
                                        .With(x => x.Status, "IN PROGRESS")
                                        .With(x => x.AssigneeType, AssigneeType.CustomerUser.ToString())
                                        .With(x => x.Assignee, assignee)
                                        .With(x => x.Priority, "pe-emergency-onsite w/i 2 hours")
                                        .With(x => x.Creator, creator)
                                        .Without(x => x.TwinId)
                                        .Without(x => x.Summary)
                                        .Without(x => x.Description)
                                        .Without(x => x.DueDate)
                                        .Without(x => x.ExternalCreatedDate)
                                        .Without(x => x.ExternalUpdatedDate)
                                        .Without(x => x.ResolvedDate)
                                        .Without(x => x.RequestType)
                                        .Without(x => x.JobType)
                                        .Without(x => x.ServiceNeeded)
                                        .Without(x => x.Solution)
                                        .Without(x => x.ClosedBy)
                                        .Create();

        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Update.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();

        var existingTicket = Fixture.Build<TicketEntity>()
                             .With(x => x.SiteId, ticketData.SiteId)
                             .With(x => x.ExternalId, ticketData.ExternalId)
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

        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithReadOnlyMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                   .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                   .ReturnsJson(Fixture.CreateMany<UserProfile>(3).ToList());

            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.Tickets.Add(existingTicket);
            db.SaveChanges();

            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);
           

            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();
            response.Data.TicketId.Should().Be(existingTicket.Id);
            var updatedTicket = db.Tickets.FirstOrDefault(x => x.ExternalId == existingTicket.ExternalId);
            var updatedAssignee = db.ExternalProfiles.FirstOrDefault(x => x.Email == assignee.Email);
            updatedTicket.AssigneeId.Should().Be(updatedAssignee.Id);
            updatedTicket.AssigneeName.Should().Be(assignee.Name);
            updatedTicket.Status.Should().Be((int)TicketStatusEnum.InProgress);
            updatedTicket.Should().NotBeNull();
            updatedTicket.Summary.Should().Be(string.Empty);
            updatedTicket.Description.Should().Be(string.Empty);
            updatedTicket.Priority.Should().Be((int)Priority.Urgent);
            updatedTicket.DueDate.Should().Be(ticketData.DueDate);
            updatedTicket.ExternalUpdatedDate.Should().Be(ticketData.ExternalUpdatedDate);
            updatedTicket.CategoryId.Should().NotBeNull();
            updatedTicket.ServiceNeededId.Should().NotBeNull();
            updatedTicket.JobTypeId.Should().NotBeNull();
            updatedTicket.ExternalId.Should().Be(ticketData.ExternalId);
        }
    }

    [Fact]
    public async Task ValidUpdateWorkGroupTickeUserAssignee_UpsertTickets_ReturnsOk()
    {


        var creator = Fixture.Build<MappedUserProfile>()
                                .With(x => x.Email, "aa@aa.comm")
                                .With(x => x.Phone, "1234567890")
                                .Create();

        var workGroup = Fixture.Build<MappedWorkgroup>()
                            .Without(x => x.Assignees)
                            .Create();

        var ticketData = Fixture.Build<TicketData>()
                                        .With(x => x.Status, "IN PROGRESS")
                                        .With(x => x.AssigneeType, AssigneeType.WorkGroup.ToString())
                                        .With(x => x.AssigneeWorkgroup, workGroup)
                                        .With(x => x.Priority, "pe-emergency-onsite w/i 2 hours")
                                        .With(x => x.Creator, creator)
                                        .Without(x => x.TwinId)
                                        .Without(x => x.Summary)
                                        .Without(x => x.Description)
                                        .Without(x => x.DueDate)
                                        .Without(x => x.ExternalCreatedDate)
                                        .Without(x => x.ExternalUpdatedDate)
                                        .Without(x => x.ResolvedDate)
                                        .Without(x => x.RequestType)
                                        .Without(x => x.JobType)
                                        .Without(x => x.ServiceNeeded)
                                        .Without(x => x.Solution)
                                        .Without(x => x.ClosedBy)
                                        .Create();

        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Update.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();

        var existingTicket = Fixture.Build<TicketEntity>()
                             .With(x => x.SiteId, ticketData.SiteId)
                             .With(x => x.ExternalId, ticketData.ExternalId)
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

        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithReadOnlyMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                   .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                   .ReturnsJson(Fixture.CreateMany<UserProfile>(3).ToList());

            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.Tickets.Add(existingTicket);
            db.SaveChanges();

            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);


            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();
            response.Data.TicketId.Should().Be(existingTicket.Id);
            var updatedTicket = db.Tickets.FirstOrDefault(x => x.ExternalId == existingTicket.ExternalId);
            var updatedWorkGroup = db.Workgroups.FirstOrDefault(x => x.Name == workGroup.Name);
            updatedTicket.AssigneeId.Should().Be(updatedWorkGroup.Id);
            updatedTicket.AssigneeName.Should().Be(updatedWorkGroup.Name);
            updatedTicket.Status.Should().Be((int)TicketStatusEnum.InProgress);
            updatedTicket.Should().NotBeNull();
            updatedTicket.Summary.Should().Be(string.Empty);
            updatedTicket.Description.Should().Be(string.Empty);
            updatedTicket.Priority.Should().Be((int)Priority.Urgent);
            updatedTicket.DueDate.Should().Be(ticketData.DueDate);
            updatedTicket.ExternalUpdatedDate.Should().Be(ticketData.ExternalUpdatedDate);
            updatedTicket.CategoryId.Should().NotBeNull();
            updatedTicket.ServiceNeededId.Should().NotBeNull();
            updatedTicket.JobTypeId.Should().NotBeNull();
        }
    }


    [Fact]
    public async Task ValidUpdateTickt_UpsertTickets_ReturnsOk()
    {


        var creator = Fixture.Build<MappedUserProfile>()
                                .With(x => x.Email, "aa@aa.comm")
                                .With(x => x.Phone, "1234567890")
                                .Create();

        var workGroup = Fixture.Build<MappedWorkgroup>()
                            .Without(x => x.Assignees)
                            .Create();

        var ticketData = Fixture.Build<TicketData>()
                                        .With(x => x.Status, "IN PROGRESS")
                                        .With(x => x.AssigneeType, AssigneeType.WorkGroup.ToString())
                                        .With(x => x.AssigneeWorkgroup, workGroup)
                                        .With(x => x.Priority, "pe-emergency-onsite w/i 2 hours")
                                        .With(x => x.Creator, creator)
                                        .Without(x => x.TwinId)
                                        .Without(x => x.Summary)
                                        .Without(x => x.Description)
                                        .Without(x => x.DueDate)
                                        .Without(x => x.ExternalCreatedDate)
                                        .Without(x => x.ExternalUpdatedDate)
                                        .Without(x => x.ResolvedDate)
                                        .Without(x => x.RequestType)
                                        .Without(x => x.JobType)
                                        .Without(x => x.ServiceNeeded)
                                        .Without(x => x.Solution)
                                        .Without(x => x.ClosedBy)
                                        .Create();

        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Update.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();

        var existingTicket = Fixture.Build<TicketEntity>()
                             .With(x => x.SiteId, ticketData.SiteId)
                             .With(x => x.ExternalId, ticketData.ExternalId)
                             .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                             .With(x => x.AssigneeId, workGroup.Id)
                             .With(x => x.AssigneeName, workGroup.Name)
                             .With(x => x.Priority, 4)
                             .Without(x => x.Attachments)
                             .Without(x => x.Comments)
                             .Without(x => x.Category)
                             .Without(x => x.Tasks)
                             .Without(x => x.JobType)
                             .Without(x => x.Diagnostics)
                             .With(x => x.TemplateId, (Guid?)null)
                             .Create();

        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithReadOnlyMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                   .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                   .ReturnsJson(Fixture.CreateMany<UserProfile>(3).ToList());

            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.Tickets.Add(existingTicket);
            db.Workgroups.Add(new WorkgroupEntity { Id = workGroup.Id.Value, Name = workGroup.Name, SiteId = ticketData.SiteId.Value });
            db.SaveChanges();

            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);


            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();
            response.Data.TicketId.Should().Be(existingTicket.Id);
            var updatedTicket = db.Tickets.FirstOrDefault(x => x.ExternalId == existingTicket.ExternalId);

            updatedTicket.AssigneeId.Should().Be(workGroup.Id);
            updatedTicket.AssigneeName.Should().Be(workGroup.Name);
            updatedTicket.Status.Should().Be((int)TicketStatusEnum.InProgress);
            updatedTicket.Should().NotBeNull();
            updatedTicket.Summary.Should().Be(string.Empty);
            updatedTicket.Description.Should().Be(string.Empty);
            updatedTicket.Priority.Should().Be((int)Priority.Urgent);
            updatedTicket.DueDate.Should().Be(ticketData.DueDate);
            updatedTicket.ExternalUpdatedDate.Should().Be(ticketData.ExternalUpdatedDate);
            updatedTicket.CategoryId.Should().NotBeNull();
            updatedTicket.ServiceNeededId.Should().NotBeNull();
            updatedTicket.JobTypeId.Should().NotBeNull();
        }
    }

    // ticket metadata = categories, job types and service needed

    [Fact]
    public async Task ValidCreateTicketEventWithMetadata_UpsertTickets_ReturnsOk()
    {
        var creator = Fixture.Build<MappedUserProfile>()
                                .With(x => x.Email, "aa@aa.comm")
                                .With(x => x.Phone, "1234567890")
                                .Create();

        var ticketData = Fixture.Build<TicketData>()
                                        .With(x => x.Status, "COMPLETED")
                                        .With(x => x.AssigneeType, AssigneeType.NoAssignee.ToString())
                                        .With(x => x.Priority, "pe-emergency-onsite w/i 2 hours")
                                        .With(x => x.Creator, creator)
                                        .Without(x => x.TwinId)
                                        .Without(x => x.Summary)
                                        .Without(x => x.Description)
                                        .Without(x => x.DueDate)
                                        .Without(x => x.ExternalCreatedDate)
                                        .Without(x => x.ExternalUpdatedDate)
                                        .Without(x => x.ResolvedDate)
                                        .Without(x => x.Solution)
                                        .Without(x => x.ClosedBy)
                                        .Create();

        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Create.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithReadOnlyMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                   .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                   .ReturnsJson(Fixture.CreateMany<UserProfile>(3).ToList());

            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);
            var db = server.Assert().GetDbContext<WorkflowContext>();
            var ticket = db.Tickets.Where(x => x.ExternalId == ticketData.ExternalId);

            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ticket.Should().NotBeEmpty();
            var category = db.TicketCategories.FirstOrDefault(x => x.Name == ticketData.RequestType);
            var jobType = db.JobTypes.FirstOrDefault(x => x.Name == ticketData.JobType);
            var serviceNeeded = db.ServiceNeeded.FirstOrDefault(x => x.Name == ticketData.ServiceNeeded);

            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();
            response.Data.TicketId.Should().Be(ticket.FirstOrDefault().Id);

            category.Should().NotBeNull();
            category.IsActive.Should().BeTrue();
            category.LastUpdate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

            jobType.Should().NotBeNull();
            jobType.IsActive.Should().BeTrue();
            jobType.LastUpdate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

            serviceNeeded.Should().NotBeNull();
            serviceNeeded.IsActive.Should().BeTrue();
            serviceNeeded.LastUpdate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        }
    }


}

