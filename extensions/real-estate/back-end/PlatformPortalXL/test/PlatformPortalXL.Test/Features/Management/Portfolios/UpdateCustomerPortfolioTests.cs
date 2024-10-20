using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Management;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Api.DataValidation;
using Willow.Platform.Models;

namespace PlatformPortalXL.Test.Features.Management.Portfolios
{
    public class UpdateCustomerPortfolioTests : BaseInMemoryTest
    {
        public UpdateCustomerPortfolioTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateCustomerPortfolio_ReturnsForbidden()
        {
            var customerId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnCustomer(null, Permissions.ManagePortfolios, customerId))
            {
                var response = await client.PutAsJsonAsync($"customers/{customerId}/portfolios/{Guid.NewGuid()}", new UpdatePortfolioRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InvalidInput_UpdateCustomerPortfolio_ReturnsValidationError()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(null, Permissions.ManagePortfolios, customerId))
            {
                var response = await client.PutAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}", new UpdatePortfolioRequest());

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
            }
        }

        [Fact]
        public async Task GivenValidInput_UpdateCustomerPortfolio_ReturnUpdatedPortfolio()
        {
            var customerId = Guid.NewGuid();
            var portfolio = Fixture.Create<Portfolio>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(null, Permissions.ManagePortfolios, customerId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"customers/{customerId}/portfolios/{portfolio.Id}")
                    .ReturnsJson(portfolio);

                var response = await client.PutAsJsonAsync($"customers/{customerId}/portfolios/{portfolio.Id}", new UpdatePortfolioRequest { Name = portfolio.Name });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<PortfolioDto>();
                result.Should().BeEquivalentTo(PortfolioDto.MapFrom(portfolio));
            }
        }
    }
}
