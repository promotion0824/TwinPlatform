using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Requests;
using SiteCore.Services.ImageHub;
using SiteCore.Tests;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Floors
{
    public class UpdateFloorModulesTests : BaseInMemoryTest
    {
        public const string PngBase64 =
            "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";

        public UpdateFloorModulesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FloorExists_Upload2dmodule_ReturnsUpdatedFloor()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var moduleTypeId1 = Guid.NewGuid();
            var moduleTypeId2 = Guid.NewGuid();
            var moduleTypeIdBase = Guid.NewGuid();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Postcode, "111250")
                .With(x => x.LogoId, (Guid?)null)
                .With(x => x.Id, siteId)
                .With(x => x.CustomerId, customerId)
                .Create();

            var floor = Fixture.Build<FloorEntity>()
                .With(x => x.Id, floorId)
                .Without(x => x.LayerGroups)
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .With(x => x.Code, "Code1")
                .With(x => x.SiteId, siteId)
                .Create();

            var moduleTypeBase = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, moduleTypeIdBase)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, false)
                .With(x => x.Prefix, "base_")
                .With(x => x.SiteId, siteId)
                .With(x => x.Name, ModuleConstants.ModuleBaseName)
                .Without(x => x.Modules)
                .Create();

            var moduleType1 = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, moduleTypeId1)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, false)
                .With(x => x.SiteId, siteId)
                .With(x => x.Prefix, "floor_")
                .Without(x => x.Modules)
                .Create();

            var moduleType2 = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, moduleTypeId2)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, false)
                .With(x => x.SiteId, siteId)
                .With(x => x.Prefix, "floor2_")
                .Without(x => x.Modules)
                .Create();

            var moduleBase = Fixture.Build<ModuleEntity>()
                .With(x => x.ModuleTypeId, moduleTypeIdBase)
                .With(x => x.ImageHeight, 100)
                .With(x => x.ImageWidth, 100)
                .With(x => x.FloorId, floorId)
                .Without(x => x.Floor)
                .Without(x => x.ModuleType)
                .Create();

            var imageBinary = Convert.FromBase64String(PngBase64);
            var newImageId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.ModuleTypes.Add(moduleType1);
                db.ModuleTypes.Add(moduleType2);
                db.ModuleTypes.Add(moduleTypeBase);
                db.Modules.Add(moduleBase);
                db.SaveChanges();

                var pathHelper = arrangement.MainServices.GetRequiredService<IImagePathHelper>();
                var imagePath = pathHelper.GetFloorModulePath(customerId, siteId, floorId);

                arrangement.GetImageHubApi()
                    .SetupRequest(HttpMethod.Post, imagePath)
                    .ReturnsJson(new OriginalImageDescriptor() { ImageId = newImageId });

                var dataContent = new MultipartFormDataContent();
                var fileContent1 = new ByteArrayContent(imageBinary)
                {
                    Headers = { ContentLength = imageBinary.Length }
                };
                var fileContent2 = new ByteArrayContent(imageBinary)
                {
                    Headers = { ContentLength = imageBinary.Length }
                };
                dataContent.Add(fileContent1, "files", "floor_abc.png");
                dataContent.Add(fileContent2, "files", "floor2_abc.png");

                var response = await client.PostAsync($"sites/{siteId}/floors/{floorId}/2dmodules", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.Id.Should().Be(floorId);

                var module = await db.Modules.FirstOrDefaultAsync(di => di.FloorId == floorId && di.VisualId == newImageId);
                module.Should().NotBeNull();
                module.ImageHeight.Should().Be(100);
                module.ImageWidth.Should().Be(100);
            }
        }

        [Fact]
        public async Task FloorExists_NoBaseModule_Upload2dmodule_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var moduleTypeId1 = Guid.NewGuid();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Postcode, "111250")
                .With(x => x.LogoId, (Guid?)null)
                .With(x => x.Id, siteId)
                .With(x => x.CustomerId, customerId)
                .Create();

            var floor = Fixture.Build<FloorEntity>()
                .With(x => x.Id, floorId)
                .Without(x => x.LayerGroups)
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .With(x => x.Code, "Code1")
                .With(x => x.SiteId, siteId)
                .Create();

            var moduleType1 = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, moduleTypeId1)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, false)
                .With(x => x.Prefix, "floor_")
                .Without(x => x.Modules)
                .Create();

            var imageBinary = Convert.FromBase64String(PngBase64);
            var newImageId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.ModuleTypes.Add(moduleType1);
                db.SaveChanges();

                var pathHelper = arrangement.MainServices.GetRequiredService<IImagePathHelper>();
                var imagePath = pathHelper.GetFloorModulePath(customerId, siteId, floorId);

                arrangement.GetImageHubApi()
                    .SetupRequest(HttpMethod.Post, imagePath)
                    .ReturnsJson(new OriginalImageDescriptor() { ImageId = newImageId });

                var dataContent = new MultipartFormDataContent();
                var fileContent1 = new ByteArrayContent(imageBinary)
                {
                    Headers = { ContentLength = imageBinary.Length }
                };
                
                dataContent.Add(fileContent1, "files", "floor_abc.png");

                var response = await client.PostAsync($"sites/{siteId}/floors/{floorId}/2dmodules", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
               
            }
        }

        [Fact]
        public async Task FloorExists_Upload3dmodule_ReturnsUpdatedFloor()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var moduleTypeId = Guid.NewGuid();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Postcode, "111250")
                .With(x => x.LogoId, (Guid?)null)
                .With(x => x.Id, siteId)
                .With(x => x.CustomerId, customerId)
                .Create();

            var floor = Fixture.Build<FloorEntity>()
                .With(x => x.Id, floorId)
                .Without(x => x.LayerGroups)
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .With(x => x.Code, "Code1")
                .With(x => x.SiteId, siteId)
                .Create();

            var imageType = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, moduleTypeId)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, true)
                .With(x => x.SiteId, siteId)
                .With(x => x.Prefix, "floor_")
                .Without(x => x.Modules)
                .Create();

            var moduleRequest = new CreateUpdateModule3DRequest
            {
                Modules3D = new List<Module3DInfo>
                {
                    new Module3DInfo
                    {
                        ModuleName = "floor_module.3d",
                        Url = "https://floor.module.3d"
                    }
                }
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.ModuleTypes.Add(imageType);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"sites/{siteId}/floors/{floorId}/3dmodules", moduleRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.Id.Should().Be(floorId);

                var module = await db.Modules.FirstOrDefaultAsync(di => di.FloorId == floorId && di.Url == moduleRequest.Modules3D.First().Url);
                module.Should().NotBeNull();
            }
        }


        [Fact]
        public async Task Upload3dmodule_TooLongUrl_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();

            var moduleRequest = new CreateUpdateModule3DRequest
            {
                Modules3D = new List<Module3DInfo>
                {
                    new Module3DInfo
                    {
                        ModuleName = "floor_module.3d",
                        Url = "https://floor.module.3d" + new string('x', 1025)
                    }
                }
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/floors/{floorId}/3dmodules", moduleRequest);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var strResponse = await response.Content.ReadAsStringAsync();
                strResponse.Should().Contain("The length of 'Url' must be 1024 characters or fewer");
            }
        }

        [Fact]
        public async Task FloorExists_Upload2dmodule_TypeNotDetected_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var moduleTypeId = Guid.NewGuid();
            var moduleId = Guid.NewGuid();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Postcode, "111250")
                .With(x => x.LogoId, (Guid?)null)
                .With(x => x.Id, siteId)
                .With(x => x.CustomerId, customerId)
                .Create();

            var floor = Fixture.Build<FloorEntity>()
                .With(x => x.Id, floorId)
                .Without(x => x.LayerGroups)
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .With(x => x.Code, "Code1")
                .With(x => x.SiteId, siteId)
                .Create();

            var moduleType = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, moduleTypeId)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, false)
                .With(x => x.Prefix, "qqq_")
                .Without(x => x.Modules)
                .Create();

            var existingModule = Fixture.Build<ModuleEntity>()
                .With(x => x.Id, moduleId)
                .With(x => x.ModuleTypeId, moduleTypeId)
                .With(x => x.FloorId, floorId)
                .Without(x => x.Floor)
                .Without(x => x.ModuleType)
                .Create();

            var imageBinary = Convert.FromBase64String(PngBase64);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.ModuleTypes.Add(moduleType);
                db.Modules.Add(existingModule);
                db.SaveChanges();

                var dataContent = new MultipartFormDataContent();
                var fileContent1 = new ByteArrayContent(imageBinary)
                {
                    Headers = { ContentLength = imageBinary.Length }
                };
                var fileContent2 = new ByteArrayContent(imageBinary)
                {
                    Headers = { ContentLength = imageBinary.Length }
                };
                dataContent.Add(fileContent1, "files", "floor_abc.png");
                dataContent.Add(fileContent2, "files", "floor2_abc.png");

                var response = await client.PostAsync($"sites/{siteId}/floors/{floorId}/2dmodules", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
               
            }
        }


        [Fact]
        public async Task FloorExists_Upload2dmodules_DuplicatedTypes_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var moduleTypeId = Guid.NewGuid();
            var moduleId = Guid.NewGuid();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Postcode, "111250")
                .With(x => x.LogoId, (Guid?)null)
                .With(x => x.Id, siteId)
                .With(x => x.CustomerId, customerId)
                .Create();

            var floor = Fixture.Build<FloorEntity>()
                .With(x => x.Id, floorId)
                .Without(x => x.LayerGroups)
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .With(x => x.Code, "Code1")
                .With(x => x.SiteId, siteId)
                .Create();

            var moduleType = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, moduleTypeId)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, false)
                .With(x => x.Prefix, "floor_")
                .Without(x => x.Modules)
                .Create();

            var imageBinary = Convert.FromBase64String(PngBase64);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.ModuleTypes.Add(moduleType);
                db.SaveChanges();

                var dataContent = new MultipartFormDataContent();
                var fileContent1 = new ByteArrayContent(imageBinary)
                {
                    Headers = { ContentLength = imageBinary.Length }
                };
                var fileContent2 = new ByteArrayContent(imageBinary)
                {
                    Headers = { ContentLength = imageBinary.Length }
                };
                dataContent.Add(fileContent1, "files", "floor_abc.png");
                dataContent.Add(fileContent2, "files", "floor_abc.png");

                var response = await client.PostAsync($"sites/{siteId}/floors/{floorId}/2dmodules", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            
            }
        }

        [Fact]
        public async Task FloorExists_Upload2dmodule_DifferentDimensions_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var moduleTypeId1 = Guid.NewGuid();
            var moduleTypeIdBase = Guid.NewGuid();

            var site = Fixture.Build<SiteEntity>()
                .Without(x => x.Floors)
                .Without(x => x.PortfolioId)
                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                .With(x => x.Postcode, "111250")
                .With(x => x.LogoId, (Guid?)null)
                .With(x => x.Id, siteId)
                .With(x => x.CustomerId, customerId)
                .Create();

            var floor = Fixture.Build<FloorEntity>()
                .With(x => x.Id, floorId)
                .Without(x => x.LayerGroups)
                .Without(x => x.Site)
                .Without(x => x.Modules)
                .With(x => x.Code, "Code1")
                .With(x => x.SiteId, siteId)
                .Create();

            var moduleTypeBase = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, moduleTypeIdBase)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, false)
                .With(x => x.Prefix, "base_")
                .With(x => x.Name, ModuleConstants.ModuleBaseName)
                .Without(x => x.Modules)
                .Create();

            var moduleType1 = Fixture.Build<ModuleTypeEntity>()
                .With(x => x.Id, moduleTypeId1)
                .With(x => x.CanBeDeleted, true)
                .With(x => x.Is3D, false)
                .With(x => x.Prefix, "floor_")
                .Without(x => x.Modules)
                .Create();

            var moduleBase = Fixture.Build<ModuleEntity>()
                .With(x => x.ModuleTypeId, moduleTypeIdBase)
                .With(x => x.ImageHeight, 50)
                .With(x => x.ImageWidth, 50)
                .With(x => x.FloorId, floorId)
                .Without(x => x.Floor)
                .Without(x => x.ModuleType)
                .Create();

            var imageBinary = Convert.FromBase64String(PngBase64);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Floors.Add(floor);
                db.ModuleTypes.Add(moduleType1);
                db.ModuleTypes.Add(moduleTypeBase);
                db.Modules.Add(moduleBase);
                db.SaveChanges();

                var dataContent = new MultipartFormDataContent();
                var fileContent1 = new ByteArrayContent(imageBinary)
                {
                    Headers = { ContentLength = imageBinary.Length }
                };
                
                dataContent.Add(fileContent1, "files", "floor_abc.png");

                var response = await client.PostAsync($"sites/{siteId}/floors/{floorId}/2dmodules", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
               
            }
        }        
    }
}
