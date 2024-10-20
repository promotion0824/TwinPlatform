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
using WorkflowCore.Controllers.Request;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using WorkflowCore.Services.MappedIntegration.Dtos.Requests;
using WorkflowCore.Services.MappedIntegration.Dtos.Responses;
using WorkflowCore.Services.MappedIntegration.Models;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.MappedIntegration;

public class UpsertTicketsTests : BaseInMemoryTest
{

    public UpsertTicketsTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task TokenIsNotGiven_UpsertTickets_RequiresAuthorization()
    {
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient())
        {
            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", new CreateTicketRequest());
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task ValidCreateTicketEvent_UpsertTickets_ReturnsOk()
    {
        var reporter = Fixture.Build<MappedReporter>()
                                .With(x => x.ReporterName, "TestReporter")
                                .With(x => x.ReporterPhone, "1234567890")
                                .With(x => x.ReporterEmail, "aa@aa.comm")
                                .Create();

        var creator = Fixture.Build<MappedUserProfile>()
                                .With(x => x.Email, "aa@aa.comm")
                                .With(x => x.Phone, "1234567890")
                                .Create();

        var ticketData = Fixture.Build<TicketData>()
                                        .With(x => x.Status, "Open")
                                        .With(x => x.SubStatus, "AdditionalResourcesRequired")
                                        .With(x => x.AssigneeType, "NoAssignee")
                                        .With(x => x.JobType, "EventPlanning")
                                        .With(x => x.Priority, "Low")
                                        .With(x => x.Reporter, reporter)
                                        .With(x => x.Creator, creator)
                                        .Without(x => x.TwinId)
                                        .Without(x => x.ClosedBy)
                                        .Create();

        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Create.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                   .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                   .ReturnsJson(Fixture.CreateMany<UserProfile>(3).ToList());

            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);
            var db = server.Assert().GetDbContext<WorkflowContext>();
            var ticket = db.Tickets.Where(x => x.ExternalId == ticketData.ExternalId);
            ticket.Should().NotBeEmpty();
            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();
            response.Data.TicketId.Should().Be(ticket.FirstOrDefault().Id);
        }
    }


    [Fact]
    public async Task ValidUpdateTicketEvent_UpsertTickets_ReturnsOk()
    {
        var reporter = Fixture.Build<MappedReporter>()
                               .With(x => x.ReporterName, "TestReporter")
                               .With(x => x.ReporterPhone, "1234567890")
                               .With(x => x.ReporterEmail, "aa@aa.comm")
                               .Create();
        var ticketData = Fixture.Build<TicketData>()
                                       .With(x => x.Status, "New")
                                       .With(x => x.SubStatus, "AdditionalResourcesRequired")
                                       .With(x => x.AssigneeType, "NoAssignee")
                                       .With(x => x.JobType, "EventPlanning")
                                       .With(x => x.Priority, "Low")
                                       .With(x => x.Reporter, reporter)
                                       .Without(x => x.TwinId)
                                       .Without(x => x.ClosedBy)
                                       .Create();

        var existingTicket = Fixture.Build<TicketEntity>()
                                        .With(x => x.SiteId, ticketData.SiteId)
                                        .With(x => x.Id, ticketData.TicketId)
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


        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Update.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.Tickets.Add(existingTicket);
            db.TicketStatusTransitions.AddRange(GetTicketStatusTransition());
            db.SaveChanges();
            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);


            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();
            response.Data.TicketId.Should().Be(existingTicket.Id);

            var ticket = db.Tickets.Where(x => x.ExternalId == ticketData.ExternalId).FirstOrDefault();
            ticket.Should().NotBeNull();
            ticket.Summary.Should().Be(ticketData.Summary);
            ticket.Description.Should().Be(ticketData.Description);
            ticket.Priority.Should().Be((int)Enum.Parse<Priority>(ticketData.Priority, true));
            ticket.DueDate.Should().Be(ticketData.DueDate);
            ticket.ExternalUpdatedDate.Should().Be(ticketData.ExternalUpdatedDate);
            ticket.CategoryId.Should().NotBeNull();
            ticket.ServiceNeededId.Should().NotBeNull();
            ticket.JobTypeId.Should().NotBeNull();

        }
    }

    [Theory]
    [InlineData(TicketStatusEnum.New, TicketStatusEnum.ReadyForWork)]
    [InlineData(TicketStatusEnum.New, TicketStatusEnum.InProgress)]
    [InlineData(TicketStatusEnum.New, TicketStatusEnum.Reassign)]
    [InlineData(TicketStatusEnum.New, TicketStatusEnum.ClosedCancelled)]

    [InlineData(TicketStatusEnum.ReadyForWork, TicketStatusEnum.New)]
    [InlineData(TicketStatusEnum.ReadyForWork, TicketStatusEnum.ReadyForWork)]
    [InlineData(TicketStatusEnum.ReadyForWork, TicketStatusEnum.InProgress)]

    [InlineData(TicketStatusEnum.InProgress, TicketStatusEnum.OnHold)]
    [InlineData(TicketStatusEnum.InProgress, TicketStatusEnum.RequestOnHold)]
    [InlineData(TicketStatusEnum.InProgress, TicketStatusEnum.ClosedCompleted)]
    [InlineData(TicketStatusEnum.InProgress, TicketStatusEnum.ClosedCancelled)]
    [InlineData(TicketStatusEnum.InProgress, TicketStatusEnum.Reassign)]

    [InlineData(TicketStatusEnum.OnHold, TicketStatusEnum.InProgress)]
    [InlineData(TicketStatusEnum.OnHold, TicketStatusEnum.ClosedCancelled)]

    [InlineData(TicketStatusEnum.RequestOnHold, TicketStatusEnum.InProgress)]
    [InlineData(TicketStatusEnum.RequestOnHold, TicketStatusEnum.OnHold)]

    [InlineData(TicketStatusEnum.Reassign, TicketStatusEnum.ReadyForWork)]
    [InlineData(TicketStatusEnum.Reassign, TicketStatusEnum.InProgress)]
    public async Task ValidStatusUpdate_UpsertTickets_ReturnOk(TicketStatusEnum existingStatus, TicketStatusEnum newStatus)
    {
        var reporter = Fixture.Build<MappedReporter>()
                               .With(x => x.ReporterName, "TestReporter")
                               .With(x => x.ReporterPhone, "1234567890")
                               .With(x => x.ReporterEmail, "aa@aa.comm")
                               .Create();
        var ticketData = Fixture.Build<TicketData>()
                                       .With(x => x.Status, newStatus.ToString())
                                       .With(x => x.SubStatus, "AdditionalResourcesRequired")
                                       .With(x => x.AssigneeType, "NoAssignee")
                                       .With(x => x.JobType, "EventPlanning")
                                       .With(x => x.Priority, "Low")
                                       .With(x => x.Reporter, reporter)
                                       .Without(x => x.TwinId)
                                       .Without(x => x.ClosedBy)
                                       .Create();

        var existingTicket = Fixture.Build<TicketEntity>()
                                        .With(x => x.SiteId, ticketData.SiteId)
                                        .With(x => x.Id, ticketData.TicketId)
                                        .With(x => x.Status, (int)existingStatus)
                                        .With(x => x.Priority, 4)
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .Create();


        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                       .With(x => x.EventType, TicketEventType.Update.ToString())
                                       .With(x => x.Data, ticketData)
                                       .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.TicketStatusTransitions.AddRange(GetTicketStatusTransition());
            db.Tickets.Add(existingTicket);
            db.SaveChanges();
            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);

            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();

            var ticket = db.Tickets.Where(x => x.ExternalId == ticketData.ExternalId).FirstOrDefault();
            ticket.Should().NotBeNull();
            ticket.Status.Should().Be((int)Enum.Parse<TicketStatusEnum>(ticketData.Status, true));

        }


    }


    [Theory]
    [InlineData(TicketStatusEnum.New, TicketStatusEnum.OnHold)]
    [InlineData(TicketStatusEnum.New, TicketStatusEnum.RequestOnHold)]
    [InlineData(TicketStatusEnum.New, TicketStatusEnum.ClosedCompleted)]

    [InlineData(TicketStatusEnum.ReadyForWork, TicketStatusEnum.OnHold)]
    [InlineData(TicketStatusEnum.ReadyForWork, TicketStatusEnum.RequestOnHold)]
    [InlineData(TicketStatusEnum.ReadyForWork, TicketStatusEnum.ClosedCompleted)]
    [InlineData(TicketStatusEnum.ReadyForWork, TicketStatusEnum.ClosedCancelled)]

    [InlineData(TicketStatusEnum.InProgress, TicketStatusEnum.New)]

    [InlineData(TicketStatusEnum.OnHold, TicketStatusEnum.ClosedCompleted)]
    [InlineData(TicketStatusEnum.OnHold, TicketStatusEnum.RequestOnHold)]
    [InlineData(TicketStatusEnum.OnHold, TicketStatusEnum.New)]


    [InlineData(TicketStatusEnum.RequestOnHold, TicketStatusEnum.New)]
    [InlineData(TicketStatusEnum.RequestOnHold, TicketStatusEnum.ClosedCompleted)]

    [InlineData(TicketStatusEnum.Reassign, TicketStatusEnum.ClosedCompleted)]
    [InlineData(TicketStatusEnum.Reassign, TicketStatusEnum.OnHold)]
    public async Task InvalidStatusUpdate_UpsertTickets_ReturnError(TicketStatusEnum existingStatus, TicketStatusEnum newStatus)
    {
        var reporter = Fixture.Build<MappedReporter>()
                               .With(x => x.ReporterName, "TestReporter")
                               .With(x => x.ReporterPhone, "1234567890")
                               .With(x => x.ReporterEmail, "aa@aa.comm")
                               .Create();
        var ticketData = Fixture.Build<TicketData>()
                                       .With(x => x.Status, newStatus.ToString())
                                       .With(x => x.SubStatus, "AdditionalResourcesRequired")
                                       .With(x => x.AssigneeType, "NoAssignee")
                                       .With(x => x.JobType, "EventPlanning")
                                       .With(x => x.Priority, "Low")
                                       .With(x => x.Reporter, reporter)
                                       .Without(x => x.TwinId)
                                       .Without(x => x.ClosedBy)
                                       .Create();

        var existingTicket = Fixture.Build<TicketEntity>()
                                        .With(x => x.SiteId, ticketData.SiteId)
                                        .With(x => x.Id, ticketData.TicketId)
                                        .With(x => x.Status, (int)existingStatus)
                                        .With(x => x.Priority, 4)
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .Create();


        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                       .With(x => x.EventType, TicketEventType.Update.ToString())
                                       .With(x => x.Data, ticketData)
                                       .Create();


       
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.Tickets.Add(existingTicket);
            db.TicketStatusTransitions.AddRange(GetTicketStatusTransition());
            db.SaveChanges();
            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);

            var response = await result.Content.ReadFromJsonAsync<BaseResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.IsSuccess.Should().BeFalse();
            response.ErrorList.Should().NotBeEmpty();
            response.ErrorList.Should().Contain($"Invalid status transition from {existingStatus} to {newStatus}");
        }
    }

    [Fact]
    public async Task InvalidCreateTicketEvent_UpsertTickets_ReturnsBadRequestWithErrorList()
    {

        var ticketData = new TicketData
        {
            ExternalId = "TestExternalId",
        };

        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Create.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);


            var response = await result.Content.ReadFromJsonAsync<BaseResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.IsSuccess.Should().BeFalse();
            response.ErrorList.Should().HaveCount(19);
            response.ErrorList.Should().Contain("'Data Sequence Number Prefix' must not be empty.");
            response.ErrorList.Should().Contain("'Data Customer Id' must not be empty.");
            response.ErrorList.Should().Contain("'Data Site Id' must not be empty.");
            response.ErrorList.Should().Contain("'Data Summary' must not be empty.");
            response.ErrorList.Should().Contain("'Data Description' must not be empty.");
            response.ErrorList.Should().Contain("'Data Source Id' must not be empty.");
            response.ErrorList.Should().Contain("'Data Source Name' must not be empty.");
            response.ErrorList.Should().Contain("'Data Priority' must not be empty.");
            response.ErrorList.Should().Contain("'Data Status' must not be empty.");
            response.ErrorList.Should().Contain("'Data Assignee Type' must not be empty.");
            response.ErrorList.Should().Contain("'Data Space Twin Id' must not be empty.");
            response.ErrorList.Should().Contain("'Data Job Type' must not be empty.");
            response.ErrorList.Should().Contain("'Data Request Type' must not be empty.");
            response.ErrorList.Should().Contain("'Data Creator' must not be empty.");
            response.ErrorList.Should().Contain("'Data Reporter' must not be empty.");
            response.ErrorList.Should().Contain("'Data Service Needed' must not be empty.");
            response.ErrorList.Should().Contain(" is invalid AssigneeType , the available values are: NoAssignee, CustomerUser, WorkGroup");
        }
    }


    [Fact]
    public async Task InvalidUpdateTicketEvent_UpsertTickets_ReturnsBadRequestWithErrorList()
    {

        var ticketData = new TicketData
        {
            ExternalId = "TestExternalId",
        };

        var existingTicket = Fixture.Build<TicketEntity>()
                                        .With(x => x.SiteId, ticketData.SiteId)
                                        .With(x => x.Id, ticketData.TicketId)
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


        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Update.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();


        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);


            var response = await result.Content.ReadFromJsonAsync<BaseResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.IsSuccess.Should().BeFalse();
            response.ErrorList.Should().HaveCount(18);
            response.ErrorList.Should().Contain("'Data Customer Id' must not be empty.");
            response.ErrorList.Should().Contain("'Data Site Id' must not be empty.");
            response.ErrorList.Should().Contain("'Data Summary' must not be empty.");
            response.ErrorList.Should().Contain("'Data Description' must not be empty.");
            response.ErrorList.Should().Contain("'Data Source Id' must not be empty.");
            response.ErrorList.Should().Contain("'Data Source Name' must not be empty.");
            response.ErrorList.Should().Contain("'Data Priority' must not be empty.");
            response.ErrorList.Should().Contain("'Data Status' must not be empty.");
            response.ErrorList.Should().Contain("'Data Assignee Type' must not be empty.");
            response.ErrorList.Should().Contain("'Data Space Twin Id' must not be empty.");
            response.ErrorList.Should().Contain("'Data Job Type' must not be empty.");
            response.ErrorList.Should().Contain("'Data Request Type' must not be empty.");
            response.ErrorList.Should().Contain("'Data Reporter' must not be empty.");
            response.ErrorList.Should().Contain("'Data Service Needed' must not be empty.");
            response.ErrorList.Should().Contain(" is invalid AssigneeType , the available values are: NoAssignee, CustomerUser, WorkGroup");
        }
    }

    [Fact]
    public async Task CreateTicketEventAndSyncIdentities_UpsertTickets_ReturnsOk()
    {
        var reporter = Fixture.Build<MappedReporter>()
                                .With(x => x.ReporterName, "TestReporter")
                                .With(x => x.ReporterPhone, "1234567890")
                                .With(x => x.ReporterEmail, "Reporter@aa.com")
                                .Create();

        var creator = Fixture.Build<MappedUserProfile>()
                                .With(x => x.Email, "creator@aa.com")
                                .With(x => x.Phone, "1234567890")
                                .Create();

        var assignee = Fixture.Build<MappedAssignee>()
                              .With(x => x.Email, "assignee@aa.com")
                              .Create();

        var responseUserProfile = Fixture.Build<UserProfile>()
                                        .With(x => x.Email, "assignee@aa.com")
                                        .Create();

        var ticketData = Fixture.Build<TicketData>()
                                        .With(x => x.Status, "Open")
                                        .With(x => x.SubStatus, "AdditionalResourcesRequired")
                                        .With(x => x.AssigneeType, AssigneeType.CustomerUser.ToString())
                                        .With(x => x.Assignee, assignee)
                                        .With(x => x.JobType, "EventPlanning")
                                        .With(x => x.Priority, "Low")
                                        .With(x => x.Reporter, reporter)
                                        .With(x => x.Creator, creator)
                                        .Without(x => x.TwinId)
                                        .Without(x => x.ClosedBy)
                                        .Create();

        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Create.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                   .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                   .ReturnsJson(new List<UserProfile> { responseUserProfile });

            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var db = server.Assert().GetDbContext<WorkflowContext>();

            var insertedExternalUserProfile = db.ExternalProfiles.FirstOrDefault(x => x.Email == "creator@aa.com");
            insertedExternalUserProfile.Should().NotBeNull();
            insertedExternalUserProfile.Name.Should().Be(creator.Name);

            var insertedReporter = db.Reporters.FirstOrDefault(x => x.Email == reporter.ReporterEmail);
            insertedReporter.Email.Should().Be(insertedReporter.Email);
            insertedReporter.Name.Should().Be(insertedReporter.Name);
            insertedReporter.Phone.Should().Be(insertedReporter.Phone);
            insertedReporter.Company.Should().Be(insertedReporter.Company);
            insertedReporter.SiteId.ToString().Should().Be(ticketData.SiteId.ToString());
            insertedReporter.CustomerId.ToString().Should().Be(ticketData.CustomerId.ToString());

            var ticket = db.Tickets.Where(x => x.ExternalId == ticketData.ExternalId).FirstOrDefault();
            ticket.Should().NotBeNull();
            ticket.AssigneeId.Should().Be(responseUserProfile.Id);
            ticket.AssigneeName.Should().Be($"{responseUserProfile.FirstName} {responseUserProfile.LastName}");
            ticket.CreatorId.Should().Be(insertedExternalUserProfile.Id);
            ticket.ReporterId.Should().Be(insertedReporter.Id);
            ticket.ReporterName.Should().Be(insertedReporter.Name);
            ticket.ReporterPhone.Should().Be(insertedReporter.Phone);
            ticket.ReporterCompany.Should().Be(insertedReporter.Company);
            ticket.ReporterEmail.Should().Be(insertedReporter.Email);
            
            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();
            response.Data.TicketId.Should().Be(ticket.Id);
        }
    }

    [Fact]
    public async Task UpdateTicketIdentities_UpsertTickets_ReturnsOk()
    {
        var externalId = "TestExternalId";
        var reporter = Fixture.Build<MappedReporter>()
                               .With(x => x.ReporterName, "TestReporter")
                               .With(x => x.ReporterPhone, "1234567890")
                               .With(x => x.ReporterEmail, "aa@aa.comm")
                               .Create();

        var assignee = Fixture.Build<MappedAssignee>()
                             .With(x => x.Email, "assignee@aa.com")
                             .Create();

        var responseUserProfiles = Fixture.Build<UserProfile>()
                                        .CreateMany(3);
        var ticketData = Fixture.Build<TicketData>()
                                       .With(x => x.Status, "New")
                                       .With(x => x.SubStatus, "AdditionalResourcesRequired")
                                       .With(x => x.AssigneeType, AssigneeType.CustomerUser.ToString())
                                       .With(x => x.Assignee, assignee)
                                       .With(x => x.JobType, "EventPlanning")
                                       .With(x => x.Priority, "Low")
                                       .With(x => x.Reporter, reporter)
                                       .With(x => x.ExternalId, externalId)
                                       .Without(x => x.TwinId)
                                       .Without(x => x.ClosedBy)
                                       .Create();

        var existingTicket = Fixture.Build<TicketEntity>()
                                        .With(x => x.SiteId, ticketData.SiteId)
                                        .With(x => x.Id, ticketData.TicketId)
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


        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Update.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                 .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                 .ReturnsJson(responseUserProfiles);


            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.TicketStatusTransitions.AddRange(GetTicketStatusTransition());
            db.Tickets.Add(existingTicket);
            db.SaveChanges();
            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);



            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();
            response.Data.TicketId.Should().Be(existingTicket.Id);

            var insertedExternalUserProfile = db.ExternalProfiles.FirstOrDefault(x => x.Email == assignee.Email);
            insertedExternalUserProfile.Should().NotBeNull();
            insertedExternalUserProfile.Name.Should().Be(assignee.Name);

            var insertedReporter = db.Reporters.FirstOrDefault(x => x.Email == reporter.ReporterEmail);
            insertedReporter.Email.Should().Be(insertedReporter.Email);
            insertedReporter.Name.Should().Be(insertedReporter.Name);
            insertedReporter.Phone.Should().Be(insertedReporter.Phone);
            insertedReporter.Company.Should().Be(insertedReporter.Company);
            insertedReporter.SiteId.Should().Be(existingTicket.SiteId);
            insertedReporter.CustomerId.Should().Be(existingTicket.CustomerId);

            var ticket = db.Tickets.Where(x => x.ExternalId == ticketData.ExternalId).FirstOrDefault();
            ticket.Should().NotBeNull();
            ticket.Summary.Should().Be(ticketData.Summary);
            ticket.Description.Should().Be(ticketData.Description);
            ticket.Priority.Should().Be((int)Enum.Parse<Priority>(ticketData.Priority, true));
            ticket.DueDate.Should().Be(ticketData.DueDate);
            ticket.ExternalUpdatedDate.Should().Be(ticketData.ExternalUpdatedDate);
            ticket.AssigneeId.Should().Be(insertedExternalUserProfile.Id);
            ticket.AssigneeName.Should().Be(insertedExternalUserProfile.Name);

            ticket.ReporterId.Should().Be(insertedReporter.Id);
            ticket.ReporterName.Should().Be(insertedReporter.Name);
            ticket.ReporterPhone.Should().Be(insertedReporter.Phone);
            ticket.ReporterCompany.Should().Be(insertedReporter.Company);
            ticket.ReporterEmail.Should().Be(insertedReporter.Email);
            ticket.ExternalId.Should().Be(externalId);
        }
    }

    [Fact]
    public async Task InvalidTicketEventWithClosedStatus_UpsertTickets_ReturnsBadRequestWithErrorList()
    {

        var ticketData = new TicketData
        {
            ExternalId = "TestExternalId",
            Status = TicketStatusEnum.ClosedCompleted.ToString()
        };

        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Create.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);


            var response = await result.Content.ReadFromJsonAsync<BaseResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.IsSuccess.Should().BeFalse();
            response.ErrorList.Should().HaveCount(20);
            response.ErrorList.Should().Contain("'Data Sequence Number Prefix' must not be empty.");
            response.ErrorList.Should().Contain("'Data Customer Id' must not be empty.");
            response.ErrorList.Should().Contain("'Data Site Id' must not be empty.");
            response.ErrorList.Should().Contain("'Data Summary' must not be empty.");
            response.ErrorList.Should().Contain("'Data Description' must not be empty.");
            response.ErrorList.Should().Contain("'Data Source Id' must not be empty.");
            response.ErrorList.Should().Contain("'Data Source Name' must not be empty.");
            response.ErrorList.Should().Contain("'Data Priority' must not be empty.");
            response.ErrorList.Should().Contain("'Data Assignee Type' must not be empty.");
            response.ErrorList.Should().Contain("'Data Space Twin Id' must not be empty.");
            response.ErrorList.Should().Contain("'Data Job Type' must not be empty.");
            response.ErrorList.Should().Contain("'Data Request Type' must not be empty.");
            response.ErrorList.Should().Contain("'Data Creator' must not be empty.");
            response.ErrorList.Should().Contain("'Data Reporter' must not be empty.");
            response.ErrorList.Should().Contain("'Data Service Needed' must not be empty.");
            response.ErrorList.Should().Contain("'Data Closed Date' must not be empty.");
            response.ErrorList.Should().Contain("'Data Cause' must not be empty.");
            response.ErrorList.Should().Contain("'Data Solution' must not be empty.");
            response.ErrorList.Should().Contain(" is invalid AssigneeType , the available values are: NoAssignee, CustomerUser, WorkGroup");
        }
    }
    [Fact]
    public async Task ValidCreateTicketWithWorkGroupAssignee_UpsertTickets_ReturnsOk()
    {
        var reporter = Fixture.Build<MappedReporter>()
                                .With(x => x.ReporterName, "TestReporter")
                                .With(x => x.ReporterPhone, "1234567890")
                                .With(x => x.ReporterEmail, "aa@aa.comm")
                                .Create();

        var creator = Fixture.Build<MappedUserProfile>()
                                .With(x => x.Email, "aa@aa.comm")
                                .With(x => x.Phone, "1234567890")
                                .Create();
        var workGroup = Fixture.Build<MappedWorkgroup>()
                                .Without(x => x.Id)
                               .Create();

        var ticketData = Fixture.Build<TicketData>()
                                        .With(x => x.Status, "Open")
                                        .With(x => x.SubStatus, "AdditionalResourcesRequired")
                                        .With(x => x.AssigneeType, AssigneeType.WorkGroup.ToString())
                                        .With(x => x.AssigneeWorkgroup, workGroup)
                                        .With(x => x.JobType, "EventPlanning")
                                        .With(x => x.Priority, "Low")
                                        .With(x => x.Reporter, reporter)
                                        .With(x => x.Creator, creator)
                                        .Without(x => x.TwinId)
                                        .Without(x => x.ClosedBy)
                                        .Without(x => x.ClosedBy)
                                        .Create();

        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Create.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                   .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                   .ReturnsJson(Fixture.CreateMany<UserProfile>(3).ToList());

            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);
            var db = server.Assert().GetDbContext<WorkflowContext>();
            var ticket = db.Tickets.Where(x => x.ExternalId == ticketData.ExternalId).FirstOrDefault();
            var workGroupCreated = db.Workgroups.FirstOrDefault(x => x.Name == workGroup.Name);
            workGroupCreated.Should().NotBeNull();
            ticket.Should().NotBeNull();
            ticket.AssigneeId.Should().Be(workGroupCreated.Id);
            ticket.AssigneeName.Should().Be(workGroup.Name);
            ticket.AssigneeType.Should().Be(AssigneeType.WorkGroup);



            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();
            response.Data.TicketId.Should().Be(ticket.Id);
        }
    }

    [Fact]
    public async Task UpdateTicketWithWorkGroupAssignee_UpsertTickets_ReturnsOk()
    {
        var reporter = Fixture.Build<MappedReporter>()
                               .With(x => x.ReporterName, "TestReporter")
                               .With(x => x.ReporterEmail, "aa@aa.comm")
                               .Without(x => x.ReporterPhone)
                               .Without(x => x.ReporterCompany)
                               .Create();

        var workGroup = Fixture.Build<MappedWorkgroup>()
                                .Without(x => x.Id)
                               .Create();

        var responseUserProfiles = Fixture.Build<UserProfile>()
                                        .CreateMany(3);
        var ticketData = Fixture.Build<TicketData>()
                                       .With(x => x.Status, "New")
                                       .With(x => x.SubStatus, "AdditionalResourcesRequired")
                                       .With(x => x.AssigneeType, AssigneeType.WorkGroup.ToString())
                                       .With(x => x.AssigneeWorkgroup, workGroup)
                                       .With(x => x.JobType, "EventPlanning")
                                       .With(x => x.Priority, "Low")
                                       .With(x => x.Reporter, reporter)
                                       .Without(x => x.TwinId)
                                       .Without(x => x.ClosedBy)
                                       .Create();

        var existingTicket = Fixture.Build<TicketEntity>()
                                        .With(x => x.SiteId, ticketData.SiteId)
                                        .With(x => x.Id, ticketData.TicketId)
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


        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Update.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetDirectoryApi()
                 .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                 .ReturnsJson(responseUserProfiles);


            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.TicketStatusTransitions.AddRange(GetTicketStatusTransition());
            db.Tickets.Add(existingTicket);
            db.SaveChanges();
            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);



            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();
            response.Data.TicketId.Should().Be(existingTicket.Id);



            var insertedReporter = db.Reporters.FirstOrDefault(x => x.Email == reporter.ReporterEmail);
            insertedReporter.Email.Should().Be(reporter.ReporterEmail);
            insertedReporter.Name.Should().Be(reporter.ReporterName);
            insertedReporter.Phone.Should().Be(reporter.ReporterPhone);
            insertedReporter.Company.Should().Be(reporter.ReporterCompany);
            insertedReporter.SiteId.Should().Be(existingTicket.SiteId);
            insertedReporter.CustomerId.Should().Be(existingTicket.CustomerId);

            var ticket = db.Tickets.Where(x => x.ExternalId == ticketData.ExternalId).FirstOrDefault();
            var insertedWorkgroup = db.Workgroups.FirstOrDefault(x => x.Name == workGroup.Name);
            insertedWorkgroup.Name.Should().Be(insertedWorkgroup.Name);

            ticket.Should().NotBeNull();
            ticket.Summary.Should().Be(ticketData.Summary);
            ticket.Description.Should().Be(ticketData.Description);
            ticket.Priority.Should().Be((int)Enum.Parse<Priority>(ticketData.Priority, true));
            ticket.DueDate.Should().Be(ticketData.DueDate);
            ticket.ExternalUpdatedDate.Should().Be(ticketData.ExternalUpdatedDate);
            ticket.AssigneeId.Should().Be(insertedWorkgroup.Id);
            ticket.AssigneeName.Should().Be(workGroup.Name);

            ticket.ReporterId.Should().Be(insertedReporter.Id);
            ticket.ReporterName.Should().Be(insertedReporter.Name);
            ticket.ReporterPhone.Should().Be(insertedReporter.Phone);
            ticket.ReporterCompany.Should().Be(insertedReporter.Company);
            ticket.ReporterEmail.Should().Be(insertedReporter.Email);
        }
    }

    [Fact]
    public async Task TicketClosed_UpsertTickets_SyncIdentitiesAndReturnsOk()
    {
        var closedBy = Fixture.Build<MappedUserProfile>()
                              .With(x => x.Email, "closedBy@aa.com")
                              .With(x => x.Phone, "1234567890")
                              .Create();

        var reporter = Fixture.Build<MappedReporter>()
                               .With(x => x.ReporterName, "TestReporter")
                               .With(x => x.ReporterPhone, "1234567890")
                               .With(x => x.ReporterEmail, "aa@aa.comm")
                               .Create();
        var ticketData = Fixture.Build<TicketData>()
                                       .With(x => x.Status, "New")
                                       .With(x => x.SubStatus, "AdditionalResourcesRequired")
                                       .With(x => x.AssigneeType, "NoAssignee")
                                       .With(x => x.JobType, "EventPlanning")
                                       .With(x => x.Priority, "Low")
                                       .With(x => x.Reporter, reporter)
                                       .With(x => x.ClosedBy, closedBy)
                                       .Without(x => x.TwinId)
                                       .Create();

        var existingTicket = Fixture.Build<TicketEntity>()
                                        .With(x => x.SiteId, ticketData.SiteId)
                                        .With(x => x.Id, ticketData.TicketId)
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


        var upsertTicketData = Fixture.Build<MappedTicketUpsertRequest>()
                                        .With(x => x.EventType, TicketEventType.Update.ToString())
                                        .With(x => x.Data, ticketData)
                                        .Create();



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {

            server.Arrange().GetDirectoryApi()
                  .SetupRequestSequence(HttpMethod.Post, "users/profiles")
                  .ReturnsJson(Fixture.CreateMany<UserProfile>(3).ToList());

            var db = server.Assert().GetDbContext<WorkflowContext>();
            db.Tickets.Add(existingTicket);
            db.TicketStatusTransitions.AddRange(GetTicketStatusTransition());
            db.SaveChanges();
            var result = await client.PostAsJsonAsync($"api/mapped/tickets/upsert", upsertTicketData);


            var response = await result.Content.ReadFromJsonAsync<UpsertTicketResponse>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccess.Should().BeTrue();
            response.ErrorList.Should().BeNullOrEmpty();
            response.Data.TicketId.Should().Be(existingTicket.Id);

            var ticket = db.Tickets.Where(x => x.ExternalId == ticketData.ExternalId).FirstOrDefault();
            ticket.Should().NotBeNull();
            ticket.Summary.Should().Be(ticketData.Summary);
            ticket.Description.Should().Be(ticketData.Description);
            ticket.Priority.Should().Be((int)Enum.Parse<Priority>(ticketData.Priority, true));
            ticket.DueDate.Should().Be(ticketData.DueDate);
            ticket.ExternalUpdatedDate.Should().Be(ticketData.ExternalUpdatedDate);
            ticket.CategoryId.Should().NotBeNull();
            ticket.ServiceNeededId.Should().NotBeNull();
            ticket.JobTypeId.Should().NotBeNull();

        }
    }
    private List<TicketStatusTransitionsEntity> GetTicketStatusTransition()
    {
        var ticketStatusTransition = new List<TicketStatusTransitionsEntity> {
            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.New, ToStatus = (int)TicketStatusEnum.ReadyForWork },
            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.New, ToStatus = (int)TicketStatusEnum.InProgress },
            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.New, ToStatus = (int)TicketStatusEnum.Reassign },
            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.New, ToStatus = (int)TicketStatusEnum.ClosedCancelled },

            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.ReadyForWork, ToStatus = (int)TicketStatusEnum.New },
            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.ReadyForWork, ToStatus = (int)TicketStatusEnum.InProgress },
            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.ReadyForWork, ToStatus = (int)TicketStatusEnum.Reassign },

            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.InProgress, ToStatus = (int)TicketStatusEnum.OnHold },
            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.InProgress, ToStatus = (int)TicketStatusEnum.RequestOnHold },
            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.InProgress, ToStatus = (int)TicketStatusEnum.ClosedCompleted },
            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.InProgress, ToStatus = (int)TicketStatusEnum.ClosedCancelled },
            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.InProgress, ToStatus = (int)TicketStatusEnum.Reassign },

            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.RequestOnHold, ToStatus = (int)TicketStatusEnum.InProgress },
            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.RequestOnHold, ToStatus = (int)TicketStatusEnum.OnHold },

            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.OnHold, ToStatus = (int)TicketStatusEnum.InProgress },
            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.OnHold, ToStatus = (int)TicketStatusEnum.ClosedCancelled },

            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.Reassign, ToStatus = (int)TicketStatusEnum.ReadyForWork },
            new TicketStatusTransitionsEntity {Id = Guid.NewGuid(), FromStatus = (int)TicketStatusEnum.Reassign, ToStatus = (int)TicketStatusEnum.InProgress }


        };

        return ticketStatusTransition;
    }
}

