using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Management;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Api.DataValidation;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Management.Workgroups
{
    public class CreateWorkgroupTests : BaseInMemoryTest
    {
        public CreateWorkgroupTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateWorkgroup_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageUsers, siteId))
            {
                var response = await client.PostAsJsonAsync($"management/sites/{siteId}/workgroups", new CreateWorkgroupRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task NameIsEmpty_CreateWorkgroup_ReturnsError()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateWorkgroupRequest>()
                .Without(x => x.Name)
                .Create();
            var createdWorkgroup = Fixture.Create<Workgroup>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{siteId}/workgroups", request)
                    .ReturnsJson(createdWorkgroup);

                var response = await client.PostAsJsonAsync($"management/sites/{siteId}/workgroups", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
            }
        }

        [Fact]
        public async Task ValidInput_CreateWorkgroup_ReturnsCreatedWorkgroup()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Create<CreateWorkgroupRequest>();
            var createdWorkgroup = Fixture.Create<Workgroup>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);

                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{site.Id}/workgroups", request)
                    .ReturnsJson(createdWorkgroup);

                var response = await client.PostAsJsonAsync($"management/sites/{site.Id}/workgroups", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<WorkgroupDetailDto>();
                result.Should().BeEquivalentTo(WorkgroupDetailDto.MapFromModel(createdWorkgroup));
            }
        }


        [Fact]
        public async Task SiteDoesNotExist_CreateWorkgroup_ReturnsNotFound()
        {
            var site = Fixture.Create<Site>();
            var request = Fixture.Create<CreateWorkgroupRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson((Site)null);

                var response = await client.PostAsJsonAsync($"management/sites/{site.Id}/workgroups", request);

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
