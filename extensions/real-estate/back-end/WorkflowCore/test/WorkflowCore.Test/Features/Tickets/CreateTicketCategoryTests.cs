using AutoFixture;
using WorkflowCore.Entities;
using WorkflowCore.Dto;
using FluentAssertions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Models;
using WorkflowCore.Controllers.Request;
using Willow.Infrastructure;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace WorkflowCore.Test.Features.Tickets
{
    public class CreateTicketCategoryTests : BaseInMemoryTest
    {
        public CreateTicketCategoryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_CreateTicketCategory_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickets/categories", new CreateTicketCategoryRequest());
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task TicketCategoryExists_CreateTicketCategory_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var categoryName = "Category";
            var request = new CreateTicketCategoryRequest { Name = categoryName };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets/categories", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db.TicketCategories.Count().Should().Be(1);
                db.TicketCategories.First().Name.Should().Be(categoryName);
                db.TicketCategories.First().IsActive.Should().BeTrue();
                db.TicketCategories.First().LastUpdate.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromMinutes(1));

            }
        }
    }
}
