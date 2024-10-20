using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Extensions;
using Newtonsoft.Json;

namespace WorkflowCore.Test.Features.Tickets
{
    public class UpdateDynamicTicketTests : BaseInMemoryTest
    {
        public UpdateDynamicTicketTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TicketDoesNotExist_UpdateDynamicTicket_ReturnsNotFound()
        {
            string sequenceNumber = "test";
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"dynamics/tickets/{sequenceNumber}", new DynamicsUpdateTicketRequest 
                {
                    Description = "test"
                });
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task GivenValidInput_UpdateDynamicTicket_ReturnsUpdatedValues()
        {
             var siteId = Guid.NewGuid();
           string sequenceNumber = "test0001";
            string description = "New description";
            string summary = "new summary";
            string reporterName = "new reportName";
            string reporterPhone = "new reportPhone";
            string reporterEmail = "new reporterEmail";
            int priority = 1;
            string status = "InProgress";

            var existingTicket = Fixture.Build<TicketEntity>()
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.IssueType, IssueType.NoIssue)
                                        .With(x => x.SequenceNumber, sequenceNumber)
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(new Site { Id = siteId, Name = "Site54" } );

                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"dynamics/tickets/{sequenceNumber}", new DynamicsUpdateTicketRequest 
                {
                    Description = description,
                    Priority = priority.ToString(),
                    TicketStatus = status,
                    Summary = summary,
                    ReporterName = reporterName,      
                    ReporterPhone = reporterPhone,
                    ReporterEmail = reporterEmail  
                });

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(1);
                var updatedTicket = db.Tickets.Include(x => x.Attachments).Include(x => x.Comments).First();
                updatedTicket.Should().NotBeNull();
                updatedTicket.Priority.Should().Be(priority);
                updatedTicket.Status.Should().Be((int)Enum.Parse<TicketStatusEnum>(status));
                updatedTicket.IssueType.Should().Be(existingTicket.IssueType);
                updatedTicket.IssueId.Should().Be(existingTicket.IssueId);
                updatedTicket.IssueName.Should().Be(existingTicket.IssueName);
                updatedTicket.Summary.Should().Be(summary);
                updatedTicket.Description.Should().Be(description);
                updatedTicket.Cause.Should().Be(existingTicket.Cause);
                updatedTicket.Solution.Should().Be(existingTicket.Solution);
                updatedTicket.AssigneeType.Should().Be(existingTicket.AssigneeType);
                updatedTicket.AssigneeId.Should().Be(existingTicket.AssigneeId);
                updatedTicket.DueDate.Should().Be(existingTicket.DueDate);
                updatedTicket.ReporterEmail.Should().Be(reporterEmail);
                updatedTicket.ReporterPhone.Should().Be(reporterPhone);
                updatedTicket.ReporterName.Should().Be(reporterName);
            }
        }

        [Fact]
        public async Task GivenValidInputSameDescription_UpdateDynamicTicket_ReturnsSameValues()
        {
            string sequenceNumber = "test0001";
            var existingTicketStatus = (int)Fixture.Create<TicketStatusEnum>();
            var siteId = Guid.NewGuid();
            
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .With(x => x.SiteId, siteId)
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.Tasks, new List<TicketTaskEntity>())
                                        .With(x => x.Priority, 1)
                                        .With(x => x.Status, existingTicketStatus)
                                        .With(x => x.SequenceNumber, sequenceNumber)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();
            string description = existingTicket.Description;
            string issueName = existingTicket.IssueName;
            string summary = existingTicket.Summary;
            string reporterName = existingTicket.ReporterName;
            string reporterPhone = existingTicket.ReporterPhone;
            string reporterEmail = existingTicket.ReporterEmail;
            int priority = existingTicket.Priority;
            string status = Enum.GetName(typeof(TicketStatusEnum), existingTicket.Status);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(new Site { Id = siteId, Name = "Site54" } );

                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"dynamics/tickets/{sequenceNumber}", new DynamicsUpdateTicketRequest 
                {
                    Description = description,
                    Priority = priority.ToString(),
                    TicketStatus = status,
                    Summary = summary,
                    ReporterName = reporterName,
                    ReporterPhone = reporterPhone,
                    ReporterEmail = reporterEmail,
                });

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(1);
                var updatedTicket = db.Tickets.Include(x => x.Attachments).Include(x => x.Comments).First();
                updatedTicket.Should().NotBeNull();
                updatedTicket.Priority.Should().Be(priority);
                updatedTicket.Status.Should().Be((int)Enum.Parse<TicketStatusEnum>(status));
                updatedTicket.IssueType.Should().Be(existingTicket.IssueType);
                updatedTicket.IssueId.Should().Be(existingTicket.IssueId);
                updatedTicket.IssueName.Should().Be(issueName);
                updatedTicket.Summary.Should().Be(summary);
                updatedTicket.Description.Should().Be(description);
                updatedTicket.Cause.Should().Be(existingTicket.Cause);
                updatedTicket.Solution.Should().Be(existingTicket.Solution);
                updatedTicket.AssigneeType.Should().Be(existingTicket.AssigneeType);
                updatedTicket.AssigneeId.Should().Be(existingTicket.AssigneeId);
                updatedTicket.DueDate.Should().Be(existingTicket.DueDate);
                updatedTicket.ReporterEmail.Should().Be(reporterEmail);
                updatedTicket.ReporterPhone.Should().Be(reporterPhone);
                updatedTicket.ReporterName.Should().Be(reporterName);
            }
        }
    }
}
