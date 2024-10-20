using AutoFixture;
using FluentAssertions;
using MobileXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Workflow.Tickets
{
    public class GetPossibleTicketAssigneesTests : BaseInMemoryTest
    {
        public GetPossibleTicketAssigneesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SiteHasCustomerUsers_GetPossibleTicketAssignees_ReturnsThoseIssues()
        {
            var siteId = Guid.NewGuid();
            var expectedUsers = Fixture.Build<TicketAssignee>()
                .With(x => x.Type, TicketAssigneeType.CustomerUser)
                .Without(x => x.FirstName)
                .Without(x => x.LastName)
                .Without(x => x.Name)
                .Do(x => {
                    var firstName = Guid.NewGuid().ToString();
                    var lastName = Guid.NewGuid().ToString();
                    x.FirstName = firstName;
                    x.LastName = lastName;
                    x.Name = $"{firstName} {lastName}";
                })
                .CreateMany(10).ToList();

            var customerUsers = expectedUsers.Select(x => new CustomerUser
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                Status = UserStatus.Active
            });

            var expectedWorkgroups = Fixture.Build<TicketAssignee>()
                .With(x => x.Type, TicketAssigneeType.WorkGroup)
                .Without(x => x.FirstName)
                .Without(x => x.LastName)
                .Without(x => x.Email)
                .CreateMany(5)
                .ToList();

            var workgroups = expectedWorkgroups.Select(x => new Workgroup
            {
                Id = x.Id,
                Name = x.Name
            });

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(customerUsers);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/workgroups")
                    .ReturnsJson(workgroups);

                var response = await client.GetAsync($"sites/{siteId}/possibleTicketAssignees");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketAssignee>>();
                result.Should().BeEquivalentTo(expectedUsers.Union(expectedWorkgroups));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetPossibleTicketAssignees_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/possibleTicketAssignees");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
