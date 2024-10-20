using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Dto;
using System.Linq;
using System.Collections.Generic;

namespace PlatformPortalXL.Test.Features.Twins
{
    public class GetTenantsTests : BaseInMemoryTest
    {
        public GetTenantsTests(ITestOutputHelper output) : base(output)
        {
        }

		[Fact]
		public async Task NoPermissions_GetTenants_ReturnsForbidden()
		{
			var siteId = Guid.NewGuid();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
			{
				var response = await client.GetAsync($"tenants?siteIds={siteId}");
				response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
			}
		}

		[Fact]
        public async Task NoTenant_GetTenants_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"tenants?siteIds={siteId}")
                    .ReturnsResponse(HttpStatusCode.NotFound);

                var response = await client.GetAsync($"tenants?siteIds={siteId}");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task HasTenants_GetTenants_ReturnsTenants()
        {
            var siteId = Guid.NewGuid();
			var expectedTenants = Fixture.Build<TenantDto>().CreateMany(3).ToList();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"tenants?siteIds={siteId}")
					.ReturnsJson(expectedTenants);

                var response = await client.GetAsync($"tenants?siteIds={siteId}");
				response.StatusCode.Should().Be(HttpStatusCode.OK);

				var result = await response.Content.ReadAsAsync<List<TenantDto>>();
				result.Should().BeEquivalentTo(expectedTenants);
			}
        }
    }
}
