using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Entities;
using Xunit;
using Xunit.Abstractions;


namespace WorkflowCore.Test.Features.Tickets
{
    public class UpdateTicketCategoryTests : BaseInMemoryTest
    {
        public UpdateTicketCategoryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_UpdateTicketCategory_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var response = await client.PutAsJsonAsync($"sites/{Guid.NewGuid()}/tickets/categories/{Guid.NewGuid()}", new UpdateTicketRequest());
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task TicketCategoryDoesNotExist_UpdateTicketCategory_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"sites/{Guid.NewGuid()}/tickets/categories/{Guid.NewGuid()}", new UpdateTicketRequest());
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task TicketCategoryExists_UpdateTicketCategory_ReturnsNoContent()
        {
            var newCategoryName = "Updated Name";
            var ticketCategoryEntity = Fixture.Build<TicketCategoryEntity>()
                .Without(x => x.Tickets)
                .Without(x => x.LastUpdate)
                .Without(x => x.IsActive)
                .Create();
            var request = new UpdateTicketCategoryRequest { Name = newCategoryName };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketCategories.Add(ticketCategoryEntity);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{ticketCategoryEntity.SiteId}/tickets/categories/{ticketCategoryEntity.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                db.TicketCategories.First().Name.Should().Be(newCategoryName);
                db.TicketCategories.First().IsActive.Should().BeTrue();
                db.TicketCategories.First().LastUpdate.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromMinutes(1));

            }
        }
    }
}
