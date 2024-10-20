using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Assets.Assets
{
    public class GetAssetPinTests : BaseInMemoryTest
    {
        public GetAssetPinTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(65)]
        [InlineData(null)]
        public async Task WhenAssetWithBindingEquipmentExist_GetAssetPin_ReturnsAssetPin(int? displayPriorityInt)
        {
            decimal? displayPriority = displayPriorityInt.HasValue ? decimal.Parse(displayPriorityInt.ToString()) : null;

            var site = Fixture.Create<Site>();

            var expectedPoint = Fixture.Build<DigitalTwinPoint>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.DisplayPriority, displayPriority)
                .With(x => x.TwinId, Guid.NewGuid().ToString())
                .With(x => x.Tags, new List<Tag> { new Tag { Name = "temperature" } })
                .With(x => x.Assets, new List<DigitalTwinPoint.PointAssetDto> { new DigitalTwinPoint.PointAssetDto { Id = Guid.NewGuid() } })
                .CreateMany(2).ToList();

            var asset = Fixture.Build<DigitalTwinAsset>()
                .Without(x => x.Tags).Without(x => x.PointTags)
                .With(x=>x.Points, expectedPoint)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/assets/{asset.Id}")
                    .ReturnsJson(asset);

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                var urlBuilder = new StringBuilder();
                urlBuilder.Append($"api/telemetry/sites/{site.Id}/lastTrendlogs?clientId={site.CustomerId}");
                foreach (var point in expectedPoint)
                {
                    urlBuilder.Append($"&twinId={point.TwinId}");
                }

                var expectedLiveData = expectedPoint.Select(p => Fixture.Build<PointTimeSeriesRawData>()
                    .With(d => d.PointEntityId, p.TrendId)
                    .With(x => x.Id, Guid.NewGuid().ToString())
                    .Create()).ToList();

                server.Arrange().GetLiveDataApi()
                    .SetupRequest(HttpMethod.Get, urlBuilder.ToString())
                    .ReturnsJson(expectedLiveData);

                var response = await client.GetAsync($"sites/{site.Id}/assets/{asset.Id}/pinOnLayer");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<AssetPinDto>();
                result.Title.Should().Be(asset.Name);
                result.LiveDataPoints.Should().HaveCount(displayPriority != null ? expectedPoint.Count : 0);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetAssetPin_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/assets/{Guid.NewGuid()}/pinOnLayer");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
