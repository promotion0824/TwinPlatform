using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.SiteStructure.Requests;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Api.DataValidation;
using Willow.Platform.Models;
using Willow.Workflow;
using System.Collections.Generic;
using Willow.Management;
using Willow.Directory.Models;

namespace PlatformPortalXL.Test.Features.SiteStructure.Sites
{
    public class UpdateSiteTests : BaseInMemoryTest
    {
        public UpdateSiteTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_UpdateSite_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var expectedSite = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .With(x => x.CustomerId, customerId)
                .With(x => x.PortfolioId, portfolioId)
                .With(x=>x.Status, SiteStatus.Unknown)
                .Create();
            var request = Fixture.Create<UpdateSiteRequest>();
            var expectedRequestToSiteApi = new SiteApiUpdateSiteRequest
            {
                Name = request.Name,
                Address = request.Address,
                Suburb = request.Suburb,
                Country = request.Country,
                State = request.State,
                TimeZoneId = request.TimeZoneId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Area = request.Area,
                Type = request.Type.Value,
                Status = request.Status.Value,
                ConstructionYear = request.ConstructionYear,
                DateOpened = request.DateOpened,
                SiteCode = request.SiteCode,
                SiteContactEmail = request.SiteContactEmail,
                SiteContactName = request.SiteContactName,
                SiteContactPhone = request.SiteContactPhone,
                SiteContactTitle = request.SiteContactTitle
            };
            var expectedRequestToDirectoryApi = new DirectoryApiUpdateSiteRequest
            {
                Name = request.Name,
                Features = request.Features,
                TimeZoneId = request.TimeZoneId,
                Status = request.Status.Value,
				ArcGisLayers = request.ArcGisLayers,
				WebMapId = request.WebMapId
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}", expectedRequestToSiteApi)
                    .ReturnsJson(expectedSite);
                server.Arrange().GetDirectoryApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}", expectedRequestToDirectoryApi)
                    .ReturnsJson(expectedSite);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/settings")
                    .ReturnsJson(new SiteSettings());
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(new[] { new PortfolioDto { Id = portfolioId } });

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                    .ReturnsJson(new List<RoleAssignmentDto>());

                var response = await client.PutAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }


        [Fact]
        public async Task InvalidInput_UpdateSite_ReturnsValidationError()
        {
            var siteId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var request = new UpdateSiteRequest();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(new [] {new PortfolioDto{Id = portfolioId}});

                var response = await client.PutAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(9);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateSite_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = Fixture.Build<UpdateSiteRequest>()
                         .With(x=>x.Status, SiteStatus.Construction)
                         .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                    .ReturnsJson(new List<RoleAssignmentDto>());
                var response = await client.PutAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task PortfolioDoesntBelongToACustomer_UpdateSite_ReturnsBadRequest()
        {
            var siteId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = Fixture.Build<UpdateSiteRequest>()
                         .With(x=>x.Status, SiteStatus.Construction)
                         .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(new [] {new PortfolioDto{Id = Guid.NewGuid()}});

                server.Arrange().GetDirectoryApi()
                   .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                   .ReturnsJson(new List<RoleAssignmentDto>());

                var response = await client.PutAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task NonCustomerAdmin_UpdateSiteWithDeleteStatus_ReturnsUnAuthorized()
        {
            var siteId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var currentUserAssignments = new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto{
                    PrincipalId = userId,
                    ResourceId = customerId,
                    ResourceType = RoleResourceType.Customer,
                    RoleId = WellKnownRoleIds.SiteAdmin,
                    CustomerId = customerId},
            };

            var request = Fixture.Build<UpdateSiteRequest>()
                         .With(x => x.Status, SiteStatus.Deleted)
                         .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetDirectoryApi()
                   .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                   .ReturnsJson(currentUserAssignments);

                var response = await client.PutAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task CustomerAdmin_UpdateSiteWithDeleteStatus_ReturnsNoContent()
        {
            var siteId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var currentUserAssignments = new List<RoleAssignmentDto>
            {
                new RoleAssignmentDto{
                    PrincipalId = userId,
                    ResourceId = customerId,
                    ResourceType = RoleResourceType.Customer,
                    RoleId = WellKnownRoleIds.CustomerAdmin,
                    CustomerId = customerId},
            };
            var expectedSite = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .With(x => x.CustomerId, customerId)
                .With(x => x.PortfolioId, portfolioId)
                .With(x => x.Status, SiteStatus.Deleted)
                .Create();
            var request = Fixture.Build<UpdateSiteRequest>()
                                 .With(x=>x.Status, SiteStatus.Deleted)
                                 .Create();

            var expectedRequestToSiteApi = new SiteApiUpdateSiteRequest
            {
                Name = request.Name,
                Address = request.Address,
                Suburb = request.Suburb,
                Country = request.Country,
                State = request.State,
                TimeZoneId = request.TimeZoneId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Area = request.Area,
                Type = request.Type.Value,
                Status = request.Status.Value,
                DateOpened = request.DateOpened,
                ConstructionYear = request.ConstructionYear,
                SiteCode = request.SiteCode,
                SiteContactEmail = request.SiteContactEmail,
                SiteContactName = request.SiteContactName,
                SiteContactPhone = request.SiteContactPhone,
                SiteContactTitle = request.SiteContactTitle
            };
            var expectedRequestToDirectoryApi = new DirectoryApiUpdateSiteRequest
            {
                Name = request.Name,
                Features = request.Features,
                TimeZoneId = request.TimeZoneId,
                Status = request.Status.Value,
				ArcGisLayers = request.ArcGisLayers,
				WebMapId = request.WebMapId
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}", expectedRequestToSiteApi)
                    .ReturnsJson(expectedSite);
                server.Arrange().GetDirectoryApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}", expectedRequestToDirectoryApi)
                    .ReturnsJson(expectedSite);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/settings")
                    .ReturnsJson(new SiteSettings());
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/portfolios")
                    .ReturnsJson(new[] { new PortfolioDto { Id = portfolioId } });

                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/permissionAssignments")
                    .ReturnsJson(currentUserAssignments);

                var response = await client.PutAsJsonAsync($"customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
