using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PlatformPortalXL.Features.Pilot;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.ServicesApi.AssetApi;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Platform.Models;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets
{
    public class GetPossibleTicketIssuesTests : BaseInMemoryTest
    {
        public GetPossibleTicketIssuesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SiteHasAssetsAndEquipments_GetPossibleTicketIssues_ReturnsThoseIssues()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();

            var floorId = Guid.NewGuid();
            var keyword = Guid.NewGuid().ToString();
            var expectedAssetTree = Fixture.Build<DigitalTwinAssetCategory>()
                .With(x => x.Name, keyword)
                .Without(a => a.Categories)
                .Without(a => a.Assets)
                .CreateMany(3)
                .ToList();
            expectedAssetTree.ForEach(c =>
                c.Assets = Fixture.Build<DigitalTwinAsset>().With(a => a.FloorId, floorId)
                    .With(a => a.Name, Fixture.Create<string>() + keyword).CreateMany(2).ToList());

            var assets = expectedAssetTree.SelectMany(x => x.Assets).Select(x => new Asset { Id = x.Id, Name = x.Name, TwinId = x.TwinId }).ToList();

            var expectedIssues = TicketIssueDto.MapFromModels(assets).ToList();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/AssetTree?floorId={floorId}&isCategoryOnly=False" + string.Join("", AdtConstants.DefaultAdtModels.Select(m => $"&modelNames={m}")))
                    .ReturnsJson(expectedAssetTree);


                var response = await client.GetAsync($"sites/{siteId}/possibleTicketIssues?floorId={floorId}&keyword={keyword}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketIssueDto>>();
                result.Should().BeEquivalentTo(expectedIssues);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetPossibleTicketIssues_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/possibleTicketIssues?floorId={Guid.NewGuid()}&keyword=abc");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GetPossibleTicketIssues_WithScopeId_ReturnsThoseIssues()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var keyword = Guid.NewGuid().ToString();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();

            var expectedAssetTree = Fixture.Build<DigitalTwinAssetCategory>()
                .With(x => x.Name, keyword)
                .Without(a => a.Categories)
                .Without(a => a.Assets)
                .CreateMany(3)
                .ToList();
            expectedAssetTree.ForEach(c =>
                c.Assets = Fixture.Build<DigitalTwinAsset>().With(a => a.FloorId, floorId)
                    .With(a => a.Name, Fixture.Create<string>() + keyword).CreateMany(2).ToList());

            var assets = expectedAssetTree.SelectMany(x => x.Assets).Select(x => new Asset { Id = x.Id, Name = x.Name, TwinId = x.TwinId }).ToList();

            var expectedIssues = TicketIssueDto.MapFromModels(assets).ToList();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var expectedTwinDto =Fixture.Build<TwinDto>().CreateMany(2).ToList();
            expectedTwinDto[0].SiteId = userSites[0].Id;
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/assets/AssetTree?floorId={floorId}&isCategoryOnly=False" + string.Join("", AdtConstants.DefaultAdtModels.Select(m => $"&modelNames={m}")))
                    .ReturnsJson(expectedAssetTree);

                var response = await client.GetAsync($"sites/{siteId}/possibleTicketIssues?floorId={floorId}&keyword={keyword}&scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketIssueDto>>();
                result.Should().BeEquivalentTo(expectedIssues);
            }
        }

        [Fact]
        public async Task GetPossibleTicketIssues_WithInvalidScopeId_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var expectedTwinDto = new List<TwinDto>();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                var response = await client.GetAsync($"sites/{siteId}/possibleTicketIssues?floorId={Guid.NewGuid()}&keyword=abc&scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GetPossibleTicketIssues_WithValidScopeId_UserHasNoAccess_ReturnsForbidden()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();

            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                var response = await client.GetAsync($"sites/{siteId}/possibleTicketIssues?floorId={Guid.NewGuid()}&keyword=abc&scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task FloorIdWasNotProvidedAndFloorWasNotFoundByFloorCode_GetPossibleTicketIssues_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();

            var floorCode = Guid.NewGuid();
            var keyword = Guid.NewGuid().ToString();
            var equipments = Fixture.Build<Equipment>().Without(x => x.Points).CreateMany().ToList();
            var twinCreatorAssets = Fixture.CreateMany<TwinCreatorAsset>().ToList();
            var assets = twinCreatorAssets.Select(x => new Asset { Id = x.Id, Name = x.Name } ).ToList();
            var expectedIssues = TicketIssueDto.MapFromModels(equipments);
            expectedIssues = expectedIssues.Union(TicketIssueDto.MapFromModels(assets)).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetSiteApi()
                                .SetupRequest(HttpMethod.Get, $"sites/{siteId}/floors")
                                .ReturnsJson(new List<Floor>());

                var response = await client.GetAsync($"sites/{siteId}/possibleTicketIssues?keyword=abc&floorCode={floorCode}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task FloorIdWasNotProvidedAndFloorWasFoundByCode_GetPossibleTicketIssues_ReturnsIssues()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
               .With(x => x.Id, siteId)
               .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
               .CreateMany(2).ToList();

            var floorCode = Guid.NewGuid().ToString();
            var floorId = Guid.NewGuid();
            var keyword = Guid.NewGuid().ToString();

            var expectedAssetTree = Fixture.Build<DigitalTwinAssetCategory>()
                .With(x=>x.Name,keyword)
                .Without(a => a.Categories)
                .Without(a => a.Assets)
                .CreateMany(3)
                .ToList();
            expectedAssetTree.ForEach(c =>
              c.Assets=  Fixture.Build<DigitalTwinAsset>().With(a => a.FloorId, floorId)
                  .With(a=>a.Name,Fixture.Create<string>()+keyword).CreateMany(2).ToList());

            var assets = expectedAssetTree.SelectMany(x=>x.Assets).Select(x => new Asset { Id = x.Id, Name = x.Name ,TwinId = x.TwinId} ).ToList();

            var expectedIssues =TicketIssueDto.MapFromModels(assets).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/floors")
                    .ReturnsJson(new List<Floor>() { new Floor() { Code = floorCode, Id = floorId } });

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/AssetTree?floorId={floorId}&isCategoryOnly=False" + string.Join("", AdtConstants.DefaultAdtModels.Select(m => $"&modelNames={m}")))
                    .ReturnsJson(expectedAssetTree);

                var response = await client.GetAsync($"sites/{siteId}/possibleTicketIssues?keyword={keyword}&floorCode={floorCode}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketIssueDto>>();
                result.Should().BeEquivalentTo(expectedIssues);
            }
        }
    }
}
