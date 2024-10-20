using WorkflowCore.Dto;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Entities;
using AutoFixture;
using WorkflowCore.Models;
using System.Linq;
using Willow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using WorkflowCore.Controllers.Request;
using System.Net.Http.Json;

namespace WorkflowCore.Test.Features.Tickets
{
    public class GetInsightOpenTicketStatisticsTests : BaseInMemoryTest
    {
        public GetInsightOpenTicketStatisticsTests(ITestOutputHelper output) : base(output)
        {
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }
        
        [Fact]
        public async Task TokenIsNotGiven_GetInsightOpenTicketStatistics_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"insights/{Guid.NewGuid()}/tickets/open");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

       

        [Fact]
        public async Task GetInsightOpenTicketStatistics_InsightHasNoTicket_ReturnsFalse()
        {
            var siteId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
			var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
											   .Without(x => x.Attachments)
											   .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.Occurrence, 0)
											   .With(x => x.InsightId, Guid.NewGuid())
                                               .With(x => x.SiteId, siteId)
											   .CreateMany(10);
          
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.SaveChanges();

			
				var response = await client.GetAsync($"insights/{insightId}/tickets/open");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<bool>();
                result.Should().BeFalse();
            }
        }

        [Fact]
        public async Task GetInsightOpenTicketStatistics_InsightHasNoOpenTicket_ReturnsFalse()
        {
	        var siteId = Guid.NewGuid();
	        var insightId = Guid.NewGuid();
	        var utcNow = DateTime.UtcNow;
	        var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
		        .Without(x => x.Attachments)
		        .Without(x => x.Comments)
                .Without(x=>x.JobType)
                .With(c=>c.Status,(int)TicketStatusEnum.Closed)
		        .With(x => x.Occurrence, 0)
		        .With(x => x.InsightId, insightId)
		        .With(x => x.SiteId, siteId)
		        .CreateMany(10);

            var ticketStatusEntities = new List<TicketStatusEntity>
            {
                Fixture.Build<TicketStatusEntity>()
                        .With(x=>x.Tab, TicketTabs.CLOSED)
                        .With(x=>x.StatusCode,(int)TicketStatusEnum.Closed)
                        .With(x=>x.Status, "Closed")
                       .Create(),
                Fixture.Build<TicketStatusEntity>()
                        .With(x=>x.Tab, TicketTabs.OPEN)
                        .With(x=>x.StatusCode,(int)TicketStatusEnum.Open)
                        .With(x=>x.Status, "Open")
                       .Create(),
            };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
	        using (var client = server.CreateClient(null))
	        {
		        var db = server.Arrange().CreateDbContext<WorkflowContext>();
		        db.Tickets.AddRange(ticketEntitiesForSite);
                db.TicketStatuses.AddRange(ticketStatusEntities);
		        db.SaveChanges();


		        var response = await client.GetAsync($"insights/{insightId}/tickets/open");

		        response.StatusCode.Should().Be(HttpStatusCode.OK);
		        var result = await response.Content.ReadAsAsync<bool>();
		        result.Should().BeFalse();
	        }
        }

        [Fact]
        public async Task GetInsightOpenTicketStatistics_InsightHasOpenTicket_ReturnsTrue()
        {
	        var siteId = Guid.NewGuid();
	        var insightId = Guid.NewGuid();
	        var utcNow = DateTime.UtcNow;
	        var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
		        .Without(x => x.Attachments)
		        .Without(x => x.Comments)
                .Without(x => x.JobType)
                .Without(x => x.Diagnostics)
                .With(c => c.Status, (int)TicketStatusEnum.InProgress)
		        .With(x => x.Occurrence, 0)
		        .With(x => x.InsightId, insightId)
		        .With(x => x.SiteId, siteId)
		        .CreateMany(3).ToList();
	        ticketEntitiesForSite.AddRange(Fixture.Build<TicketEntity>()
		        .Without(x => x.Attachments)
		        .Without(x => x.Comments)
                .Without(x => x.JobType)
                .Without(x => x.Diagnostics)
                .With(c => c.Status, (int)TicketStatusEnum.LimitedAvailability)
		        .With(x => x.Occurrence, 0)
		        .With(x => x.InsightId, insightId)
		        .With(x => x.SiteId, siteId)
		        .CreateMany(3));
	        ticketEntitiesForSite.AddRange(Fixture.Build<TicketEntity>()
		        .Without(x => x.Attachments)
		        .Without(x => x.Comments)
                .Without(x => x.JobType)
                .Without(x => x.Diagnostics)
                .With(c => c.Status, (int)TicketStatusEnum.Closed)
		        .With(x => x.Occurrence, 0)
		        .With(x => x.InsightId, insightId)
		        .With(x => x.SiteId, siteId)
		        .CreateMany(3));

            var ticketStatusEntities = new List<TicketStatusEntity>
            {
                Fixture.Build<TicketStatusEntity>()
                        .With(x=>x.Tab, TicketTabs.CLOSED)
                        .With(x=>x.StatusCode,(int)TicketStatusEnum.Closed)
                        .With(x=>x.Status, "Closed")
                       .Create(),
                Fixture.Build<TicketStatusEntity>()
                        .With(x=>x.Tab, TicketTabs.OPEN)
                        .With(x=>x.StatusCode,(int)TicketStatusEnum.Open)
                        .With(x=>x.Status, "Open")
                       .Create(),
            };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
	        using (var client = server.CreateClient(null))
	        {
		        var db = server.Arrange().CreateDbContext<WorkflowContext>();
		        db.Tickets.AddRange(ticketEntitiesForSite);
                db.TicketStatuses.AddRange(ticketStatusEntities);
		        db.SaveChanges();


		        var response = await client.GetAsync($"insights/{insightId}/tickets/open");

		        response.StatusCode.Should().Be(HttpStatusCode.OK);
		        var result = await response.Content.ReadAsAsync<bool>();
		        result.Should().BeTrue();
	        }
        }
    }
}
