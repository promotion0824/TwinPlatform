using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Management;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Management;
using Willow.Directory.Models;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;

using System.Net.Http.Json;
using Moq.Contrib.HttpClient;
using System.Collections.Generic;

namespace PlatformPortalXL.Test.Features.Management.Persons
{
    public class ManagementController : BaseInMemoryTest
    {
        public ManagementController(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateSiteUsers_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageUsers, siteId))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/persons", new CreateSiteUserRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task ValidInput_CreateCustomerUser_ReturnsCreatedCustomerUser()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .With(x => x.Email, "abc@xyz.com")
                                 .With(x => x.ContactNumber, "2423423423")
                                 .Without(x => x.FullName)
                                 .Create();
            var createdUser = Fixture.Create<User>();
            var expectedRequestToDirectoryApi = new DirectoryCreateCustomerUserRequest
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Mobile = request.ContactNumber,
                Company = request.Company,
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(request.Email)}")
                    .ReturnsResponse(HttpStatusCode.NotFound);
                server.Arrange().GetDirectoryApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"customers/{site.CustomerId}/users", expectedRequestToDirectoryApi)
                    .ReturnsJson(createdUser);
                CreateUserAssignmentRequest requestToDirectoryApi = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{createdUser.Id}/permissionAssignments", async requestMessage =>
                    {
                        requestToDirectoryApi = await requestMessage.Content.ReadAsAsync<CreateUserAssignmentRequest>();
                        return true;
                    })
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CreatePersonResponse>();
                result.Person.Should().BeEquivalentTo(PersonDto.Map(createdUser));
                result.Message.Should().BeEmpty();
                requestToDirectoryApi.RoleId.Should().Be(request.RoleId.Value);
                requestToDirectoryApi.ResourceId.Should().Be(site.Id);
                requestToDirectoryApi.ResourceType.Should().Be(RoleResourceType.Site);
            }
        }

        [Fact]
        public async Task ValidInput_CreateReporter_ReturnsCreatedReporter()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.Reporter)
                                 .With(x => x.Email, "abc@xyz.com")
                                 .With(x => x.ContactNumber, "232 232 4333")
                                 .Create();
            var createdReporter = Fixture.Create<Reporter>();
            var expectedRequestToWorkflowApi = new WorkflowCreateReporterRequest
            {
                Name = request.FullName,
                Email = request.Email,
                CustomerId = site.CustomerId,
                Phone = request.ContactNumber,
                Company = request.Company
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{site.Id}/reporters", expectedRequestToWorkflowApi)
                    .ReturnsJson(createdReporter);

                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CreatePersonResponse>();
                result.Person.Should().BeEquivalentTo(PersonDto.Map(createdReporter));
                result.Message.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task InvalidInput_CreateCustomerUser_ReturnsUnprocessableEntity()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Email, "abc@xyz.com")
                                 .Without(x => x.FullName)
                                 .Create();
            request.Type = null;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);
                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            }
        }

        [Fact]
        public async Task SiteDoesNotExist_CreateReporter_ReturnsNotFound()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.Reporter)
                                 .With(x => x.Email, "abc@xyz.com")
                                 .With(x => x.ContactNumber, "006112341232")
                                 .Without(x => x.FirstName)
                                 .Without(x => x.LastName)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson((Site)null);

                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task FirstNameWasNotRequired_CreateCustomerUser_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .Without(x => x.FirstName)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("First name is required");
            }
        }

        [Fact]
        public async Task LastNameWasNotRequired_CreateCustomerUser_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .Without(x => x.LastName)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Last name is required");
            }
        }

        [Fact]
        public async Task EmailWasNotRequired_CreateCustomerUser_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .Without(x => x.Email)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Email address is required");
            }
        }

        [Fact]
        public async Task ContactWasNotRequired_CreateCustomerUser_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .Without(x => x.ContactNumber)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Contact is required");
            }
        }

        [Fact]
        public async Task RoleWasNotProvided_CreateCustomerUser_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .Without(x => x.RoleId)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Role is required");
            }
        }

        [Fact]
        public async Task InvalidEmailProvided_CreateCustomerUser_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .With(x => x.Email, "incorrectformat")
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Email address is invalid");
            }
        }

        [Fact]
        public async Task AccountWithGivinEmailExistAndAssignedToSameSite_CreateCustomerUser_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var existingEmail = "dummy@email.com";
            var existingAccount = Fixture.Build<Account>()
                                        .With(x => x.Email, existingEmail)
                                        .Create();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .With(x => x.Email, existingEmail)
                                 .Create();
            var roleAssignment = Fixture.Build<RoleAssignment>()
                                        .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(existingEmail)}")
                    .ReturnsJson(existingAccount);

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{existingAccount.UserId}/permissionAssignments?siteId={site.Id}")
                    .ReturnsJson(new List<RoleAssignment> { roleAssignment });

                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("User already exists");
            }
        }

        [Fact]
        public async Task FullNameWasNotProvided_CreateReporter_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.Reporter)
                                 .With(x => x.Email, "abc@xyz.com")
                                 .Without(x => x.FullName)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Name is required");
            }
        }

        [Fact]
        public async Task EmailWasNotProvided_CreateReporter_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.Reporter)
                                 .Without(x => x.Email)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Email address is required");
            }
        }

        [Fact]
        public async Task ContactNumberWasNotProvided_CreateReporter_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.Reporter)
                                 .With(x => x.Email, "abc@xyz.com")
                                 .Without(x => x.ContactNumber)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Contact is required");
            }
        }

        //[Fact]
        private async Task AccountWithGivinEmailExistButAssignedToDifferentSite_CreateCustomerUser_ReturnsExistingCustomerUserEntityWithMessage()
        {
            var customer = Fixture.Create<Customer>();
            var site = Fixture.Build<Site>()
                              .With(s => s.CustomerId, customer.Id)
                              .Create();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .With(x => x.Email, "abc@xyz.com")
                                 .With(x => x.ContactNumber, "+61 123455678")
                                 .Without(x => x.FullName)
                                 .Create();
            var existingAccount = Fixture.Build<Account>()
                                        .With(x => x.Email, request.Email)
                                        .Create();
            var existingUser = Fixture.Build<User>()
                                      .With(u => u.Email, request.Email)
                                      .With(u => u.Id, existingAccount.UserId)
                                      .With(u => u.CustomerId, customer.Id)
                                      .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(request.Email)}")
                    .ReturnsJson(existingAccount)
                    .ReturnsJson(existingAccount);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{existingAccount.UserId}/permissionAssignments?siteId={site.Id}")
                    .ReturnsJson(new List<RoleAssignment>());
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{existingAccount.UserId}")
                    .ReturnsJson(existingUser);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}")
                    .ReturnsJson(customer);
                CreateUserAssignmentRequest requestToDirectoryApi = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{existingUser.Id}/permissionAssignments", async requestMessage =>
                    {
                        requestToDirectoryApi = await requestMessage.Content.ReadAsAsync<CreateUserAssignmentRequest>();
                        return true;
                    })
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"sites/{site.Id}/persons", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CreatePersonResponse>();
                result.Person.Should().BeEquivalentTo(PersonDto.Map(existingUser));
                result.Message.Should().Contain("User already exists and will be provided access to the site");
                requestToDirectoryApi.RoleId.Should().Be(request.RoleId.Value);
                requestToDirectoryApi.ResourceId.Should().Be(site.Id);
                requestToDirectoryApi.ResourceType.Should().Be(RoleResourceType.Site);
            }
        }
    }
}
