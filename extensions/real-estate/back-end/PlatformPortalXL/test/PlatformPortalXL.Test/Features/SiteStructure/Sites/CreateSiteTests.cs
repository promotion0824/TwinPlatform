using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.SiteStructure.Requests;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.SiteApi;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Api.DataValidation;

namespace PlatformPortalXL.Test.Features.SiteStructure.Sites
{
    public class CreateSiteTests : BaseInMemoryTest
    {
        public CreateSiteTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_CreateSite_ReturnsTheCreatedSite()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var expectedSite = Fixture.Create<Site>();
            var request = Fixture.Create<CreateSiteRequest>();

            request.Features = new SiteFeatures {  IsHideOccurrencesEnabled = true };

            var expectedRequestToSiteApi = new SiteApiCreateSiteRequest
            {
                Name = request.Name,
                Code = request.Code,
                Address = request.Address,
                Suburb = request.Suburb,
                Country = request.Country,
                State = request.State,
                TimeZoneId = request.TimeZoneId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                FloorCodes = request.FloorCodes,
                Area = request.Area,
                Type = request.Type!.Value,
                Status = request.Status!.Value,
                DateOpened = request.DateOpened,
                ConstructionYear = request.ConstructionYear,
                SiteCode = request.SiteCode,
                SiteContactEmail = request.SiteContactEmail,
                SiteContactName = request.SiteContactName,
                SiteContactPhone = request.SiteContactPhone,
                SiteContactTitle = request.SiteContactTitle
            };
            var expectedRequestToDirectoryApi = new DirectoryApiCreateSiteRequest
            {
                Id = expectedSite.Id,
                Name = request.Name,
                Code = request.Code,
                Features = request.Features,
                TimeZoneId = request.TimeZoneId
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(null, Permissions.ManageSites, customerId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"customers/{customerId}/portfolios/{portfolioId}/sites", expectedRequestToSiteApi)
                    .ReturnsJson(expectedSite);
                server.Arrange().GetDirectoryApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"customers/{customerId}/portfolios/{portfolioId}/sites", expectedRequestToDirectoryApi)
                    .ReturnsJson(expectedSite);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(new [] {new PortfolioDto{Id = portfolioId}});

                var response = await client.PostAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteDetailDto>();
                var expectedSiteDto = SiteDetailDto.Map(expectedSite, server.Assert().GetImageUrlHelper());
                result.Should().BeEquivalentTo(expectedSiteDto);
            }
        }

        [Fact]
        public async Task InvalidInput_CreateSite_ReturnsValidationError()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var request = new CreateSiteRequest();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnPortfolio(null, Permissions.ManageSites, portfolioId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(new [] {new PortfolioDto{Id = portfolioId}});

                var response = await client.PostAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(11);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateSite_ReturnsForbidden()
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

        [Fact]
        public async Task PortfolioDoesntBelongToACustomer_CreateSite_ReturnsBadRequest()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var request = Fixture.Create<CreateSiteRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(null, Permissions.ManageSites, customerId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(new [] {new PortfolioDto{Id = Guid.NewGuid()}});

                var response = await client.PostAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }
    }
}
