using AutoFixture;
using FluentAssertions;
using InsightCore.Dto;
using InsightCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace InsightCore.Test.Controllers.Insights
{
    public class GetInsightsOccurrencesTests : BaseInMemoryTest
    {
        public GetInsightsOccurrencesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetInsightsOccurrencesTests_InsightExist_ReturnsOccurrences()
        {
	        var insightId = Guid.NewGuid();
			
			var expectedOccurrencesEntities = Fixture.Build<InsightOccurrenceEntity>()
				.With(i => i.InsightId, insightId)
				.Without(x => x.Insight)
				.CreateMany(5).ToList();

            var nonExpectedOccurrencesEntities = Fixture.Build<InsightOccurrenceEntity>()
	            .Without(x => x.Insight)
								 .CreateMany(10)
                                 .ToList();

			
			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<InsightDbContext>();
                db.InsightOccurrences.RemoveRange(db.InsightOccurrences.ToList());
                await db.InsightOccurrences.AddRangeAsync(expectedOccurrencesEntities);
                await db.InsightOccurrences.AddRangeAsync(nonExpectedOccurrencesEntities);
                db.SaveChanges();

				var expectedResponse = InsightOccurrenceDto.MapFrom(InsightOccurrenceEntity.MapTo(expectedOccurrencesEntities));
				var response = await client.GetAsync($"insights/{insightId}/occurrences");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightOccurrenceDto>>();
                result.Should().BeEquivalentTo(expectedResponse);
            }
        }

		[Fact]
		public async Task GetInsightsOccurrencesTests_InsightIdIsInvalid_ReturnsEmpty()
		{
			var insightId =Guid.NewGuid();

			var nonExpectedOccurrencesEntities = Fixture.Build<InsightOccurrenceEntity>()
				.Without(x => x.Insight)
				.CreateMany(10)
				.ToList();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<InsightDbContext>();
				db.InsightOccurrences.RemoveRange(db.InsightOccurrences.ToList());
				await db.InsightOccurrences.AddRangeAsync(nonExpectedOccurrencesEntities);
				db.SaveChanges();

				var response = await client.GetAsync($"insights/{insightId}/occurrences");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<InsightOccurrenceDto>>();
				result.Should().BeEquivalentTo(new List<InsightOccurrenceDto>());
			}
		}


	}
}
