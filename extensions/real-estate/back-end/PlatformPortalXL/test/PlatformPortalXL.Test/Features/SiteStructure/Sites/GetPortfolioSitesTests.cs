using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Test.MockServices;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Platform.Models;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.SiteStructure.Sites
{
    public class GetPortfolioSitesTests : BaseInMemoryTest
    {
        public GetPortfolioSitesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task PortfolioHasSites_GetPortfolioSites_ReturnsThoseSites()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.PortfolioId, portfolioId)
                                       .Without(x => x.Features)
                                       .CreateMany();

            var expectedCustomer = Fixture.Build<Customer>()
                .With(x => x.Id, customerId)
                .With(x => x.Features, new CustomerFeatures())
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(null, Permissions.ViewPortfolios, customerId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/sites?portfolioId={portfolioId}")
                    .ReturnsJson(expectedSites);

                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var workflowApiHandler = server.Arrange().GetWorkflowApi();

                foreach (var site in expectedSites)
                {
                    var directorySite = new Site
                    {
                        Id = site.Id,
                        Name = site.Name,
                        Code = site.Code,
                        State = site.State,
						WebMapId = site.WebMapId,
                        Features = new SiteFeatures()
                    };

                    workflowApiHandler
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/settings")
                        .ReturnsJson(new SiteSettings());
                    directoryApiHandler
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                        .ReturnsJson(directorySite);
                    directoryApiHandler
                        .SetupRequest(HttpMethod.Get, $"customers/{customerId}")
                        .ReturnsJson(expectedCustomer);
                }


                var response = await client.GetAsync($"customers/{customerId}/portfolios/{portfolioId}/sites");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteDetailDto>>();
                var expectedSiteDetailDtos = SiteDetailDto.Map(expectedSites, server.Assert().GetImageUrlHelper());

                foreach (var site in expectedSiteDetailDtos)
                {
                    site.Features = new SiteFeaturesDto();
                    site.Settings = new SiteSettingsDto();
                }

                result.Should().BeEquivalentTo(expectedSiteDetailDtos);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetPortfolioSites_ReturnsForbidden()
        {
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnCustomer(null, Permissions.ViewPortfolios, customerId))
            {
                var response = await client.GetAsync($"customers/{customerId}/portfolios/{portfolioId}/sites");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
