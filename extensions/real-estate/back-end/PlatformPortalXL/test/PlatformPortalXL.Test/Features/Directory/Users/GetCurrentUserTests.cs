using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using Willow.Directory.Models;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Users
{
    public class GetCurrentUserTests : BaseInMemoryTest
    {
        public GetCurrentUserTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CurrentUserExists_GetCurrentUser_ReturnsCurrentUser()
        {
            var user = Fixture.Create<User>();
            var customer = Fixture.Build<Customer>()
                .With(x => x.Features, new CustomerFeatures { IsConnectivityViewEnabled = true, IsRulingEngineEnabled = true, IsSmartPolesEnabled = true })
                .Create();
            var allowedPortfolios = Fixture.Build<Portfolio>().With(x => x.Sites, new List<Site>()).CreateMany(10);
            var deniedPortfolios = Fixture.Build<Portfolio>().With(x => x.Sites, new List<Site>()).CreateMany(10);
            var allPortfolios = allowedPortfolios.Union(deniedPortfolios);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.DefaultWithSingleTenantOption))
            using (var client = server.CreateClient(null, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                handler
                    .SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                    .ReturnsJson(user);
                handler
                    .SetupRequest(HttpMethod.Get, $"customers/{user.CustomerId}")
                    .ReturnsJson(customer);
                handler
                    .SetupRequest(HttpMethod.Get, $"customers/{user.CustomerId}/users/{user.Id}/preferences")
                    .ReturnsJson(new { test = "abc" });
                handler
                    .SetupRequest(HttpMethod.Get, $"users/{user.Id}/permissionAssignments")
                    .ReturnsJson(new RoleAssignment[] { new RoleAssignment { RoleId = WellKnownRoleIds.CustomerAdmin }});
                handler
                    .SetupRequest(HttpMethod.Get, $"customers/{customer.Id}/portfolios?includeSites={true}")
                    .ReturnsJson(allPortfolios);
                handler
                    .SetupRequest(HttpMethod.Get, $"users/{user.Id}/sites?permissionId=view-sites")
                    .ReturnsJson(Fixture.CreateMany<Site>(3));
                foreach (var portfolio in allPortfolios)
                {
                    var isAllowed = allowedPortfolios.Any(x => x.Id == portfolio.Id);
                    handler
                        .SetupRequest(HttpMethod.Get, $"users/{user.Id}/permissions/{Permissions.ViewPortfolios}/eligibility?portfolioId={portfolio.Id}")
                        .ReturnsJson(new { IsAuthorized = isAllowed });
                }

                var response = await client.GetAsync($"me");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<MeDto>();
                var expectedResult = MeDto.Map(user);
                expectedResult.Customer = CustomerDto.Map(customer, server.Assert().GetImageUrlHelper());
                expectedResult.IsCustomerAdmin = true;
                expectedResult.ShowAdminMenu = true;
                expectedResult.ShowPortfolioTab = true;
                expectedResult.ShowRulingEngineMenu = true;
                expectedResult.Portfolios = PortfolioDto.MapFrom(allowedPortfolios);
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

    }
}
