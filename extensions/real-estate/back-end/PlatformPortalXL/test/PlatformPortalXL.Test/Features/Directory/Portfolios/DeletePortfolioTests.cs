using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Models;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Portfolios
{
    public class DeletePortfolioTests : BaseInMemoryTest
    {
        public DeletePortfolioTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_DeletePortfolio_ReturnsNoContent()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(null, Permissions.ManagePortfolios, customerId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios/{portfolioId}/sites")
                    .ReturnsJson(new List<Site>());

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Delete, $"customers/{customerId}/portfolios/{portfolioId}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.DeleteAsync($"customers/{customerId}/portfolios/{portfolioId}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_DeletePortfolio_ReturnsForbidden()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnCustomer(null, Permissions.ManagePortfolios, customerId))
            {
                var response = await client.DeleteAsync($"customers/{customerId}/portfolios/{portfolioId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
