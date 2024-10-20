using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using Willow.Directory.Models;
using Willow.Management;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

using PlatformPortalXL.ServicesApi.DirectoryApi;
using System.Linq;
using System.Text;

namespace PlatformPortalXL.Test.Features.Management
{
    public class GetManagedUserTests: BaseInMemoryTest
    {
        public GetManagedUserTests(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public async Task UserIsACustomerAdmin_GetManagedUser_ReturnUserWithoutPortfolios()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
          
            var user = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.CustomerId, customerId)
                .With(x => x.FirstName, "Fred")
                .With(x => x.LastName, "Flintstone")
                .With(x => x.Status, UserStatus.Active)
                .Create();
            var managedUserAssignments = new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto{PrincipalId = userId, ResourceId = customerId, ResourceType = RoleResourceType.Customer, RoleId = WellKnownRoleIds.CustomerAdmin, CustomerId = customerId},
            };

            var currentUserAssignments = new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto{PrincipalId = currentUserId, ResourceId = customerId, ResourceType = RoleResourceType.Customer, RoleId = WellKnownRoleIds.CustomerAdmin, CustomerId = customerId},
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, currentUserId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                    .ReturnsJson(managedUserAssignments);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}/permissionAssignments")
                    .ReturnsJson(currentUserAssignments);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(user);
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}")
                    .ReturnsJson(new User { Id = currentUserId, CustomerId = customerId, Status = UserStatus.Active } );
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissions/manage-users/eligibility?customerId={customerId}")
                    .ReturnsJson(new CheckPermissionResponse { IsAuthorized = true } );
                
                var response = await client.GetAsync($"management/customers/{customerId}/users/{userId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ManagedUserDto>();
                result.Portfolios.Should().BeEmpty();
                result.IsCustomerAdmin.Should().BeTrue();
                var managedUserDto = ManagedUserDto.Map(user);
                managedUserDto.IsCustomerAdmin = true;
                result.Should().BeEquivalentTo(managedUserDto);
            }
        }

        [Fact]
        public async Task UserIsNotACustomerAdmin_GetManagedUser_ReturnUserWithPortfolios()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            var siteIdForAdmin = Guid.NewGuid();
            var siteIdForViewer = Guid.NewGuid();
            var portfolioIdForAdmin = Guid.NewGuid();
            var portfolioIdForViewer = Guid.NewGuid();
            var portfolioAssignments = new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto{PrincipalId = userId, ResourceId = portfolioIdForViewer, ResourceType = RoleResourceType.Portfolio, RoleId = WellKnownRoleIds.PortfolioViewer, CustomerId = customerId, PortfolioId = portfolioIdForViewer},
                new RoleAssignmentDto{PrincipalId = userId, ResourceId = portfolioIdForAdmin, ResourceType = RoleResourceType.Portfolio, RoleId = WellKnownRoleIds.PortfolioAdmin, CustomerId = customerId, PortfolioId = portfolioIdForAdmin},
                new RoleAssignmentDto{PrincipalId = userId, ResourceId = siteIdForViewer, ResourceType = RoleResourceType.Site, RoleId = WellKnownRoleIds.SiteViewer, CustomerId = customerId, PortfolioId = portfolioIdForViewer},
                new RoleAssignmentDto{PrincipalId = userId, ResourceId = siteIdForAdmin, ResourceType = RoleResourceType.Site, RoleId = WellKnownRoleIds.SiteAdmin, CustomerId = customerId, PortfolioId = portfolioIdForAdmin },
            };

            var currentUserAssignments = new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto{PrincipalId = currentUserId, ResourceId = customerId, ResourceType = RoleResourceType.Customer, RoleId = WellKnownRoleIds.CustomerAdmin, CustomerId = customerId},
            };

            var portfolios = new List<Portfolio>
            {
                Fixture.Build<Portfolio>()
                    .With(x => x.Id, portfolioIdForAdmin)
                    .With(x => x.Sites, new List<Site>
                    {
                        Fixture.Build<Site>().With(x => x.PortfolioId, portfolioIdForAdmin).With(x => x.Id, siteIdForViewer).Create(),
                        Fixture.Build<Site>().With(x => x.PortfolioId, portfolioIdForAdmin).With(x => x.Id, siteIdForAdmin).Create(),
                    })
                    .Create(),
                Fixture.Build<Portfolio>()
                    .With(x => x.Id, portfolioIdForViewer)
                    .With(x => x.Sites, (List<Site>)null)
                    .Create()
            };

            var user = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With( x=> x.CustomerId, customerId)
                .Create();
            
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, currentUserId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                    .ReturnsJson(portfolioAssignments);
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}/permissionAssignments")
                    .ReturnsJson(currentUserAssignments);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{user.CustomerId}/portfolios?includeSites={true}")
                    .ReturnsJson(portfolios);
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(user);
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}")
                    .ReturnsJson(new User { Id = currentUserId, CustomerId = customerId, Status = UserStatus.Active } );
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissions/manage-users/eligibility?customerId={customerId}")
                    .ReturnsJson(new CheckPermissionResponse { IsAuthorized = true } );

                var sites = portfolios.SelectMany(x => x.Sites ?? new List<Site>());
                
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Post, "sites")
                    .ReturnsJson(sites);

                var response = await client.GetAsync($"management/customers/{customerId}/users/{userId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ManagedUserDto>();
                result.Portfolios.Should().HaveCount(2);
                result.Portfolios[0].PortfolioId.Should().Be(portfolioIdForAdmin);
                result.Portfolios[0].PortfolioName.Should().Be(portfolios[0].Name);
                result.Portfolios[0].Role.Should().Be("Admin");

                var returnedSites = result.Portfolios[0].Sites;
                returnedSites.Should().HaveCount(2);
                returnedSites[0].SiteId.Should().Be(siteIdForViewer);
                returnedSites[0].SiteName.Should().Be(portfolios[0].Sites[0].Name);
                returnedSites[0].Role.Should().Be("Viewer");
                returnedSites[0].LogoUrl.Should().Contain(sites.First(x => x.Id == returnedSites[0].SiteId).LogoPath);

                returnedSites[1].SiteId.Should().Be(siteIdForAdmin);
                returnedSites[1].SiteName.Should().Be(portfolios[0].Sites[1].Name);
                returnedSites[1].Role.Should().Be("Admin");
                returnedSites[1].LogoUrl.Should().Contain(sites.First(x => x.Id == returnedSites[1].SiteId).LogoPath);
                
                result.Portfolios[1].PortfolioId.Should().Be(portfolioIdForViewer);
                result.Portfolios[1].PortfolioName.Should().Be(portfolios[1].Name);
                result.Portfolios[1].Role.Should().Be("Viewer");
                result.Portfolios[1].Sites.Should().BeEmpty();
                
                result.Portfolios = new List<ManagedPortfolioDto>();
                var managedUser = ManagedUserDto.Map(user);

                result.Should().BeEquivalentTo(managedUser);
            }
        }
    }
}
