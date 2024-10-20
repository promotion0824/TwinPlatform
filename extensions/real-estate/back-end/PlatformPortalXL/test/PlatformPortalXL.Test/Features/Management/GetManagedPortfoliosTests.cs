using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Willow.Directory.Models;
using Willow.Management;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Management
{
    public class GetManagedPortfoliosTests: BaseInMemoryTest
    {
        public GetManagedPortfoliosTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task UserHasPortfolios_GetManagedPortfolios_ReturnPortfolios()
        {
            var userId = Guid.NewGuid();
            var siteIdForAdmin = Guid.NewGuid();
            var siteIdForViewer = Guid.NewGuid();
            var portfolioIdForAdmin = Guid.NewGuid();
            var portfolioIdForViewer = Guid.NewGuid();
            var portfolioAssignments = new List<RoleAssignment>
            {
                new RoleAssignment{PrincipalId = userId, ResourceId = portfolioIdForViewer, ResourceType = RoleResourceType.Portfolio, RoleId = WellKnownRoleIds.PortfolioViewer},
                new RoleAssignment{PrincipalId = userId, ResourceId = portfolioIdForAdmin, ResourceType = RoleResourceType.Portfolio, RoleId = WellKnownRoleIds.PortfolioAdmin},
                new RoleAssignment{PrincipalId = userId, ResourceId = siteIdForViewer, ResourceType = RoleResourceType.Site, RoleId = WellKnownRoleIds.SiteViewer},
                new RoleAssignment{PrincipalId = userId, ResourceId = siteIdForAdmin, ResourceType = RoleResourceType.Site, RoleId = WellKnownRoleIds.SiteAdmin},
            };
            var portfolios = new List<Portfolio>
            {
                Fixture.Build<Portfolio>()
                    .With(x => x.Id, portfolioIdForAdmin)
                    .With(x => x.Sites, new List<Site>
                    {
                        Fixture.Build<Site>().With(x => x.Id, siteIdForViewer).Create(),
                        Fixture.Build<Site>().With(x => x.Id, siteIdForAdmin).Create(),
                    })
                    .Create(),
                Fixture.Build<Portfolio>()
                    .With(x => x.Id, portfolioIdForViewer)
                    .With(x => x.Sites, (List<Site>)null)
                    .Create()
            };
            
            var user = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .Create();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                    .ReturnsJson(portfolioAssignments);
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(user);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{user.CustomerId}/portfolios?includeSites={true}")
                    .ReturnsJson(portfolios);

                var sites = portfolios.SelectMany(x => x.Sites ?? new List<Site>());
             
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Post, "sites")
                    .ReturnsJson(sites);

                var response = await client.GetAsync($"management/managedPortfolios");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ManagedPortfolioDto>>();
                result.Should().HaveCount(2);
                result[0].PortfolioId.Should().Be(portfolioIdForAdmin);
                result[0].PortfolioName.Should().Be(portfolios[0].Name);
                result[0].Role.Should().Be("Admin");

                var returnedSites = result[0].Sites;
                returnedSites.Should().HaveCount(2);
                returnedSites[0].SiteId.Should().Be(siteIdForViewer);
                returnedSites[0].SiteName.Should().Be(portfolios[0].Sites[0].Name);
                returnedSites[0].Role.Should().Be("Viewer");
                returnedSites[0].LogoUrl.Contains(portfolios[0].Sites[0].LogoPath);
                returnedSites[0].LogoOriginalSizeUrl.Contains(portfolios[0].Sites[0].LogoPath);

                returnedSites[1].SiteId.Should().Be(siteIdForAdmin);
                returnedSites[1].SiteName.Should().Be(portfolios[0].Sites[1].Name);
                returnedSites[1].Role.Should().Be("Admin");
                returnedSites[1].LogoUrl.Contains(portfolios[0].Sites[1].LogoPath);
                returnedSites[1].LogoOriginalSizeUrl.Contains(portfolios[0].Sites[1].LogoPath);

                result[1].PortfolioId.Should().Be(portfolioIdForViewer);
                result[1].PortfolioName.Should().Be(portfolios[1].Name);
                result[1].Role.Should().Be("Viewer");
                result[1].Sites.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task UserHasSites_GetManagedPortfolios_ReturnPortfolios()
        {
            var userId = Guid.NewGuid();
            var siteIdForAdmin = Guid.NewGuid();
            var portfolioAssignments = new List<RoleAssignment>
            {
                new RoleAssignment{PrincipalId = userId, ResourceId = siteIdForAdmin, ResourceType = RoleResourceType.Site, RoleId = WellKnownRoleIds.SiteAdmin},
            };
            var portfolios = new List<Portfolio>
            {
                Fixture.Build<Portfolio>()
                    .With(x => x.Sites, new List<Site>
                    {
                        Fixture.Build<Site>().With(x => x.Id, siteIdForAdmin).Create(),
                    })
                    .Create()
            };

            var user = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                    .ReturnsJson(portfolioAssignments);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(user);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{user.CustomerId}/portfolios?includeSites={true}")
                    .ReturnsJson(portfolios);

                var sites = portfolios.SelectMany(x => x.Sites ?? new List<Site>());
               
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Post, "sites")
                    .ReturnsJson(sites);

                var response = await client.GetAsync($"management/managedPortfolios");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ManagedPortfolioDto>>();
                result.Should().HaveCount(1);
                result[0].PortfolioId.Should().Be(portfolios[0].Id);
                result[0].PortfolioName.Should().Be(portfolios[0].Name);
                result[0].Role.Should().Be("");

                var returnedSites = result[0].Sites;
                returnedSites.Should().HaveCount(1);

                returnedSites[0].SiteId.Should().Be(siteIdForAdmin);
                returnedSites[0].SiteName.Should().Be(portfolios[0].Sites[0].Name);
                returnedSites[0].Role.Should().Be("Admin");
                returnedSites[0].LogoUrl.Contains(portfolios[0].Sites[0].LogoPath);
                returnedSites[0].LogoOriginalSizeUrl.Contains(portfolios[0].Sites[0].LogoPath);
            }
        }

        [Fact]
        public async Task UserIsACustomerAdmin_GetManagedPortfolios_ReturnAllCustomerPortfoliosWithAdminPermissions()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var assignments = new List<RoleAssignment>
            {
                new RoleAssignment{PrincipalId = userId, ResourceId = customerId, ResourceType = RoleResourceType.Customer, RoleId = WellKnownRoleIds.CustomerAdmin},
            };

            var portfolios = new List<Portfolio>();
            portfolios.AddRange(
                Fixture.Build<Portfolio>()
                    .With(x => x.Sites, Fixture.Build<Site>().CreateMany(2).ToList())
                    .CreateMany(3).ToList()
            );
            portfolios.AddRange(
                Fixture.Build<Portfolio>()
                    .With(x => x.Sites, (List<Site>)null)
                    .CreateMany(2).ToList()
            );

            var portfoliosWithSites = portfolios.Where(x => x.Sites != null).ToList();
            foreach (var portfolio in portfoliosWithSites)
            {
                foreach (var site in portfolio.Sites)
                {
                    site.PortfolioId = portfolio.Id;
                }
            }

            var user = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.CustomerId, customerId)
                .Create();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                    .ReturnsJson(assignments);
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(user);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{user.CustomerId}/portfolios?includeSites={true}")
                    .ReturnsJson(portfolios);

                var sites = portfolios.SelectMany(x => x.Sites ?? new List<Site>());
                
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Post, "sites")
                    .ReturnsJson(sites);

                var response = await client.GetAsync($"management/managedPortfolios");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<ManagedPortfolioDto>>();
                result.Should().HaveCount(5);
                result.Where(x => x.Sites != null).Should().HaveCount(5);
                result.Where(x => !x.Sites.Any()).Should().HaveCount(2);
                result.Select(x => x.Role)
                    .Concat(result.Where(x => x.Sites != null)
                        .SelectMany(x => x.Sites).Select(x => x.Role))
                    .Should().OnlyContain(x => x == "Admin");

                var expectedLogoPaths = sites.Select(x => x.LogoPath);
                result.SelectMany(x => x.Sites ?? new List<ManagedSiteDto>()).Select(x => x.LogoUrl).All(x => expectedLogoPaths.Any(y => x.Contains(y)));  
            }
        }
    }
}
