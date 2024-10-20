using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.ServicesApi.AssetApi;
using Moq.Contrib.HttpClient;
using System.Collections.Generic;
using System.Linq;


namespace PlatformPortalXL.Test.Features.Insights.Insights
{
    public class GetInsightOccurrencesTests : BaseInMemoryTest
    {
        public GetInsightOccurrencesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetInsightOccurrences_InsightIdIsValid_ReturnsOccurrences()
        {
            var siteId = Guid.NewGuid();
            var insightId = Guid.NewGuid();
			var expectedOccurrences = Fixture.Build<InsightOccurrence>()
                                        .CreateMany(10).ToList();
        

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
               
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Get, $"insights/{insightId}/occurrences")
                    .ReturnsJson(expectedOccurrences);

                var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/occurrences");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightOccurrenceDto>>();
                var insightOccurrenceDtos = InsightOccurrenceDto.MapFromModels(expectedOccurrences, insightId);
                
                result.Should().BeEquivalentTo(insightOccurrenceDtos);
            }
        }

		[Fact]
		public async Task GetInsightOccurrences_OccurrencesDoesntExist_ReturnsNoContent()
		{
			var siteId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
			List<InsightOccurrence> expectedOccurrences = null;


			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
			{

				server.Arrange().GetInsightApi()
					.SetupRequest(HttpMethod.Get, $"insights/{insightId}/occurrences")
					.ReturnsJson(expectedOccurrences);

				var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/occurrences");

				response.StatusCode.Should().Be(HttpStatusCode.NoContent);

			}
		}



		[Fact]
		public async Task UnauthorizedUser_GetInsight_ReturnUnauthorized()
		{
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClient())
			{
				var siteId = Guid.NewGuid();
				var insightId = Guid.NewGuid();
				var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/occurrences");
				response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
				
			}
		}

		 
	}
}
