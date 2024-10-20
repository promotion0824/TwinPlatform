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

namespace WorkflowCore.Test.Features.Tickets
{
    public class GetTicketCategoriesTests : BaseInMemoryTest
    {
        public GetTicketCategoriesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_GetTicketCategories_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"sites/{Guid.NewGuid()}/tickets/categories");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task TicketCategoryExists_GetTicketCategory_ReturnsTicket()
        {
            var siteId = Guid.NewGuid();
            var ticketCategoryEntities = Fixture.Build<TicketCategoryEntity>()
                                                .With(x => x.SiteId, siteId)
                                                .Without(x => x.Tickets)
                                                .CreateMany(3);
            var expectedTicketCategoryDtos = TicketCategoryDto.MapFromModels(TicketCategoryEntity.MapToModels(ticketCategoryEntities));

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketCategories.AddRange(ticketCategoryEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/tickets/categories/");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketCategoryDto>>();
                result.Should().BeEquivalentTo(expectedTicketCategoryDtos);
            }
        }
    }
}
