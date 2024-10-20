using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Tickets;

public class GetTicketsCountsByCreatedDateTests : BaseInMemoryTest
{
    public GetTicketsCountsByCreatedDateTests(ITestOutputHelper output) : base(output)
    {
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
               .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task TokenIsNotGiven_GetTicketsCountsByCreatedDate_RequiresAuthorization()
    {
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient())
        {
            var result = await client.GetAsync($"tickets/twins/spaceTwinId/ticketCountsByDate");
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Theory]
    [InlineData("2024-01-01", null)]
    [InlineData(null, "2024-01-01")]
    [InlineData(null, null)]
    public async Task StartOrEndDateIsMissing_GetTicketsCountsByCreatedDate_ReturnBadRequest(string startDate, string endDate)
    {
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var url = $"tickets/twins/spaceTwinId/ticketCountsByDate";
            if (!string.IsNullOrEmpty(startDate))
            {
                url = QueryHelpers.AddQueryString(url, "startDate", startDate);
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                url = QueryHelpers.AddQueryString(url, "endDate", endDate);
            }
            var result = await client.GetAsync(url);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var response = await result.Content.ReadAsStringAsync();
            response.Should().Contain("The start date and end date are required");
        }
    }

    [Fact]
    public async Task StartDateGreaterThanEndDate_GetTicketsCountsByCreatedDate_ReturnBadRequest()
    {
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var url = $"tickets/twins/spaceTwinId/ticketCountsByDate";

            url = QueryHelpers.AddQueryString(url, "startDate", "2025-01-01");
            url = QueryHelpers.AddQueryString(url, "endDate", "2024-01-01");

            var result = await client.GetAsync(url);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var response = await result.Content.ReadAsStringAsync();
            response.Should().Contain("The end date must be greater than start date");
        }
    }

    [Theory]
    [MemberData(nameof(GetTestData))]
    public async Task TicketExistsWithinDateRange_GetTicketsCountsByCreatedDate_ReturnTicketCount(string startDate, string endDate, TicketCountsByDateDto expectedData)
    {

        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var spaceTwinId = "SpaceTwin";
            var ticketEntities = GetTicketEntities(spaceTwinId);
            var db = server.Arrange().CreateDbContext<WorkflowContext>();
            db.Tickets.AddRange(ticketEntities);
            db.SaveChanges();

            var url = $"tickets/twins/{spaceTwinId}/ticketCountsByDate";

            url = QueryHelpers.AddQueryString(url, "startDate", startDate);
            url = QueryHelpers.AddQueryString(url, "endDate", endDate);

            var result = await client.GetAsync(url);
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            var response = await result.Content.ReadAsAsync<TicketCountsByDateDto>();
            response.Should().BeEquivalentTo(expectedData);
        }
    }
    public static IEnumerable<object[]> GetTestData()
    {
        return new List<object[]>
        {
            new object[] { "2024-01-01","2024-12-31", new TicketCountsByDateDto { Counts =  new Dictionary<DateTime, int> {
                {new DateTime(2024,01,25), 10 },
                {new DateTime(2024,02,20), 7 },
                {new DateTime(2024,03,12), 5 },
                {new DateTime(2024,05,10), 3 },
                {new DateTime(2024,10,10), 1 },

                    }
                }
            },

             new object[] { "2024-01-01","2024-5-31", new TicketCountsByDateDto { Counts =  new Dictionary<DateTime, int> {
                {new DateTime(2024,01,25), 10 },
                {new DateTime(2024,02,20), 7 },
                {new DateTime(2024,03,12), 5 },
                {new DateTime(2024,05,10), 3 }

                    }
                }
            },

            new object[] { "2024-05-01","2024-12-31", new TicketCountsByDateDto { Counts =  new Dictionary<DateTime, int> {
                {new DateTime(2024,05,10), 3 },
                {new DateTime(2024,10,10), 1 },

                    }
                }
            },

              new object[] { "2024-11-01","2024-12-31", new TicketCountsByDateDto { Counts = [] } },


        };
    }
    private List<TicketEntity> GetTicketEntities(string spaceTwinId)
    {

        var ticketEntities = new List<TicketEntity>();

        //Create 10 tickets with created date 2024-01-25
        var ticketEntitiesWithCategory1 = Fixture.Build<TicketEntity>()
                                          .Without(x => x.Attachments)
                                          .Without(x => x.Comments)
                                          .Without(x => x.JobType)
                                          .Without(x => x.Diagnostics)
                                          .Without(x => x.Category)
                                          .With(x => x.SpaceTwinId, spaceTwinId)
                                          .With(x => x.CreatedDate, new DateTime(2024, 01, 25))
                                          .CreateMany(10)
                                          .ToList();
        //Create 7 tickets with created date 2024-02-20
        var ticketEntitiesWithCategory2 = Fixture.Build<TicketEntity>()
                                          .Without(x => x.Attachments)
                                          .Without(x => x.Comments)
                                          .Without(x => x.JobType)
                                          .Without(x => x.Diagnostics)
                                          .Without(x => x.Category)
                                          .With(x => x.SpaceTwinId, spaceTwinId)
                                          .With(x => x.CreatedDate, new DateTime(2024, 02, 20))
                                          .CreateMany(7)
                                          .ToList();
        //Create 5 tickets with created date 2024-03-12
        var ticketEntitiesWithCategory3 = Fixture.Build<TicketEntity>()
                                          .Without(x => x.Attachments)
                                          .Without(x => x.Comments)
                                          .Without(x => x.JobType)
                                          .Without(x => x.Diagnostics)
                                          .Without(x => x.Category)
                                          .With(x => x.SpaceTwinId, spaceTwinId)
                                          .With(x => x.CreatedDate, new DateTime(2024, 03, 12))
                                          .CreateMany(5)
                                          .ToList();
        // Create 3 tickets with created date 2024-05-10
        var ticketEntitiesWithCategory4 = Fixture.Build<TicketEntity>()
                                         .Without(x => x.Attachments)
                                         .Without(x => x.Comments)
                                         .Without(x => x.JobType)
                                         .Without(x => x.Diagnostics)
                                         .Without(x => x.Category)
                                         .With(x => x.SpaceTwinId, spaceTwinId)
                                         .With(x => x.CreatedDate, new DateTime(2024, 05, 10))
                                         .CreateMany(3)
                                         .ToList();
        // Create 1 ticket with created date 2024-10-10
        var ticketEntitiesWithCategory5 = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .Without(x => x.Category)
                                        .With(x => x.SpaceTwinId, spaceTwinId)
                                        .With(x => x.CreatedDate, new DateTime(2024, 10, 10))
                                        .CreateMany(1)
                                        .ToList();

        ticketEntities.AddRange(ticketEntitiesWithCategory1);
        ticketEntities.AddRange(ticketEntitiesWithCategory2);
        ticketEntities.AddRange(ticketEntitiesWithCategory3);
        ticketEntities.AddRange(ticketEntitiesWithCategory4);
        ticketEntities.AddRange(ticketEntitiesWithCategory5);

        return ticketEntities;
    }
}

