using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Management.Persons
{
    public class GetSitePersonsTests : BaseInMemoryTest
    {
        public GetSitePersonsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetSiteUsers_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewUsers, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/persons");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task CustomerHasUsers_GetCustomerUsers_ReturnsUsers()
        {
            var siteId = Guid.NewGuid();
            var users = Fixture.CreateMany<User>(5);
            var reporters = Fixture.CreateMany<Reporter>(2);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewUsers, siteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/users")
                    .ReturnsJson(users);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/reporters")
                    .ReturnsJson(reporters);

                var response = await client.GetAsync($"sites/{siteId}/persons");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<PersonDto>>();
                result.Should().HaveCount(7);
                var toCompare = new List<PersonDto>();
                toCompare.AddRange(PersonDto.Map(users));
                toCompare.AddRange(PersonDto.Map(reporters));
                result.Should().BeEquivalentTo(toCompare);
            }
        }

    }
}
