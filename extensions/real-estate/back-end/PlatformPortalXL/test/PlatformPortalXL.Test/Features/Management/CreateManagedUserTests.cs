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
    public class CreateManagedUserTests : BaseInMemoryTest
    {
        public CreateManagedUserTests(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public async Task InputNonCustomerAdminToCreateACustomerAdmin_CreateManagedUser_ReturnsValidationError()
        {
            var customerId = Guid.NewGuid();
            var request = Fixture.Build<CreateManagedUserRequest>()
                .With(x => x.IsCustomerAdmin, true)
                .With(x => x.FirstName, "Bob")
                .With(x => x.LastName, "Jones")
                .With(x => x.Company, "Acme Widgets")
                .With(x => x.Email, "abc@xyz.com")
                .With(x => x.ContactNumber, "006112341232")
                .Create();
            var currentUserId = Guid.NewGuid();
            var currentUserAssignments = new List<RoleAssignmentDto>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(currentUserId, Permissions.ManageUsers, customerId))
            {
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(request.Email)}")
                    .ReturnsResponse(HttpStatusCode.NotFound);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}/permissionAssignments")
                    .ReturnsJson(currentUserAssignments);              
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}")
                    .ReturnsJson(new User { Id = currentUserId, CustomerId = customerId, Status = UserStatus.Active } );

                var response = await client.PostAsJsonAsync($"management/customers/{customerId}/users",
                    request);
                
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
        
        [Fact]
        public async Task InputNonCustomerMissingPermissions_CreateManagedUser_ReturnsValidationError()
        {
            var customerId = Guid.NewGuid();
            var adminPortfolioId = Guid.NewGuid();
            var adminSiteId = Guid.NewGuid();
            var portfoliosForRequest = new List<ManagedPortfolioDto>
            {
                new ManagedPortfolioDto
                {
                    Role = "Admin", PortfolioId = adminPortfolioId, PortfolioName = "AdminPortfolio",
                    Sites = new List<ManagedSiteDto>
                        {new ManagedSiteDto { SiteId = Guid.NewGuid(), SiteName = "NonAdminSite", Role = ""}}
                },
                new ManagedPortfolioDto
                {
                    Role = "", PortfolioId = Guid.NewGuid(), PortfolioName = "NonAdminPortfolio",
                    Sites = new List<ManagedSiteDto>
                        {new ManagedSiteDto { SiteId = adminSiteId, SiteName = "AdminSite", Role = "Admin"}}
                },
            };
            var request = Fixture.Build<CreateManagedUserRequest>()
                .With(x => x.IsCustomerAdmin, false)
                .With(x => x.Email)
                .With(x => x.Email, "abc@xyz.com")
                .With(x => x.ContactNumber, "006112341232")
                .With(x => x.Portfolios, portfoliosForRequest)
                .Create();
            var currentUserId = Guid.NewGuid();
            var currentUserAssignments = new List<RoleAssignmentDto> { };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(currentUserId, Permissions.ManageUsers, customerId))
            {
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(request.Email)}")
                    .ReturnsResponse(HttpStatusCode.NotFound);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}/permissionAssignments")
                    .ReturnsJson(currentUserAssignments);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}")
                    .ReturnsJson(new User { Id = currentUserId, CustomerId = customerId, Status = UserStatus.Active } );

                var response = await client.PostAsJsonAsync($"management/customers/{customerId}/users",
                    request);
                
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InvalidInput_CreateManagedUser_ReturnsValidationError()
        {
            var customerId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var assignments = new List<RoleAssignment> { };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(userId, Permissions.ManageUsers, customerId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                    .ReturnsJson(assignments);
                var response = await client.PostAsJsonAsync($"management/customers/{customerId}/users",
                    new CreateManagedUserRequest());

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            }
        }

        [Fact]
        public async Task ValidInputCustomerUser_CreateManagedUser_ReturnsCreatedUser()
        {
            var customerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            var request = Fixture.Build<CreateManagedUserRequest>()
                .With(x => x.IsCustomerAdmin, true)
                .With(x => x.Email, "abc@xyz.com")
                .With(x => x.ContactNumber, "006112341232")
                .Create();
            var user = Fixture.Build<User>()
                .With(x => x.Id, currentUserId)
                .With(x => x.Company, request.Company)
                .With(x => x.Email, request.Email)
                .With(x => x.FirstName, request.FirstName)
                .With(x => x.LastName, request.LastName)
                .With(x => x.Mobile, request.ContactNumber)
                .With(x => x.CustomerId, customerId)
                .Create();
            var customer = Fixture.Build<Customer>()
                .With(c => c.Id, customerId)
                .Create();
            var currentUserAssignments = new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto{PrincipalId = currentUserId, ResourceId = customerId, ResourceType = RoleResourceType.Customer, RoleId = WellKnownRoleIds.CustomerAdmin, CustomerId = customerId},
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(currentUserId, Permissions.ManageUsers, customerId))
            {
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(request.Email)}")
                    .ReturnsResponse(HttpStatusCode.NotFound);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}")
                    .ReturnsJson(new User { Id = currentUserId, CustomerId = customerId, Status = UserStatus.Active});
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}/permissionAssignments")
                    .ReturnsJson(currentUserAssignments);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}")
                    .ReturnsJson(customer);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"customers/{customerId}/users")
                    .ReturnsJson(user);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{currentUserId}/permissionAssignments/list")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"management/customers/{customerId}/users",
                    request);
                
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ManagedUserDto>();
                result.Email.Should().Be(request.Email);
                result.FirstName.Should().Be(request.FirstName);
                result.LastName.Should().Be(request.LastName);
                result.Company.Should().Be(request.Company);
                result.IsCustomerAdmin.Should().BeTrue();
                result.ContactNumber.Should().Be(request.ContactNumber);
            }
        }

                [Fact]
        public async Task ValidInputCustomerUser_CreateManagedUser_ReturnsCreatedUser_fr()
        {
            var customerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            var request = Fixture.Build<CreateManagedUserRequest>()
                .With(x => x.IsCustomerAdmin, true)
                .With(x => x.Email, "abc@xyz.com")
                .With(x => x.ContactNumber, "006112341232")
                .Create();
            var user = Fixture.Build<User>()
                .With(x => x.Id, currentUserId)
                .With(x => x.Company, request.Company)
                .With(x => x.Email, request.Email)
                .With(x => x.FirstName, request.FirstName)
                .With(x => x.LastName, request.LastName)
                .With(x => x.Mobile, request.ContactNumber)
                .With(x => x.CustomerId, customerId)
                .Create();
            var customer = Fixture.Build<Customer>()
                .With(c => c.Id, customerId)
                .Create();
            var currentUserAssignments = new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto{PrincipalId = currentUserId, ResourceId = customerId, ResourceType = RoleResourceType.Customer, RoleId = WellKnownRoleIds.CustomerAdmin, CustomerId = customerId},
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(currentUserId, Permissions.ManageUsers, customerId))
            {
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(request.Email)}")
                    .ReturnsResponse(HttpStatusCode.NotFound);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}")
                    .ReturnsJson(new User { Id = currentUserId, CustomerId = customerId, Status = UserStatus.Active});
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}/permissionAssignments")
                    .ReturnsJson(currentUserAssignments);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}")
                    .ReturnsJson(customer);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"customers/{customerId}/users")
                    .ReturnsJson(user);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{currentUserId}/permissionAssignments/list")
                    .ReturnsResponse(HttpStatusCode.NoContent);

               client.DefaultRequestHeaders.Add("language", "fr");

                var response = await client.PostAsJsonAsync($"management/customers/{customerId}/users",
                    request);
                
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ManagedUserDto>();
                result.Email.Should().Be(request.Email);
                result.FirstName.Should().Be(request.FirstName);
                result.LastName.Should().Be(request.LastName);
                result.Company.Should().Be(request.Company);
                result.IsCustomerAdmin.Should().BeTrue();
                result.ContactNumber.Should().Be(request.ContactNumber);
            }
        }

        [Fact]
        public async Task ValidInputCustomerUser_CreateManagedUserNotAnAdmin_ReturnsCreatedUser()
        {
            var siteId1 = Guid.NewGuid();
            var siteId2 = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            var request = Fixture.Build<CreateManagedUserRequest>()
                .With(x => x.IsCustomerAdmin, false)
                .With(x => x.Email)
                .With(x => x.Portfolios, new List<ManagedPortfolioDto>
                {
                    Fixture.Build<ManagedPortfolioDto>()
                        .With(x => x.Role, "Viewer")
                        .With(x => x.Sites, new List<ManagedSiteDto>
                        {
                            Fixture.Build<ManagedSiteDto>()
                                .With(x => x.Role, "Admin")
                                .With(x => x.SiteId, siteId1)
                                .Create(),
                            Fixture.Build<ManagedSiteDto>()
                                .With(x => x.Role, "Viewer")
                                .With(x => x.SiteId, siteId2)
                                .Create()
                        })
                        .Create()
                })
                .With(x => x.Email, "abc@xyz.com")
                .With(x => x.ContactNumber, "006112341232")
                .Create();
            var user = Fixture.Build<User>()
                .With(x => x.Id, currentUserId)
                .With(x => x.Company, request.Company)
                .With(x => x.Email, request.Email)
                .With(x => x.FirstName, request.FirstName)
                .With(x => x.LastName, request.LastName)
                .With(x => x.Mobile, request.ContactNumber)
                .Create();
            var customer = Fixture.Build<Customer>()
                .With(c => c.Id, customerId)
                .Create();
            var currentUserAssignments = new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto{PrincipalId = currentUserId, ResourceId = customerId, ResourceType = RoleResourceType.Customer, RoleId = WellKnownRoleIds.CustomerAdmin, CustomerId = customerId},
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(currentUserId, Permissions.ManageUsers, customerId))
            {
                
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(request.Email)}")
                    .ReturnsResponse(HttpStatusCode.NotFound);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}/permissionAssignments")
                    .ReturnsJson(currentUserAssignments);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{currentUserId}/permissionAssignments/list")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}")
                    .ReturnsJson(customer);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"customers/{customerId}/users")
                    .ReturnsJson(user);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{currentUserId}")
                    .ReturnsJson(new User { Id = currentUserId, CustomerId = customerId, Status = UserStatus.Active } );
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId1}")
                    .ReturnsJson(new Site { Id = siteId1, CustomerId = customerId } );
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId2}")
                    .ReturnsJson(new Site { Id = siteId2, CustomerId = customerId } );

                var response = await client.PostAsJsonAsync($"management/customers/{customerId}/users",
                    request);
                
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<ManagedUserDto>();
                result.Email.Should().Be(request.Email);
                result.FirstName.Should().Be(request.FirstName);
                result.LastName.Should().Be(request.LastName);
                result.Company.Should().Be(request.Company);
                result.IsCustomerAdmin.Should().BeFalse();
                result.ContactNumber.Should().Be(request.ContactNumber);
            }
        }
        
    }
}