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
using PlatformPortalXL.Features.Insights;
using System.Net.Http.Json;
using Willow.Platform.Users;

namespace PlatformPortalXL.Test.Features.Insights.Insights
{
    public class UpdateInsightStatusV2Tests : BaseInMemoryTest
    {
        public UpdateInsightStatusV2Tests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task InsightExists_UpdateInsightStatus_ReturnsUpdatedInsight()
        {
            var siteId = Guid.NewGuid();
            var expectedInsight = Fixture.Build<Insight>()
                                         .With(x => x.LastStatus, InsightStatus.InProgress)
                                         .With(x => x.SourceType, InsightSourceType.Willow)
                                         .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetInsightApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/insights/{expectedInsight.Id}")
                    .ReturnsJson(expectedInsight);

                var response = await client.PutAsJsonAsync($"v2/sites/{siteId}/insights/{expectedInsight.Id}/status", new UpdateInsightStatusRequest { Status = expectedInsight.LastStatus });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InsightDetailDto>();
                result.Should().BeEquivalentTo(InsightDetailDto.MapFromModel(expectedInsight));
            }
        }

        [Fact]
        public async Task InsightExists_UpdateInsightStatusWithTwinIdNull_SiteAdtIsDisabled_ReturnsUpdatedInsightWithTwinId()
        {
	        var siteId = Guid.NewGuid();

	        var expectedInsight = Fixture.Build<Insight>()
		        .Without(x => x.TwinId)
		        .With(x => x.LastStatus, InsightStatus.InProgress)
				.With(x => x.SourceType, InsightSourceType.Willow)
		        .Create();
	       
	        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
	        using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
	        {
		       
		        server.Arrange().GetInsightApi()
			        .SetupRequest(HttpMethod.Put, $"sites/{siteId}/insights/{expectedInsight.Id}")
			        .ReturnsJson(expectedInsight);
		      
		        var response = await client.PutAsJsonAsync($"v2/sites/{siteId}/insights/{expectedInsight.Id}/status", new UpdateInsightStatusRequest { Status = expectedInsight.LastStatus });

		        response.StatusCode.Should().Be(HttpStatusCode.OK);
		        var result = await response.Content.ReadAsAsync<InsightDetailDto>();
		        expectedInsight.TwinId = null;
		        result.Should().BeEquivalentTo(InsightDetailDto.MapFromModel(expectedInsight));
	        }
        }

		[Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateInsightStatus_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.PutAsJsonAsync($"v2/sites/{siteId}/insights/{Guid.NewGuid()}/status", new UpdateInsightStatusRequest { Status = InsightStatus.InProgress });

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InsightExists_UpdateInsightStatusToDeleted_ReturnsUpdatedInsight()
        {
	        var siteId = Guid.NewGuid();
	        Insight expectedInsight = null;
			var expectedInsightId=Guid.NewGuid();

			var expectedUser = Fixture.Build<User>()
				.With(x => x.Email, $"{Guid.NewGuid()}@willowinc.com")
				.With(x => x.Id, Guid.NewGuid)
				.Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(expectedUser.Id, Permissions.ViewSites, siteId))
			{

				server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{expectedUser.Id}")
					.ReturnsJson(expectedUser);
				server.Arrange().GetInsightApi()
			        .SetupRequest(HttpMethod.Put, $"sites/{siteId}/insights/{expectedInsightId}")
			        .ReturnsJson(expectedInsight);

		        var response = await client.PutAsJsonAsync($"v2/sites/{siteId}/insights/{expectedInsightId}/status", new UpdateInsightStatusRequest { Status =InsightStatus.Deleted });

		        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
		       
	        }
        }
[Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateInsightStatusToDeleted_ReturnsForbidden()
        {
	        var siteId = Guid.NewGuid();
			var expectedUser = Fixture.Build<User>()
				.With(x => x.Email, $"{Guid.NewGuid()}@microsoft.com")
				.With(x => x.Id, Guid.NewGuid)
				.Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(expectedUser.Id, Permissions.ViewSites, siteId))
			{
				server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{expectedUser.Id}")
					.ReturnsJson(expectedUser);
				var response = await client.PutAsJsonAsync($"v2/sites/{siteId}/insights/{Guid.NewGuid()}/status", new UpdateInsightStatusRequest { Status = InsightStatus.Deleted });

		        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
	        }
        }
}
}
