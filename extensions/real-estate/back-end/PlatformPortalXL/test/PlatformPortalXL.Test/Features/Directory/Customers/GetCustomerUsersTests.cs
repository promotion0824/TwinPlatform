using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Customers
{
    public class GetCustomerUsersTests : BaseInMemoryTest
    {
        public GetCustomerUsersTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CustomerHasUsers_GetCustomerUsers_ReturnsUsers()
        {
            var customerId = Guid.NewGuid();
            var users = Fixture.CreateMany<User>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(null, Permissions.ManageUsers, customerId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/users")
                    .ReturnsJson(users);

                var response = await client.GetAsync($"customers/{customerId}/users");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<UserSimpleDto>>();
                result.Should().BeEquivalentTo(UserSimpleDto.Map(users));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetCustomerUsers_ReturnsForbidden()
        {
            var customerId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnCustomer(null, Permissions.ManageUsers, customerId))
            {
                var response = await client.GetAsync($"customers/{customerId}/users");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}