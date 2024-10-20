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
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using Xunit;
using Xunit.Abstractions;
using static WorkflowCore.Controllers.Request.CreateTicketStatusRequest;

namespace WorkflowCore.Test.Features.Tickets
{
	public class CreateOrUpdateTicketStatusTests : BaseInMemoryTest
    {
        public CreateOrUpdateTicketStatusTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SequenceNumberPrefixIsNotProvided_CreateTickets_ReturnsBadRequest()
        {
            var request = Fixture.Build<CreateTicketStatusRequest>()
                                 .Without(x => x.TicketStatuses)
                                 .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"customers/{Guid.NewGuid()}/ticketstatus", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task TicketStatusNotExists_GetTicketStatus_ReturnsNewlyCreatedTicketStatus()
        {
            var customerId = Guid.NewGuid();
            var request = Fixture.Build<CreateTicketStatusRequest>()
                                 .With(x => x.TicketStatuses, Fixture.CreateMany<CreateTicketStatusRequestItem>().ToList())
                                 .Create();
            var expectedEntities = request.TicketStatuses.Select(s => new TicketStatusEntity
            {
                CustomerId = customerId,
                StatusCode = s.StatusCode,
                Status = s.Status,
                Color = s.Color,
                Tab = s.Tab
            });

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"customers/{customerId}/ticketstatus", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.TicketStatuses.Should().HaveCount(request.TicketStatuses.Count);
                db.TicketStatuses.Should().BeEquivalentTo(expectedEntities);

                var result = await response.Content.ReadAsAsync<List<TicketStatusDto>>();
                result.Should().HaveCount(request.TicketStatuses.Count);
            }
        }

        [Fact]
        public async Task TicketStatusExists_GetTicketStatus_ReturnsUpdatedTicketStatus()
        {
            var customerId = Guid.NewGuid();
            var existingTicketStatusEntities = Fixture.Build<TicketStatusEntity>().With(s => s.CustomerId, customerId).CreateMany().ToList();

            var request = Fixture.Build<CreateTicketStatusRequest>()
                                 .With(x => x.TicketStatuses, Fixture.CreateMany<CreateTicketStatusRequestItem>().ToList())
                                 .Create();
			request.TicketStatuses[0].StatusCode = existingTicketStatusEntities[0].StatusCode;
			request.TicketStatuses[1].Status = existingTicketStatusEntities[1].Status;

			var expectedEntities = new List<TicketStatusEntity>();
			foreach (var entity in existingTicketStatusEntities)
			{
				var ticketStatus = request.TicketStatuses.Find(s => s.Status == entity.Status || s.StatusCode == entity.StatusCode);
				if (ticketStatus != null)
                {
					entity.Status = ticketStatus.Status;
					entity.Tab = ticketStatus.Tab;
					entity.Color = ticketStatus.Color;
				}
				expectedEntities.Add(entity);
			}
			var newTicketStatusEntities = request.TicketStatuses
													.Where(s => !existingTicketStatusEntities.Any(ets => ets.Status == s.Status || ets.StatusCode == s.StatusCode))
													.Select(s => new TicketStatusEntity
													{
														CustomerId = customerId,
														StatusCode = s.StatusCode,
														Status = s.Status,
														Color = s.Color,
														Tab = s.Tab
													}).ToList();
			expectedEntities.AddRange(newTicketStatusEntities);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketStatuses.AddRange(existingTicketStatusEntities);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"customers/{customerId}/ticketstatus", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                db = server.Assert().GetDbContext<WorkflowContext>();
				db.TicketStatuses.Should().HaveCount(request.TicketStatuses.Count(s => !existingTicketStatusEntities.Any(es => es.Status == s.Status || es.StatusCode == s.StatusCode)) + existingTicketStatusEntities.Count);
				db.TicketStatuses.Should().BeEquivalentTo(expectedEntities);

                var result = await response.Content.ReadAsAsync<List<TicketStatusDto>>();
                result.Should().HaveCount(expectedEntities.Count);
				result.Should().BeEquivalentTo(TicketStatusDto.MapFromModels(TicketStatusEntity.MapToModels(expectedEntities)));
            }
        }
    }
}
