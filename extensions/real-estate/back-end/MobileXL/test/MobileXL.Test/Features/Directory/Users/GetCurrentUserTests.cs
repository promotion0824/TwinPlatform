using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MobileXL.Dto;
using MobileXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Directory.Users
{
    public class GetCurrentUserTests : BaseInMemoryTest
    {
        public GetCurrentUserTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CurrentCustomerUserExists_GetCurrentCustomerUser_ReturnsCurrentCustomerUser()
        {
            var customerUser = Fixture.Create<CustomerUser>();
            var customer = Fixture.Create<Customer>();
            var siteFeatures = Fixture.Build<SiteFeatures>()
                                        .With(x => x.Is2DViewerDisabled, true)
                                        .With(x => x.Is3DAutoOffsetEnabled, true)
                                        .With(x => x.IsInsightsDisabled, true)
                                        .With(x => x.IsInspectionEnabled, true)
                                        .With(x => x.IsReportsEnabled, true)
                                        .With(x => x.IsTicketingDisabled, false)
                                        .With(x => x.IsScheduledTicketsEnabled, true)
                                        .Create();
            var sites = Fixture.Build<Site>().With(x => x.Features, siteFeatures).CreateMany();
            var custUserPref = Fixture.Create<CustomerUserPreferences>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserRole(customerUser.Id))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{customerUser.Id}")
                    .ReturnsJson(customerUser);
                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"customers/{customerUser.CustomerId}")
                    .ReturnsJson(customer);
                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"customers/{customerUser.CustomerId}/users/{customerUser.Id}/preferences")
                    .ReturnsJson(custUserPref);
                var siteApiHandler = server.Arrange().GetSiteApi();
                siteApiHandler
                    .SetupRequest(HttpMethod.Get, $"customers/{customerUser.CustomerId}/sites")
                    .ReturnsJson(sites);
                foreach (var site in sites)
                {
                    var directoryApiSite = new Site() { Features = siteFeatures };
                    directoryApiHandler
                        .SetupRequest(HttpMethod.Get, $"users/{customerUser.Id}/permissions/{Permissions.ViewSites}/eligibility?siteId={site.Id}")
                        .ReturnsJson(new { IsAuthorized = true });
                    directoryApiHandler
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                        .ReturnsJson(directoryApiSite);
                    siteApiHandler
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                        .ReturnsJson(site);
                }

                var response = await client.GetAsync($"me");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<MeDto>();
                result.Id.Should().Be(customerUser.Id);
                result.FirstName.Should().Be(customerUser.FirstName);
                result.LastName.Should().Be(customerUser.LastName);
                result.Initials.Should().Be(customerUser.Initials);
                result.Email.Should().Be(customerUser.Email);
				result.CustomerId.Should().Be(customerUser.CustomerId);
				result.Sites.Should().BeEquivalentTo(SiteSimpleDto.MapFrom(sites));
            }
        }

    }
}
