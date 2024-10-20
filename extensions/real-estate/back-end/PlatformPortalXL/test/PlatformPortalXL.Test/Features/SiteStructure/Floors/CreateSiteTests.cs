using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using PlatformPortalXL.Models;
using PlatformPortalXL.Features.SiteStructure.Requests;
using PlatformPortalXL.Dto;

namespace PlatformPortalXL.Test.Features.SiteStructure.Floors
{

    public class CreateSiteTests : BaseInMemoryTest
    {
        public CreateSiteTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_CreateSite_ReturnCreatedSite()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var expectedSite = Fixture.Create<Site>();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(null, Permissions.ManageSites, customerId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Post, $"customers/{customerId}/portfolios/{portfolioId}/sites")
                    .ReturnsJson(expectedSite);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"customers/{customerId}/portfolios/{portfolioId}/sites")
                    .ReturnsJson(expectedSite);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(new [] {new PortfolioDto{Id = portfolioId}});

                var request = new CreateSiteRequest()
                {
                    Address = "Addr",
                    Code = "1234",
                    Country = "Australia",
                    State = "NSW",
                    Suburb = "Suburb",
                    FloorCodes = new List<string>() { "F1" },
                    Latitude = 1.0,
                    Longitude = 1.9,
                    Name = "Site1",
                    TimeZoneId = "Sydney",
                    Area = "2,000,000 sqft",
                    Type = PropertyType.Office,
                    Status = SiteStatus.Operations
                };

                var response = await client.PostAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteDetailDto>();
                var expectedSiteDto = SiteDetailDto.Map(expectedSite, server.Assert().GetImageUrlHelper());
                result.Should().BeEquivalentTo(expectedSiteDto);
            }
        }

        [Fact]
        public async Task MissingFloorCode_CreateSite_ReturnUnprocessableEntity()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnPortfolio(null, Permissions.ManageSites, portfolioId))
            {
                var request = new CreateSiteRequest()
                {
                    Address = "Addr",
                    Code = "1234",
                    Country = "Australia",
                    State = "NSW",
                    Suburb = "Suburb",
                    FloorCodes = new List<string>(),
                    Latitude = 1.0,
                    Longitude = 1.9,
                    Name = "Site1",
                    TimeZoneId = "Sydney",
                    Area = "2,000,000 sqft",
                    Type = PropertyType.Office,
                    Status = SiteStatus.Operations
                };

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(new [] {new PortfolioDto{Id = portfolioId}});

                var response = await client.PostAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites", request);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task GivenUserWithoutPermissions_CreateSite_ReturnForbidden()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var request = Fixture.Create<CreateSiteRequest>();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnCustomer(null, Permissions.ManageSites, customerId))
            {
                var response = await client.PostAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites", request);
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}
