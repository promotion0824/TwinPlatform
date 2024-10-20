using AutoFixture;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.Features.Management;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System.Net.Http.Json;
using Willow.Api.DataValidation;
using Willow.Platform.Models;

namespace PlatformPortalXL.Test.Features.Management.Portfolios
{
    public class CreateCustomerPortfolioTests : BaseInMemoryTest
    {
        public CreateCustomerPortfolioTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_CreateCustomerPortfolio_ReturnCreatedPortfolio()
        {
            var customerId =Guid.NewGuid();
            var portfolio = Fixture.Create<Portfolio>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(null, Permissions.ManagePortfolios, customerId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"customers/{customerId}/portfolios")
                    .ReturnsJson(portfolio);

                var response = await client.PostAsJsonAsync($"customers/{customerId}/portfolios", new CreatePortfolioRequest { Name = portfolio.Name });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<PortfolioDto>();
                result.Should().BeEquivalentTo(PortfolioDto.MapFrom(portfolio));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateCustomerPortfolio_ReturnsForbidden()
        {
            var customerId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnCustomer(null, Permissions.ManagePortfolios, customerId))
            {
                var response = await client.PostAsJsonAsync($"customers/{customerId}/portfolios", new CreatePortfolioRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InvalidInput_CreatePortfolio_ReturnsValidationError()
        {
            var customerId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(null, Permissions.ManagePortfolios, customerId))
            {
                var response = await client.PostAsJsonAsync($"customers/{customerId}/portfolios", new CreatePortfolioRequest());

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
            }
        }
    }
}
