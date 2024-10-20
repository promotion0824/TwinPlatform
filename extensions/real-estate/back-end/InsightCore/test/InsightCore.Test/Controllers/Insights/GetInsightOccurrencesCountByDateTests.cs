using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using InsightCore.Controllers.Requests;
using InsightCore.Dto;
using InsightCore.Entities;
using InsightCore.Models;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Features.Insights
{
    public class GetInsightOccurrencesCountByDateTests : BaseInMemoryTest
    {
        public GetInsightOccurrencesCountByDateTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetInsightOccurrencesCountByDateTests_TokenIsNotGiven_RequiresAuthorization()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync("insights/twin/spaceTwinId/insightOccurrencesByDate");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task GetInsightOccurrencesCountByDateTests_StartDateIsNull_ReturnsBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var result = await client.GetAsync($"insights/twin/spaceTwinId/insightOccurrencesByDate?enddate={DateTime.UtcNow.ToString("MM/dd/yyyy")}");
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }
        [Fact]
        public async Task GetInsightOccurrencesCountByDateTests_EndDateIsNull_ReturnsBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var result = await client.GetAsync($"insights/twin/spaceTwinId/insightOccurrencesByDate?startdate={DateTime.UtcNow.ToString("MM/dd/yyyy")}& enddate=");
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task GetInsightOccurrencesCountByDateTests_InvalidDateRange_ReturnsBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var result = await client.GetAsync($"insights/twin/spaceTwinId/insightOccurrencesByDate?startdate={DateTime.UtcNow.AddDays(+4).ToString("MM/dd/yyyy")}&enddate={DateTime.UtcNow.ToString("MM/dd/yyyy")}");
                result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task GetInsightOccurrencesCountByDateTests_NoDate_ReturnNotFound()
        {

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var result = await client.GetAsync($"insights/twin/spaceTwinId/insightOccurrencesByDate?startdate={DateTime.UtcNow.AddDays(-10).ToString("MM/dd/yyyy")}&enddate={DateTime.UtcNow.ToString("MM/dd/yyyy")}");
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
        [Fact]
        public async Task GetInsightOccurrencesCountByDateTests_ValidRequest_ReturnsResponse()
        {
            
            var minDate=DateTime.UtcNow.Date.AddDays(-10);
            var maxDate = DateTime.UtcNow.Date;
            var locations1= new List<InsightLocationEntity>{ new () { LocationId = "twin1" }, new () { LocationId = "twin2" }};
            var locations2 = new List<InsightLocationEntity> { new() { LocationId = "twin1" }, new() { LocationId = "spaceTwinId" } };
            var existingInsights = Fixture.Build<InsightEntity>()
                .Without(x=>x.Locations)
                .Without(x => x.ImpactScores)
                .Without(x => x.InsightOccurrences)
                .Without(x => x.StatusLogs)
                .CreateMany(6).ToList();
        

            Random rnd = new Random();
            var faulted=false;
            
            var index = 0;
            existingInsights.ForEach(i =>
               {

                  var occurrences = new List<InsightOccurrenceEntity>();
                   for (var x = 0; x < 6; x++)
                   {
                       faulted = !faulted;
                       occurrences.Add(Fixture.Build<InsightOccurrenceEntity>().With(c => c.Started,
                               minDate.AddDays(rnd.Next(-2, 12)))
                           .With(c=>c.Insight,i)
                           .With(c => c.IsFaulted, faulted)
                           .Without(c => c.Id)
                           .Without(c => c.Insight).Create());

                   }
                  i.InsightOccurrences = occurrences;
                   
                   if (index % 2 == 0)
                       i.Locations = locations1.Select(c => new InsightLocationEntity()
                       {
                           LocationId = c.LocationId,
                           InsightId = i.Id
                       }).ToList();
                   else
                   {
                       i.Locations = locations2.Select(c => new InsightLocationEntity()
                       {
                           LocationId = c.LocationId,
                           InsightId = i.Id
                       }).ToList();
                   }

                   index++;
               });

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                
                db.Insights.AddRange(existingInsights);
                db.InsightOccurrences.AddRange(existingInsights.SelectMany(c=>c.InsightOccurrences));
                db.InsightLocations.AddRange(existingInsights.SelectMany(c=>c.Locations));
                db.SaveChanges();

                var insightOccurrencesCount = db.InsightOccurrences.Where(c => c.IsFaulted && c.Started >= minDate && c.Started <= maxDate && c.Insight.Locations.Any(l => l.LocationId == "spaceTwinId")).GroupBy(c => c.Started.Date)
                    .Select(c => new InsightOccurrencesCountByDate() { Date = c.Key, Count = c.Count(), AverageDuration = c.Average(y => (y.Ended - y.Started).TotalHours) }).ToList();

                var expectedResult= new InsightOccurrencesCountByDateResponse()
                {
                    Counts = insightOccurrencesCount
                        .Select(c => new InsightOccurrencesCountDto() { Count = c.Count, Date = c.Date }).OrderBy(c=>c.Date).ToList(),
                    AverageDuration =(int) insightOccurrencesCount.Average(c => c.AverageDuration)
                };
                var response = await client.GetAsync($"insights/twin/spaceTwinId/insightOccurrencesByDate?startdate={minDate.ToString("MM/dd/yyyy")}&enddate={maxDate.ToString("MM/dd/yyyy")}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<InsightOccurrencesCountByDateResponse>();
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

         

    }
}
