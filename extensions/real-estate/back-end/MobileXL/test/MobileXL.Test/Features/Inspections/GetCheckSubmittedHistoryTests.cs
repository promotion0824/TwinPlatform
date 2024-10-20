using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MobileXL.Dto;
using MobileXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Inspections
{
	public class GetCheckSubmittedHistoryTests : BaseInMemoryTest
	{
		public GetCheckSubmittedHistoryTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task GetCheckSubmittedHistory_ReturnsCheckSubmittedHistory()
		{
			int count = 10;
			var siteId = Guid.NewGuid();
			var inspectionId = Guid.NewGuid();
			var checkId = Guid.NewGuid();
			var userId = Guid.NewGuid();
			var users = Fixture.Build<CustomerUser>()
							.With(x => x.Id, userId)
							.With(x => x.FirstName, "Test")
							.With(x => x.LastName, "User")
							.CreateMany(1)
							.ToList();
			var checkRecords = Fixture.Build<CheckRecord>()
									.With(x => x.CheckId, checkId)
									.With(x => x.SubmittedUserId, userId)
									.CreateMany(3)
									.ToList();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithCustomerUserPermissionOnSite(userId, Permissions.ViewSites, siteId))
			{
				server.Arrange()
					.GetWorkflowApi()
					.SetupRequest(HttpMethod.Get, $"sites/{siteId}/inspections/{inspectionId}/checks/{checkId}/submittedhistory/{count}")
					.ReturnsJson(checkRecords);
				server.Arrange()
					.GetDirectoryApi()
					.SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
					.ReturnsJson(users);
				var response = await client.GetAsync($"sites/{siteId}/inspections/{inspectionId}/checks/{checkId}/submittedhistory?count={count}");

				response.StatusCode.Should().Be(HttpStatusCode.OK);

				var result = await response.Content.ReadAsAsync<List<CheckRecordDto>>();
				var expectedCheckRecordsDto = CheckRecordDto.Map(checkRecords, server.Assert().GetImageUrlHelper());
				expectedCheckRecordsDto.Select(c =>
				{
					c.EnteredBy = users.FirstOrDefault();
					return c;
				}).ToList().OrderByDescending(x => x.SubmittedDate).Take(10);
				result.Should().BeEquivalentTo(expectedCheckRecordsDto);
			}
		}

		[Fact]
		public async Task UserDoesNotHaveCorrectPermission_GetCheckSubmittedHistory_ReturnsForbidden()
		{
			int count = 10;
			var siteId = Guid.NewGuid();
			var inspectionId = Guid.NewGuid();
			var checkId = Guid.NewGuid();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithCustomerUserDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
			{
				var response = await client.GetAsync($"sites/{siteId}/inspections/{inspectionId}/checks/{checkId}/submittedhistory?count={count}");

				response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
			}
		}
	}
}
