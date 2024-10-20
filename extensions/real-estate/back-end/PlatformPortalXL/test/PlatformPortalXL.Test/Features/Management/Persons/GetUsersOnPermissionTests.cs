using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Common;
using Willow.Directory.Models;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Management.Persons
{
    public class GetUsersOnPermissionTests : BaseInMemoryTest
    {
        public GetUsersOnPermissionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHavePermission_GetUsersOnPermission_ReturnsEmptyUserList()
        {
            var customerId = Guid.NewGuid();
            var user = Fixture.Build<User>().With(x => x.CustomerId, customerId).Create();
            var portfolioId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var users = Fixture.CreateMany<User>(5);
            var portfolios = Fixture.Build<Portfolio>().With(p => p.Id, portfolioId).CreateMany(1);
            var portfolioUsers = Fixture.CreateMany<User>(5);
            var sites = Fixture.Build<Site>().With(s => s.Id, siteId).CreateMany(1);
            var siteUsers = Fixture.CreateMany<User>(5);
            var reporters = Fixture.CreateMany<Reporter>(2);
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Customer, customerId, false);
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Portfolio, portfolioId, false);
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Site, siteId, false);

                handler.SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                    .ReturnsJson(user);
                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/users")
                    .ReturnsJson(users);

                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(portfolios);

                handler.SetupRequest(HttpMethod.Get, $"portfolios/{portfolioId}/users")
                    .ReturnsJson(portfolioUsers);

                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios/{portfolioId}/sites")
                    .ReturnsJson(sites);

                handler.SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(siteUsers);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/reporters")
                    .ReturnsJson(reporters);

                var response = await client.GetAsync($"me/persons");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<PersonDetailDto>>();
                result.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task UserHavePermissionOnSite_GetUsersOnPermission_ReturnsSiteUserList()
        {
            var customerId = Guid.NewGuid();
            var user = Fixture.Build<User>().With(x => x.CustomerId, customerId).Create();
            var portfolioId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var users = Fixture.Build<User>().With(x => x.Status, UserStatus.Active).CreateMany(5);
            var portfolios = Fixture.Build<Portfolio>().With(p => p.Id, portfolioId).CreateMany(1).ToList();
            var portfolioUsers = Fixture.Build<User>().With(x => x.Status, UserStatus.Active).CreateMany(5);
            var sites = Fixture.Build<Site>().With(s => s.Id, siteId).CreateMany(1).ToList();
            var siteUsers = Fixture.Build<User>().With(x => x.Status, UserStatus.Active).CreateMany(5);
            var reporters = Fixture.CreateMany<Reporter>(2);
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Customer, customerId, false);
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Portfolio, portfolioId, false);
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Site, siteId, true);

                handler.SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                    .ReturnsJson(user);
                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/users")
                    .ReturnsJson(users);

                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(portfolios);

                handler.SetupRequest(HttpMethod.Get, $"portfolios/{portfolioId}/users")
                    .ReturnsJson(portfolioUsers);

                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios/{portfolioId}/sites")
                    .ReturnsJson(sites);

                handler.SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(siteUsers);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/reporters")
                    .ReturnsJson(reporters);

                var response = await client.GetAsync($"me/persons");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<PersonDetailDto>>();
                result.Should().HaveCount(7);
                var siteSimpleDto = new SiteSimpleDto { Id = siteId, Name = sites[0].Name, Portfolio = new PortfolioSimpleDto { Id = portfolioId, Name = portfolios[0].Name } };
                var expectedResult = new List<PersonDetailDto>();
                expectedResult.AddRange(PersonDetailDto.Map(siteUsers));
                expectedResult.AddRange(PersonDetailDto.Map(reporters));

                result.Should().BeEquivalentTo(expectedResult, config => config.Excluding(p => p.Sites));
                result[0].Sites[0].Should().BeEquivalentTo(siteSimpleDto);
            }
        }

        [Fact]
        public async Task UserHavePermissionOnPortfolio_GetUsersOnPermission_ReturnsPortofoiloUserList()
        {
            var customerId = Guid.NewGuid();
            var user = Fixture.Build<User>().With(x => x.CustomerId, customerId).Create();
            var portfolioId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var users = Fixture.Build<User>().With(x => x.Status, UserStatus.Active).CreateMany(5);
            var portfolios = Fixture.Build<Portfolio>().With(p => p.Id, portfolioId).CreateMany(1).ToList();
            var portfolioUsers = Fixture.Build<User>().With(x => x.Status, UserStatus.Active).CreateMany(5);
            var sites = Fixture.Build<Site>().With(s => s.Id, siteId).CreateMany(1).ToList();
            var siteUsers = Fixture.Build<User>().With(x => x.Status, UserStatus.Active).CreateMany(5);
            var reporters = Fixture.CreateMany<Reporter>(2);
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Customer, customerId, false);
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Portfolio, portfolioId, true);
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Site, siteId, false);

                handler.SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                    .ReturnsJson(user);
                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/users")
                    .ReturnsJson(users);

                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(portfolios);

                handler.SetupRequest(HttpMethod.Get, $"portfolios/{portfolioId}/users")
                    .ReturnsJson(portfolioUsers);

                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios/{portfolioId}/sites")
                    .ReturnsJson(sites);

                handler.SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(siteUsers);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/reporters")
                    .ReturnsJson(reporters);

                var response = await client.GetAsync($"me/persons");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<PersonDetailDto>>();
                result.Should().HaveCount(5);
                var portfolioSimpleDto = new PortfolioSimpleDto { Id = portfolioId, Name = portfolios[0].Name };
                var expectedResult = new List<PersonDetailDto>();
                expectedResult.AddRange(PersonDetailDto.Map(portfolioUsers));

                result.Should().BeEquivalentTo(expectedResult, config => config.Excluding(p => p.Portfolios));
                result[0].Portfolios[0].Should().BeEquivalentTo(portfolioSimpleDto);
            }
        }

        [Fact]
        public async Task UserHavePermissionOnCustomer_GetUsersOnPermission_ReturnsCustomerUserList()
        {
            var customerId = Guid.NewGuid();
            var user = Fixture.Build<User>().With(x => x.CustomerId, customerId).Create();
            var portfolioId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var users = Fixture.Build<User>()
                .With(x => x.Status, UserStatus.Active)
                .CreateMany(5);
            var portfolios = Fixture.Build<Portfolio>().With(p => p.Id, portfolioId).CreateMany(1).ToList();
            var portfolioUsers = Fixture.Build<User>().With(x => x.Status, UserStatus.Active).CreateMany(5);
            var sites = Fixture.Build<Site>().With(s => s.Id, siteId).CreateMany(1).ToList();
            var siteUsers = Fixture.Build<User>().With(x => x.Status, UserStatus.Active).CreateMany(5);
            var reporters = Fixture.CreateMany<Reporter>(2);
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Customer, customerId, true);
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Portfolio, portfolioId, false);
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Site, siteId, false);

                handler.SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                    .ReturnsJson(user);
                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/users")
                    .ReturnsJson(users);

                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(portfolios);

                handler.SetupRequest(HttpMethod.Get, $"portfolios/{portfolioId}/users")
                    .ReturnsJson(portfolioUsers);

                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios/{portfolioId}/sites")
                    .ReturnsJson(sites);

                handler.SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(siteUsers);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/reporters")
                    .ReturnsJson(reporters);

                var response = await client.GetAsync($"me/persons");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<PersonDetailDto>>();
                result.Should().HaveCount(5);

                var expectedResult = new List<PersonDetailDto>();
                expectedResult.AddRange(PersonDetailDto.Map(users));
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task UserHavePermissionOnPortfolioAndSite_GetUsersOnPermission_ReturnsPortofoiloAndSiteUserList()
        {
            var customerId = Guid.NewGuid();
            var user = Fixture.Build<User>().With(x => x.CustomerId, customerId).Create();
            var portfolioId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var users = Fixture.Build<User>().With(x => x.Status, UserStatus.Active).CreateMany(5);
            var portfolios = Fixture.Build<Portfolio>().With(p => p.Id, portfolioId).CreateMany(1).ToList();
            var portfolioUsers = Fixture.Build<User>().With(x => x.Status, UserStatus.Active).CreateMany(5);
            var sites = Fixture.Build<Site>().With(s => s.Id, siteId).CreateMany(1).ToList();
            var siteUsers = Fixture.Build<User>().With(x => x.Status, UserStatus.Active).CreateMany(5);
            var reporters = Fixture.CreateMany<Reporter>(2);
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Customer, customerId, false);
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Portfolio, portfolioId, true);
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Site, siteId, true);

                handler.SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                    .ReturnsJson(user);
                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/users")
                    .ReturnsJson(users);

                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(portfolios);

                handler.SetupRequest(HttpMethod.Get, $"portfolios/{portfolioId}/users")
                    .ReturnsJson(portfolioUsers);

                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios/{portfolioId}/sites")
                    .ReturnsJson(sites);

                handler.SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(siteUsers);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/reporters")
                    .ReturnsJson(reporters);

                var response = await client.GetAsync($"me/persons");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<PersonDetailDto>>();
                result.Should().HaveCount(12);
                var portfolioSimpleDto = new PortfolioSimpleDto { Id = portfolioId, Name = portfolios[0].Name };
                var siteSimpleDto = new SiteSimpleDto { Id = siteId, Name = sites[0].Name, Portfolio = portfolioSimpleDto };
                var expectedResult = new List<PersonDetailDto>();
                expectedResult.AddRange(PersonDetailDto.Map(portfolioUsers));
                expectedResult.AddRange(PersonDetailDto.Map(siteUsers));
                expectedResult.AddRange(PersonDetailDto.Map(reporters));

                result.Should().BeEquivalentTo(expectedResult, config => {
                    config.Excluding(p => p.Portfolios);
                    config.Excluding(p => p.Sites);
                    return config;
                });
                result[0].Portfolios[0].Should().BeEquivalentTo(portfolioSimpleDto);
                result[5].Sites[0].Should().BeEquivalentTo(siteSimpleDto);
            }
        }

        [Fact]
        public async Task SameUserUserHavePermissionOnPortfolioAndSite_GetUsersOnPermission_ReturnsPortofoiloAndSiteUserList()
        {
            var customerId = Guid.NewGuid();
            var user = Fixture.Build<User>().With(x => x.CustomerId, customerId).Create();
            var portfolioId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var users = Fixture.CreateMany<User>(5);
            var portfolios = Fixture.Build<Portfolio>().With(p => p.Id, portfolioId).CreateMany(1).ToList();
            var portfolioTwo = Fixture.Create<Portfolio>();
            portfolios.Add(portfolioTwo);
            var portfolioUsers = Fixture.Build<User>()
                .With(x => x.Status, UserStatus.Active).CreateMany(5);
            var sites = Fixture.Build<Site>()
                                .With(s => s.PortfolioId, portfolioTwo.Id)
                                .With(s => s.Id, siteId)
                                .CreateMany(1)
                                .ToList();
            var siteTwo = Fixture.Build<Site>()
                                .With(s => s.PortfolioId, portfolioTwo.Id)
                                .Create();
            sites.Add(siteTwo);

            var reporters = Fixture.CreateMany<Reporter>(2);
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, user.Id))
            {
                var handler = server.Arrange().GetDirectoryApi();
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Customer, customerId, false);
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Portfolio, portfolioId, true);
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Portfolio, portfolioTwo.Id, false);
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Site, siteId, true);
                SetupPermissionRequest(handler, user.Id, RoleResourceType.Site, siteTwo.Id, true);

                handler.SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                    .ReturnsJson(user);
                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/users")
                    .ReturnsJson(users);

                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(portfolios);

                handler.SetupRequest(HttpMethod.Get, $"portfolios/{portfolioId}/users")
                    .ReturnsJson(portfolioUsers);

                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios/{portfolioId}/sites")
                    .ReturnsJson(new List<Site>());

                handler.SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios/{portfolioTwo.Id}/sites")
                    .ReturnsJson(sites);

                handler.SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(portfolioUsers);

                handler.SetupRequest(HttpMethod.Get, $"sites/{siteTwo.Id}/users")
                    .ReturnsJson(portfolioUsers);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/reporters")
                    .ReturnsJson(reporters);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteTwo.Id}/reporters")
                    .ReturnsJson(new List<Reporter>());

                var response = await client.GetAsync($"me/persons");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<PersonDetailDto>>();
                result.Should().HaveCount(7);
                var portfolioSimpleDto = new PortfolioSimpleDto { Id = portfolioId, Name = portfolios[0].Name };
                var siteOneSimpleDto = new SiteSimpleDto { Id = siteId, Name = sites[0].Name, Portfolio = new PortfolioSimpleDto { Id = portfolioTwo.Id, Name = portfolioTwo.Name } };
                var siteTwoSimpleDto = new SiteSimpleDto { Id = siteTwo.Id, Name = siteTwo.Name, Portfolio = new PortfolioSimpleDto { Id = portfolioTwo.Id, Name = portfolioTwo.Name } };
                var expectedResult = new List<PersonDetailDto>();
                expectedResult.AddRange(PersonDetailDto.Map(portfolioUsers));
                expectedResult.AddRange(PersonDetailDto.Map(reporters));

                result.Should().BeEquivalentTo(expectedResult, config => {
                    config.Excluding(p => p.Portfolios);
                    config.Excluding(p => p.Sites);
                    return config;
                });
                result[0].Portfolios[0].Should().BeEquivalentTo(portfolioSimpleDto);
                result[0].Sites.Should().BeEquivalentTo(new List<SiteSimpleDto> { siteOneSimpleDto, siteTwoSimpleDto });
                result[5].Sites[0].Should().BeEquivalentTo(siteOneSimpleDto);
            }
        }

        private static void SetupPermissionRequest(DependencyServiceHttpHandler directoryApiHttpHandler, Guid userId, RoleResourceType resourceType, Guid resourceId, bool isAuthorized)
        {
            var url = $"users/{userId}/permissions/{Permissions.ViewUsers}/eligibility";
            switch (resourceType)
            {
                case RoleResourceType.Customer:
                    url = QueryHelpers.AddQueryString(url, "customerId", resourceId.ToString());
                    break;
                case RoleResourceType.Portfolio:
                    url = QueryHelpers.AddQueryString(url, "portfolioId", resourceId.ToString());
                    break;
                case RoleResourceType.Site:
                    url = QueryHelpers.AddQueryString(url, "siteId", resourceId.ToString());
                    break;
                default:
                    throw new ArgumentException().WithData(new { resourceType });
            }
            directoryApiHttpHandler
                .SetupRequest(HttpMethod.Get, url)
                .ReturnsJson(new { IsAuthorized = isAuthorized });
        }
    }
}
