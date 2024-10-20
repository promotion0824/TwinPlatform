using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.FloorManagement.Points
{
    public class GetPointsLivedataByLayerTests : BaseInMemoryTest
    {
        public GetPointsLivedataByLayerTests(ITestOutputHelper output) : base(output)
        {

        }

        [Fact]
        public async Task LayerGroupsAndPointsExist_GetPointsLivedataByLayer_ReturnsPoints()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var layerGroupId = Guid.NewGuid();
            var layerId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var tagName = "TagName";

            var expectedEquipments = Fixture.Build<LayerGroupEquipmentCore>().CreateMany(5).ToList();

            var expectedLayer = Fixture.Build<LayerGroupLayerCore>()
                .With(x => x.TagName, tagName)
                .With(x => x.Id, layerId)
                .Create();

            var expectedLayerGroup = Fixture.Build<LayerGroupCore>()
                .With(x => x.Equipments, expectedEquipments)
                .Create();

            expectedLayerGroup.Layers.Add(expectedLayer);

            var expectedPoints = expectedEquipments.Select(eq => Fixture.Build<PointCore>()
                .With(x => x.ClientId, customerId)
                .With(x => x.TwinId, Guid.NewGuid().ToString())
                .Without(x => x.Tags)
                .With(x => x.Equipment,
                    () => new List<EquipmentCore>
                    {
                        Fixture.Build<EquipmentCore>().With(x => x.Id, eq.Id).Without(x => x.Points)
                            .Without(x => x.Tags).Without(x => x.PointTags).Create()
                    })
                .Create()
            ).ToList();

            var expectedLiveData = expectedPoints.Select(p => Fixture.Build<PointTimeSeriesRawData>().With(d => d.PointEntityId, p.EntityId)
                .With(x => x.Id, Guid.NewGuid().ToString()).Create()).ToList();
            expectedLiveData = expectedLiveData.Skip(1).ToList();
            var expectedResponse = PointLivedataDto.MapFrom((from point in expectedPoints
                join rawValue in expectedLiveData on point.TwinId equals rawValue.Id into joinedValues
                from joinedValue in joinedValues.DefaultIfEmpty()
                select new PointLive {Point = PointCore.MapToModel(point), RawData = joinedValue}));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append($"api/telemetry/sites/{siteId}/lastTrendlogs?clientId={customerId}");
            foreach (var point in expectedPoints)
            {
                urlBuilder.Append($"&twinId={point.TwinId}");
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/points/bytag/{tagName}?includeEquipment=true")
                    .ReturnsJson(expectedPoints);

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}")
                    .ReturnsJson(expectedLayerGroup);

                server.Arrange().GetLiveDataApi()
                    .SetupRequest(HttpMethod.Get, urlBuilder.ToString())
                    .ReturnsJson(expectedLiveData);

                var response = await client.GetAsync($"sites/{siteId}/floors/{floorId}/layerGroup/{layerGroupId}/Layers/{layerId}/points/livedata");

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var responseData = await response.Content.ReadAsAsync<List<PointLivedataDto>>();
                responseData.Should().BeEquivalentTo(expectedResponse);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetPointsLivedataByLayer_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/floors/{Guid.NewGuid()}/layerGroup/{Guid.NewGuid()}/Layers/{Guid.NewGuid()}/points/livedata");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
