using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Autodesk.Forge.Model;
using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using Willow.Tests.Infrastructure;
using Willow.Management;
using Willow.Platform.Users;
using Xunit;
using Xunit.Abstractions;

using Willow.Api.DataValidation;
using Willow.Directory.Models;
using Willow.Platform.Models;

namespace PlatformPortalXL.Test.Features.Management
{
    public class UpdateManagedUserTests : BaseInMemoryTest
    {
        public UpdateManagedUserTests(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public async Task ValidInputCustomerUser_UpdateManagedUser_ReturnsUpdatedUser()
        {
            var customerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = Fixture.Build<UpdateManagedUserRequest>()
                .With(x => x.IsCustomerAdmin, true)
                .With(x => x.ContactNumber, "006112341232")
                .Create();
            var user = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.Company, request.Company)
                .With(x => x.FirstName, request.FirstName)
                .With(x => x.LastName, request.LastName)
                .With(x => x.Mobile, request.ContactNumber)
                .With(x => x.CustomerId, customerId)
                .With(x => x.Status, UserStatus.Active)
                .Create();
            var currentUser = Fixture.Build<User>()
                .With(x => x.Id, currentUserId)
                .With(x => x.Company, request.Company)
                .With(x => x.FirstName, "Bob")
                .With(x => x.LastName, "Jones")
                .With(x => x.Mobile, request.ContactNumber)
                .With(x => x.CustomerId, customerId)
                .With(x => x.Status, UserStatus.Active)
                .Create();
            var customer = Fixture.Build<Customer>()
                .With(c => c.Id, customerId)
                .Create();
            var currentUserAssignments = new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto{PrincipalId = currentUserId, ResourceId = customerId, ResourceType = RoleResourceType.Customer, RoleId = WellKnownRoleIds.CustomerAdmin, CustomerId = customerId},
            };
            var managedUserAssignments = new List<RoleAssignmentDto>
            {
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(currentUserId, Permissions.ManageUsers, customerId))
            {
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}")
                    .ReturnsJson(currentUser);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(user);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}/permissionAssignments")
                    .ReturnsJson(currentUserAssignments);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                    .ReturnsJson(managedUserAssignments);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}")
                    .ReturnsJson(customer);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/users/{userId}")
                    .ReturnsJson(user);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/users/{currentUserId}")
                    .ReturnsJson(currentUser);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{userId}/permissionAssignments/list")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"customers/{customerId}/users/{userId}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"management/customers/{customerId}/users/{userId}",
                    request);
                
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }       
        
        [Fact]
        public async Task ValidInputCustomerUser_UpdateManagedUser_ReturnsUpdatedUser_fr()
        {
            var customerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = Fixture.Build<UpdateManagedUserRequest>()
                .With(x => x.IsCustomerAdmin, true)
                .With(x => x.ContactNumber, "006112341232")
                .Create();
            var user = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.Company, request.Company)
                .With(x => x.FirstName, request.FirstName)
                .With(x => x.LastName, request.LastName)
                .With(x => x.Mobile, request.ContactNumber)
                .With(x => x.CustomerId, customerId)
                .With(x => x.Status, UserStatus.Active)
                .Create();
            var currentUser = Fixture.Build<User>()
                .With(x => x.Id, currentUserId)
                .With(x => x.Company, request.Company)
                .With(x => x.FirstName, "Bob")
                .With(x => x.LastName, "Jones")
                .With(x => x.Mobile, request.ContactNumber)
                .With(x => x.CustomerId, customerId)
                .With(x => x.Status, UserStatus.Active)
                .Create();
            var customer = Fixture.Build<Customer>()
                .With(c => c.Id, customerId)
                .Create();
            var currentUserAssignments = new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto{PrincipalId = currentUserId, ResourceId = customerId, ResourceType = RoleResourceType.Customer, RoleId = WellKnownRoleIds.CustomerAdmin, CustomerId = customerId},
            };
            var managedUserAssignments = new List<RoleAssignmentDto>
            {
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(currentUserId, Permissions.ManageUsers, customerId))
            {
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}")
                    .ReturnsJson(currentUser);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(user);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}/permissionAssignments")
                    .ReturnsJson(currentUserAssignments);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                    .ReturnsJson(managedUserAssignments);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}")
                    .ReturnsJson(customer);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/users/{userId}")
                    .ReturnsJson(user);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/users/{currentUserId}")
                    .ReturnsJson(currentUser);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{userId}/permissionAssignments/list")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"customers/{customerId}/users/{userId}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                client.DefaultRequestHeaders.Add("language", "fr");

                var response = await client.PutAsJsonAsync($"management/customers/{customerId}/users/{userId}",
                    request);
                
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }       
    }
}