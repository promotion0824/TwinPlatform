using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Features.LiveData;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.LiveData.LiveData
{
    public class QueryEquipmentsOrPointsTests : BaseInMemoryTest
    {
        public QueryEquipmentsOrPointsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenPointsAreValid_QueryEquipmentsOrPointsTests_ReturnsSiteIds()
        {
            var expectedResult = Fixture.Build<QueryEquipmentsOrPointsResponseItem>().Without(x => x.EquipmentId).CreateMany(100).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                foreach(var item in expectedResult)
                {
                    arrangement.GetConnectorApi()
                        .SetupRequest(HttpMethod.Get, $"temp/points/{item.PointId.Value}")
                        .ReturnsJson(new PointCore { Id = item.PointId.Value, EntityId = item.PointEntityId.Value, SiteId = item.SiteId });
                }

                var response = await client.GetAsync($"timemachine/models?" + string.Join("&", expectedResult.Select(x => $"pointIds={x.PointId.Value}")));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<QueryEquipmentsOrPointsResponseItem>>();
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task GivenPointsDoNotExist_QueryEquipmentsOrPointsTests_ReturnsEmptyArray()
        {
            var expectedResult = Fixture.Build<QueryEquipmentsOrPointsResponseItem>().Without(x => x.EquipmentId).CreateMany(100).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                foreach(var item in expectedResult)
                {
                    arrangement.GetConnectorApi()
                        .SetupRequest(HttpMethod.Get, $"temp/points/{item.PointId.Value}")
                        .ReturnsResponse(HttpStatusCode.NotFound);
                }

                var response = await client.GetAsync($"timemachine/models?" + string.Join("&", expectedResult.Select(x => $"pointIds={x.PointId.Value}")));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<QueryEquipmentsOrPointsResponseItem>>();
                result.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task GivenEquipmentsAreValid_QueryEquipmentsOrPointsTests_ReturnsSiteIds()
        {
            var expectedResult = Fixture.Build<QueryEquipmentsOrPointsResponseItem>()
                                        .Without(x => x.PointId)
                                        .Without(x => x.PointEntityId)
                                        .CreateMany(100).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                foreach(var item in expectedResult)
                {
                    arrangement.GetConnectorApi()
                        .SetupRequest(HttpMethod.Get, $"equipments/{item.EquipmentId.Value}?includePoints=True&includePointTags=True")
                        .ReturnsJson(new Equipment { Id = item.EquipmentId.Value, SiteId = item.SiteId });
                }

                var response = await client.GetAsync($"timemachine/models?" + string.Join("&", expectedResult.Select(x => $"equipmentIds={x.EquipmentId.Value}")));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<QueryEquipmentsOrPointsResponseItem>>();
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task GivenEquipmentsDoNotExist_QueryEquipmentsOrPointsTests_ReturnsEmptyArray()
        {
            var expectedResult = Fixture.Build<QueryEquipmentsOrPointsResponseItem>()
                                        .Without(x => x.PointId)
                                        .Without(x => x.PointEntityId)
                                        .CreateMany(100).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                foreach(var item in expectedResult)
                {
                    arrangement.GetConnectorApi()
                        .SetupRequest(HttpMethod.Get, $"equipments/{item.EquipmentId.Value}?includePoints=True&includePointTags=True")
                        .ReturnsResponse(HttpStatusCode.NotFound);
                }

                var response = await client.GetAsync($"timemachine/models?" + string.Join("&", expectedResult.Select(x => $"equipmentIds={x.EquipmentId.Value}")));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<QueryEquipmentsOrPointsResponseItem>>();
                result.Should().BeEmpty();
            }
        }
    }
}