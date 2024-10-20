using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MobileXL.Dto;
using MobileXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Floors
{
    public class GetSiteFloorsTests : BaseInMemoryTest
    {
        public GetSiteFloorsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FloorsExist_GetFloors_ReturnsFloors()
        {
            var siteId = Guid.NewGuid();
            var expectedFloors = Fixture
                .Build<Floor>()
                .CreateMany(3).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/floors")
                    .ReturnsJson(expectedFloors);

                var response = await client.GetAsync($"sites/{siteId}/floors");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<FloorSimpleDto>>();

                result.Should().BeEquivalentTo(FloorSimpleDto.MapFrom(expectedFloors));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetFloors_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/floors");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
