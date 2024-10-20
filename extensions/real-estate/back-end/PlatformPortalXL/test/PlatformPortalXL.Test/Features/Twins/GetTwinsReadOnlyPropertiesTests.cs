using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Twins
{
    public class GetTwinsReadOnlyPropertiesTests : BaseInMemoryTest
    {
        public GetTwinsReadOnlyPropertiesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserWithPermissions_GetTwinsReadOnlyProperties_ReturnsProperties()
        {
            var siteId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/twins/readOnlyProperties");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<string>>();
                result.Should().BeEquivalentTo(TwinHelper.ReadOnly.ToList());
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetTwinsReadOnlyProperties_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var twinId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/twins/readOnlyProperties");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }


        [Fact]
        public async Task UserWithPermissions_GetTwinsRestrictedFieldsByTwinId_ReturnsNoneExpected()
        {
            var siteId = Guid.NewGuid();
            var twinId = "dummy";

            var twinFields = new TwinFieldsDto();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/fields")
                    .ReturnsJson(twinFields);

                var response = await client.GetAsync($"sites/{siteId}/twins/{twinId}/restrictedFields");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<RestrictedFieldsDto>();
                result.ExpectedFields.Should().BeNull();
            }
        }

        [Fact]
        public async Task UserWithPermissions_GetTwinsRestrictedFields_ReturnsNoneExpected()
        {
            var siteId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/twins/restrictedFields");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<RestrictedFieldsDto>();
                result.ExpectedFields.Should().BeNull();
            }
        }

        [Fact]
        public async Task UserWithPermissions_GetTwinsExpectedFields_ReturnsExpected()
        {
            var siteId = Guid.NewGuid();
            var twinId = "dummy";

            var twinFields = Fixture.Create<TwinFieldsDto>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/fields")
                    .ReturnsJson(twinFields);

                var response = await client.GetAsync($"sites/{siteId}/twins/{twinId}/restrictedFields");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<RestrictedFieldsDto>();
                result.ExpectedFields.Should().BeEquivalentTo(twinFields.ExpectedFields);
            }
        }
    }
}
