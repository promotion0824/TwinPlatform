using WorkflowCore.Dto;
using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Entities;
using AutoFixture;

namespace WorkflowCore.Test.Features.Tickets
{
    public class GetTicketCategoryTests : BaseInMemoryTest
    {
        public GetTicketCategoryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_GetTicketCategory_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"sites/{Guid.NewGuid()}/tickets/categories/{Guid.NewGuid()}");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task TicketCategoryNotExists_GetTicketCategory_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var result = await client.GetAsync($"sites/{Guid.NewGuid()}/tickets/categories/{Guid.NewGuid()}");
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task TicketCategoryExists_GetTicketCategory_ReturnsTicketCategory()
        {
            var ticketCategoryEntity = Fixture.Build<TicketCategoryEntity>().Without(x => x.Tickets).Create();
            var expectedTicketCategoryDto = TicketCategoryDto.MapFromModel(TicketCategoryEntity.MapToModel(ticketCategoryEntity));

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketCategories.Add(ticketCategoryEntity);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{ticketCategoryEntity.SiteId}/tickets/categories/{ticketCategoryEntity.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketCategoryDto>();
                result.Should().BeEquivalentTo(expectedTicketCategoryDto);
            }
        }
    }
}
