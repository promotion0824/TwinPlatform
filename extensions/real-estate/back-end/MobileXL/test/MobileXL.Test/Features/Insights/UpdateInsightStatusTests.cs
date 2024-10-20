using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MobileXL.Dto;
using MobileXL.Features.Insight.Requests;
using MobileXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Zones;

public class UpdateInsightStatusTests : BaseInMemoryTest
{
	public UpdateInsightStatusTests(ITestOutputHelper output) : base(output)
	{
	}

	[Theory]
	[InlineData(InsightStatus.New)]
	[InlineData(InsightStatus.Open)]
	[InlineData(InsightStatus.Ignored)]
	[InlineData(InsightStatus.InProgress)]
	[InlineData(InsightStatus.Resolved)]
	public async Task UpdateInsightStatus_ReturnsInsights(InsightStatus newStatus)
	{

		var siteId = Guid.NewGuid();
		var expectedInsight = Fixture.Build<Insight>()
			.With(x=>x.LastStatus,newStatus)
			.With(x => x.SourceType, InsightSourceType.Willow)
			.Create();

		using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
		using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, siteId))
		{
			server.Arrange().GetInsightApi()
				.SetupRequest(HttpMethod.Put, $"sites/{siteId}/insights/{expectedInsight.Id}")
				.ReturnsJson(expectedInsight);

			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{expectedInsight.Id}/status", new UpdateInsightStatusRequest { Status = expectedInsight.LastStatus });

			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<InsightDetailDto>();
			result.Should().BeEquivalentTo(InsightDetailDto.MapFromModel(expectedInsight));
		}
	}

	[Fact]
	public async Task UserDoesNotHavePermission_UpdateInsightStatus_ReturnsForbidden()
	{
		var siteId = Guid.NewGuid();
		using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
		using (var client = server.CreateClientWithCustomerUserDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
		{
			var response = await client.PutAsJsonAsync($"sites/{siteId}/insights/{Guid.NewGuid()}/status",new UpdateInsightStatusRequest
			{
				Status = InsightStatus.Ignored
			});

			response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
		}
	}
}
