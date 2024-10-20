using AutoFixture;
using FluentAssertions;
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

public class GetTicketCategoryCountBySpaceTwinIdTests : BaseInMemoryTest
{
    public GetTicketCategoryCountBySpaceTwinIdTests(ITestOutputHelper output) : base(output)
    {
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
               .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task TokenIsNotGiven_GetTicketCategoryCountBySpaceTwinId_RequiresAuthorization()
    {
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient())
        {
            var result = await client.GetAsync($"tickets/twins/spaceTwinId/ticketCountsByCategory");
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Theory]
    [MemberData(nameof(GetTestData))]
    public async Task TicketExists_GetTicketCategoryCountBySpaceTwinId_ReturnTicketCategoryCount(int limitCount, TicketCategoryCountDto expectedResult)
    {
        var spaceTwinId = "TestSpaceTwinId";
        var ticketCategoryEntities = GetTicketCategoryEntities();
        var ticketEntities = GetTicketEntities(spaceTwinId);



        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var db = server.Arrange().CreateDbContext<WorkflowContext>();
            db.Tickets.AddRange(ticketEntities);
            db.TicketCategories.AddRange(ticketCategoryEntities);
            db.SaveChanges();

            var response = await client.GetAsync($"tickets/twins/{spaceTwinId}/ticketCountsByCategory?limit={limitCount}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsAsync<TicketCategoryCountDto>();
            responseContent.CategoryCounts.Count.Should().Be(limitCount);
            responseContent.Should().BeEquivalentTo(expectedResult);

        }


    }
    public static IEnumerable<object[]> GetTestData()
    {
        return new List<object[]>
        {
            new object[] { 2, new TicketCategoryCountDto { CategoryCounts =  new List<CategoryCountDto> {
                new CategoryCountDto("HVAC",10),
                 new CategoryCountDto("Plumbing",7)
            },

                OtherCount = 12 } },
            new object[] { 4, new TicketCategoryCountDto { CategoryCounts =  new List<CategoryCountDto> {
                new CategoryCountDto("HVAC",10),
                new CategoryCountDto("Plumbing",7),
                new CategoryCountDto("Radio",5),
                new CategoryCountDto("Cabling",3)
            },

                OtherCount = 4 } },

             new object[] { 6, new TicketCategoryCountDto { CategoryCounts =  new List<CategoryCountDto> {
                new CategoryCountDto("HVAC",10),
                new CategoryCountDto("Plumbing",7),
                new CategoryCountDto("Radio",5),
                new CategoryCountDto("Cabling",3),
                new CategoryCountDto("Electrical",2),
                new CategoryCountDto("Unknown",2)
            },

                OtherCount = 0 } },

             new object[] { 0, new TicketCategoryCountDto { CategoryCounts =  new List<CategoryCountDto> (),
                OtherCount = 29 } },
        };
    }
    private List<TicketCategoryEntity> GetTicketCategoryEntities()
    {
        return new List<TicketCategoryEntity>
        {
            new TicketCategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = "HVAC"

            },
             new TicketCategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = "Plumbing"

            },
              new TicketCategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = "Electrical"

            },
               new TicketCategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = "Radio"

            },
                new TicketCategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = "Cabling"

            },

        };
    }

    private List<TicketEntity> GetTicketEntities(string spaceTwinId)
    {

        var ticketEntities = new List<TicketEntity>();
        var ticketCategoryEntities = GetTicketCategoryEntities();

        var hvacCategory = ticketCategoryEntities.Where(x => x.Name == "HVAC").FirstOrDefault();
        var plumbingCategory = ticketCategoryEntities.Where(x => x.Name == "Plumbing").FirstOrDefault();
        var radioCategory = ticketCategoryEntities.Where(x => x.Name == "Radio").FirstOrDefault();
        var cablingCategory = ticketCategoryEntities.Where(x => x.Name == "Cabling").FirstOrDefault();
        var electricalCategory = ticketCategoryEntities.Where(x => x.Name == "Electrical").FirstOrDefault();

        //Create 10 tickets with category HVAC
        var ticketEntitiesWithCategory1 = Fixture.Build<TicketEntity>()
                                          .Without(x => x.Attachments)
                                          .Without(x => x.Comments)
                                          .Without(x => x.JobType)
                                          .Without(x => x.Diagnostics)
                                          .With(x => x.Category, hvacCategory)
                                          .With(x => x.SpaceTwinId, spaceTwinId)
                                          .With(x => x.CategoryId, hvacCategory.Id)
                                          .CreateMany(10)
                                          .ToList();
        //Create 7 tickets with category Plumbing
        var ticketEntitiesWithCategory2 = Fixture.Build<TicketEntity>()
                                     .Without(x => x.Attachments)
                                     .Without(x => x.Comments)
                                     .Without(x => x.JobType)
                                     .Without(x => x.Diagnostics)
                                     .With(x => x.Category, plumbingCategory)
                                     .With(x => x.SpaceTwinId, spaceTwinId)
                                     .With(x => x.CategoryId, plumbingCategory.Id)
                                     .CreateMany(7)
                                      .ToList();
        //Create 5 tickets with category Radio
        var ticketEntitiesWithCategory3 = Fixture.Build<TicketEntity>()
                                     .Without(x => x.Attachments)
                                     .Without(x => x.Comments)
                                     .Without(x => x.JobType)
                                     .Without(x => x.Diagnostics)
                                     .With(x => x.Category, radioCategory)
                                     .With(x => x.SpaceTwinId, spaceTwinId)
                                     .With(x => x.CategoryId, radioCategory.Id)
                                     .CreateMany(5)
                                      .ToList();
        //Create 3 tickets with category Cabling
        var ticketEntitiesWithCategory4 = Fixture.Build<TicketEntity>()
                                     .Without(x => x.Attachments)
                                     .Without(x => x.Comments)
                                     .Without(x => x.JobType)
                                     .Without(x => x.Diagnostics)
                                     .With(x => x.Category, cablingCategory)
                                     .With(x => x.SpaceTwinId, spaceTwinId)
                                     .With(x => x.CategoryId, cablingCategory.Id)
                                     .CreateMany(3)
                                      .ToList();


        //Create 2 tickets with category Electrical
        var ticketEntitiesWithCategory5 = Fixture.Build<TicketEntity>()
                                     .Without(x => x.Attachments)
                                     .Without(x => x.Comments)
                                     .Without(x => x.JobType)
                                     .Without(x => x.Diagnostics)
                                     .With(x => x.Category, electricalCategory)
                                     .With(x => x.SpaceTwinId, spaceTwinId)
                                     .With(x => x.CategoryId, electricalCategory.Id)
                                     .CreateMany(2)
                                      .ToList();

        //Create 2 tickets without category
        var ticketEntitiesWithoutCategory = Fixture.Build<TicketEntity>()
                                     .Without(x => x.Attachments)
                                     .Without(x => x.Comments)
                                     .Without(x => x.JobType)
                                     .Without(x => x.Diagnostics)
                                     .Without(x => x.Category)
                                     .With(x => x.SpaceTwinId, spaceTwinId)
                                     .Without(x => x.CategoryId)
                                     .CreateMany(2)
                                     .ToList();

        ticketEntities.AddRange(ticketEntitiesWithCategory1);
        ticketEntities.AddRange(ticketEntitiesWithCategory2);
        ticketEntities.AddRange(ticketEntitiesWithCategory3);
        ticketEntities.AddRange(ticketEntitiesWithCategory4);
        ticketEntities.AddRange(ticketEntitiesWithCategory5);
        ticketEntities.AddRange(ticketEntitiesWithoutCategory);

        return ticketEntities;
    }

}





