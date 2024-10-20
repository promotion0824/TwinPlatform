using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Tickets
{
    public class GetTicketStatusTests : BaseInMemoryTest
    {
        public GetTicketStatusTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WorkgroupsExist_GetWorkgroups_ReturnWorkgroups()
        {
            var customerId = Guid.NewGuid();
            var ticketStatusEntities = Fixture.Build<TicketStatusEntity>().With(s => s.CustomerId, customerId).CreateMany();

             await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketStatuses.AddRange(ticketStatusEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"customers/{customerId}/ticketstatus");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketStatusDto>>();
                result.Should().BeEquivalentTo(TicketStatusDto.MapFromModels(TicketStatusEntity.MapToModels(ticketStatusEntities)));
            }
        }
    }
}
