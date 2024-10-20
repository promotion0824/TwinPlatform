using AssetCoreTwinCreator.Domain;
using AssetCoreTwinCreator.Domain.Models;
using AssetCoreTwinCreator.Dto;
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

namespace AssetCore.Test.Controllers.Assets
{
    public class GetCategoriesTests : BaseInMemoryTest
    {
        public GetCategoriesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task Category_DontIncludeChildren_ReturnsNoChildren()
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
               .With(c => c.DbTableName, dbTableName)
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

                    await context.SaveChangesAsync();
                }
                #endregion

                var response = await client.GetAsync($"api/sites/{siteId}/categories/roots?includeChildren=false");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var rootCategories = await response.Content.ReadAsAsync<List<CategoryDto>>();

                //Assert
                Assert.Equal(categories.Count(c => c.BuildingId == building.Id && c.ParentId == null && c.Archived == false), rootCategories.Count);

                void CheckCategories(List<CategoryDto> categoryDtos)
                {
                    foreach (var category in categoryDtos)
                    {
                        Assert.Empty(category.ChildCategories);

                        CheckCategories(category.ChildCategories);
                    }
                }

                CheckCategories(rootCategories);
            }
        }

        [Fact]
        public async Task Category_IncludeChildren_ReturnsChildren()
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
               .With(c => c.DbTableName, dbTableName)
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

                    await context.SaveChangesAsync();
                }
                #endregion

                var response = await client.GetAsync($"api/sites/{siteId}/categories/roots?includeChildren=true");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var rootCategories = await response.Content.ReadAsAsync<List<CategoryDto>>();

                //Assert
                Assert.Equal(categories.Count(c => c.BuildingId == building.Id && c.ParentId == null && c.Archived == false), rootCategories.Count);

                void CheckCategories(List<CategoryDto> categoryDtos)
                {
                    foreach (var category in categoryDtos)
                    {
                        Assert.Equal(categories.Count(c => c.ParentId == category.Id.ToCategoryId() && c.Archived == false), category.ChildCategories.Count);

                        CheckCategories(category.ChildCategories);
                    }
                }

                CheckCategories(rootCategories);
            }
        }
    }
}
