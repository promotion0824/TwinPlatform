using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Twins
{
    public class UpdateTwinsTests : BaseInMemoryTest
    {
        public UpdateTwinsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ValidInput_UpdateTwin_ReturnsUpdatedTwin()
        {
            var siteId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var twinId = "twin123";
            var request = new TwinDto
            {
                Id = "twinId1",
                Metadata = new TwinMetadataDto(),
                UserId = userId.ToString()
            };
            var updatedTwin = Fixture.Build<TwinDto>().Without(x => x.CustomProperties).Without(x => x.Metadata).Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"admin/sites/{siteId}/twins/{twinId}", request)
                    .ReturnsJson(updatedTwin);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/twins/{twinId}", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TwinDto>();
                result.Id.Should().BeEquivalentTo(updatedTwin.Id);
                result.Name.Should().BeEquivalentTo(updatedTwin.Name);
                result.UserId.Should().BeEquivalentTo(updatedTwin.UserId);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateTwisn_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/twins/{Guid.NewGuid()}", new TwinDto());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
