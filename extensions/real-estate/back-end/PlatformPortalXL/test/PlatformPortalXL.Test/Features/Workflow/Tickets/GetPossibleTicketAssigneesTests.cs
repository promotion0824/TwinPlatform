using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PlatformPortalXL.Features.Pilot;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Workflow;
using Willow.Workflow.Models;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets
{
    public class GetPossibleTicketAssigneesTests : BaseInMemoryTest
    {
        public GetPossibleTicketAssigneesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SiteHasCustomerUsers_GetPossibleTicketAssignees_ReturnsAssignees()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();

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

            var customerUsers = expectedUsers.Select(x => new User
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
            }).ToList();

            var workflowResponse = Fixture.Build<TicketAssigneesData>()
                                        .With(x => x.Workgroups, workgroups)
                                        .Without(x => x.ExternalUserProfiles)
                                        .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(customerUsers);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/possibleTicketAssignees")
                    .ReturnsJson(workflowResponse);

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
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/possibleTicketAssignees");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GetPossibleTicketAssignees_WithValidScopeId_ReturnsAssignees()
        {
            var siteId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
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

            var customerUsers = expectedUsers.Select(x => new User
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
            }).ToList();

            var workflowResponse = Fixture.Build<TicketAssigneesData>()
                                          .With(x => x.Workgroups, workgroups)
                                          .Without(x => x.ExternalUserProfiles)
                                          .Create();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/users")
                    .ReturnsJson(customerUsers);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/possibleTicketAssignees")
                    .ReturnsJson(workflowResponse);

                var response = await client.GetAsync($"sites/{siteId}/possibleTicketAssignees?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketAssignee>>();
                result.Should().BeEquivalentTo(expectedUsers.Union(expectedWorkgroups));
            }
        }

        [Fact]
        public async Task GetPossibleTicketAssignees_WithInvalidScopeId_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();


            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var expectedTwinDto = new List<TwinDto>();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                var response = await client.GetAsync($"sites/{siteId}/possibleTicketAssignees?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }

        [Fact]
        public async Task GetPossibleTicketAssignees_WithValidScopeId_UserHasNoAccess_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();

            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                var response = await client.GetAsync($"sites/{siteId}/possibleTicketAssignees?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }


        [Fact]
        public async Task SiteHasExternalProfiles_GetPossibleTicketAssignees_ReturnsAssignees()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();

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
            }).ToList();


            var workflowResponse = Fixture.Build<TicketAssigneesData>()
                                        .With(x => x.Workgroups, workgroups)
                                        .Create();

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

            var customerUsers = expectedUsers.Select(x => new User
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                Status = UserStatus.Active
            });


            var expectedExternalUsers = workflowResponse.ExternalUserProfiles.Select(x => new TicketAssignee
            {
                Type = TicketAssigneeType.CustomerUser,
                Id = x.Id,
                Name = x.Name,
                Email = x.Email
            });



            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(customerUsers);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/possibleTicketAssignees")
                    .ReturnsJson(workflowResponse);

                var response = await client.GetAsync($"sites/{siteId}/possibleTicketAssignees");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketAssignee>>();
                result.Should().BeEquivalentTo(expectedUsers.Union(expectedWorkgroups).Union(expectedExternalUsers));
            }
        }

    }
}
