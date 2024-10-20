using AssetCore.TwinCreatorAsset.Dto;
using AssetCoreTwinCreator.Domain;
using AssetCoreTwinCreator.Domain.Models;
using AssetCoreTwinCreator.MappingId;
using AssetCoreTwinCreator.MappingId.Extensions;
using AssetCoreTwinCreator.MappingId.Models;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace AssetCore.Test.Controllers.Categories
{
    public class AttachmentsController : BaseInMemoryTest
    {
        public AttachmentsController(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task AssetTree_GetAssetTree_BaseBuildingAssetRegister_ReturnsCategoriesAndAssets()
        {
            var siteId = Guid.NewGuid();
            var dbTableName = "TecInv567Asset10304";

            var building = Fixture.Build<Building>()
                .With(b => b.Enabled, true)
                .Without(b => b.Floors)
                .Create();

            var siteMapping = new SiteMapping() { BuildingId = building.Id, SiteId = siteId };

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

            var childCategory = Fixture.Build<Category>()
               .With(c => c.BuildingId, building.Id)
               .With(c => c.Archived, false)
               .With(c => c.Name, "Asset Register")
               .With(c => c.ParentId, parentCategory.Id)
               .With(c => c.DbTableName, (string)null)
               .Without(b => b.Building)
               .Without(b => b.ParentCategory)
               .Without(b => b.ChildCategories)
               .Without(b => b.CategoryGroups)
               .Without(b => b.CategoryColumns)
               .Create();

            var grandChildCategory = Fixture.Build<Category>()
               .With(c => c.BuildingId, building.Id)
               .With(c => c.Archived, false)
               .With(c => c.ParentId, childCategory.Id)
               .With(c => c.DbTableName, dbTableName)
               .Without(b => b.Building)
               .Without(b => b.ParentCategory)
               .Without(b => b.ChildCategories)
               .Without(b => b.CategoryGroups)
               .Without(b => b.CategoryColumns)
               .Create();

            var categories = new List<Category>() {
                parentCategory,
                childCategory,
                grandChildCategory
            };

            var categoryModuleTypeMappings = categories.Select(c => Fixture.Build<AssetCategoryExtensionEntity>()
                                                                            .With(e => e.SiteId, siteId)
                                                                            .With(e => e.CategoryId, Guid.Parse("003" + $"{c.Id:D29}"))
                                                                            .Create()).ToList();

            var categoryColumns = new List<CategoryColumn>() {
                Fixture.Build<CategoryColumn>().With(c => c.CategoryId, grandChildCategory.Id).With(c => c.DbColumnName, "Type").Create(),
                Fixture.Build<CategoryColumn>().With(c => c.CategoryId, grandChildCategory.Id).With(c => c.DbColumnName, "WarrantyDurationParts").Create(),
                Fixture.Build<CategoryColumn>().With(c => c.CategoryId, grandChildCategory.Id).With(c => c.DbColumnName, "InstallationDate").Create()
            };

            var assets = Fixture.Build<Asset>()
               .With(a => a.BuildingId, building.Id)
               .With(a => a.CategoryId, grandChildCategory.Id)
               .With(a => a.FloorCode, "")
               .With(a => a.Archived, false)
               .Without(a => a.Category)
               .CreateMany(100)
               .ToList();

            var assetEquipmentMappings = assets.Select(a => Fixture.Build<AssetEquipmentMapping>().With(aeq => aeq.AssetRegisterId, a.Id).Create()).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.SqliteInMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();

                #region Seed Databases
                using (var context = arrangement.CreateDbContext<AssetDbContext>())
                {
                    await context.Database.EnsureCreatedAsync();
                }

                using (var context = arrangement.CreateDbContext<AssetDbContext>())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        context.Buildings.Add(building);
                        context.Categories.AddRange(categories);
                        context.CategoryColumns.AddRange(categoryColumns);
                        context.Assets.AddRange(assets);
                        await context.SaveChangesAsync();

                        var queryBuilder = new StringBuilder();
                        queryBuilder.AppendLine($@"
                            CREATE TABLE [{dbTableName}](
	                        [Id] [int] NOT NULL,
	                        [AssetRegisterId] [int] NOT NULL,
	                        [Type] [nvarchar] NULL,
	                        [WarrantyDurationParts] [int] NULL,
	                        [InstallationDate] [date] NULL);

                            INSERT INTO [{dbTableName}]
                            VALUES");

                        for (int i = 0; i < assets.Count; i++)
                        {
                            if (i != assets.Count - 1)
                            {
                                queryBuilder.AppendLine($"({i + 1}, {assets[i].Id}, 'f0ac388c-ae86-4a90-854b-ac6e4a02ba48-0020db10', NULL, NULL),");
                            }
                            else
                            {
                                queryBuilder.AppendLine($"({i + 1}, {assets[i].Id}, 'f0ac388c-ae86-4a90-854b-ac6e4a02ba48-0020db10', NULL, NULL);");
                            }
                        }

                        var query = queryBuilder.ToString();
                        await context.Database.ExecuteSqlRawAsync(query);

                        await transaction.CommitAsync();
                    }
                }

                using (var context = arrangement.CreateDbContext<MappingDbContext>())
                {
                    await context.Database.EnsureCreatedAsync();
                }

                using (var context = arrangement.CreateDbContext<MappingDbContext>())
                {
                    context.SiteMappings.Add(siteMapping);
                    context.AssetEquipmentMappings.AddRange(assetEquipmentMappings);
                    context.AssetCategoryExtensions.AddRange(categoryModuleTypeMappings);
                    await context.SaveChangesAsync();
                }
                #endregion

                var response = await client.GetAsync($"api/sites/{siteId}/assetTree");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var assetTreeRootCategories = await response.Content.ReadAsAsync<List<AssetTreeCategoryDto>>();

                //Assert
                void CheckCategories(List<AssetTreeCategoryDto> assetTreeCategoryDtos)
                {
                    foreach (var category in assetTreeCategoryDtos)
                    {
                        Assert.Equal(categories.Count(c => c.ParentId == category.Id.ToCategoryId() && c.Archived == false), category.Categories.Count);
                        Assert.Equal(assets.Count(a => (a.CategoryId == category.Id.ToCategoryId()) && a.Archived == false), category.Assets.Count);
                        Assert.Equal(categoryModuleTypeMappings.First(m => m.SiteId == siteId && m.CategoryId == category.Id).ModuleTypeNamePath, category.ModuleTypeNamePath);

                        CheckCategories(category.Categories);
                    }
                }

                CheckCategories(assetTreeRootCategories);
            }
        }

        [Fact]
        public async Task AssetTree_GetAssetTree_AssetRegister_ReturnsCategoriesAndAssets()
        {
            var siteId = Guid.NewGuid();
            var dbTableName = "TecInv567Asset10304";

            var building = Fixture.Build<Building>()
                .With(b => b.Enabled, true)
                .Without(b => b.Floors)
                .Create();

            var siteMapping = new SiteMapping() { BuildingId = building.Id, SiteId = siteId };

            var parentCategory = Fixture.Build<Category>()
                    .With(c => c.BuildingId, building.Id)
                    .With(c => c.Archived, false)
                    .With(c => c.Name, "Asset Register")
                    .With(c => c.ParentId, (int?)null)
                    .With(c => c.DbTableName, (string)null)
                    .Without(b => b.Building)
                    .Without(b => b.ParentCategory)
                    .Without(b => b.ChildCategories)
                    .Without(b => b.CategoryGroups)
                    .Without(b => b.CategoryColumns)
                    .Create();

            var childCategory = Fixture.Build<Category>()
               .With(c => c.BuildingId, building.Id)
               .With(c => c.Archived, false)
               .With(c => c.ParentId, parentCategory.Id)
               .With(c => c.DbTableName, (string)null)
               .Without(b => b.Building)
               .Without(b => b.ParentCategory)
               .Without(b => b.ChildCategories)
               .Without(b => b.CategoryGroups)
               .Without(b => b.CategoryColumns)
               .Create();

            var categories = new List<Category>() {
                parentCategory,
                childCategory
            };

            var categoryColumns = new List<CategoryColumn>() {
                Fixture.Build<CategoryColumn>().With(c => c.CategoryId, childCategory.Id).With(c => c.DbColumnName, "Type").Create(),
                Fixture.Build<CategoryColumn>().With(c => c.CategoryId, childCategory.Id).With(c => c.DbColumnName, "WarrantyDurationParts").Create(),
                Fixture.Build<CategoryColumn>().With(c => c.CategoryId, childCategory.Id).With(c => c.DbColumnName, "InstallationDate").Create()
            };

            var assets = Fixture.Build<Asset>()
               .With(a => a.BuildingId, building.Id)
               .With(a => a.CategoryId, childCategory.Id)
               .With(a => a.FloorCode, "")
               .With(a => a.Archived, false)
               .Without(a => a.Category)
               .CreateMany(100)
               .ToList();

            var assetEquipmentMappings = assets.Select(a => Fixture.Build<AssetEquipmentMapping>().With(aeq => aeq.AssetRegisterId, a.Id).Create()).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.SqliteInMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();

                #region Seed Databases
                using (var context = arrangement.CreateDbContext<AssetDbContext>())
                {
                    await context.Database.EnsureCreatedAsync();
                }

                using (var context = arrangement.CreateDbContext<AssetDbContext>())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        context.Buildings.Add(building);
                        context.Categories.AddRange(categories);
                        context.CategoryColumns.AddRange(categoryColumns);
                        context.Assets.AddRange(assets);
                        await context.SaveChangesAsync();

                        var queryBuilder = new StringBuilder();
                        queryBuilder.AppendLine($@"
                            CREATE TABLE [{dbTableName}](
	                        [Id] [int] NOT NULL,
	                        [AssetRegisterId] [int] NOT NULL,
	                        [Type] [nvarchar] NULL,
	                        [WarrantyDurationParts] [int] NULL,
	                        [InstallationDate] [date] NULL);

                            INSERT INTO [{dbTableName}]
                            VALUES");

                        for (int i = 0; i < assets.Count; i++)
                        {
                            if (i != assets.Count - 1)
                            {
                                queryBuilder.AppendLine($"({i + 1}, {assets[i].Id}, 'f0ac388c-ae86-4a90-854b-ac6e4a02ba48-0020db10', NULL, NULL),");
                            }
                            else
                            {
                                queryBuilder.AppendLine($"({i + 1}, {assets[i].Id}, 'f0ac388c-ae86-4a90-854b-ac6e4a02ba48-0020db10', NULL, NULL);");
                            }
                        }

                        var query = queryBuilder.ToString();
                        await context.Database.ExecuteSqlRawAsync(query);

                        await transaction.CommitAsync();
                    }
                }

                using (var context = arrangement.CreateDbContext<MappingDbContext>())
                {
                    await context.Database.EnsureCreatedAsync();
                }

                using (var context = arrangement.CreateDbContext<MappingDbContext>())
                {
                    context.SiteMappings.Add(siteMapping);
                    context.AssetEquipmentMappings.AddRange(assetEquipmentMappings);
                    await context.SaveChangesAsync();
                }
                #endregion

                var response = await client.GetAsync($"api/sites/{siteId}/assetTree");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var assetTreeRootCategories = await response.Content.ReadAsAsync<List<AssetTreeCategoryDto>>();

                //Assert
                void CheckCategories(List<AssetTreeCategoryDto> assetTreeCategoryDtos)
                {
                    foreach (var category in assetTreeCategoryDtos)
                    {
                        Assert.Equal(categories.Count(c => c.ParentId == category.Id.ToCategoryId() && c.Archived == false), category.Categories.Count);
                        Assert.Equal(assets.Count(a => (a.CategoryId == category.Id.ToCategoryId()) && a.Archived == false), category.Assets.Count);
                        Assert.Empty(category.ModuleTypeNamePath);

                        CheckCategories(category.Categories);
                    }
                }

                CheckCategories(assetTreeRootCategories);
            }
        }

        [Fact]
        public async Task AssetTree_GetAssetTree_InvalidTreeStructure_ReturnsEmptyCategories()
        {
            var siteId = Guid.NewGuid();
            var dbTableName = "TecInv567Asset10304";

            var building = Fixture.Build<Building>()
                .With(b => b.Enabled, true)
                .Without(b => b.Floors)
                .Create();

            var siteMapping = new SiteMapping() { BuildingId = building.Id, SiteId = siteId };

            var parentCategory = Fixture.Build<Category>()
                    .With(c => c.BuildingId, building.Id)
                    .With(c => c.Archived, false)
                    .With(c => c.Name, "Invalid Asset Register")
                    .With(c => c.ParentId, (int?)null)
                    .With(c => c.DbTableName, (string)null)
                    .Without(b => b.Building)
                    .Without(b => b.ParentCategory)
                    .Without(b => b.ChildCategories)
                    .Without(b => b.CategoryGroups)
                    .Without(b => b.CategoryColumns)
                    .Create();

            var childCategory = Fixture.Build<Category>()
               .With(c => c.BuildingId, building.Id)
               .With(c => c.Archived, false)
               .With(c => c.ParentId, parentCategory.Id)
               .With(c => c.DbTableName, (string)null)
               .Without(b => b.Building)
               .Without(b => b.ParentCategory)
               .Without(b => b.ChildCategories)
               .Without(b => b.CategoryGroups)
               .Without(b => b.CategoryColumns)
               .Create();

            var categories = new List<Category>() {
                parentCategory,
                childCategory
            };

            var categoryColumns = new List<CategoryColumn>() {
                Fixture.Build<CategoryColumn>().With(c => c.CategoryId, childCategory.Id).With(c => c.DbColumnName, "Type").Create(),
                Fixture.Build<CategoryColumn>().With(c => c.CategoryId, childCategory.Id).With(c => c.DbColumnName, "WarrantyDurationParts").Create(),
                Fixture.Build<CategoryColumn>().With(c => c.CategoryId, childCategory.Id).With(c => c.DbColumnName, "InstallationDate").Create()
            };

            var assets = Fixture.Build<Asset>()
               .With(a => a.BuildingId, building.Id)
               .With(a => a.CategoryId, childCategory.Id)
               .With(a => a.FloorCode, "")
               .With(a => a.Archived, false)
               .Without(a => a.Category)
               .CreateMany(100)
               .ToList();

            var assetEquipmentMappings = assets.Select(a => Fixture.Build<AssetEquipmentMapping>().With(aeq => aeq.AssetRegisterId, a.Id).Create()).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.SqliteInMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();

                #region Seed Databases
                using (var context = arrangement.CreateDbContext<AssetDbContext>())
                {
                    await context.Database.EnsureCreatedAsync();
                }

                using (var context = arrangement.CreateDbContext<AssetDbContext>())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        context.Buildings.Add(building);
                        context.Categories.AddRange(categories);
                        context.CategoryColumns.AddRange(categoryColumns);
                        context.Assets.AddRange(assets);
                        await context.SaveChangesAsync();

                        var queryBuilder = new StringBuilder();
                        queryBuilder.AppendLine($@"
                            CREATE TABLE [{dbTableName}](
	                        [Id] [int] NOT NULL,
	                        [AssetRegisterId] [int] NOT NULL,
	                        [Type] [nvarchar] NULL,
	                        [WarrantyDurationParts] [int] NULL,
	                        [InstallationDate] [date] NULL);

                            INSERT INTO [{dbTableName}]
                            VALUES");

                        for (int i = 0; i < assets.Count; i++)
                        {
                            if (i != assets.Count - 1)
                            {
                                queryBuilder.AppendLine($"({i + 1}, {assets[i].Id}, 'f0ac388c-ae86-4a90-854b-ac6e4a02ba48-0020db10', NULL, NULL),");
                            }
                            else
                            {
                                queryBuilder.AppendLine($"({i + 1}, {assets[i].Id}, 'f0ac388c-ae86-4a90-854b-ac6e4a02ba48-0020db10', NULL, NULL);");
                            }
                        }

                        var query = queryBuilder.ToString();
                        await context.Database.ExecuteSqlRawAsync(query);

                        await transaction.CommitAsync();
                    }
                }

                using (var context = arrangement.CreateDbContext<MappingDbContext>())
                {
                    await context.Database.EnsureCreatedAsync();
                }

                using (var context = arrangement.CreateDbContext<MappingDbContext>())
                {
                    context.SiteMappings.Add(siteMapping);
                    context.AssetEquipmentMappings.AddRange(assetEquipmentMappings);
                    await context.SaveChangesAsync();
                }
                #endregion

                var response = await client.GetAsync($"api/sites/{siteId}/assetTree");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var assetTreeRootCategories = await response.Content.ReadAsAsync<List<AssetTreeCategoryDto>>();

                //Assert
                Assert.Empty(assetTreeRootCategories);
            }
        }
    }
}
