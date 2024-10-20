using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Inspections
{
	public class CreateInspectionsTests : BaseInMemoryTest
	{
		public CreateInspectionsTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task TokenIsNotGiven_CreateInspections_RequiresAuthorization()
		{
			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient())
			{
				var result = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/inspections/batch-create", new CreateInspectionsRequest());
				result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
			}
		}

		[Fact]
		public async Task InvalidFrequency_CreateInspections_ReturnsBadRequest()
		{
			var siteId = Guid.NewGuid();
			var request = Fixture.Build<CreateInspectionsRequest>()
									.With(i => i.Frequency, -1)
									.With(i => i.FrequencyUnit, SchedulingUnit.Hours)
									.Create();

			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var response = await client.PostAsJsonAsync($"sites/{siteId}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
				var error = await response.Content.ReadAsErrorResponseAsync();
				Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("Frequency", out _));
			}
		}

		[Fact]
		public async Task InvalidDependency_CreateInspections_ReturnsBadRequest()
		{
			var siteId = Guid.NewGuid();
			var request = Fixture.Build<CreateInspectionsRequest>()
									.With(i => i.Frequency, 8)
									.With(i => i.FrequencyUnit, SchedulingUnit.Hours)
									.Without(i => i.Checks)
									.Create();
			var firstCheckRequest = Fixture.Build<CheckRequest>()
										.Without(c => c.Name)
										.Without(c => c.Type)
										.Without(c => c.TypeValue)
										.Without(c => c.DecimalPlaces)
										.Without(c => c.DependencyName)
										.Without(c => c.DependencyValue)
										.Create();

			var secondCheckRequest = Fixture.Build<CheckRequest>()
												.With(c => c.Type, CheckType.Numeric)
												.Without(c => c.DecimalPlaces)
												.Create();

			var thirdCheckRequest = Fixture.Build<CheckRequest>()
									.With(c => c.Type, CheckType.Date)
									.Without(c => c.DecimalPlaces)
									.Without(c => c.DependencyName)
									.Without(c => c.DependencyValue)
									.Create();

			request.Checks = new List<CheckRequest> { firstCheckRequest, secondCheckRequest, thirdCheckRequest };

			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var response = await client.PostAsJsonAsync($"sites/{siteId}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
				var error = await response.Content.ReadAsErrorResponseAsync();
				Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("DependencyName", out _));
			}
		}

		[Theory]
		[InlineData("")]
		[InlineData(null)]
		[InlineData("TwinId_MN")]
		public async Task ValidInput_CreateInspections_ReturnsCreatedInspections(string requestedTwinId)
		{
			var siteId = Guid.NewGuid();
			var request = Fixture.Build<CreateInspectionsRequest>()
									.With(i => i.Frequency, 8)
									.With(i => i.FrequencyUnit, SchedulingUnit.Hours)
									.Without(i => i.Checks)
									.Without(i => i.AssetList)
									.Create();

			var assetList = Fixture.Build<AssetDto>().With(c=>c.TwinId,requestedTwinId).CreateMany(3).ToList();

			var firstCheckRequest = Fixture.Build<CheckRequest>()
												.With(c => c.Type, CheckType.List)
												.With(c => c.TypeValue, "Yes|No|Auto")
												.Without(c => c.DecimalPlaces)
												.Without(c => c.DependencyName)
												.Without(c => c.DependencyValue)
												.Create();

			var secondCheckRequest = Fixture.Build<CheckRequest>()
												.With(c => c.Type, CheckType.Numeric)
												.With(c => c.DecimalPlaces, 2)
												.With(c => c.DependencyName, firstCheckRequest.Name)
												.With(c => c.DependencyValue, "No")
												.Create();

			var thirdCheckRequest = Fixture.Build<CheckRequest>()
												.With(c => c.Type, CheckType.Date)
												.Without(c => c.DecimalPlaces)
												.Without(c => c.DependencyName)
												.Without(c => c.DependencyValue)
												.Create();

			request.Checks = new List<CheckRequest> { firstCheckRequest, secondCheckRequest, thirdCheckRequest };
			request.AssetList = assetList;
			var assetTwinIds = assetList.Select(c => new TwinIdDto
			{
				Id = $"TwinId_{c.AssetId}",
				UniqueId = c.AssetId.ToString()
			});
			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				server.Arrange().GetDigitalTwinApi()
					.SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/byUniqueId/batch?uniqueIds={assetList[0].AssetId}&uniqueIds={assetList[1].AssetId}&uniqueIds={assetList[2].AssetId}")
					.ReturnsJson(assetTwinIds);
				var response = await client.PostAsJsonAsync($"sites/{siteId}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var inspectionResult = await response.Content.ReadAsAsync<List<InspectionDto>>();
				var db = server.Assert().GetDbContext<WorkflowContext>();
				db.Inspections.Should().HaveCount(3);
				db.Checks.Should().HaveCount(9);

				foreach (var asset in assetList)
				{
					var result = inspectionResult.First(i => i.AssetId == asset.AssetId);
					var createdInspection = db.Inspections.First(i => i.AssetId == asset.AssetId);
					createdInspection.Id.Should().Be(result.Id);
					createdInspection.SiteId.Should().Be(siteId);
					createdInspection.Name.Should().Be(request.Name);
					createdInspection.FloorCode.Should().Be(asset.FloorCode);
					createdInspection.ZoneId.Should().Be(request.ZoneId);
					createdInspection.AssetId.Should().Be(asset.AssetId);
					createdInspection.TwinId.Should().Be(string.IsNullOrWhiteSpace(requestedTwinId)?  assetTwinIds.First(c => c.UniqueId == asset.AssetId.ToString()).Id:requestedTwinId);
					createdInspection.AssignedWorkgroupId.Should().Be(request.AssignedWorkgroupId);
					createdInspection.Frequency.Should().Be(request.Frequency);
					createdInspection.FrequencyUnit.Should().Be(request.FrequencyUnit);
					createdInspection.StartDate.Should().Be(request.StartDate);
					createdInspection.EndDate.Should().Be(request.EndDate);

					var checks = db.Checks.Where(x => x.InspectionId == result.Id).OrderBy(x => x.SortOrder).ToList();

					var firstCreatedCheck = checks.First(x => x.InspectionId == result.Id);
					firstCreatedCheck.Id.Should().Be(result.Checks.First().Id);
					firstCreatedCheck.InspectionId.Should().Be(result.Id);
					firstCreatedCheck.SortOrder.Should().Be(1);
					firstCreatedCheck.Name.Should().Be(firstCheckRequest.Name);
					firstCreatedCheck.Type.Should().Be(firstCheckRequest.Type);
					firstCreatedCheck.DecimalPlaces.Should().Be(0);
					firstCreatedCheck.MinValue.Should().Be(firstCheckRequest.MinValue);
					firstCreatedCheck.MaxValue.Should().Be(firstCheckRequest.MaxValue);
					firstCreatedCheck.DependencyId.Should().BeNull();
					firstCreatedCheck.DependencyValue.Should().Be(firstCheckRequest.DependencyValue);
					firstCreatedCheck.PauseStartDate.Should().Be(firstCheckRequest.PauseStartDate);
					firstCreatedCheck.PauseEndDate.Should().Be(firstCheckRequest.PauseEndDate);
					firstCreatedCheck.CanGenerateInsight.Should().Be(firstCheckRequest.CanGenerateInsight);

					var secondCreatedCheck = checks.Skip(1).First();
					secondCreatedCheck.Id.Should().Be(result.Checks.Skip(1).First().Id);
					secondCreatedCheck.InspectionId.Should().Be(result.Id);
					secondCreatedCheck.SortOrder.Should().Be(2);
					secondCreatedCheck.Name.Should().Be(secondCheckRequest.Name);
					secondCreatedCheck.Type.Should().Be(secondCheckRequest.Type);
					secondCreatedCheck.DecimalPlaces.Should().Be(2);
					secondCreatedCheck.MinValue.Should().Be(secondCheckRequest.MinValue);
					secondCreatedCheck.MaxValue.Should().Be(secondCheckRequest.MaxValue);
					secondCreatedCheck.DependencyId.Should().Be(firstCreatedCheck.Id);
					secondCreatedCheck.DependencyValue.Should().Be(secondCheckRequest.DependencyValue);
					secondCreatedCheck.PauseStartDate.Should().Be(secondCheckRequest.PauseStartDate);
					secondCreatedCheck.PauseEndDate.Should().Be(secondCheckRequest.PauseEndDate);
					secondCreatedCheck.CanGenerateInsight.Should().Be(secondCheckRequest.CanGenerateInsight);

					var thirdCreatedCheck = checks.Last();
					thirdCreatedCheck.Id.Should().Be(result.Checks.Last().Id);
					thirdCreatedCheck.InspectionId.Should().Be(result.Id);
					thirdCreatedCheck.SortOrder.Should().Be(3);
					thirdCreatedCheck.Name.Should().Be(thirdCheckRequest.Name);
					thirdCreatedCheck.Type.Should().Be(thirdCheckRequest.Type);
					var expectedInspection = InspectionEntity.MapToModel(createdInspection); ;
					expectedInspection.Checks = CheckEntity.MapToModels(checks.ToList());
					var expectedInspectionDto = InspectionDto.MapFromModel(expectedInspection);
					result.Should().BeEquivalentTo(expectedInspectionDto);
				}


			}
		}


		[Fact]
		public async Task AssetListEmpty_CreateInspections_ReturnsBadRequest()
		{
			var siteId = Guid.NewGuid();
			var request = Fixture.Build<CreateInspectionsRequest>()
									.With(i => i.Frequency, 8)
									.With(i => i.FrequencyUnit, SchedulingUnit.Hours)
									.Without(i => i.Checks)
									.With(i => i.AssetList, new List<AssetDto>())
									.Create();

			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var response = await client.PostAsJsonAsync($"sites/{siteId}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
				var error = await response.Content.ReadAsStringAsync();
				error.Should().Contain("Asset list is null or empty");
			}
		}

		[Fact]
		public async Task AssetListNull_CreateInspections_ReturnsBadRequest()
		{
			var siteId = Guid.NewGuid();
			var request = Fixture.Build<CreateInspectionsRequest>()
									.With(i => i.Frequency, 8)
									.With(i => i.FrequencyUnit, SchedulingUnit.Hours)
									.Without(i => i.Checks)
									.Without(i => i.AssetList)
									.Create();

			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var response = await client.PostAsJsonAsync($"sites/{siteId}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
				var error = await response.Content.ReadAsStringAsync();
				error.Should().Contain("Asset list is null or empty");
			}
		}

		[Theory]
		[InlineData("")]
		[InlineData(null)]
		[InlineData("TwinId_MN")]
		public async Task InspectionsWithoutFloorCode_CreateInspections_ReturnsCreatedInspections(string requestTwinId)
		{
			var siteId = Guid.NewGuid();
			var request = Fixture.Build<CreateInspectionsRequest>()
									.With(i => i.Frequency, 8)
									.With(i => i.FrequencyUnit, SchedulingUnit.Hours)
									.Without(i => i.Checks)
									.Without(i => i.AssetList)
									.Create();

			var assetList = Fixture.Build<AssetDto>()
								   .With(x=>x.TwinId, requestTwinId)
				                   .Without(x => x.FloorCode)	
								   .CreateMany(3).ToList();

			var firstCheckRequest = Fixture.Build<CheckRequest>()
												.With(c => c.Type, CheckType.List)
												.With(c => c.TypeValue, "Yes|No|Auto")
												.Without(c => c.DecimalPlaces)
												.Without(c => c.DependencyName)
												.Without(c => c.DependencyValue)
												.Create();

			var secondCheckRequest = Fixture.Build<CheckRequest>()
												.With(c => c.Type, CheckType.Numeric)
												.With(c => c.DecimalPlaces, 2)
												.With(c => c.DependencyName, firstCheckRequest.Name)
												.With(c => c.DependencyValue, "No")
												.Create();

			var thirdCheckRequest = Fixture.Build<CheckRequest>()
												.With(c => c.Type, CheckType.Date)
												.Without(c => c.DecimalPlaces)
												.Without(c => c.DependencyName)
												.Without(c => c.DependencyValue)
												.Create();

			request.Checks = new List<CheckRequest> { firstCheckRequest, secondCheckRequest, thirdCheckRequest };
			request.AssetList = assetList;
			
			var assetTwinIds = assetList.Select(c => new TwinIdDto
			{
				Id = $"TwinId_{c.AssetId}",
				UniqueId = c.AssetId.ToString()
			});
			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{

				server.Arrange().GetDigitalTwinApi()
					.SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/byUniqueId/batch?uniqueIds={assetList[0].AssetId}&uniqueIds={assetList[1].AssetId}&uniqueIds={assetList[2].AssetId}")
					.ReturnsJson(assetTwinIds);
				var response = await client.PostAsJsonAsync($"sites/{siteId}/inspections/batch-create", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var inspectionResult = await response.Content.ReadAsAsync<List<InspectionDto>>();
				var db = server.Assert().GetDbContext<WorkflowContext>();
				db.Inspections.Should().HaveCount(3);
				db.Checks.Should().HaveCount(9);

				foreach (var asset in assetList)
				{
					var result = inspectionResult.First(i => i.AssetId == asset.AssetId);
					var createdInspection = db.Inspections.First(i => i.AssetId == asset.AssetId);
					createdInspection.Id.Should().Be(result.Id);
					createdInspection.SiteId.Should().Be(siteId);
					createdInspection.Name.Should().Be(request.Name);
					createdInspection.FloorCode.Should().Be(string.Empty);
					createdInspection.ZoneId.Should().Be(request.ZoneId);
					createdInspection.AssetId.Should().Be(asset.AssetId);
					createdInspection.TwinId.Should().Be(string.IsNullOrWhiteSpace(requestTwinId)?  assetTwinIds.First(c=>c.UniqueId==asset.AssetId.ToString()).Id:requestTwinId);
					createdInspection.AssignedWorkgroupId.Should().Be(request.AssignedWorkgroupId);
					createdInspection.Frequency.Should().Be(request.Frequency);
					createdInspection.FrequencyUnit.Should().Be(request.FrequencyUnit);
					createdInspection.StartDate.Should().Be(request.StartDate);
					createdInspection.EndDate.Should().Be(request.EndDate);

					var checks = db.Checks.Where(x => x.InspectionId == result.Id).OrderBy(x => x.SortOrder).ToList();

					var firstCreatedCheck = checks.First(x => x.InspectionId == result.Id);
					firstCreatedCheck.Id.Should().Be(result.Checks.First().Id);
					firstCreatedCheck.InspectionId.Should().Be(result.Id);
					firstCreatedCheck.SortOrder.Should().Be(1);
					firstCreatedCheck.Name.Should().Be(firstCheckRequest.Name);
					firstCreatedCheck.Type.Should().Be(firstCheckRequest.Type);
					firstCreatedCheck.DecimalPlaces.Should().Be(0);
					firstCreatedCheck.MinValue.Should().Be(firstCheckRequest.MinValue);
					firstCreatedCheck.MaxValue.Should().Be(firstCheckRequest.MaxValue);
					firstCreatedCheck.DependencyId.Should().BeNull();
					firstCreatedCheck.DependencyValue.Should().Be(firstCheckRequest.DependencyValue);
					firstCreatedCheck.PauseStartDate.Should().Be(firstCheckRequest.PauseStartDate);
					firstCreatedCheck.PauseEndDate.Should().Be(firstCheckRequest.PauseEndDate);
					firstCreatedCheck.CanGenerateInsight.Should().Be(firstCheckRequest.CanGenerateInsight);

					var secondCreatedCheck = checks.Skip(1).First();
					secondCreatedCheck.Id.Should().Be(result.Checks.Skip(1).First().Id);
					secondCreatedCheck.InspectionId.Should().Be(result.Id);
					secondCreatedCheck.SortOrder.Should().Be(2);
					secondCreatedCheck.Name.Should().Be(secondCheckRequest.Name);
					secondCreatedCheck.Type.Should().Be(secondCheckRequest.Type);
					secondCreatedCheck.DecimalPlaces.Should().Be(2);
					secondCreatedCheck.MinValue.Should().Be(secondCheckRequest.MinValue);
					secondCreatedCheck.MaxValue.Should().Be(secondCheckRequest.MaxValue);
					secondCreatedCheck.DependencyId.Should().Be(firstCreatedCheck.Id);
					secondCreatedCheck.DependencyValue.Should().Be(secondCheckRequest.DependencyValue);
					secondCreatedCheck.PauseStartDate.Should().Be(secondCheckRequest.PauseStartDate);
					secondCreatedCheck.PauseEndDate.Should().Be(secondCheckRequest.PauseEndDate);
					secondCreatedCheck.CanGenerateInsight.Should().Be(secondCheckRequest.CanGenerateInsight);

					var thirdCreatedCheck = checks.Last();
					thirdCreatedCheck.Id.Should().Be(result.Checks.Last().Id);
					thirdCreatedCheck.InspectionId.Should().Be(result.Id);
					thirdCreatedCheck.SortOrder.Should().Be(3);
					thirdCreatedCheck.Name.Should().Be(thirdCheckRequest.Name);
					thirdCreatedCheck.Type.Should().Be(thirdCheckRequest.Type);
					var expectedInspection = InspectionEntity.MapToModel(createdInspection); ;
					expectedInspection.Checks = CheckEntity.MapToModels(checks.ToList());
					var expectedInspectionDto = InspectionDto.MapFromModel(expectedInspection);
					result.Should().BeEquivalentTo(expectedInspectionDto);
				}


			}
		}

	}
}
