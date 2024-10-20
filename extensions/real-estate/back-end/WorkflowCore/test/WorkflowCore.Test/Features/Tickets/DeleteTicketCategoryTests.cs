using FluentAssertions;
using System;
using System.Net;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Entities;
using AutoFixture;
using System.Linq;
using Willow.Infrastructure;

namespace WorkflowCore.Test.Features.Tickets
{
    public class DeleteTicketCategoriesTests : BaseInMemoryTest
    {
        public DeleteTicketCategoriesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_DeleteTicketCategories_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"sites/{Guid.NewGuid()}/tickets/categories/{Guid.NewGuid()}");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task TicketCategoryDoesNotExist_UpdateTicketCategory_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.DeleteAsync($"sites/{Guid.NewGuid()}/tickets/categories/{Guid.NewGuid()}");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task TicketCategoryExists_DeleteTicketCategory_ReturnsNoContent()
        {
            var ticketCategoryEntity = Fixture.Build<TicketCategoryEntity>().Without(x => x.Tickets).Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketCategories.Add(ticketCategoryEntity);
                db.SaveChanges();

                var response = await client.DeleteAsync($"sites/{ticketCategoryEntity.SiteId}/tickets/categories/{ticketCategoryEntity.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                db.TicketCategories.Count().Should().Be(0);
            }
        }
    }
}
