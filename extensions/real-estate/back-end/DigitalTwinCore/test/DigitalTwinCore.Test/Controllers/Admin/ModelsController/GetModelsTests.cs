using DigitalTwinCore.Constants;
using DigitalTwinCore.Dto;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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

namespace DigitalTwinCore.Test.Controllers.Admin.ModelsController
{
    public class GetModelsTests : BaseInMemoryTest
    {
        public GetModelsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ThereAreModels_GetModels_ReturnsModels()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost" });
            context.SaveChanges();

            List<Model> expectedModels = await CreateModelsAsync(serverArrangement, siteId);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"admin/sites/{siteId}/models");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<ModelDto>>();
            result.Should().BeEquivalentTo(ModelDto.MapFrom(null, expectedModels));
        }

        [Fact]
        public async Task ThereAreModels_GetModelById_ReturnsModel()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var expectedModelId = "dtmi:com:willowinc:Asset;1";

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost" });
            context.SaveChanges();

            Model expectedModel = (await CreateModelsAsync(serverArrangement, siteId)).Single(m => m.Id == expectedModelId);

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"admin/sites/{siteId}/models/{expectedModelId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<ModelDto>();
            result.Should().BeEquivalentTo(ModelDto.MapFrom(null, expectedModel));
        }

        [Fact]
        public async Task NewModel_PostModel_CreatesAndReturnsNewModel()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var expectedModelId = "dtmi:com:willowinc:HVACAsset;1";

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost" });
            context.SaveChanges();

            string expectedModelJson = @"{
                      ""@id"": ""dtmi:com:willowinc:HVACAsset;1"",
                      ""@type"": ""Interface"",
                      ""displayName"": ""HVAC Asset"",
                      ""extends"" : [
                        ""dtmi:com:willowinc:Asset;1""
                      ],
                      ""contents"": [
                      ],
                      ""@context"": ""dtmi:dtdl:context;2""
                    }";

            using var client = server.CreateClient(null, userId);

            var response = await client.PostAsync($"admin/sites/{siteId}/models", new StringContent(expectedModelJson, Encoding.UTF8, "application/json"));

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().Be($"/admin/sites/{siteId}/models/{expectedModelId}");

            var result = await response.Content.ReadAsAsync<ModelDto>();
            result.Model.Should().Be(expectedModelJson);
        }

        [Fact]
        public async Task DeleteModel_DeletesModel()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var expectedModelId = "dtmi:com:willowinc:Asset;1";

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost" });
            context.SaveChanges();

            await CreateModelsAsync(serverArrangement, siteId);

            using var client = server.CreateClient(null, userId);

            var response = await client.DeleteAsync($"admin/sites/{siteId}/models/{expectedModelId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        private static async Task<List<Model>> CreateModelsAsync(ServerArrangement serverArrangement, Guid siteId)
        {
            var models = new List<Model>
            {
                new Model
                {
                    Id = WillowInc.AssetModelId,
                    DisplayNames = new Dictionary<string, string>(new [] { new KeyValuePair<string, string>("en", "Asset") }),
                    ModelDefinition = AdtSetupHelper.ReadTestJson("GetModels", "GetModelsTests.json"),
                }
            };

            var dtsp = serverArrangement.MainServices.GetRequiredService<IDigitalTwinServiceProvider>();
            var dts = await dtsp.GetForSiteAsync(siteId) as TestDigitalTwinService;
            foreach (var model in models)
            {
                await dts.AddModel(model.ModelDefinition);
            }
            dts.Reload();

            return models;
        }

        [Fact]
        public async Task ThereAreNoModels_GetModels_ReturnsEmptyList()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();

            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();

            context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost" });
            context.SaveChanges();

            using var client = server.CreateClient(null, userId);

            var response = await client.GetAsync($"admin/sites/{siteId}/models");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<ModelDto>>();
            result.Should().BeEquivalentTo(new List<ModelDto>());
        }

    }
}