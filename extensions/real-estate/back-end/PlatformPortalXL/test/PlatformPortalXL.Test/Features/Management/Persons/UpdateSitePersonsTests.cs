using FluentAssertions;
using PlatformPortalXL.Features.Management;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Directory.Models;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.Features.Workflow;
using Moq.Contrib.HttpClient;
using System.Net.Http.Json;
using System.Collections.Generic;

namespace PlatformPortalXL.Test.Features.Management.Persons
{
    public class UpdateSitePersonsTests : BaseInMemoryTest
    {
        public UpdateSitePersonsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateSiteUsers_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var personId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageUsers, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/persons/{personId}", new UpdateSiteUserRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task ValidInput_UpdateCustomerUser_ReturnsNoContent()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<UpdateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .With(x => x.Email, "abc@abc.com")
                                 .With(x => x.ContactNumber, "006112341232")
                                 .Without(x => x.FullName)
                                 .Create();
            var customerUser = Fixture.Create<User>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(request.Email)}")
                    .ReturnsResponse(HttpStatusCode.NotFound);
                DirectoryUpdateCustomerUserRequest requestToDirectoryApi = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"customers/{site.CustomerId}/users/{customerUser.Id}", async requestMessage =>
                    {
                        requestToDirectoryApi = await requestMessage.Content.ReadAsAsync<DirectoryUpdateCustomerUserRequest>();
                        return true;
                    })
                    .ReturnsResponse(HttpStatusCode.NoContent);
                UpdateUserAssignmentRequest requestToUpdateUserAssignment = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"users/{customerUser.Id}/permissionAssignments", async requestMessage =>
                    {
                        requestToUpdateUserAssignment = await requestMessage.Content.ReadAsAsync<UpdateUserAssignmentRequest>();
                        return true;
                    })
                    .ReturnsResponse(HttpStatusCode.NoContent);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/users")
                    .ReturnsJson(new List<User>() { new User() { Id = customerUser.Id } });

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/persons/{customerUser.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                requestToDirectoryApi.FirstName.Should().Be(request.FirstName);
                requestToDirectoryApi.LastName.Should().Be(request.LastName);
                requestToDirectoryApi.Mobile.Should().Be(request.ContactNumber);
                requestToDirectoryApi.Company.Should().Be(request.Company);
                requestToUpdateUserAssignment.RoleId.Should().Be(request.RoleId.Value);
                requestToUpdateUserAssignment.ResourceId.Should().Be(site.Id);
                requestToUpdateUserAssignment.ResourceType.Should().Be(RoleResourceType.Site);
            }
        }

        [Fact]
        public async Task UpdateDifferentSiteUser_UpdateCustomerUser_ReturnsNotFound()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<UpdateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .With(x => x.Email, "abc@abc.com")
                                 .With(x => x.ContactNumber, "+61 123455678")
                                 .Without(x => x.FullName)
                                 .Create();
            var customerUser = Fixture.Create<User>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(request.Email)}")
                    .ReturnsResponse(HttpStatusCode.NotFound);
                DirectoryUpdateCustomerUserRequest requestToDirectoryApi = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"customers/{site.CustomerId}/users/{customerUser.Id}", async requestMessage =>
                    {
                        requestToDirectoryApi = await requestMessage.Content.ReadAsAsync<DirectoryUpdateCustomerUserRequest>();
                        return true;
                    })
                    .ReturnsResponse(HttpStatusCode.NoContent);
                UpdateUserAssignmentRequest requestToUpdateUserAssignment = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"users/{customerUser.Id}/permissionAssignments", async requestMessage =>
                    {
                        requestToUpdateUserAssignment = await requestMessage.Content.ReadAsAsync<UpdateUserAssignmentRequest>();
                        return true;
                    })
                    .ReturnsResponse(HttpStatusCode.NoContent);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/users")
                    .ReturnsJson(new List<User>());

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/persons/{customerUser.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);

                var content = await response.Content.ReadAsStringAsync();

                content.Should().Contain($"{customerUser.Id}");
            }
        }

        [Fact]
        public async Task ValidInput_UpdateReporter_ReturnsNoContent()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Build<UpdateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.Reporter)
                                 .With(x => x.Email, "abc@xyz.com")
                                 .With(x => x.ContactNumber, "+1713131212")
                                 .Without(x => x.FirstName)
                                 .Without(x => x.LastName)
                                 .Create();
            var reporter = Fixture.Create<Reporter>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                UpdateReporterRequest requestToWorkflowApi = null;
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{site.Id}/reporters/{reporter.Id}", async requestMessage =>
                    {
                        requestToWorkflowApi = await requestMessage.Content.ReadAsAsync<UpdateReporterRequest>();
                        return true;
                    })
                    .ReturnsJson(reporter);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/reporters")
                    .ReturnsJson(new List<Reporter>() { reporter });

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/persons/{reporter.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                requestToWorkflowApi.Name.Should().Be(request.FullName);
                requestToWorkflowApi.Phone.Should().Be(request.ContactNumber);
                requestToWorkflowApi.Email.Should().Be(request.Email);
                requestToWorkflowApi.Company.Should().Be(request.Company);
            }
        }

        [Fact]
        public async Task FirstNameWasNotRequired_UpdateCustomerUser_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var personId = Guid.NewGuid();
            var request = Fixture.Build<CreateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .Without(x => x.FirstName)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PutAsJsonAsync($"sites/{site.Id}/persons/{personId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("First name is required");
            }
        }

        [Fact]
        public async Task LastNameWasNotRequired_UpdateCustomerUser_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var personId = Guid.NewGuid();
            var request = Fixture.Build<UpdateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .Without(x => x.LastName)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PutAsJsonAsync($"sites/{site.Id}/persons/{personId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Last name is required");
            }
        }

        [Fact]
        public async Task ContactWasNotRequired_UpdateCustomerUser_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var personId = Guid.NewGuid();
            var request = Fixture.Build<UpdateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .Without(x => x.ContactNumber)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PutAsJsonAsync($"sites/{site.Id}/persons/{personId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Contact is required");
            }
        }

        [Fact]
        public async Task RoleWasNotProvided_UpdateCustomerUser_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var personId = Guid.NewGuid();
            var request = Fixture.Build<UpdateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.CustomerUser)
                                 .Without(x => x.RoleId)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PutAsJsonAsync($"sites/{site.Id}/persons/{personId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Role is required");
            }
        }

        [Fact]
        public async Task FullNameWasNotProvided_UpdateReporter_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var personId = Guid.NewGuid();
            var request = Fixture.Build<UpdateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.Reporter)
                                 .With(x => x.Email, "abc@xyz.com")
                                 .Without(x => x.FullName)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PutAsJsonAsync($"sites/{site.Id}/persons/{personId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Name is required");
            }
        }

        [Fact]
        public async Task EmailWasNotProvided_UpdateReporter_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var personId = Guid.NewGuid();
            var request = Fixture.Build<UpdateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.Reporter)
                                 .Without(x => x.Email)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PutAsJsonAsync($"sites/{site.Id}/persons/{personId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Email address is required");
            }
        }

        [Fact]
        public async Task EmailWasInvalid_UpdateReporter_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var personId = Guid.NewGuid();
            var request = Fixture.Build<UpdateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.Reporter)
                                 .With(x => x.Email, "invalidemail")
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PutAsJsonAsync($"sites/{site.Id}/persons/{personId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Email address is invalid");
            }
        }

        [Fact]
        public async Task ContactNumberWasNotProvided_UpdateReporter_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var personId = Guid.NewGuid();
            var request = Fixture.Build<UpdateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.Reporter)
                                 .With(x => x.Email, "abc@xyz.com")
                                 .Without(x => x.ContactNumber)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                var response = await client.PutAsJsonAsync($"sites/{site.Id}/persons/{personId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Contact is required");
            }
        }

        [Fact]
        public async Task SiteDoesNotExist_UpdateReporter_ReturnsUnprocessableEntityWithErrorMessage()
        {
            var site = Fixture.Create<Site>();
            var personId = Guid.NewGuid();
            var request = Fixture.Build<UpdateSiteUserRequest>()
                                 .With(x => x.Type, SitePersonType.Reporter)
                                 .With(x => x.Email, "abc@xyz.com")
                                 .With(x => x.ContactNumber, "342-234-2342")
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson((Site)null);

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/persons/{personId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
