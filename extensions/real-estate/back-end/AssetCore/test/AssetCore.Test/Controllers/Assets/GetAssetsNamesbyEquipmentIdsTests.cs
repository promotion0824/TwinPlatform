using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AssetCore.TwinCreatorAsset.Dto;
using AssetCoreTwinCreator.Domain;
using AssetCoreTwinCreator.Domain.Models;
using AssetCoreTwinCreator.MappingId;
using AssetCoreTwinCreator.MappingId.Models;
using AutoFixture;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace AssetCore.Test.Controllers.Assets
{
	public class GetAssetsNamesbyEquipmentIdsTests : BaseInMemoryTest
	{
		public GetAssetsNamesbyEquipmentIdsTests(ITestOutputHelper output) : base(output)
		{

		}

		[Fact]
		public async Task AssetNameExists_GetAssetsNamesbyEquipmentIds_ReturnAssetNames()
		{
			var customAssetIdsArr = new int[] { 12345, 12346, 12347 };
			var customAssetGuidArr = new Guid[] {
													Guid.Parse("006" + $"{customAssetIdsArr[0]:D29}"),
													Guid.Parse("006" + $"{customAssetIdsArr[1]:D29}"),
													Guid.Parse("006" + $"{customAssetIdsArr[2]:D29}")
												};

			var equibmentsIds = Fixture.CreateMany<Guid>(3)
									   .ToList();


			var building = Fixture.Build<Building>()
									.With(b => b.Enabled, true)
									.Without(b => b.Floors)
									.Create();

			var parentCategory = Fixture.Build<Category>()
				   .With(c => c.BuildingId, building.Id)
				   .With(c => c.Archived, false)
				   .With(c => c.Name, "Base Building")
				   .With(c => c.ParentId, (int?)null)
				   .With(c => c.DbTableName, (string)null)
				   .Without(b => b.Building)
				   .Without(b => b.ParentCategory)
				   .Without(b => b.ChildCategories)
				   .Without(b => b.CategoryGroups)
				   .Without(b => b.CategoryColumns)
				   .Create();
			var categories = new List<Category>() {
				parentCategory,
			};
			var assets = new List<Asset>();
			for (int i = 1; i < 4; i++)
			{
				assets.Add(Fixture.Build<Asset>()
								   .With(x => x.Id, i)
									.With(x => x.BuildingId, building.Id)
									.With(a => a.CategoryId, parentCategory.Id)
									.Without(x => x.Category)
									.Without(x => x.CategoryId)
									.Create());
			}



			var assetEquipmentMappings = new List<AssetEquipmentMapping>();
			var expectedResult = new List<EquipmentNameMappingDto>();
			for (int i = 0; i < 3; i++)
			{
				var assetEquipmentMapping = Fixture.Build<AssetEquipmentMapping>()
			   .With(x => x.AssetRegisterId, assets[i].Id)
			   .With(x => x.EquipmentId, equibmentsIds[i])
			   .Create();

				assetEquipmentMappings.Add(assetEquipmentMapping);
				expectedResult.Add(new EquipmentNameMappingDto { EquipmentId = equibmentsIds[i], Name = assets[i].Name });

			}


			for (int i = 0; i < 3; i++)
			{
				var asset = Fixture.Build<Asset>()
									 .With(x => x.Id, customAssetIdsArr[i])
									 .With(x => x.BuildingId, building.Id)
									 .With(a => a.CategoryId, parentCategory.Id)
									 .Without(x => x.Category)
									 .Without(x => x.CategoryId)
									 .Create();
				assets.Add(asset);

				expectedResult.Add(new EquipmentNameMappingDto { EquipmentId = customAssetGuidArr[i], Name = asset.Name });
			}




			using (var server = CreateServerFixture(ServerFixtureConfigurations.SqliteInMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var arrangement = server.Arrange();

				using (var context = arrangement.CreateDbContext<AssetDbContext>())
				{
					await context.Database.EnsureCreatedAsync();
				}
				using (var context = arrangement.CreateDbContext<AssetDbContext>())
				{
					context.Categories.AddRange(categories);
					context.Buildings.Add(building);
					context.Assets.AddRange(assets);
					await context.SaveChangesAsync();
				}
				using (var context = arrangement.CreateDbContext<MappingDbContext>())
				{
					await context.Database.EnsureCreatedAsync();
				}

				using (var context = arrangement.CreateDbContext<MappingDbContext>())
				{
					context.AssetEquipmentMappings.AddRange(assetEquipmentMappings);

					await context.SaveChangesAsync();
				}

				var url = "/api/sites/assets/names";
				var postContent = new List<Guid>();
				foreach (var id in equibmentsIds)
				{
					postContent.Add(id);
				}
				foreach (var customGuid in customAssetGuidArr)
				{
					postContent.Add(customGuid);
				}
				var duplicateGuid = equibmentsIds.FirstOrDefault();
				var duplicateCustomGuid = customAssetGuidArr[0];
				postContent.Add(duplicateGuid);
				postContent.Add(duplicateCustomGuid);

				var response = await client.PostAsJsonAsync(url, postContent);
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<EquipmentNameMappingDto>>();
				result.Should().BeEquivalentTo(expectedResult);

			}
		}

		[Fact]
		public async Task EmptyEquipmentIds_GetAssetsNamesbyEquipmentIds_ReturnEmptyList()
		{
			using (var server = CreateServerFixture(ServerFixtureConfigurations.SqliteInMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var url = "/api/sites/assets/names";
				var response = await client.PostAsJsonAsync(url, new List<Guid>());
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<EquipmentNameMappingDto>>();
				result.Should().HaveCount(0);
			}
		}

		[Fact]
		public async Task InvalidEquipmentIds_GetAssetsNamesbyEquipmentIds_ReturnEmptyList()
		{
			using (var server = CreateServerFixture(ServerFixtureConfigurations.SqliteInMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var arrangement = server.Arrange();
				using (var context = arrangement.CreateDbContext<AssetDbContext>())
				{
					await context.Database.EnsureCreatedAsync();
				}
				using (var context = arrangement.CreateDbContext<MappingDbContext>())
				{
					await context.Database.EnsureCreatedAsync();
				}

				var url = "/api/sites/assets/names";
				var invalidEquipmentIds = new List<object> {
					12345,
					"text",
					null,
					Guid.NewGuid()
				};
				var response = await client.PostAsJsonAsync(url, invalidEquipmentIds);
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<EquipmentNameMappingDto>>();
				result.Should().HaveCount(0);
			}
		}

		[Fact]
		public async Task UnauthorizedUser_GetAssetsNamesbyEquipmentIds_ReturnUnAuthorized()
		{
			using (var server = CreateServerFixture(ServerFixtureConfigurations.SqliteInMemoryDb))
			using (var client = server.CreateClient())
			{
				var url = "/api/sites/assets/names";
				var response = await client.PostAsJsonAsync(url, new List<Guid> { Guid.NewGuid() });
				response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
			}
		}

	}
}
