using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using AdminPortalXL.Models.Directory;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;
using System;
using AdminPortalXL.Features.Directory;

namespace AdminPortalXL.Test.Features.Directory.Customers
{
    public class ImpersonateCustomerTests : BaseInMemoryTest
    {
        public ImpersonateCustomerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CustomerExist_ImpersonateCustomer_ReturnsImpersonateInformation()
        {
            var regionId0 = ServerFixtureConfigurations.Default.RegionIds[0];
            var customerId = Guid.NewGuid();
            var impersonateInfo = new ImpersonateInfo { AccessToken = "my-access-token" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithSupervisorRole())
            {
                server.Arrange().SetCustomerRegion(ServerFixtureConfigurations.Default, regionId0, customerId);
                server.Arrange().GetRegionalDirectoryApi(regionId0)
                    .SetupRequest(HttpMethod.Post, $"customers/{customerId}/impersonate")
                    .ReturnsJson(impersonateInfo);

                var response = await client.PostAsync($"customers/{customerId}/impersonate", null);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ImpersonateResponse>();
                result.Should().BeEquivalentTo(new ImpersonateResponse
                {
                    RegionId = regionId0,
                    RegionCode = string.Empty,
                    AccessToken = impersonateInfo.AccessToken
                });
            }
        }

        [Theory]
        [InlineData(0, "")]
        [InlineData(1, "au")]
        [InlineData(2, "us")]
        [InlineData(3, "eu")]
        public async Task CustomerExistInDifferentRegions_ImpersonateCustomer_ReturnsCorrectRegionCode(int regionIndex, string expectedRegionCode)
        {
            var regionId = ServerFixtureConfigurations.MultiRegionsForTestRegionCode.RegionIds[regionIndex];
            var customerId = Guid.NewGuid();
            var impersonateInfo = new ImpersonateInfo { AccessToken = "my-access-token" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.MultiRegionsForTestRegionCode))
            using (var client = server.CreateClientWithSupervisorRole())
            {
                server.Arrange().SetCustomerRegion(ServerFixtureConfigurations.MultiRegionsForTestRegionCode, regionId, customerId);
                server.Arrange().GetRegionalDirectoryApi(regionId)
                    .SetupRequest(HttpMethod.Post, $"customers/{customerId}/impersonate")
                    .ReturnsJson(impersonateInfo);

                var response = await client.PostAsync($"customers/{customerId}/impersonate", null);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ImpersonateResponse>();
                result.Should().BeEquivalentTo(new ImpersonateResponse
                {
                    RegionId = regionId,
                    RegionCode = expectedRegionCode,
                    AccessToken = impersonateInfo.AccessToken
                });
            }
        }

        [Fact]
        public async Task CustomerDoesNotExist_ImpersonateCustomer_ReturnsNotFound()
        {
            var regionId0 = ServerFixtureConfigurations.Default.RegionIds[0];
            var customerId = Guid.NewGuid();
            var nonExistingCustomerId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithSupervisorRole())
            {
                server.Arrange().SetCustomerRegion(ServerFixtureConfigurations.Default, regionId0, customerId);

                var response = await client.PostAsync($"customers/{nonExistingCustomerId}/impersonate", null);

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

    }
}