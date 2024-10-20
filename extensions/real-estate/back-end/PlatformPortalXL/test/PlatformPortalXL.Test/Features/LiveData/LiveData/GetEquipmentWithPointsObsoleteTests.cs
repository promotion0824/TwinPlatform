using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.LiveData.LiveData
{
    public class GetEquipmentWithPointsObsoleteTests : BaseInMemoryTest
    {
        public GetEquipmentWithPointsObsoleteTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidEquipmentId_ReturnsEquipmentAndPoints()
        {
            var points = Fixture.Build<PointCore>().Without(x => x.Equipment).CreateMany().ToList();
            var equipment = Fixture.Build<EquipmentCore>().With(x => x.Points, points).Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, equipment.SiteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"equipments/{equipment.Id}?includePoints=True&includePointTags=True")
                    .ReturnsJson(equipment);

                var response = await client.GetAsync($"equipments/{equipment.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<EquipmentDto>();
                result.Should().BeEquivalentTo(EquipmentDto.MapFrom(EquipmentCore.MapToModel(equipment)));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_ReturnsForbidden()
        {
            var points = Fixture.Build<Point>().Without(x => x.Equipment).CreateMany().ToList();
            var equipment = Fixture.Build<Equipment>().With(x => x.Points, points).Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, equipment.SiteId))
            {
                server.Arrange().GetConnectorApi()
                    .SetupRequest(HttpMethod.Get, $"equipments/{equipment.Id}?includePoints=True&includePointTags=True")
                    .ReturnsJson(equipment);

                var response = await client.GetAsync($"equipments/{equipment.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}