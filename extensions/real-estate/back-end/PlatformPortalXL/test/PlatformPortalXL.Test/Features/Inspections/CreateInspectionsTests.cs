using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using AutoFixture;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Willow.Api.DataValidation;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Willow.Workflow;

using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Test.Features.Inspections
{
	public class CreateInspectionsTests : BaseInMemoryTest
	{
		public CreateInspectionsTests(ITestOutputHelper output) : base(output)
		{
		}

		private CreateInspectionsRequest _request = new CreateInspectionsRequest
		{
			Name = "bob",
			Frequency = 4,
			FrequencyUnit = SchedulingUnit.Hours,
			StartDate = "2021-01-02T01:01:01",
			EndDate = "2031-01-02T01:01:01",
			AssetList = new List<AssetDto> { new AssetDto { AssetId = Guid.NewGuid(), FloorCode = "F21" } },
			ZoneId = Guid.NewGuid(),
			AssignedWorkgroupId = Guid.NewGuid(),
			Checks = new List<CreateCheckRequest> { new CreateCheckRequest
														{
															Name = "fred",
															Type = CheckType.Numeric,
															DecimalPlaces = 0,
															TypeValue = "4"
														}
													  }
		};
		[Fact]
		public async Task UserDoesNotHaveCorrectPermission_CreateInspections_ReturnsForbidden()
		{
			var siteId = Guid.NewGuid();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageSites, siteId))
			{
				var response = await client.PostAsJsonAsync($"sites/{siteId}/inspections/batch-create", _request);

				response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
			}
		}
		[Fact]
		public async Task UserUnAuthorized_CreateInspections_ReturnsUnAuthorized()
		{
			var siteId = Guid.NewGuid();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClient())
			{
				var response = await client.PostAsJsonAsync($"sites/{siteId}/inspections/batch-create", _request);

				response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
			}
		}

		[Fact]
		public async Task ValidInput_CreateInspections_ReturnsCreatedInspections()
		{
			var site = Fixture.Create<Site>();
			var request = Fixture.Build<CreateInspectionsRequest>()
									.With(i => i.Frequency, 1)
									.Without(i => i.Checks)
									.With(i => i.StartDate, DateTime.Now.ToString("s"))
									.Without(i => i.EndDate)
									.Create();

			var listCheckRequest = Fixture.Build<CreateCheckRequest>()
												.With(c => c.Type, CheckType.List)
												.With(c => c.TypeValue, "Yes|No|Auto")
                                                .Without(c => c.Multiplier)
                                                .Without(c => c.DecimalPlaces)
												.Without(c => c.DependencyName)
												.Without(c => c.DependencyValue)
												.Without(c => c.PauseStartDate)
												.Create();

			var numericCheckRequest = Fixture.Build<CreateCheckRequest>()
												.With(c => c.Type, CheckType.Numeric)
												.With(c => c.DecimalPlaces, 2)
												.With(c => c.DependencyName, listCheckRequest.Name)
												.With(c => c.DependencyValue, "No")
                                                .Without(c => c.Multiplier)
                                                .Without(c => c.PauseStartDate)
												.Create();
			var dateCheckRequest = Fixture.Build<CreateCheckRequest>()
												.With(x => x.Type, CheckType.Date)
                                                .Without(c => c.Multiplier)
                                                .Without(c => c.TypeValue)
												.Without(c => c.DecimalPlaces)
												.Without(c => c.DependencyName)
												.Without(c => c.DependencyValue)
												.Without(c => c.PauseStartDate)
												.Create();
			request.Checks = new List<CreateCheckRequest> { listCheckRequest, numericCheckRequest, dateCheckRequest };
			var createdInspections = Fixture.CreateMany<Inspection>().ToList();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);
				server.Arrange().GetWorkflowApi()
					.SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{site.Id}/inspections/batch-create", request)
					.ReturnsJson(createdInspections);

				var response = await client.PostAsJsonAsync($"sites/{site.Id}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<InspectionDto>>();
				result.Should().BeEquivalentTo(InspectionDto.MapFromModels(createdInspections));
			}
		}

		[Fact]
		public async Task SiteDoesNotExist_CreateInspections_ReturnsNotFound()
		{
			var site = Fixture.Create<Site>();
			var request = Fixture.Build<CreateInspectionsRequest>()
								 .With(i => i.StartDate, DateTime.Now.ToString("s"))
								 .Without(i => i.EndDate)
								 .Create();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson((Site)null);

				var response = await client.PostAsJsonAsync($"sites/{site.Id}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.NotFound);
			}
		}

		[Fact]
		public async Task MissingFields_CreateInspections_ReturnsError()
		{
			var site = Fixture.Create<Site>();
			var request = Fixture.Build<CreateInspectionsRequest>()
				.Without(i => i.Name)
				.Without(i => i.AssetList)
				.Without(i => i.AssignedWorkgroupId)
				.Without(i => i.Frequency)
				.With(i => i.FrequencyUnit)
				.With(i => i.StartDate, "2021-02-02T02:02:02")
				.Without(i => i.EndDate)
				.Without(i => i.Checks)
				.Create();
			var firstCheckRequest = Fixture.Build<CreateCheckRequest>()
										.Without(c => c.Name)
										.Without(c => c.Type)
										.Without(c => c.TypeValue)
										.Without(c => c.DecimalPlaces)
										.Without(c => c.DependencyName)
										.Without(c => c.DependencyValue)
                                        .Without(c => c.Multiplier)
                                        .Without(c => c.PauseStartDate)
										.Create();

			var secondCheckRequest = Fixture.Build<CreateCheckRequest>()
												.With(c => c.Type, CheckType.Numeric)
												.Without(c => c.DecimalPlaces)
												.Without(c => c.PauseStartDate)
                                                .Without(c => c.Multiplier)
                                                .Create();
			request.Checks = new List<CreateCheckRequest> { firstCheckRequest, secondCheckRequest };

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);

				var response = await client.PostAsJsonAsync($"sites/{site.Id}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
				var result = await response.Content.ReadAsAsync<ValidationError>();
				var names = string.Join(';', result.Items.Select(i => i.Name));

				Assert.Contains("Name", names);
				Assert.Contains("AssetList", names);
				Assert.Contains("AssignedWorkgroupId", names);
				Assert.Contains("Frequency", names);

				Assert.Contains("Checks[1].DecimalPlaces", names);

				result.Items.Should().HaveCount(8);
			}
		}

		[Fact]
		public async Task AssetListEmpty_CreateInspections_ReturnsError()
		{
			var site = Fixture.Create<Site>();
			var request = Fixture.Build<CreateInspectionsRequest>()
				.With(i => i.AssetList, new List<AssetDto>())
				.With(i => i.FrequencyUnit)
				.With(i => i.StartDate, "2021-02-02T02:02:02")
				.Without(i => i.EndDate)
				.Without(i => i.Checks)
				.Create();
			var firstCheckRequest = Fixture.Build<CreateCheckRequest>().Create();

			var secondCheckRequest = Fixture.Build<CreateCheckRequest>().Create();
			request.Checks = new List<CreateCheckRequest> { firstCheckRequest, secondCheckRequest };

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);

				var response = await client.PostAsJsonAsync($"sites/{site.Id}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
				var result = await response.Content.ReadAsAsync<ValidationError>();
				result.Items.First().Name.Should().Be("AssetList");
				result.Items.First().Message.Should().Be("AssetList must have at least one Asset");
				result.Items.Should().HaveCount(1);
			}
		}

		[Fact]
		public async Task MissingAssetId_CreateInspections_ReturnsError()
		{
			var site = Fixture.Create<Site>();
			var request = Fixture.Build<CreateInspectionsRequest>()
				.With(i => i.AssetList, new List<AssetDto> { new AssetDto { AssetId = null, FloorCode = "FL11" } })

				.With(i => i.FrequencyUnit)
				.With(i => i.StartDate, "2021-02-02T02:02:02")
				.Without(i => i.EndDate)
				.Without(i => i.Checks)
				.Create();
			var firstCheckRequest = Fixture.Build<CreateCheckRequest>().Create();

			var secondCheckRequest = Fixture.Build<CreateCheckRequest>().Create();
			request.Checks = new List<CreateCheckRequest> { firstCheckRequest, secondCheckRequest };

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);

				var response = await client.PostAsJsonAsync($"sites/{site.Id}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
				var result = await response.Content.ReadAsAsync<ValidationError>();
				var names = string.Join(';', result.Items.Select(i => i.Name));
				var messages = string.Join(';', result.Items.Select(i => i.Message));
				Assert.Contains("AssetId", names);
				Assert.Contains("AssetId is required", messages);

				result.Items.Should().HaveCount(1);
			}
		}

		[Fact]
		public async Task MissingChecks_CreateInspections_ReturnsError()
		{
			var site = Fixture.Create<Site>();

			_request.Checks = null;
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);

				var response = await client.PostAsJsonAsync($"sites/{site.Id}/inspections/batch-create", _request);

				response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
				var result = await response.Content.ReadAsAsync<ValidationError>();
				result.Items.Should().HaveCount(1);
				result.Items[0].Message.Should().Be("Checks is required");
			}
		}

		[Fact]
		public async Task OnlyPausedChecks_CreateInspections_ReturnsCreatedInspection()
		{
			var site = Fixture.Create<Site>();
			var request = Fixture.Build<CreateInspectionsRequest>()
				.With(i => i.Frequency, 1)
				.Without(i => i.Checks)
				.With(i => i.StartDate, DateTime.Now.ToString("s"))
				.Without(i => i.EndDate)
				.Create();

			request.Checks = Fixture.Build<CreateCheckRequest>()
									.Without(c => c.DependencyName)
									.Without(c => c.DependencyValue)
                                    .Without(c => c.Multiplier)
                                    .CreateMany(3).ToList();

			var createdInspections = Fixture.CreateMany<Inspection>().ToList();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);
				server.Arrange().GetWorkflowApi()
					.SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{site.Id}/inspections/batch-create", request)
					.ReturnsJson(createdInspections);

				var response = await client.PostAsJsonAsync($"sites/{site.Id}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<InspectionDto>>();
				result.Should().BeEquivalentTo(InspectionDto.MapFromModels(createdInspections));
			}
		}

		[Fact]
		public async Task ContainDuplicateCheckNames_CreateInspections_ReturnError()
		{
			var site = Fixture.Create<Site>();
			var request = Fixture.Build<CreateInspectionsRequest>()
									.Without(i => i.Checks)
									.Without(i => i.EndDate)
									.With(i => i.StartDate, DateTime.Now.ToString("s"))
									.Create();
			var firstCheckRequest = Fixture.Build<CreateCheckRequest>()
												.With(c => c.Name, "CHECK")
												.With(c => c.Type, CheckType.List)
												.With(c => c.TypeValue, "Yes|No|Auto")
												.Without(c => c.DecimalPlaces)
												.Without(c => c.DependencyName)
												.Without(c => c.DependencyValue)
												.Without(c => c.PauseStartDate)
                                                .Without(c => c.Multiplier)
                                                .Create();

			var secondCheckRequest = Fixture.Build<CreateCheckRequest>()
												.With(c => c.Name, "check")
												.With(c => c.Type, CheckType.Numeric)
												.With(c => c.DecimalPlaces, 2)
												.With(c => c.DependencyName, firstCheckRequest.Name)
												.With(c => c.DependencyValue, "No")
												.Without(c => c.PauseStartDate)
                                                .Without(c => c.Multiplier)
                                                .Create();
			request.Checks = new List<CreateCheckRequest> { firstCheckRequest, secondCheckRequest };

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);
				server.Arrange().GetWorkflowApi()
					.SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{site.Id}/inspections/batch-create", request);

				var response = await client.PostAsJsonAsync($"sites/{site.Id}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
				var result = await response.Content.ReadAsAsync<ValidationError>();
				result.Items.Should().HaveCount(1);
				result.Items[0].Message.Should().Be("Name must be unique");
			}
		}

		[Fact]
		public async Task ContainDuplicatesInTypeValueChecks_CreateInspections_ReturnsError()
		{
			var site = Fixture.Create<Site>();
			var request = Fixture.Build<CreateInspectionsRequest>()
				.Without(i => i.EndDate)
				.With(i => i.StartDate, DateTime.Now.ToString("s"))
				.Without(i => i.Checks)
				.Create();
			var CheckRequest = Fixture.Build<CreateCheckRequest>()
												.With(c => c.Type, CheckType.List)
												.With(c => c.TypeValue, "Yes|No|Auto|yEs")
												.Without(c => c.DecimalPlaces)
												.Without(c => c.DependencyName)
												.Without(c => c.DependencyValue)
												.Without(c => c.PauseStartDate)
                                                .Without(c => c.Multiplier)
                                                .Create();
			request.Checks = new List<CreateCheckRequest> { CheckRequest };

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);

				var response = await client.PostAsJsonAsync($"sites/{site.Id}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
				var result = await response.Content.ReadAsAsync<ValidationError>();
				result.Items.Should().HaveCount(1);
				result.Items[0].Message.Should().Be("Checks[0].TypeValue contains duplicates");
			}
		}

		[Fact]
		public async Task InvalidEndDate_CreateInspections_ReturnsError()
		{
			var utcNow = DateTime.UtcNow;
			var site = Fixture.Create<Site>();
			var request = Fixture.Build<CreateInspectionsRequest>()
									.Without(i => i.Checks)
									.With(i => i.StartDate, utcNow.AddDays(1).ToString("s"))
									.With(i => i.EndDate, utcNow.ToString("s"))
									.Create();
			var CheckRequest = Fixture.Build<CreateCheckRequest>()
										.Without(c => c.PauseStartDate)
										.Without(c => c.DependencyName)
										.Without(c => c.DependencyValue)
                                        .Without(c => c.Multiplier)
                                        .Create();
			request.Checks = new List<CreateCheckRequest> { CheckRequest };

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);

				var response = await client.PostAsJsonAsync($"sites/{site.Id}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
				var result = await response.Content.ReadAsAsync<ValidationError>();
				result.Items.Should().HaveCount(1);
				result.Items[0].Message.Should().Be("EndDate cannot be before start date");
			}
		}

		[Fact]
		public async Task ValidInseptionsWithoutFlorrCode_CreateInspections_ReturnsCreatedInspections()
		{
			var site = Fixture.Create<Site>();
			var request = Fixture.Build<CreateInspectionsRequest>()
									.With(i => i.Frequency, 1)
									.Without(i => i.Checks)
									.With(i => i.StartDate, DateTime.Now.ToString("s"))
									.Without(i => i.EndDate)
									.Without(i => i.AssetList)
									.Create();

			var assetList = Fixture.Build<AssetDto>()
									.Without(x => x.FloorCode)
									.CreateMany().ToList();

			request.AssetList = assetList;
			var listCheckRequest = Fixture.Build<CreateCheckRequest>()
												.With(c => c.Type, CheckType.List)
												.With(c => c.TypeValue, "Yes|No|Auto")
												.Without(c => c.DecimalPlaces)
												.Without(c => c.DependencyName)
												.Without(c => c.DependencyValue)
												.Without(c => c.PauseStartDate)
                                                .Without(c => c.Multiplier)
                                                .Create();

			var numericCheckRequest = Fixture.Build<CreateCheckRequest>()
												.With(c => c.Type, CheckType.Numeric)
												.With(c => c.DecimalPlaces, 2)
												.With(c => c.DependencyName, listCheckRequest.Name)
												.With(c => c.DependencyValue, "No")
												.Without(c => c.PauseStartDate)
                                                .Without(c => c.Multiplier)
                                                .Create();
			var dateCheckRequest = Fixture.Build<CreateCheckRequest>()
												.With(x => x.Type, CheckType.Date)
												.Without(c => c.TypeValue)
												.Without(c => c.DecimalPlaces)
												.Without(c => c.DependencyName)
												.Without(c => c.DependencyValue)
												.Without(c => c.PauseStartDate)
                                                .Without(c => c.Multiplier)
                                                .Create();
			request.Checks = new List<CreateCheckRequest> { listCheckRequest, numericCheckRequest, dateCheckRequest };
			var createdInspections = Fixture.CreateMany<Inspection>().ToList();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);
				server.Arrange().GetWorkflowApi()
					.SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{site.Id}/inspections/batch-create", request)
					.ReturnsJson(createdInspections);

				var response = await client.PostAsJsonAsync($"sites/{site.Id}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<InspectionDto>>();
				result.Should().BeEquivalentTo(InspectionDto.MapFromModels(createdInspections));
			}
		}
        [Fact]
        public async Task InvalidCheckMultiplier_CreateInspections_ReturnsError()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateInspectionsRequest>()
                                    .With(i => i.Frequency, 1)
                                    .Without(i => i.Checks)
                                    .With(i => i.StartDate, DateTime.Now.ToString("s"))
                                    .Without(i => i.EndDate)
                                    .Create();
            var totalCheckRequest = Fixture.Build<CreateCheckRequest>()
                                                .With(c => c.Type, CheckType.Total)
                                                .With(c => c.TypeValue, "3")
                                                .With(c => c.Multiplier,0)
                                                .With(c => c.DecimalPlaces,2)
                                                .Without(c => c.DependencyName)
                                                .Without(c => c.DependencyValue)
                                                .Without(c => c.PauseStartDate)
                                                .Without(c => c.Multiplier)
                                                .Create();

            request.Checks = new List<CreateCheckRequest> { totalCheckRequest };
            var createdInspections = Fixture.CreateMany<Inspection>().ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                var response = await client.PostAsJsonAsync($"sites/{site.Id}/inspections/batch-create", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Message.Should().Be("Multiplier is invalid");
            }
        }
    }
}
