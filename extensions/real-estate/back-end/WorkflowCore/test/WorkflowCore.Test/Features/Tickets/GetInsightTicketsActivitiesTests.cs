using AutoFixture;
using FluentAssertions;
using FluentAssertions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Controllers.Responses;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Tickets;
public class GetInsightTicketsActivitiesTests : BaseInMemoryTest
{
	public GetInsightTicketsActivitiesTests(ITestOutputHelper output) : base(output)
	{
		Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
	}

	[Fact]
	public async Task UserUnauthorized_GetInsightTicketsActivities_ReturnUnauthorized()
	{
		await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
		using (var client = server.CreateClient())
		{
			var result = await client.GetAsync($"insights/{Guid.NewGuid()}/tickets/activities");
			result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
		}
	}

	[Fact]
	public async Task TicketsActivityExists_GetInsightTicketsActivities_ReturnTheseActivities()
	{
		var insightId = Guid.NewGuid();
		var userId = Guid.NewGuid();
		var utcNow = DateTime.UtcNow;
		var tickets = Fixture.Build<TicketEntity>()
							 .With(x => x.InsightId, insightId)
							 .With(x => x.Status, 0)
							 .With(x => x.CreatedDate, utcNow)
							 .Without(x => x.Comments)
                             .Without(x => x.JobType)
                             .Without(x => x.Diagnostics)
                             .Without(x => x.Attachments)
							 .CreateMany(3)
							 .ToList();

		var expectedActivities = new List<TicketActivityResponse>();
		var comments = new List<CommentEntity>();
		foreach (var ticket in tickets)
		{
			var comment = Fixture.Build<CommentEntity>()
							  .With(x => x.TicketId, ticket.Id)
							  .With(x => x.Ticket, ticket)
							  .With(x => x.CreatorId, userId)
							  .With(x => x.CreatedDate, utcNow)
							  .Create();
			comments.Add(comment);

			var activityCommentResponse = new TicketActivityResponse
			{
				TicketId = ticket.Id,
				ActivityDate = comment.CreatedDate,
				ActivityType = TicketActivityType.TicketComment.ToString(),
				SourceId = userId,
				SourceType = SourceType.Platform,
                TicketSummary = ticket.Summary,
				Activities = new()
				{
					new KeyValuePair<string, string>(nameof(TicketEntity.Comments),comment.Text)
				}

			};

			expectedActivities.Add(activityCommentResponse);

			var ticketActivity = new TicketActivityResponse
			{
				TicketId = ticket.Id,
				ActivityDate = ticket.CreatedDate,
				ActivityType = TicketActivityType.NewTicket.ToString(),
				SourceId = userId,
				SourceType = SourceType.Platform,
                TicketSummary = ticket.Summary,
				Activities = new()
				{
					new KeyValuePair<string, string>(nameof(TicketEntity.Status), Enum.GetName(typeof(TicketStatusEnum), ticket.Status)),
					new KeyValuePair<string, string>(nameof(TicketEntity.AssigneeId), ticket.AssigneeId.ToString()),
					new KeyValuePair<string, string>(nameof(TicketEntity.AssigneeType), ticket.AssigneeType.ToString()),
					new KeyValuePair<string, string>(nameof(TicketEntity.AssigneeName), ticket.AssigneeName),
					new KeyValuePair<string, string>(nameof(TicketEntity.Summary), ticket.Summary),
					new KeyValuePair<string, string>(nameof(TicketEntity.Description), ticket.Description),
					new KeyValuePair<string, string>(nameof(TicketEntity.DueDate), ticket.DueDate.ToString()),
					new KeyValuePair<string, string>(nameof(TicketEntity.Priority), ticket.Priority.ToString())

				}
			};
			expectedActivities.Add(ticketActivity);
		}




		await using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
		using var client = server.CreateClient(null, auth0UserId: userId.ToString());
		server.Arrange().SetSessionData(SourceType.Platform, userId);
		var db = server.Assert().GetDbContext<WorkflowContext>();
		db.Tickets.AddRange(tickets);
		db.SaveChanges();
		db.Comments.AddRange(comments);
		db.SaveChanges();

		var response = await client.GetAsync($"insights/{insightId}/tickets/activities");
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var result = await response.Content.ReadAsAsync<List<TicketActivityResponse>>();
		result.Should().NotBeNull();
		result.Should().HaveCount(6);
		result.Should().BeEquivalentTo(expectedActivities, config =>
		{
			config.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 30000.Seconds())).WhenTypeIs<DateTime>();
			return config;

		}); ;

	}

	[Fact]
	public async Task TicketsActivityNotExists_GetInsightTicketsActivities_ReturnEmptyList()
	{
		var insightId = Guid.NewGuid();
		var userId = Guid.NewGuid();
		var utcNow = DateTime.UtcNow;
		var tickets = Fixture.Build<TicketEntity>()
							 .With(x => x.InsightId, insightId)
							 .With(x => x.Status, 0)
							 .With(x => x.CreatedDate, utcNow)
							 .Without(x => x.Comments)
                             .Without(x => x.JobType)
                             .Without(x => x.Diagnostics)
                             .CreateMany(3)
							 .ToList();

		var expectedActivities = new List<TicketActivityResponse>();
		var comments = new List<CommentEntity>();
		foreach (var ticket in tickets)
		{
			var comment = Fixture.Build<CommentEntity>()
							  .With(x => x.TicketId, ticket.Id)
							  .With(x => x.Ticket, ticket)
							  .With(x => x.CreatorId, userId)
							  .With(x => x.CreatedDate, utcNow)
							  .Create();
			comments.Add(comment);

			var activityCommentResponse = new TicketActivityResponse
			{
				TicketId = ticket.Id,
				ActivityDate = comment.CreatedDate,
				ActivityType = TicketActivityType.TicketComment.ToString(),
				SourceId = userId,
				SourceType = SourceType.Platform,
				Activities = new()
				{
					new KeyValuePair<string, string>(nameof(TicketEntity.Comments),comment.Text)
				}

			};

			expectedActivities.Add(activityCommentResponse);

			var ticketActivity = new TicketActivityResponse
			{
				TicketId = ticket.Id,
				ActivityDate = ticket.CreatedDate,
				ActivityType = TicketActivityType.NewTicket.ToString(),
				SourceId = userId,
				SourceType = SourceType.Platform,
				Activities = new()
				{
					new KeyValuePair<string, string>(nameof(TicketEntity.Status), Enum.GetName(typeof(TicketStatusEnum), ticket.Status)),
					new KeyValuePair<string, string>(nameof(TicketEntity.AssigneeId), ticket.AssigneeId.ToString()),
					new KeyValuePair<string, string>(nameof(TicketEntity.AssigneeType), ticket.AssigneeType.ToString()),
					new KeyValuePair<string, string>(nameof(TicketEntity.AssigneeName), ticket.AssigneeName),

				}
			};
			expectedActivities.Add(ticketActivity);
		}

		await using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
		using var client = server.CreateClient(null, auth0UserId: userId.ToString());
		server.Arrange().SetSessionData(SourceType.Platform, userId);
		var db = server.Assert().GetDbContext<WorkflowContext>();
		db.Tickets.AddRange(tickets);
		db.SaveChanges();
		db.Comments.AddRange(comments);
		db.SaveChanges();

		var response = await client.GetAsync($"insights/{Guid.NewGuid()}/tickets/activities");
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var result = await response.Content.ReadAsAsync<List<TicketActivityResponse>>();
		result.Should().NotBeNull();
		result.Should().HaveCount(0);

	}

	[Fact]
	public async Task TicketsActivityExistsWithAttachments_GetInsightTicketsActivities_ReturnTheseActivities()
	{
		var insightId = Guid.NewGuid();
		var userId = Guid.NewGuid();
		var utcNow = DateTime.UtcNow;
		var tickets = Fixture.Build<TicketEntity>()
							 .With(x => x.InsightId, insightId)
							 .With(x => x.Status, 0)
							 .With(x => x.CreatedDate, utcNow)
							 .Without(x => x.Comments)
                             .Without(x => x.JobType)
                             .Without(x => x.Diagnostics)
                             .Without(x => x.Attachments)
							 .CreateMany(3)
							 .ToList();

		var expectedActivities = new List<TicketActivityResponse>();
		var comments = new List<CommentEntity>();
		foreach (var ticket in tickets)
		{
			var comment = Fixture.Build<CommentEntity>()
							  .With(x => x.TicketId, ticket.Id)
							  .With(x => x.Ticket, ticket)
							  .With(x => x.CreatorId, userId)
							  .With(x => x.CreatedDate, utcNow)
							  .Create();
			comments.Add(comment);

			var activityCommentResponse = new TicketActivityResponse
			{
				TicketId = ticket.Id,
				ActivityDate = comment.CreatedDate,
				ActivityType = TicketActivityType.TicketComment.ToString(),
				SourceId = userId,
				SourceType = SourceType.Platform,
                TicketSummary = ticket.Summary,
				Activities = new()
				{
					new KeyValuePair<string, string>(nameof(TicketEntity.Comments),comment.Text)
				}

			};

			expectedActivities.Add(activityCommentResponse);

			var ticketActivity = new TicketActivityResponse
			{
				TicketId = ticket.Id,
				ActivityDate = ticket.CreatedDate,
				ActivityType = TicketActivityType.NewTicket.ToString(),
				SourceId = userId,
				SourceType = SourceType.Platform,
                TicketSummary = ticket.Summary,
				Activities = new()
				{
					new KeyValuePair<string, string>(nameof(TicketEntity.Status), Enum.GetName(typeof(TicketStatusEnum), ticket.Status)),
					new KeyValuePair<string, string>(nameof(TicketEntity.AssigneeId), ticket.AssigneeId.ToString()),
					new KeyValuePair<string, string>(nameof(TicketEntity.AssigneeType), ticket.AssigneeType.ToString()),
					new KeyValuePair<string, string>(nameof(TicketEntity.AssigneeName), ticket.AssigneeName),
					new KeyValuePair<string, string>(nameof(TicketEntity.Summary), ticket.Summary),
					new KeyValuePair<string, string>(nameof(TicketEntity.Description), ticket.Description),
					new KeyValuePair<string, string>(nameof(TicketEntity.DueDate), ticket.DueDate.ToString()),
					new KeyValuePair<string, string>(nameof(TicketEntity.Priority), ticket.Priority.ToString()),

				}
			};
			expectedActivities.Add(ticketActivity);

			var attachments = Fixture.Build<AttachmentEntity>()
						  .With(x => x.TicketId, ticket.Id)
						  .With(x => x.CreatedDate, utcNow)
                          .Without(x=>x.Ticket)
                          .CreateMany(2)
						  .ToList();
			ticket.Attachments = attachments;

			var ticketAttachmentActivities = attachments.Select(x => new TicketActivityResponse
			{
				TicketId = ticket.Id,
				ActivityDate = x.CreatedDate,
				ActivityType = TicketActivityType.TicketAttachment.ToString(),
				SourceId = userId,
				SourceType = SourceType.Platform,
                TicketSummary = ticket.Summary,
				Activities = new() { new KeyValuePair<string, string>(nameof(AttachmentEntity.FileName), x.FileName) }
			}
			).ToList();

			expectedActivities.AddRange(ticketAttachmentActivities);
		}

		await using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
		using var client = server.CreateClient(null, auth0UserId: userId.ToString());
		server.Arrange().SetSessionData(SourceType.Platform, userId);
		var db = server.Assert().GetDbContext<WorkflowContext>();
		db.Tickets.AddRange(tickets);
		db.SaveChanges();
		db.Comments.AddRange(comments);
		db.SaveChanges();

		var response = await client.GetAsync($"insights/{insightId}/tickets/activities");
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var result = await response.Content.ReadAsAsync<List<TicketActivityResponse>>();
		result.Should().NotBeNull();
		result.Should().HaveCount(12);
		result.Should().BeEquivalentTo(expectedActivities, config =>
		{
			config.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 30000.Seconds())).WhenTypeIs<DateTime>();
			return config;
		}); ;

	}
}

